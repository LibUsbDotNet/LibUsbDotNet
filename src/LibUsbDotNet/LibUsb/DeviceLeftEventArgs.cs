using LibUsbDotNet.Info;

namespace LibUsbDotNet.LibUsb;

public class DeviceLeftEventArgs : DeviceEventArgs
{
    public DeviceLeftEventArgs(CachedDeviceInfo info)
    {
        DeviceInfo = info;
    }
    
    public CachedDeviceInfo DeviceInfo { get; }
}