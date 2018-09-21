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
using System;
using System.Runtime.InteropServices;
using LibUsbDotNet;
using MonoLibUsb.Descriptors;
using MonoLibUsb.Profile;

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

        /// <summary>
        /// Gets the standard device descriptor.
        /// </summary>
        /// <remarks>
        /// <note type="tip">This is a non-blocking function which does not involve any requests being sent to the device.</note>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="desc"/></note>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="deviceDescriptor">The <see cref="MonoUsbDeviceDescriptor"/> clas that will hold the data.</param>
        /// <returns>0 on success or a <see cref="MonoUsbError"/> code on failure.</returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_device_descriptor")]
        public static extern int GetDeviceDescriptor([In] MonoUsbProfileHandle deviceProfileHandle, [Out] DeviceDescriptor deviceDescriptor);

        /// <summary>
        /// Get the USB configuration descriptor for the currently active configuration.
        /// </summary>
        /// <remarks>
        /// <note type="tip">This is a non-blocking function which does not involve any requests being sent to the device.</note>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="desc"/></note>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="configHandle">A config handle.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the device is in unconfigured state </item>
        /// <item>another <see cref="MonoUsbError"/> code on error</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_active_config_descriptor")]
        public static extern int GetActiveConfigDescriptor([In] MonoUsbProfileHandle deviceProfileHandle, [Out] out MonoUsbConfigHandle configHandle);

        /// <summary>
        /// Get a USB configuration descriptor based on its index. 
        /// </summary>
        /// <remarks>
        /// <note type="tip">This is a non-blocking function which does not involve any requests being sent to the device.</note>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="desc"/></note>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="configIndex">The index of the configuration you wish to retrieve.</param>
        /// <param name="configHandle">A config handle.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the device is in unconfigured state </item>
        /// <item>another <see cref="MonoUsbError"/> code on error</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_config_descriptor")]
        public static extern int GetConfigDescriptor([In] MonoUsbProfileHandle deviceProfileHandle, byte configIndex, [Out] out MonoUsbConfigHandle configHandle);

        /// <summary>
        /// Get a USB configuration descriptor with a specific bConfigurationValue.
        /// </summary>
        /// <remarks>
        /// <note type="tip">This is a non-blocking function which does not involve any requests being sent to the device.</note>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="desc"/></note>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="bConfigurationValue">The bConfigurationValue of the configuration you wish to retrieve.</param>
        /// <param name="configHandle">A config handle.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the device is in unconfigured state </item>
        /// <item>another <see cref="MonoUsbError"/> code on error</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_config_descriptor_by_value")]
        public static extern int GetConfigDescriptorByValue([In] MonoUsbProfileHandle deviceProfileHandle, byte bConfigurationValue, [Out] out MonoUsbConfigHandle configHandle);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_free_config_descriptor")]
        internal static extern void FreeConfigDescriptor(IntPtr pConfigDescriptor);

#endregion

#region API LIBRARY FUNCTIONS - Device handling and enumeration (part 2)

        /// <summary>
        /// Get the number of the bus that a device is connected to. 
        /// </summary>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <returns>The bus number.</returns>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_bus_number")]
        public static extern byte GetBusNumber([In] MonoUsbProfileHandle deviceProfileHandle);

        /// <summary>
        /// Get the address of the device on the bus it is connected to. 
        /// </summary>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <returns>The device address.</returns>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_device_address")]
        public static extern byte GetDeviceAddress([In] MonoUsbProfileHandle deviceProfileHandle);

        /// <summary>
        /// Convenience function to retrieve the wMaxPacketSize value for a particular endpoint in the active device configuration. 
        /// </summary>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="endpoint">Endpoint address to retrieve the max packet size for.</param>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// <para>This function was originally intended to be of assistance when setting up isochronous transfers, but a design mistake resulted in this function instead. It simply returns the <see cref="MonoUsbEndpointDescriptor.wMaxPacketSize"/> value without considering its contents. If you're dealing with isochronous transfers, you probably want <see cref="GetMaxIsoPacketSize"/> instead.</para>
        /// </remarks>
        /// <returns>The <see cref="MonoUsbEndpointDescriptor.wMaxPacketSize"/></returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_max_packet_size")]
        public static extern int GetMaxPacketSize([In] MonoUsbProfileHandle deviceProfileHandle, byte endpoint);

        /// <summary>
        /// Calculate the maximum packet size which a specific endpoint is capable is sending or receiving in the duration of 1 microframe.
        /// </summary>
        /// <remarks>
        /// <para>Only the active configuration is examined. The calculation is based on the wMaxPacketSize field in the endpoint descriptor as described in section 9.6.6 in the USB 2.0 specifications.</para>
        /// <para>If acting on an isochronous or interrupt endpoint, this function will multiply the value found in bits 0:10 by the number of transactions per microframe (determined by bits 11:12). Otherwise, this function just returns the numeric value found in bits 0:10.</para>
        /// <para>This function is useful for setting up isochronous transfers, for example you might pass the return value from this function to <see  cref="MonoUsbTransfer.SetIsoPacketLengths">libusb_set_iso_packet_lengths</see> in order to set the length field of every isochronous packet in a transfer.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="endpoint">Endpoint address to retrieve the max packet size for.</param>
        /// <returns>The maximum packet size which can be sent/received on this endpoint.</returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_max_iso_packet_size")]
        public static extern int GetMaxIsoPacketSize([In] MonoUsbProfileHandle deviceProfileHandle, byte endpoint);

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_open")]
        internal static extern int Open([In] MonoUsbProfileHandle deviceProfileHandle, ref IntPtr deviceHandle);

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