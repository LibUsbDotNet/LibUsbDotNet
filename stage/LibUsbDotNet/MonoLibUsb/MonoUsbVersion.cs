using System.Runtime.InteropServices;

namespace MonoLibUsb.Descriptors
{
    /// <summary>
    /// Structure providing the version of the libusb runtime
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = MonoUsbApi.LIBUSB_PACK, CharSet = CharSet.Ansi)]
    public struct MonoUsbVersion
    {
        /// <summary>
        /// Library major version.
        /// </summary>
        public readonly ushort Major;

        /// <summary>
        /// Library minor version.
        /// </summary>
        public readonly ushort Minor;

        /// <summary>
        /// Library micro version.
        /// </summary>
        public readonly ushort Micro;

        /// <summary>
        /// Library nano version.
        /// </summary>
        public readonly ushort Nano;
        /*
        /// <summary>
        /// Library release candidate suffix string, e.g. "-rc4".
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string RC;

        /// <summary>
        /// For ABI compatibility only.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string Describe;
        */
    }
}