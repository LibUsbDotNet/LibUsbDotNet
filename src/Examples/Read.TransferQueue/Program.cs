using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

var cts = new CancellationTokenSource();
var context = new UsbContext();
var usbFinder = new UsbDeviceFinder()
{
    Vid = 0x0000, // Replace with your USB device's Vendor ID
    Pid = 0x0000 // Replace with your USB device's Product ID
};

using var usbDevice = context.Find(usbFinder);
if (usbDevice is null)
{
    Console.WriteLine("USB device not found.");
    return;
}

usbDevice.Open();
usbDevice.SetConfiguration(1);
usbDevice.ClaimInterface(0);

var reader = usbDevice.OpenEndpointTransferQueueReader(ReadEndpointID.Ep01, 2048, cts.Token, 4);
reader.ErrorOccurred += OnErrorOccurred;

var dataReceivedTask = Task.Factory.StartNew(() => ReadEchoDataAsync(reader, cts.Token), cts.Token,
    TaskCreationOptions.LongRunning, TaskScheduler.Current);

Console.WriteLine("Press any key to stop...");

Console.ReadKey();

cts.Cancel();
dataReceivedTask.Wait();

reader.ErrorOccurred -= OnErrorOccurred;

if (usbDevice.IsOpen)
{
    usbDevice.ReleaseInterface(0);
}
usbDevice.Close();

return;

static async Task ReadEchoDataAsync(UsbEndpointTransferQueueReader reader, CancellationToken token)
{
    while (await reader.DataReceived.WaitToReadAsync(token))
    {
        while (reader.DataReceived.TryRead(out var data))
        {
            Console.WriteLine($"Data received: {BitConverter.ToString(data)}");
        }
    }
}

static void OnErrorOccurred(object sender, ErrorEventArgs e)
{
    Console.WriteLine($"Error occurred: {e.GetException()}");
}