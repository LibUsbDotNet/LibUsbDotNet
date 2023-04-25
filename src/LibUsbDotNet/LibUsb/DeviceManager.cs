using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.LibUsb;

public class DeviceManager : IDisposable
{
    private readonly UsbContext _context;
    private readonly ConcurrentDictionary<UsbDeviceFinder, TaskCompletionSource<UsbDevice>> _deviceArrivedTasks = new();
    private readonly ConcurrentDictionary<UsbDeviceFinder, TaskCompletionSource<CachedDeviceInfo>> _deviceLeftTasks = new();

    public DeviceManager(UsbContext context)
    {
        _context = context;
    }

    public void Start()
    {
        _context.DeviceEvent += OnDeviceEvent;
        _context.RegisterHotPlug();
    }
    
#nullable enable
    public async Task<UsbDevice?> WaitForDevice(UsbDeviceFinder finder, TimeSpan maxWaitTime, CancellationToken cancellationToken, TimeSpan stableConnectionInterval = default)
    {
        UsbDevice? arrivedDevice = _context.DeviceInfoDictionary.Keys.FirstOrDefault(finder.Check);

        if (arrivedDevice is not null)
            return arrivedDevice;
        
        bool stableConnection = false;
        var timer = Stopwatch.StartNew();
        while (!stableConnection && timer.Elapsed < maxWaitTime)
        {
            var deviceArrivedTaskSource = new TaskCompletionSource<UsbDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var deviceArrivedCtr = cancellationToken.Register(() => deviceArrivedTaskSource.TrySetCanceled());
            if (!_deviceArrivedTasks.TryAdd(finder, deviceArrivedTaskSource))
                throw new InvalidOperationException("Could not add arrived task");

            arrivedDevice = await TaskWithTimeoutAndFallback(deviceArrivedTaskSource.Task, maxWaitTime - timer.Elapsed);
            
            if (stableConnectionInterval == default)
                return arrivedDevice;
            
            var deviceLeftTaskSource = new TaskCompletionSource<CachedDeviceInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var deviceLeftCtr = cancellationToken.Register(() => deviceLeftTaskSource.TrySetCanceled());
            if (!_deviceLeftTasks.TryAdd(finder, deviceLeftTaskSource))
                throw new InvalidOperationException("Could not add left task");

            var deviceLeftInfo = await TaskWithTimeoutAndFallback(deviceLeftTaskSource.Task, stableConnectionInterval);
            
            if (deviceLeftInfo is null)
            {
                _deviceLeftTasks.TryRemove(finder, out _);
                stableConnection = true;
            }
        }

        return arrivedDevice;
    }
    
    private static async Task<T?> DelayedResultTask<T>(TimeSpan delay, T? result = default)
    {
        await Task.Delay(delay);
        return result;
    }

    private static async Task<T?> TaskWithTimeoutAndFallback<T>(
            Task<T> task,
            TimeSpan timeout,
            T? fallback = default) =>
        await await Task.WhenAny(task, DelayedResultTask(timeout, fallback)!);

#nullable disable
    
    private void OnDeviceEvent(object sender, DeviceEventArgs e)
    {
        switch (e)
        {
            case DeviceArrivedEventArgs arrivedEventArgs:
            {
                var device = arrivedEventArgs.Device;

                var matchingFinder =
                    _deviceArrivedTasks.Keys.FirstOrDefault(finder => finder.Check(device));
                Console.WriteLine($"{DateTime.Now} {e.GetType().Name.Replace("EventArgs", string.Empty)} VendorId-0x{device.VendorId:X4} ProductId-0x{device.ProductId:X4} Port-{device.LocationId}");
                if (matchingFinder is not null && _deviceArrivedTasks.TryRemove(matchingFinder, out var taskCompletionSource))
                    taskCompletionSource.SetResult(device);
                break;
            }
            case DeviceLeftEventArgs leftEventArgs:
            {
                var info = leftEventArgs.DeviceInfo;
                var matchingFinder =
                    _deviceLeftTasks.Keys.FirstOrDefault(finder => finder.Check(info));
                Console.WriteLine($"{DateTime.Now} {e.GetType().Name.Replace("EventArgs", string.Empty)} VendorId-0x{info.Descriptor.VendorId:X4} ProductId-0x{info.Descriptor.ProductId:X4} Port-{info.PortInfo}");
                if (matchingFinder is not null && _deviceLeftTasks.TryRemove(matchingFinder, out var taskCompletionSource))
                    taskCompletionSource.SetResult(info);
                break;
            }
        }
    }

    public void Dispose()
    {
        _context.DeviceEvent -= OnDeviceEvent;
        _context.UnregisterHotPlug();
    }
}