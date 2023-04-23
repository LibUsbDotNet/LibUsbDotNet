using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace Examples;

internal static class Hotplug
{
    private static readonly ConcurrentDictionary<UsbDeviceFinder, TaskCompletionSource<UsbDevice>> DeviceArrivedTasks = new();
    private static readonly ConcurrentDictionary<UsbDeviceFinder, TaskCompletionSource<CachedDeviceInfo>> DeviceLeftTasks = new();
    
    public static async Task Main(string[] args)
    {
        using var context = new UsbContext();
        var port = new PhysicalPortId(new LocationId(5, new byte[] { 4 }), new LocationId(6, new byte[] { 4 }));
        var finder = new UsbDeviceFinder
        {
            PhyiscalPortId = port
        };
        context.SetDebugLevel(LogLevel.Info);
        context.DeviceEvent += OnDeviceEvent;
        var hotplug1 = new HotplugOptions();
        context.RegisterHotPlug(hotplug1);
        bool stableConnection = false;
        int count = 0;
        while (count < 8)
        {
            var deviceArrivedTaskSource = new TaskCompletionSource<UsbDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!DeviceArrivedTasks.TryAdd(finder, deviceArrivedTaskSource))
                throw new InvalidOperationException("Could not add arrived task");
            Console.WriteLine($"Waiting for device arrival.");

            var arriveddevice = await deviceArrivedTaskSource.Task;

            var deviceLeftTaskSource = new TaskCompletionSource<CachedDeviceInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!DeviceLeftTasks.TryAdd(finder, deviceLeftTaskSource))
                throw new InvalidOperationException("Could not add left task");
            Console.WriteLine($"Waiting for device disconnect.");
            var completion = await Task.WhenAny(deviceLeftTaskSource.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            if (completion is Task<CachedDeviceInfo> deviceLeft)
            {
                Console.WriteLine("Device left");
                Console.WriteLine($"Device is the recently arrived device: {deviceLeft.Result.GetHashCode() == arriveddevice.GetHashCode()}");
            }
            else
            {
                Console.WriteLine("Timeout waiting for device disconnect");
                DeviceLeftTasks.TryRemove(finder, out _);
                stableConnection = true;
            }
            arriveddevice.Dispose();
            count++;
        }

        Console.WriteLine("Got a stable connection.");
        
        Console.ReadKey();

        Console.WriteLine("\nUnregister hotplug.");
        
        context.UnregisterHotPlug(hotplug1);

        Console.ReadKey();
    }

    private static void OnDeviceEvent(object sender, DeviceEventArgs e)
    {
        switch (e)
        {
            case DeviceArrivedEventArgs arrivedEventArgs:
            {
                var device = arrivedEventArgs.Device;

                var matchingFinder =
                    DeviceArrivedTasks.Keys.FirstOrDefault(finder => finder.Check(device));
                Console.WriteLine($"{DateTime.Now} {e.GetType().Name.Replace("EventArgs", string.Empty)} VendorId-0x{device.VendorId:X4} ProductId-0x{device.ProductId:X4} Port-{device.LocationId}");
                if (matchingFinder is not null && DeviceArrivedTasks.TryRemove(matchingFinder, out var taskCompletionSource))
                    taskCompletionSource.SetResult(device);
                break;
            }
            case DeviceLeftEventArgs leftEventArgs:
            {
                var info = leftEventArgs.DeviceInfo;
                var matchingFinder =
                    DeviceLeftTasks.Keys.FirstOrDefault(finder => finder.Check(info));
                Console.WriteLine($"{DateTime.Now} {e.GetType().Name.Replace("EventArgs", string.Empty)} VendorId-0x{info.Descriptor.VendorId:X4} ProductId-0x{info.Descriptor.ProductId:X4} Port-{info.PortInfo}");
                if (matchingFinder is not null && DeviceLeftTasks.TryRemove(matchingFinder, out var taskCompletionSource))
                    taskCompletionSource.SetResult(info);
                break;
            }
        }
    }
}