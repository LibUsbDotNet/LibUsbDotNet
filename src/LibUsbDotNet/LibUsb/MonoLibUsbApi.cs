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

        /// <summary>
        /// Determine the <see cref="MonoUsbConfigDescriptor.bConfigurationValue"/> of the currently active configuration. 
        /// </summary>
        /// <remarks>
        /// <para>You could formulate your own control request to obtain this information, but this function has the advantage that it may be able to retrieve the information from operating system caches (no I/O involved).</para>
        /// <para>If the OS does not cache this information, then this function will block while a control transfer is submitted to retrieve the information.</para>
        /// <para>This function will return a value of 0 in the <paramref name="configuration"/> parameter if the device is in unconfigured state.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="configuration">Output location for the <see cref="MonoUsbConfigDescriptor.bConfigurationValue"/> of the active configuration. (only valid for return code 0)</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_configuration")]
        public static extern int GetConfiguration([In] MonoUsbDeviceHandle deviceHandle, ref int configuration);

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
        public static extern int GetDeviceDescriptor([In] MonoUsbProfileHandle deviceProfileHandle, [Out] MonoUsbDeviceDescriptor deviceDescriptor);

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

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_get_device")]
        private static extern IntPtr GetDeviceInternal([In] MonoUsbDeviceHandle devicehandle);

        /// <summary>
        /// Set the active configuration for a device. 
        /// </summary>
        /// <remarks>
        /// <para>The operating system may or may not have already set an active configuration on the device. It is up to your application to ensure the correct configuration is selected before you attempt to claim interfaces and perform other operations.</para>
        /// <para>If you call this function on a device already configured with the selected configuration, then this function will act as a lightweight device reset: it will issue a SET_CONFIGURATION request using the current configuration, causing most USB-related device state to be reset (altsetting reset to zero, endpoint halts cleared, toggles reset).</para>
        /// <para>You cannot change/reset configuration if your application has claimed interfaces - you should free them with <see cref="ReleaseInterface"/> first. You cannot change/reset configuration if other applications or drivers have claimed interfaces.</para>
        /// <para>A configuration value of -1 will put the device in unconfigured state. The USB specifications state that a configuration value of 0 does this, however buggy devices exist which actually have a configuration 0.</para>
        /// <para>You should always use this function rather than formulating your own SET_CONFIGURATION control request. This is because the underlying operating system needs to know when such changes happen.</para>
        /// <para>This is a blocking function.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="configuration">The <see cref="MonoUsbConfigDescriptor.bConfigurationValue"/> of the configuration you wish to activate, or -1 if you wish to put the device in unconfigured state </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the requested configuration does not exist</item>
        /// <item><see cref="MonoUsbError.ErrorBusy"/> if interfaces are currently claimed</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_set_configuration")]
        public static extern int SetConfiguration([In] MonoUsbDeviceHandle deviceHandle, int configuration);

        /// <summary>
        /// Claim an interface on a given device handle. 
        /// </summary>
        /// <remarks>
        /// <para>You must claim the interface you wish to use before you can perform I/O on any of its endpoints.</para>
        /// <para>It is legal to attempt to claim an already-claimed interface, in which case libusb just returns 0 without doing anything.</para>
        /// <para>If <see cref="SetAutoDetachKernelDriver">auto_detach_kernel_driver</see> is set to 1 for <tt>dev</tt>, the kernel driver will be detached if necessary, on failure the detach error is returned.</para>
        /// <para>Claiming of interfaces is a purely logical operation; it does not cause any requests to be sent over the bus. Interface claiming is used to instruct the underlying operating system that your application wishes to take ownership of the interface.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">the <see cref="MonoUsbAltInterfaceDescriptor.bInterfaceNumber"/> of the interface you wish to claim.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the requested interface does not exist</item>
        /// <item><see cref="MonoUsbError.ErrorBusy"/> if another program or driver has claimed the interface </item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_claim_interface")]
        public static extern int ClaimInterface([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Release an interface previously claimed with <see cref="ClaimInterface"/>.
        /// </summary>
        /// <remarks>
        /// <para>You should release all claimed interfaces before closing a device handle.</para>
        /// <para>This is a blocking function. A SET_INTERFACE control request will be sent to the device, resetting interface state to the first alternate setting.</para>
        /// <para>If <see cref="SetAutoDetachKernelDriver">auto_detach_kernel_driver</see> is set to 1 for <tt>dev</tt>, the kernel driver will be re-attached after releasing the interface.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">the <see cref="MonoUsbAltInterfaceDescriptor.bInterfaceNumber"/> of the interface you wish to claim.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the interface was not claimed</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_release_interface")]
        public static extern int ReleaseInterface([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Activate an alternate setting for an interface.
        /// </summary>
        /// <remarks>
        /// <para>The interface must have been previously claimed with <see cref="ClaimInterface"/>.</para>
        /// <para>You should always use this function rather than formulating your own SET_INTERFACE control request. This is because the underlying operating system needs to know when such changes happen.</para>
        /// <para>This is a blocking function.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The <see cref="MonoUsbAltInterfaceDescriptor.bInterfaceNumber"/> of the previously-claimed interface.</param>
        /// <param name="alternateSetting">The <see cref="MonoUsbAltInterfaceDescriptor.bAlternateSetting"/> of the alternate setting to activate.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the interface was not claimed, or the requested alternate setting does not exist</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_set_interface_alt_setting")]
        public static extern int SetInterfaceAltSetting([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber, int alternateSetting);

        /// <summary>
        /// Clear the halt/stall condition for an endpoint.
        /// </summary>
        /// <remarks>
        /// <para>Endpoints with halt status are unable to receive or transmit data until the halt condition is stalled.</para>
        /// <para>You should cancel all pending transfers before attempting to clear the halt condition.</para>
        /// <para>This is a blocking function.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="endpoint">The endpoint to clear halt status.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if the endpoint does not exist</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_clear_halt")]
        public static extern int ClearHalt([In] MonoUsbDeviceHandle deviceHandle, byte endpoint);

        /// <summary>
        /// Perform a USB port reset to reinitialize a device. 
        /// </summary>
        /// <remarks>
        /// <para>The system will attempt to restore the previous configuration and alternate settings after the reset has completed.</para>
        /// <para>If the reset fails, the descriptors change, or the previous state cannot be restored, the device will appear to be disconnected and reconnected. This means that the device handle is no longer valid (you should close it) and rediscover the device. A return code of <see cref="MonoUsbError.ErrorNotFound"/> indicates when this is the case.</para>
        /// <para>This is a blocking function which usually incurs a noticeable delay.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if re-enumeration is required, or if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failure</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_reset_device")]
        public static extern int ResetDevice([In] MonoUsbDeviceHandle deviceHandle);

        /// <summary>
        /// Determine if a kernel driver is active on an interface. 
        /// </summary>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The interface to check.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 if no kernel driver is active.</item>
        /// <item>1 if a kernel driver is active.</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected.</item>
        /// <item>Another <see cref="MonoUsbError"/> code on other failure.</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_kernel_driver_active")]
        public static extern int KernelDriverActive([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Detach a kernel driver from an interface.
        /// </summary>
        /// <remarks>
        /// <para>If successful, you will then be able to claim the interface and perform I/O.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The interface to detach the driver from.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success.</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if no kernel driver was active.</item>
        /// <item><see cref="MonoUsbError.ErrorInvalidParam"/> if the interface does not exist.</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected </item>
        /// <item>Another <see cref="MonoUsbError"/> code on other failure.</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_detach_kernel_driver")]
        public static extern int DetachKernelDriver([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Re-attach an interface's kernel driver, which was previously detached using <see cref="DetachKernelDriver"/>.
        /// </summary>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The interface to attach the driver from.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success.</item>
        /// <item><see cref="MonoUsbError.ErrorNotFound"/> if no kernel driver was active.</item>
        /// <item><see cref="MonoUsbError.ErrorInvalidParam"/> if the interface does not exist.</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected.</item>
        /// <item><see cref="MonoUsbError.ErrorBusy"/> if the driver cannot be attached because the interface is claimed by a program or driver.</item>
        /// <item>Another <see cref="MonoUsbError"/> code on other failure.</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_attach_kernel_driver")]
        public static extern int AttachKernelDriver([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Enable/disable libusb's automatic kernel driver detachment. When this is 
        /// enabled libusb will automatically detach the kernel driver on an interface
        /// when claiming the interface, and attach it when releasing the interface.
        /// <remaks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// <para>Automatic kernel driver detachment is disabled on newly opened device 
        /// handles by default.</para>
        /// <para>On platforms which do not have <see cref="MonoUsbCapability.LIBUSB_CAP_SUPPORTS_DETACH_KERNEL_DRIVER"/>
        /// this function will return <see cref="MonoUsbError.ErrorNotSupported"/>, and libusb will
        /// continue as if this function was never called.</para>
        /// </remaks>
        /// </summary>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="enable">Whether to enable or disable auto kernel driver detachment.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><see cref="MonoUsbError.Success"/> on success.</item>
        /// <item><see cref="MonoUsbError.ErrorNotSupported"/> on platforms where the functionality is not available.</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_set_auto_detach_kernel_driver")]
        public static extern int SetAutoDetachKernelDriver([In]MonoUsbDeviceHandle deviceHandle, int enable);
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

        /// <summary>
        /// Perform a USB control transfer.
        /// </summary>
        /// <remarks>
        /// <para>The direction of the transfer is inferred from the bmRequestType field of the setup packet.</para>
        /// <para>The wValue, wIndex and wLength fields values should be given in host-endian byte order.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="requestType">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="pData">A suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType).</param>
        /// <param name="dataLength">The length field for the setup packet. The data buffer should be at least this size.</param>
        /// <param name="timeout">timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>on success, the number of bytes actually transferred</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the control request was not supported by the device.</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_control_transfer")]
        public static extern int ControlTransfer([In] MonoUsbDeviceHandle deviceHandle, byte requestType, byte request, short value, short index, IntPtr pData, short dataLength, int timeout);

        /// <summary>
        /// Perform a USB bulk transfer. 
        /// </summary>
        /// <remarks>
        /// <para>The direction of the transfer is inferred from the direction bits of the endpoint address.</para>
        /// <para>
        /// For bulk reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para>
        /// <para>
        /// You should also check the transferred parameter for bulk writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="pData">
        /// A suitably-sized data buffer for either input or output (depending on endpoint).</param>
        /// <param name="length">For bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actualLength">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success (and populates <paramref name="actualLength"/>)</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the endpoint halted</item>
        /// <item><see cref="MonoUsbError.ErrorOverflow"/>if the device offered more data, see <a href="http://libusb.sourceforge.net/api-1.0/packetoverflow.html">Packets and overflows</a></item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_bulk_transfer")]
        public static extern int BulkTransfer([In] MonoUsbDeviceHandle deviceHandle, byte endpoint, IntPtr pData, int length, out int actualLength, int timeout);

        /// <summary>
        /// Perform a USB interrupt transfer. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction of the transfer is inferred from the direction bits of the endpoint address.
        /// </para><para>
        /// For interrupt reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para><para>
        /// You should also check the transferred parameter for interrupt writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="pData">A suitably-sized data buffer for either input or output (depending on endpoint).</param>
        /// <param name="length">For interrupt writes, the number of bytes from data to be sent. for interrupt reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actualLength">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success (and populates <paramref name="actualLength"/>)</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the endpoint halted</item>
        /// <item><see cref="MonoUsbError.ErrorOverflow"/>if the device offered more data, see <a href="http://libusb.sourceforge.net/api-1.0/packetoverflow.html">Packets and overflows</a></item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_interrupt_transfer")]
        public static extern int InterruptTransfer([In] MonoUsbDeviceHandle deviceHandle, byte endpoint, IntPtr pData, int length, out int actualLength, int timeout);

#endregion

#region API LIBRARY FUNCTIONS - Polling and timing

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_handle_events")]
        private static extern int HandleEvents(IntPtr pSessionHandle);

#endregion
    }
}