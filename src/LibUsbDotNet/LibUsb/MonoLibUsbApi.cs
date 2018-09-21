// Copyright © 2006-2010 Travis Robinson <libusbdotnet@gmail.com>
// Copyright © 2017 Andras Fuchs <andras.fuchs@gmail.com>
// 
// Website: http://sourceforge.net/projects/libusbdotnet
// GitHub repo: https://github.com/CoreCompat/LibUsbDotNet
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
// 
using LibUsbDotNet;
using System;
using System.Runtime.InteropServices;

namespace MonoLibUsb
{
    /// <summary>
    /// Libusb-1.0 low-level API library.
    /// </summary>
    public static partial class MonoUsbApi
    {
        #region API LIBRARY FUNCTIONS - Initialization & Deinitialization

        /// <summary>
        /// Returns a struct  with the version (major, minor, micro, nano and rc) of the running library.
        /// </summary>
        /// <returns></returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_version")]
        internal static extern IntPtr GetVersion();

        /// <summary>
        /// Check at runtime if the loaded library has a given capability. This call should be performed after \ref libusb_init(), to ensure the backend has updated its capability set.
        /// </summary>
        /// <param name="capability"></param>
        /// <returns></returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_has_capability")]
        internal static extern int HasCapability(Capability capability);

#endregion

#region API LIBRARY FUNCTIONS - Error Handling

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_error_name")]
        internal static extern string ErrorName(int errcode);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_setlocale")]
        internal static extern int SetLocale(string locale);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_strerror")]
        private static extern IntPtr StrError(int errcode);

#endregion

#region API LIBRARY FUNCTIONS - Device handling and enumeration (part 1)

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_free_device_list")]
        internal static extern void FreeDeviceList(IntPtr pHandleList, int unrefDevices);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_ref_device")]
        internal static extern IntPtr RefDevice(IntPtr pDeviceProfileHandle);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_unref_device")]
        internal static extern IntPtr UnrefDevice(IntPtr pDeviceProfileHandle);

#endregion

#region API LIBRARY FUNCTIONS - USB descriptors

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_free_config_descriptor")]
        internal static extern void FreeConfigDescriptor(IntPtr pConfigDescriptor);

#endregion

#region API LIBRARY FUNCTIONS - Device handling and enumeration (part 2)

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_close")]
        internal static extern void Close(IntPtr deviceHandle);
#endregion

#region API LIBRARY FUNCTIONS - Asynchronous device I/O

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_alloc_transfer")]
        internal static extern IntPtr AllocTransfer(int isoPackets);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_submit_transfer")]
        internal static extern int SubmitTransfer(IntPtr pTransfer);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_cancel_transfer")]
        internal static extern int CancelTransfer(IntPtr pTransfer);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_free_transfer")]
        internal static extern void FreeTransfer(IntPtr pTransfer);

#endregion

#region API LIBRARY FUNCTIONS - Synchronous device I/O

#endregion

#region API LIBRARY FUNCTIONS - Polling and timing

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_handle_events")]
        private static extern int HandleEvents(IntPtr pSessionHandle);

#endregion
    }
}