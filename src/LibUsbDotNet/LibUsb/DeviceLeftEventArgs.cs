namespace LibUsbDotNet.LibUsb;

public class DeviceLeftEventArgs : DeviceEventArgs
{
    public DeviceLeftEventArgs(UsbDevice device) : base(device)
    {
    }
}