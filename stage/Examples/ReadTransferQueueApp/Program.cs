using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace LibUsbDotNet.ReadTransferQueueApp
{
    public static class ReadTransferQueueApp
    {
        #region SET YOUR USB Vendor and Product ID!

        public static readonly UsbDeviceFinder DEFAULT_USBFINDER = new UsbDeviceFinder(1234, 1);

        #endregion

        public static void Main(string[] args)
        {
            using (var loggerFactory = new ConsoleLoggerFactory())
            {
                var logger = loggerFactory.CreateLogger(nameof(ReadTransferQueueApp));

                while (true)
                {
                    if (Console.KeyAvailable) // Check if a key is available
                    {
                        var readKeyResult = Console.ReadKey(true);
                        if (readKeyResult.Key == ConsoleKey.Enter) // Close application if Enter key is pressed
                        {
                            logger.LogInformation("Close application");
                            break;
                        }
                    }

                    var usbDevice = UsbDevice.OpenUsbDevice(DEFAULT_USBFINDER);
                    if (usbDevice == null)
                    {
                        logger.LogError("USB device not found");
                        Environment.Exit(-1);
                        return;
                    }

                    logger.LogInformation("Open USB device");
                    try
                    {
                        IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
                        wholeUsbDevice?.SetConfiguration(1);
                        wholeUsbDevice?.ClaimInterface(0);

                        var writer = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                        var reader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                        var transferQueue = new UsbTransferQueue(reader, 4, 2048, Timeout.Infinite, 0);
                        try
                        {
                            var sw = Stopwatch.StartNew();
                            while (sw.Elapsed < TimeSpan.FromSeconds(5))
                            {
                                var transferResult = transferQueue.Transfer(out var handle);
                                if (transferResult == ErrorCode.None)
                                {
                                    if (!handle.Context.IsCancelled)
                                    {
                                        logger.LogDebug("Received transfer data");
                                        continue;
                                    }
                                }
                                if (transferResult != ErrorCode.IoTimedOut)
                                {
                                    logger.LogError("Received transfer error [{errorCode}]", Enum.GetName(typeof(ErrorCode), transferResult));
                                    break;
                                }
                            }
                            sw.Stop();
                        }
                        finally
                        {
                            logger.LogInformation("Stop transfer queue");
                            transferQueue.Free();

                            writer.Dispose();
                            reader.Dispose();
                        }
                    }
                    finally
                    {
                        logger.LogInformation("Close USB device");

                        if (usbDevice.IsOpen)
                        {
                            IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
                            wholeUsbDevice?.ReleaseInterface(0);
                            wholeUsbDevice?.Close();
                        }
                        UsbDevice.Exit();
                    }
                }
            }
        }
    }
}
