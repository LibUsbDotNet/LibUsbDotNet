using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;

namespace Examples;

internal static class Hotplug
{
    public static async Task Main(string[] args)
    {
        using var context = new UsbContext();
        var port = new PhysicalPortId(new LocationId(5, new byte[] { 4 }), new LocationId(6, new byte[] { 4 }));
        context.SetDebugLevel(LogLevel.Debug);
        context.DeviceEvent += OnDeviceEvent;
        var hotplug1 = new HotplugOptions();
        context.RegisterHotPlug(hotplug1);
        bool stableConnection = false;
        int count = 0;
        while (count < 8)
        {
            var deviceArrivedTaskSource = new TaskCompletionSource<UsbDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!DeviceManagement.DeviceArrivedTasks.TryAdd(port, deviceArrivedTaskSource))
                throw new InvalidOperationException("Could not add arrived task");
            Console.WriteLine($"Waiting for device arrival on {port}");
            // UsbDevice arriveddevice = (UsbDevice)(await deviceArrivedTaskSource.Task).Clone();
            var arriveddevice = await deviceArrivedTaskSource.Task;
            // arriveddevice.Open();
            // Console.WriteLine($"Found device with serial: {arriveddevice.Descriptor.SerialNumber}");
        
            // var deviceLeftTaskSource = new TaskCompletionSource<UsbDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            // if (!DeviceManagement.DeviceLeftTasks.TryAdd(port, deviceLeftTaskSource))
            //     throw new InvalidOperationException("Could not add left task");
            // Console.WriteLine($"Waiting for device disconnect on {port}");
            // var completion = await Task.WhenAny(deviceLeftTaskSource.Task, Task.Delay(TimeSpan.FromSeconds(2)));
            // if (completion is Task<UsbDevice> deviceLeft)
            // {
            //     Console.WriteLine("Device left");
            //     Console.WriteLine($"Device is the recently arrived device: {deviceLeft.Result.GetHashCode() == arriveddevice.GetHashCode()}");
            // }
            // else
            // {
            //     Console.WriteLine("Timeout waiting for device disconnect");
            //     DeviceManagement.DeviceLeftTasks.TryRemove(port, out _);
            //     stableConnection = true;
            // }
            arriveddevice.Dispose();
            count++;
        }

        Console.WriteLine("Got a stable connection.");
        
        Console.ReadKey();

        Console.WriteLine("\nUnregister hotplug.");
        context.UnregisterHotPlug(hotplug1);

        Console.ReadKey();
    }
    
    static void OnDeviceEvent(object sender, DeviceEventArgs e)
    {
        var stopwatch = Stopwatch.StartNew();
        var device = e.Device;
        if (e is DeviceArrivedEventArgs arrivedEventArgs)
        {
            var info = new CachedDeviceInfo(device.Descriptor, device.LocationId);
            DeviceManagement.UsbDeviceInfos.TryAdd(device, info);
            var port = DeviceManagement.DeviceArrivedTasks.Keys.FirstOrDefault(portId => portId.HasDevice(device));
            Console.WriteLine($"{DateTime.Now} {e.GetType().Name.Replace("EventArgs", string.Empty)} VendorId-0x{device.VendorId:X4} ProductId-0x{device.ProductId:X4} Port-{info.PortInfo}");
            if (port is not null && DeviceManagement.DeviceArrivedTasks.TryRemove(port, out var taskCompletionSource))
                taskCompletionSource.SetResult(device);
        }
        else if (e is DeviceLeftEventArgs leftEventArgs)
        {
            if (!DeviceManagement.UsbDeviceInfos.TryRemove(device, out var info))
                throw new InvalidOperationException($"Couldn't get {nameof(info)} out of dictionary.");
            var port = DeviceManagement.DeviceLeftTasks.Keys.FirstOrDefault(portId => portId.Usb2Id == info.PortInfo || portId.Usb3Id == info.PortInfo);
            Console.WriteLine($"{DateTime.Now} {e.GetType().Name.Replace("EventArgs", string.Empty)} VendorId-0x{device.VendorId:X4} ProductId-0x{device.ProductId:X4} Port-{info.PortInfo}");
            if (port is not null && DeviceManagement.DeviceLeftTasks.TryRemove(port, out var taskCompletionSource))
                taskCompletionSource.SetResult(device);
        }
    }

    internal static class DeviceManagement
    {
        public static readonly ConcurrentDictionary<UsbDevice, CachedDeviceInfo> UsbDeviceInfos = new();
        public static readonly ConcurrentDictionary<PhysicalPortId, TaskCompletionSource<UsbDevice>> DeviceArrivedTasks = new();
        public static readonly ConcurrentDictionary<PhysicalPortId, TaskCompletionSource<UsbDevice>> DeviceLeftTasks = new();
    }

    public class CachedDeviceInfo
    {
        public CachedDeviceInfo(UsbDeviceInfo descriptor, LocationId portInfo)
        {
            Descriptor = descriptor;
            PortInfo = portInfo;
        }

        public UsbDeviceInfo Descriptor { get; }
        public LocationId PortInfo { get; }
    }
}