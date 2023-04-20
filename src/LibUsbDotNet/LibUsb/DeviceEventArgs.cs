using System;

namespace LibUsbDotNet.LibUsb;

public class DeviceEventArgs : EventArgs
{
    public DeviceEventArgs(UsbDevice device)
    {
        Device = device;
    }

    public UsbDevice Device { get; }
}