using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb
{
    /// <summary>
    /// Represents a reader for USB endpoint transfers that uses a queue to manage multiple read operations.
    /// </summary>
    public sealed class UsbEndpointTransferQueueReader : IDisposable
    {
        private readonly List<TimedTask> _transferQueue;
        private readonly Task _readTask;
        private readonly UsbEndpointReader _usbEndpointReader;
        private readonly CancellationTokenSource _cts;
        private readonly int _readBufferSize;
        private readonly Channel<byte[]> _dataChannel;

        /// <summary>
        /// Represents a task that has been timed, allowing us to track when it was completed.
        /// </summary>
        private sealed class TimedTask
        {
            /// <summary>
            /// The task that represents the read operation.
            /// </summary>
            public Task Task { get; }

            /// <summary>
            /// The stopwatch that tracks when the task was completed.
            /// </summary>
            public DateTime CompletedAt { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TimedTask"/> class with the specified task.
            /// </summary>
            public TimedTask(Task task)
            {
                Task = task;
                Task.ContinueWith(_ => CompletedAt = DateTime.UtcNow, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

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
        public UsbEndpointTransferQueueReader(IUsbDevice usbDevice, int readBufferSize, ReadEndpointID readEndpointId, int transferQueueSize = 1)
            : this(usbDevice, readBufferSize, readEndpointId, transferQueueSize, CancellationToken.None)
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
        public UsbEndpointTransferQueueReader(IUsbDevice usbDevice, int readBufferSize, ReadEndpointID readEndpointId, int transferQueueSize, CancellationToken token)
        {
            _usbEndpointReader = usbDevice.OpenEndpointReader(readEndpointId, readBufferSize);
            _transferQueue = new List<TimedTask>(transferQueueSize);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _readBufferSize = readBufferSize;
            _dataChannel = Channel.CreateUnbounded<byte[]>();

            // Pre-fill the queue with read tasks
            for (var i = 0; i < transferQueueSize; i++)
            {
                _transferQueue.Add(new TimedTask(ReadAsync()));
            }

            _readTask = Task.Factory.StartNew(TransferQueueRead, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private Task TransferQueueRead()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Wait for any task in the transfer queue to complete
                    Task.WhenAny(_transferQueue.Select(queue => queue.Task)).Wait(_cts.Token);

                    // Sort the transfer queue by the elapsed time since the task was completed
                    _transferQueue.Sort((t1, t2) => t1.CompletedAt.CompareTo(t2.CompletedAt));

                    // Remove the first completed task from the queue and enqueue a new read task
                    var completedTask = _transferQueue.FirstOrDefault(t => t.Task.IsCompleted);
                    _transferQueue.Remove(completedTask);
                    _transferQueue.Add(new TimedTask(ReadAsync()));
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
                }
            }
            return Task.CompletedTask;
        }

        private async Task ReadAsync()
        {
            var buffer = new byte[_readBufferSize];
            var (error, transferLength) = await _usbEndpointReader.ReadAsync(buffer, 0, buffer.Length, 100).ConfigureAwait(false);
            if (error != Error.Success && error != Error.Timeout)
            {
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(new UsbException(error)));
                return;
            }

            if (transferLength == 0)
            {
                // No data received, possibly endpoint is idle or no data available
                return;
            }

            // Write the received data to the channel
            var data = new byte[transferLength];
            Array.Copy(buffer, data, transferLength);
            await _dataChannel.Writer.WriteAsync(data, _cts.Token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts.Cancel();
            _readTask.Wait();

            _dataChannel.Writer.Complete();
            _dataChannel.Reader.Completion.Wait();

            _usbEndpointReader.ReadFlush();
        }
    }
}
