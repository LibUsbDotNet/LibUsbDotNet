using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace LibUsbDotNet.Descriptors
{
    [StructLayout(LayoutKind.Sequential, Pack = packStart)]
    public class UsbDeviceDescriptor : UsbDeviceDescriptorBase
    {
        public const int packStart = 1;
    }
}
