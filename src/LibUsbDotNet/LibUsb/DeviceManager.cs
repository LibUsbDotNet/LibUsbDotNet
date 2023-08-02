using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.LibUsb;

/// <summary>
/// Class used for asynchronously waiting for <see cref="UsbDevice"/> arrivals.
/// </summary>
public class DeviceManager : IDisposable
{
    private readonly bool _disposeContext;
    private readonly UsbContext _context;
    private readonly ConcurrentDictionary<UsbDeviceFinder, TaskCompletionSource<UsbDevice>> _deviceArrivedTasks = new();
    private readonly ConcurrentDictionary<UsbDeviceFinder, TaskCompletionSource<CachedDeviceInfo>> _deviceLeftTasks = new();

    private bool _hasStarted;

    /// <summary>
    /// Class used for asynchronously waiting for <see cref="UsbDevice"/> arrivals.
    /// </summary>
    /// <param name="context"><see cref="UsbContext"/> to use.</param>
    /// <param name="disposeContext">Dispose the <see cref="UsbContext"/> on <see cref="DeviceManager"/> disposal.</param>
    public DeviceManager(UsbContext context, bool disposeContext = false)
    {
        _context = context;
        _disposeContext = disposeContext;
    }

    /// <summary>
    /// Registers the needed event for <see cref="DeviceManager"/> methods to work.
    /// </summary>
    public void Start()
    {
        _context.RegisterHotPlug();
        _context.DeviceEvent += OnDeviceEvent;
        _hasStarted = true;
    }

    private void EnsureStarted()
    {
        if (!_hasStarted)
            throw new InvalidOperationException($"{nameof(Start)} method must be called before waiting for devices.");
    }

#nullable enable
    /// <summary>
    /// Waits for a <see cref="UsbDevice"/> that matches the <see cref="UsbDeviceFinder"/>.
    /// Will return immediately if device is already connected.
    /// </summary>
    /// <param name="finder"><see cref="UsbDeviceFinder"/> to match.</param>
    /// <param name="maxWaitTime">Maximum time to wait for the device's arrival.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="stableConnectionInterval">Ensure the new device stays connected for at least this amount of time.</param>
    /// <returns><see cref="UsbDevice"/> that arrived, null on timeout.</returns>
    public async Task<UsbDevice?> WaitForDeviceArrival(UsbDeviceFinder finder, TimeSpan maxWaitTime, CancellationToken cancellationToken, TimeSpan stableConnectionInterval = default)
    {
        EnsureStarted();
        
        UsbDevice? arrivedDevice = _context.DeviceInfoDictionary.Keys.FirstOrDefault(finder.Check);

        if (arrivedDevice is not null)
            return arrivedDevice;

        return await WaitForNewDeviceArrival(finder, maxWaitTime, cancellationToken, stableConnectionInterval).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for a new arrival of a <see cref="UsbDevice"/> that matches the <see cref="UsbDeviceFinder"/>.
    /// Will not return an already existing device.
    /// </summary>
    /// <remarks>Useful if a device is expected to disconnect and then reconnect (i.e. on a reboot).</remarks>
    /// <param name="finder"><see cref="UsbDeviceFinder"/> to match.</param>
    /// <param name="maxWaitTime">Maximum time to wait for the device's arrival.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="stableConnectionInterval">Ensure the new device stays connected for at least this amount of time.</param>
    /// <returns><see cref="UsbDevice"/> that arrived, null on timeout.</returns>
    public async Task<UsbDevice?> WaitForNewDeviceArrival(UsbDeviceFinder finder, TimeSpan maxWaitTime, CancellationToken cancellationToken, TimeSpan stableConnectionInterval = default)
    {
        EnsureStarted();
        
        bool stableConnection = false;
        var timer = Stopwatch.StartNew();
        UsbDevice? arrivedDevice = null;

#if NETSTANDARD2_0
        using var deviceArrivedCtr = cancellationToken.Register(() =>
        {
            _deviceArrivedTasks.TryRemove(finder, out var source);
            source?.TrySetCanceled();
        });
        
        using var deviceLeftCtr = cancellationToken.Register(() =>
        {
            _deviceLeftTasks.TryRemove(finder, out var source);
            source?.TrySetCanceled();
        });
#else
        await using var deviceArrivedCtr = cancellationToken.Register(() =>
        {
            _deviceArrivedTasks.TryRemove(finder, out var source);
            source?.TrySetCanceled();
        });
        
        await using var deviceLeftCtr = cancellationToken.Register(() =>
        {
            _deviceLeftTasks.TryRemove(finder, out var source);
            source?.TrySetCanceled();
        });
#endif

        while (!stableConnection && timer.Elapsed < maxWaitTime)
        {
            var deviceArrivedTaskSource = new TaskCompletionSource<UsbDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            
            _deviceArrivedTasks.TryAdd(finder, deviceArrivedTaskSource);

            arrivedDevice = await TaskWithTimeoutAndFallback(deviceArrivedTaskSource.Task, maxWaitTime - timer.Elapsed);
            
            if (stableConnectionInterval == default)
                return arrivedDevice;
            
            var deviceLeftTaskSource = new TaskCompletionSource<CachedDeviceInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

            _deviceLeftTasks.TryAdd(finder, deviceLeftTaskSource);

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
                var matchingFinder = _deviceArrivedTasks.Keys.FirstOrDefault(finder => finder.Check(device));
                if (matchingFinder is not null && _deviceArrivedTasks.TryRemove(matchingFinder, out var taskCompletionSource))
                    taskCompletionSource.TrySetResult(device);
                break;
            }
            case DeviceLeftEventArgs leftEventArgs:
            {
                var info = leftEventArgs.DeviceInfo;
                var matchingFinder = _deviceLeftTasks.Keys.FirstOrDefault(finder => finder.Check(info));
                if (matchingFinder is not null && _deviceLeftTasks.TryRemove(matchingFinder, out var taskCompletionSource))
                    taskCompletionSource.TrySetResult(info);
                break;
            }
        }
    }

    /// <summary>
    /// Unregisters event needed for wait methods to work.
    /// </summary>
    public void Stop()
    {
        _context.DeviceEvent -= OnDeviceEvent;
        _hasStarted = false;
    }

    /// <summary>
    /// Stops the <see cref="DeviceManager"/> and disposes the <see cref="UsbContext"/> if needed.
    /// </summary>
    public void Dispose()
    {
        Stop();
        if (_disposeContext)
            _context.Dispose();
    }
}