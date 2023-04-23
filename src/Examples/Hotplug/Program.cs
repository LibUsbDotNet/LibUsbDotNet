using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    private static readonly Stopwatch TransferTimer = new Stopwatch();
    
    public enum TransferType
    {
        Read,
        Write
    }
        
    public class TransferHandle
    {
        public TransferType Type { get; }
        public Error Error { get; }
        public int TransferLength { get; }
        public double CompletionTime { get; }
        public int Id { get; }
        public string Data { get; }

        public TransferHandle(TransferType type, Error error, int transferLength, double completionTime, int id, string data)
        {
            Type = type;
            Error = error;
            TransferLength = transferLength;
            CompletionTime = completionTime;
            Id = id;
            Data = data;
        }
    }
    
    private static async Task<TransferHandle> ReadTransfer(int id, UsbEndpointReader reader, byte[] readBuffer)
    {
        var result = await reader.ReadAsync(readBuffer, 0, readBuffer.Length, 100);
        return new TransferHandle(TransferType.Read, result.error, result.transferLength, TransferTimer.Elapsed.TotalMilliseconds, id, Encoding.Default.GetString(readBuffer, 0, result.transferLength));
    }
        
    private static async Task<TransferHandle> WriteTransfer(int id, UsbEndpointWriter writer, byte[] bytesToSend)
    {
        var result = await writer.WriteAsync(bytesToSend, 0, bytesToSend.Length, 100);
        return new TransferHandle(TransferType.Write, result.error, result.transferLength, TransferTimer.Elapsed.TotalMilliseconds, id, Encoding.Default.GetString(bytesToSend, 0, result.transferLength));
    }
    
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
        var hotplugOptions = new HotplugOptions();
        context.RegisterHotPlug(hotplugOptions);

        using var device = await WaitForDevice(finder, TimeSpan.FromSeconds(2));

        Console.WriteLine("Got a stable connection.");
        
        device.Open();
        device.SetConfiguration(1);
        device.ClaimInterface(0);
        
        // open read endpoint 1.
        var reader = device.OpenEndpointReader(ReadEndpointID.Ep01);

        // open write endpoint 1.
        var writer = device.OpenEndpointWriter(WriteEndpointID.Ep01);
            
        byte[] readBuffer = new byte[1024];
        List<Task<TransferHandle>> transfers = new();
        TransferTimer.Start();
        for (int testCount = 0; testCount < 10; testCount++)
        {
            // Create and submit transfer
            var usbReadTransfer = ReadTransfer(testCount, reader, readBuffer);
            var usbWriteTransfer = WriteTransfer(testCount, writer, Encoding.Default.GetBytes(TransferTimer.Elapsed.TotalMilliseconds.ToString()));
            transfers.Add(usbReadTransfer);
            transfers.Add(usbWriteTransfer);
        }

        var completedTransfers = await Task.WhenAll(transfers);

        foreach (var completedTransfer in completedTransfers.OrderBy(transfer => transfer.CompletionTime))
        {
            Console.WriteLine($"{completedTransfer.Type} transfer #{completedTransfer.Id} completed @ {completedTransfer.CompletionTime} ms with Error-{completedTransfer.Error} Length-{completedTransfer.TransferLength} Data-{completedTransfer.Data}");
        }
            
        Console.WriteLine("Press any key to unregister the hotplug event...");
        
        Console.ReadKey();

        Console.WriteLine("\nUnregistered hotplug.");
        
        context.UnregisterHotPlug(hotplugOptions);

        Console.WriteLine("Press any key to exit...");
        
        Console.ReadKey();
    }

    private static async Task<UsbDevice> WaitForDevice(UsbDeviceFinder finder, TimeSpan delay)
    {
        UsbDevice arrivedDevice = null;
        bool stableConnection = false;
        while (!stableConnection)
        {
            var deviceArrivedTaskSource = new TaskCompletionSource<UsbDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!DeviceArrivedTasks.TryAdd(finder, deviceArrivedTaskSource))
                throw new InvalidOperationException("Could not add arrived task");
            Console.WriteLine("Waiting for device arrival.");

            arrivedDevice = await deviceArrivedTaskSource.Task;
            
            var deviceLeftTaskSource = new TaskCompletionSource<CachedDeviceInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!DeviceLeftTasks.TryAdd(finder, deviceLeftTaskSource))
                throw new InvalidOperationException("Could not add left task");
            Console.WriteLine("Waiting for device disconnect.");
            var completion = await Task.WhenAny(deviceLeftTaskSource.Task, Task.Delay(delay));
            if (completion is Task<CachedDeviceInfo> deviceLeft)
            {
                Console.WriteLine("Device left");
            }
            else
            {
                Console.WriteLine("Timeout waiting for device disconnect");
                DeviceLeftTasks.TryRemove(finder, out _);
                stableConnection = true;
            }
        }

        return arrivedDevice;
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