using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using System;
using System.Threading;

using var ctr = new CancellationTokenSource();
using var context = new UsbContext();
using var deviceManager = new DeviceManager(context);

deviceManager.Start();

var finder = new UsbDeviceFinder()
{
    Vid = 0x0000, // Put your Vendor Id Here
    Pid = 0x0000 // Put your Product Id Here
};

using var device = await deviceManager.WaitForDeviceArrival(finder, TimeSpan.FromSeconds(30), ctr.Token, TimeSpan.FromSeconds(2));
if (device == null)
{
    Console.WriteLine($"Device [0x{finder.Vid:X4}/0x{finder.Pid:X4}] could not be found!");
    return;
}

Console.WriteLine($"Device {device} was attached");