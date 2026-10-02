using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;

namespace LibUsbDotNet.LibUsb
{
    /// <summary>
    /// Represents a reader for USB endpoint transfers that uses a queue to manage multiple read operations.
    /// </summary>
    public sealed class UsbEndpointTransferQueueReader : IDisposable
    {
        private readonly UsbEndpointReader _usbEndpointReader;
        private readonly CancellationTokenSource _cts;
        private readonly int _readBufferSize;
        private readonly Channel<byte[]> _dataChannel;
        private readonly int _readTimeoutMilliseconds;
        private readonly List<Thread> _rxThreads;

        /// <summary>
        /// Channel that provides the data received from the USB endpoint.
        /// </summary>
        public ChannelReader<byte[]> DataReceived => _dataChannel.Reader;

        /// <summary>
        /// Event that is raised when an error occurs during reading from the USB endpoint.
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbEndpointTransferQueueReader"/> class.
        /// </summary>
        /// <param name="usbDevice">USB device</param>
        /// <param name="readBufferSize">Read buffer size for <see cref="UsbEndpointReader"/></param>
        /// <param name="readEndpointId">Endpoint ID for <see cref="UsbEndpointReader"/></param>
        /// <param name="transferQueueSize">Specifies how many read operations can be queued at once and is by default set to 1.</param>
        /// <param name="readTimeoutMilliseconds">Specifies the read timeout in milliseconds and is by default set to 100.</param>
        /// <param name="threadPriority">Specifies the priority of the read threads and is by default set to <see cref="ThreadPriority.Normal"/>.</param>
        public UsbEndpointTransferQueueReader(IUsbDevice usbDevice, int readBufferSize, ReadEndpointID readEndpointId, 
            int transferQueueSize = 1, int readTimeoutMilliseconds = 100, ThreadPriority threadPriority = ThreadPriority.Normal)
            : this(usbDevice, readBufferSize, readEndpointId, transferQueueSize, readTimeoutMilliseconds, threadPriority, CancellationToken.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbEndpointTransferQueueReader"/> class.
        /// </summary>
        /// <param name="usbDevice">USB device</param>
        /// <param name="readBufferSize">Read buffer size for <see cref="UsbEndpointReader"/></param>
        /// <param name="readEndpointId">Endpoint ID for <see cref="UsbEndpointReader"/></param>
        /// <param name="transferQueueSize">Size of the transfer queue</param>
        /// <param name="token">Token</param>
        /// <param name="readTimeoutMilliseconds">Specifies the read timeout in milliseconds and is by default set to 100.</param>
        /// <param name="threadPriority">Specifies the priority of the read threads and is by default set to <see cref="ThreadPriority.Normal"/>.</param>
        public UsbEndpointTransferQueueReader(IUsbDevice usbDevice, int readBufferSize, ReadEndpointID readEndpointId, 
            int transferQueueSize, int readTimeoutMilliseconds, ThreadPriority threadPriority, CancellationToken token)
        {
            transferQueueSize = transferQueueSize < 1 ? 1 : transferQueueSize;
            _readTimeoutMilliseconds = readTimeoutMilliseconds < 1 ? 100 : readTimeoutMilliseconds;
            _readBufferSize = readBufferSize < 1 ? 1024 : readBufferSize;

            _usbEndpointReader = usbDevice.OpenEndpointReader(readEndpointId, _readBufferSize);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _dataChannel = Channel.CreateBounded<byte[]>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                });
            _rxThreads = new List<Thread>(transferQueueSize);

            for (var i = 0; i < transferQueueSize; i++)
            {
                var rxThread = new Thread(Read)
                {
                    IsBackground = true,
                    Priority = threadPriority,
                    Name = $"LibUsbTransferQueueReader-Rx-{i}"
                };
                _rxThreads.Add(rxThread);
                rxThread.Start();
            }
        }

        private void Read()
        {
            var buffer = new byte[_readBufferSize];

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var error = _usbEndpointReader.Read(buffer, 0, buffer.Length, _readTimeoutMilliseconds,
                        out var transferLength);
                    if (error != Error.Success && error != Error.Timeout)
                    {
                        ErrorOccurred?.Invoke(this, new ErrorEventArgs(new UsbException(error)));
                        return;
                    }

                    if (transferLength == 0)
                    {
                        // No data received, possibly endpoint is idle or no data available
                        continue;
                    }

                    // Write the received data to the channel
                    var data = new byte[transferLength];
                    Array.Copy(buffer, data, transferLength);

                    var writeResult = _dataChannel.Writer.TryWrite(data);
                    if (!writeResult)
                    {
                        ErrorOccurred?.Invoke(this,
                            new ErrorEventArgs(new InvalidOperationException("Failed to write data to the channel.")));
                    }

                    Array.Clear(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    ErrorOccurred?.Invoke(this, new ErrorEventArgs(e));
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts.Cancel();

            foreach (var rxThread in _rxThreads)
            {
                if (!rxThread.Join(TimeSpan.FromSeconds(5)))
                {
                    ErrorOccurred?.Invoke(this,
                        new ErrorEventArgs(new TimeoutException("Data receive thread did not terminate within the expected time.")));
                }
            }

            _dataChannel.Writer.Complete();
            _dataChannel.Reader.Completion.Wait();
        }
    }
}
