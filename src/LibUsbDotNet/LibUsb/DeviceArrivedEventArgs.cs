namespace LibUsbDotNet.LibUsb;

public class DeviceArrivedEventArgs : DeviceEventArgs
{
    public DeviceArrivedEventArgs(UsbDevice device) : base(device)
    {
    }
}