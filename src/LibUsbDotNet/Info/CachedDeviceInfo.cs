using LibUsbDotNet.LibUsb;

namespace LibUsbDotNet.Info;

public class CachedDeviceInfo
{
    private readonly UsbDevice _device;
    
    public CachedDeviceInfo(UsbDevice device)
    {
        _device = device;
        Descriptor = _device.Descriptor;
        PortInfo = _device.LocationId;
    }

    public UsbDeviceInfo Descriptor { get; }
    public LocationId PortInfo { get; }

    protected bool Equals(CachedDeviceInfo other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode() => _device.GetHashCode();
}