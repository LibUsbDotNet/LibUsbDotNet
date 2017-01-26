using System.Runtime.InteropServices;

namespace MonoLibUsb.Descriptors
{
    [StructLayout(LayoutKind.Sequential, Pack = MonoUsbApi.LIBUSB_PACK)]
    public class MonoUsbVersion
    {
        internal MonoUsbVersion()
        {
        }

        public readonly int Major;

        public readonly int Minor;

        public readonly int Micro;

        public readonly int Nano;

        public readonly int RC;
    }
}