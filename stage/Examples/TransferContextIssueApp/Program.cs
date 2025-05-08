using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransferContextIssueApp
{
    public static class TransferContextIssueApp
    {
        private static ILoggerFactory _loggerFactory;

        private static async Task ExecuteAppAsync(ILoggerFactory loggerFactory, ITestEnvironment testEnv)
        {
            var logger = loggerFactory.CreateLogger(nameof(ExecuteAppAsync));

            var usbDevice = await testEnv.OpenUsbDeviceAsync();
            if (usbDevice == null)
            {
                throw new ApplicationException("Could not open USB device");
            }

            var writer = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
            var reader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            reader.DataReceivedEnabled = true;
            reader.DataReceived += OnDataReceived;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                var retries = 0;
                while (!testEnv.Token.IsCancellationRequested && retries < 3)
                {
                    var helloWorld = Encoding.Default.GetBytes("Hello World");
                    var result = writer.Write(helloWorld, 2000, out _);
                    if (result != ErrorCode.Ok)
                    {
                        logger.LogError("Could not write data [{0}]: Error code [{1}]", BitConverter.ToString(helloWorld), result);
                    }
                    else
                    {
                        logger.LogDebug("Wrote data [{0}]", BitConverter.ToString(helloWorld));
                    }

                    var isDetached = await testEnv.DetachUsbDeviceAsync();
                    if (!isDetached)
                    {
                        throw new ApplicationException("Could not detach USB device");
                    }

                    var isAttached = await testEnv.AttachUsbDeviceAsync();
                    if (!isAttached)
                    {
                        throw new ApplicationException("Could not attach USB device");
                    }
                    retries++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred: {0}", ex.Message);
            }
            finally
            {
                writer.Dispose();
                reader.DataReceived -= OnDataReceived;
                reader.Dispose();

                testEnv.CloseUsbDevice(usbDevice);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void OnDataReceived(object sender, EndpointDataEventArgs args)
        {
            var logger = _loggerFactory.CreateLogger(nameof(OnDataReceived));
            logger.LogDebug("Data received: {0}", BitConverter.ToString(args.Buffer).Substring(0, args.Count));
        }

        public static void Main()
        {
            _loggerFactory = new ConsoleLoggerFactory(LogLevel.Debug);
            var cts = new CancellationTokenSource();
            try
            {
                var testEnv = TestEnvironmentFactory.Create(LogLevel.Debug, cts.Token);
                while (!testEnv.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable) // Check if a key is available
                    {
                        var readKeyResult = Console.ReadKey(true);
                        if (readKeyResult.Key == ConsoleKey.Enter) // Close application if Enter key is pressed
                        {
                            cts.Cancel();
                            break;
                        }
                    }
                    var executeApp = Task.Run(async () => await ExecuteAppAsync(_loggerFactory, testEnv), cts.Token);
                    Task.WaitAll(new Task[] { executeApp }, cts.Token);
                }
            }
            finally
            {
                cts.Cancel();
                cts.Dispose();
                _loggerFactory.Dispose();
            }
        }
    }
}
