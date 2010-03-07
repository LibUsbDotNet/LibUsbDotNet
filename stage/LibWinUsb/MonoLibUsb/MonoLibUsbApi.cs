// Copyright © 2006-2009 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Main;
using MonoLibUsb.Descriptors;
using MonoLibUsb.Profile;
using MonoLibUsb.Transfer;

namespace MonoLibUsb
{
    /// <summary>
    /// Libusb-1.0 low-level API library.
    /// </summary>
    public static class MonoLibUsbApi
    {
        internal const CallingConvention CC = CallingConvention.StdCall;
        internal const string LIBUSB_DLL = "libusb-1.0.dll";
        internal const int LIBUSB_PACK = 0;

        #region API LIBRARY FUNCTIONS - Initialization & Deinitialization


        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_init")]
        internal static extern int Init(ref IntPtr pContext);

        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_exit")]
        internal static extern void Exit(IntPtr pContext);


        /// <summary>Set message verbosity.</summary>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="level">Debug level to set.</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Level 0: no messages ever printed by the library. (default)</item>
        /// <item>Level 1: error messages are printed to stderr.</item>
        /// <item>Level 2: warning and error messages are printed to stderr.</item>
        /// <item>Level 3: informational messages are printed to stdout, warning and error messages are printed to stderr</item>
        /// </list>
        /// <para>The default level is 0, which means no messages are ever printed. If you choose to increase the message verbosity level, ensure that your application does not close the stdout/stderr file descriptors.</para>
        /// <para>You are advised to set level 3. libusb is conservative with its message logging and most of the time, will only log messages that explain error conditions and other oddities. This will help you debug your software.</para>
        /// <para>If the LIBUSB_DEBUG environment variable was set when libusb was initialized, this function does nothing: the message verbosity is fixed to the value in the environment variable.</para>
        /// <para>If libusb was compiled without any message logging, this function does nothing: you'll never get any messages.</para>
        /// <para>If libusb was compiled with verbose debug message logging, this function does nothing: you'll always get messages from all levels.</para>
        /// </remarks>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_set_debug")]
        public static extern void SetDebug([In]MonoUsbSessionHandle sessionHandle, int level);

        #endregion

        #region API LIBRARY FUNCTIONS - Device handling and enumeration

        /// <summary>
        /// Returns a list of USB devices currently attached to the system. 
        /// </summary>
        /// <remarks>
        /// <para>This is your entry point into finding a USB device to operate.</para>
        /// <para>You are expected to unreference all the devices when you are done with them, and then free the list with <see  cref="FreeDeviceList">libusb_free_device_list</see>. Note that <see  cref="FreeDeviceList">libusb_free_device_list</see> can unref all the devices for you. Be careful not to unreference a device you are about to open until after you have opened it.</para>
        /// <para>This return value of this function indicates the number of devices in the resultant list. The list is actually one element larger, as it is NULL-terminated.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="monoUSBProfileListHandle">	output location for a list of devices. Must be later freed with <see  cref="FreeDeviceList">libusb_free_device_list</see>.</param>
        /// <returns>The number of devices in the outputted list, or <see cref="MonoUsbError.LIBUSB_ERROR_NO_MEM"/> on memory allocation failure. </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_device_list")]
        public static extern int GetDeviceList([In]MonoUsbSessionHandle sessionHandle, [Out] out MonoUsbProfileListHandle monoUSBProfileListHandle);

        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_free_device_list")]
        internal static extern void FreeDeviceList(IntPtr pHandleList, int unref_devices);

        /// <summary>
        /// Get the number of the bus that a device is connected to. 
        /// </summary>
        /// <returns>The bus number.</returns>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_bus_number")]
        public static extern byte GetBusNumber([In] MonoUsbProfileHandle deviceProfileHandle);


        /// <summary>
        /// Get the address of the device on the bus it is connected to. 
        /// </summary>
        /// <returns>The device address.</returns>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_device_address")]
        public static extern byte GetDeviceAddress([In] MonoUsbProfileHandle deviceProfileHandle);

        /// <summary>
        /// Convenience function to retrieve the wMaxPacketSize value for a particular endpoint in the active device configuration. 
        /// </summary>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="endpoint">Endpoint address to retrieve the max packet size for.</param>
        /// <remarks>This function was originally intended to be of assistance when setting up isochronous transfers, but a design mistake resulted in this function instead. It simply returns the <see cref="MonoUsbEndpointDescriptor.wMaxPacketSize"/> value without considering its contents. If you're dealing with isochronous transfers, you probably want <see cref="GetMaxIsoPacketSize"/> instead.</remarks>
        /// <returns>The <see cref="MonoUsbEndpointDescriptor.wMaxPacketSize"/></returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_max_packet_size")]
        public static extern int GetMaxPacketSize([In] MonoUsbProfileHandle deviceProfileHandle, byte endpoint);

        /// <summary>
        /// Calculate the maximum packet size which a specific endpoint is capable is sending or receiving in the duration of 1 microframe.
        /// </summary>
        /// <remarks>
        /// <para>Only the active configuration is examined. The calculation is based on the wMaxPacketSize field in the endpoint descriptor as described in section 9.6.6 in the USB 2.0 specifications.</para>
        /// <para>If acting on an isochronous or interrupt endpoint, this function will multiply the value found in bits 0:10 by the number of transactions per microframe (determined by bits 11:12). Otherwise, this function just returns the numeric value found in bits 0:10.</para>
        /// <para>This function is useful for setting up isochronous transfers, for example you might pass the return value from this function to <see  cref="MonoUsbTransfer.SetIsoPacketLengths">libusb_set_iso_packet_lengths</see> in order to set the length field of every isochronous packet in a transfer.</para>
        /// <para></para>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="endpoint">Endpoint address to retrieve the max packet size for.</param>
        /// <returns>The maximum packet size which can be sent/received on this endpoint.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_max_iso_packet_size")]
        public static extern int GetMaxIsoPacketSize([In] MonoUsbProfileHandle deviceProfileHandle, byte endpoint);

        /// <summary>
        /// Increment the reference count of a device. 
        /// </summary>
        /// <param name="pDeviceProfileHandle">A device profile handle.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_ref_device")]
        internal static extern IntPtr RefDevice(IntPtr pDeviceProfileHandle);

        /// <summary>
        /// Decrement the reference count of a device.
        /// </summary>
        /// <remarks>
        /// If the decrement operation causes the reference count to reach zero, the device shall be destroyed.
        /// </remarks>
        /// <param name="pDeviceProfileHandle">A device profile handle.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_unref_device")]
        internal static extern IntPtr UnrefDevice(IntPtr pDeviceProfileHandle);

        /// <summary> Open a device and obtain a device handle.</summary>
        /// <remarks>
        /// <para>A handle allows you to perform I/O on the device in question.</para>
        /// <para>Internally, this function adds a reference to the device and makes it available to you through <see cref="GetDevice"/>. This reference is removed during <see cref="Close"/>.</para>
        /// <para>This is a non-blocking function; no requests are sent over the bus.</para>
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="deviceHandle">Output location for the returned device handle pointer. Only populated when the return code is 0.</param>
        /// <returns>0 on success; a <see cref="MonoUsbError"/> on failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_open")]
        public static extern int Open([In] MonoUsbProfileHandle deviceProfileHandle,
                                             [Out] out MonoUsbDeviceHandle deviceHandle);


        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_open_device_with_vid_pid")]
        private static extern IntPtr OpenDeviceWithVidPidInternal([In]MonoUsbSessionHandle sessionHandle, short vendor_id, short product_id);

        /// <summary>
        /// Convenience function for finding a device with a particular idVendor/idProduct combination. 
        /// </summary>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="vendor_id">The idVendor value to search for.</param>
        /// <param name="product_id">The idProduct value to search for.</param>
        /// <returns>Null if the device was not opened or not found, otherwise an opened device handle.</returns>
        public static MonoUsbDeviceHandle OpenDeviceWithVidPid([In]MonoUsbSessionHandle sessionHandle, short vendor_id, short product_id)
        {
            IntPtr pHandle = OpenDeviceWithVidPidInternal(sessionHandle, vendor_id, product_id);
            if (pHandle == IntPtr.Zero) return null;
            return new MonoUsbDeviceHandle(pHandle);
        }

        /// <summary>
        /// Close a device handle.
        /// </summary>
        /// <remarks>
        /// <para>Should be called on all open handles before your application exits.</para>
        /// <para>Internally, this function destroys the reference that was added by <see cref="Open"/> on the given device.</para>
        /// <para>This is a non-blocking function; no requests are sent over the bus.</para>
        /// </remarks>
        /// <param name="deviceHandle">The handle to close.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_close")]
        internal static extern void Close(IntPtr deviceHandle);


        /// <summary>
        /// Get the underlying device for a handle. 
        /// </summary>
        /// <remarks>
        /// <para>This function does not modify the reference count of the returned device, so do not feel compelled to unreference it when you are done.</para>
        /// </remarks>
        /// <param name="dev_handle">A device handle.</param>
        /// <returns>The underlying profile handle.</returns>
        public static MonoUsbProfileHandle GetDevice(MonoUsbDeviceHandle dev_handle) { return new MonoUsbProfileHandle(libusb_get_device_internal(dev_handle)); }

        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_device")]
        private static extern IntPtr libusb_get_device_internal([In] MonoUsbDeviceHandle dev_handle);

        /// <summary>
        /// Determine the <see cref="MonoUsbConfigDescriptor.bConfigurationValue"/> of the currently active configuration. 
        /// </summary>
        /// <remarks>
        /// <para>You could formulate your own control request to obtain this information, but this function has the advantage that it may be able to retrieve the information from operating system caches (no I/O involved).</para>
        /// <para>If the OS does not cache this information, then this function will block while a control transfer is submitted to retrieve the information.</para>
        /// <para>This function will return a value of 0 in the <paramref name="configuration"/> parameter if the device is in unconfigured state.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="configuration">Output location for the <see cref="MonoUsbConfigDescriptor.bConfigurationValue"/> of the active configuration. (only valid for return code 0)</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_configuration")]
        public static extern int GetConfiguration([In] MonoUsbDeviceHandle deviceHandle, ref int configuration);

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
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="configuration">The <see cref="MonoUsbConfigDescriptor.bConfigurationValue"/> of the configuration you wish to activate, or -1 if you wish to put the device in unconfigured state </param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_set_configuration")]
        public static extern int SetConfiguration([In] MonoUsbDeviceHandle deviceHandle, int configuration);

        /// <summary>
        /// Claim an interface on a given device handle. 
        /// </summary>
        /// <remarks>
        /// <para>You must claim the interface you wish to use before you can perform I/O on any of its endpoints.</para>
        /// <para>It is legal to attempt to claim an already-claimed interface, in which case libusb just returns 0 without doing anything.</para>
        /// <para>Claiming of interfaces is a purely logical operation; it does not cause any requests to be sent over the bus. Interface claiming is used to instruct the underlying operating system that your application wishes to take ownership of the interface.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interface_number">the <see cref="MonoUsbAltInterfaceDescriptor.bInterfaceNumber"/> of the interface you wish to claim.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_claim_interface")]
        public static extern int ClaimInterface([In] MonoUsbDeviceHandle deviceHandle, int interface_number);

        /// <summary>
        /// Release an interface previously claimed with <see cref="ClaimInterface"/>.
        /// </summary>
        /// <remarks>
        /// <para>You should release all claimed interfaces before closing a device handle.</para>
        /// <para>This is a blocking function. A SET_INTERFACE control request will be sent to the device, resetting interface state to the first alternate setting.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interface_number">the <see cref="MonoUsbAltInterfaceDescriptor.bInterfaceNumber"/> of the interface you wish to claim.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_release_interface")]
        public static extern int ReleaseInterface([In] MonoUsbDeviceHandle deviceHandle, int interface_number);

        /// <summary>
        /// Activate an alternate setting for an interface.
        /// </summary>
        /// <remarks>
        /// <para>The interface must have been previously claimed with <see cref="ClaimInterface"/>.</para>
        /// <para>You should always use this function rather than formulating your own SET_INTERFACE control request. This is because the underlying operating system needs to know when such changes happen.</para>
        /// <para>This is a blocking function.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interface_number">The <see cref="MonoUsbAltInterfaceDescriptor.bInterfaceNumber"/> of the previously-claimed interface.</param>
        /// <param name="alternate_setting">The <see cref="MonoUsbAltInterfaceDescriptor.bAlternateSetting"/> of the alternate setting to activate.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_set_interface_alt_setting")]
        public static extern int SetInterfaceAltSetting([In] MonoUsbDeviceHandle deviceHandle, int interface_number, int alternate_setting);

        /// <summary>
        /// Clear the halt/stall condition for an endpoint.
        /// </summary>
        /// <remarks>
        /// <para>Endpoints with halt status are unable to receive or transmit data until the halt condition is stalled.</para>
        /// <para>You should cancel all pending transfers before attempting to clear the halt condition.</para>
        /// <para>This is a blocking function.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="endpoint">The endpoint to clear halt status.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_clear_halt")]
        public static extern int ClearHalt([In] MonoUsbDeviceHandle deviceHandle, byte endpoint);

        /// <summary>
        /// Perform a USB port reset to reinitialize a device. 
        /// </summary>
        /// <remarks>
        /// <para>The system will attempt to restore the previous configuration and alternate settings after the reset has completed.</para>
        /// <para>If the reset fails, the descriptors change, or the previous state cannot be restored, the device will appear to be disconnected and reconnected. This means that the device handle is no longer valid (you should close it) and rediscover the device. A return code of <see cref="MonoUsbError.LIBUSB_ERROR_NOT_FOUND"/> indicates when this is the case.</para>
        /// <para>This is a blocking function which usually incurs a noticeable delay.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_reset_device")]
        public static extern int ResetDevice([In] MonoUsbDeviceHandle deviceHandle);

        /// <summary>
        /// Determine if a kernel driver is active on an interface. 
        /// </summary>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The interface to check.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 if no kernel driver is active.</item>
        /// <item>1 if a kernel driver is active.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_NO_DEVICE"/> if the device has been disconnected.</item>
        /// <item>Another <see cref="MonoUsbError"/> code on other failure.</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_kernel_driver_active")]
        public static extern int KernelDriverActive([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Detach a kernel driver from an interface.
        /// </summary>
        /// <remarks>
        /// <para>If successful, you will then be able to claim the interface and perform I/O.</para>
        /// </remarks>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The interface to detach the driver from.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_NOT_FOUND"/> if no kernel driver was active.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_INVALID_PARAM"/> if the interface does not exist.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_NO_DEVICE"/> if the device has been disconnected </item>
        /// <item>Another <see cref="MonoUsbError"/> code on other failure.</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_detach_kernel_driver")]
        public static extern int DetachKernelDriver([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        /// <summary>
        /// Re-attach an interface's kernel driver, which was previously detached using <see cref="DetachKernelDriver"/>.
        /// </summary>
        /// <param name="deviceHandle">A device handle.</param>
        /// <param name="interfaceNumber">The interface to attach the driver from.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_NOT_FOUND"/> if no kernel driver was active.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_INVALID_PARAM"/> if the interface does not exist.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_NO_DEVICE"/> if the device has been disconnected.</item>
        /// <item><see cref="MonoUsbError.LIBUSB_ERROR_BUSY"/> if the driver cannot be attached because the interface is claimed by a program or driver.</item>
        /// <item>Another <see cref="MonoUsbError"/> code on other failure.</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_attach_kernel_driver")]
        public static extern int AttachKernelDriver([In] MonoUsbDeviceHandle deviceHandle, int interfaceNumber);

        #endregion

        #region API LIBRARY FUNCTIONS - USB descriptors

        /// <summary>
        /// Gets the standard device descriptor.
        /// </summary>
        /// <remarks>
        /// This is a non-blocking function; the device descriptor is cached in memory.
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="deviceDescriptor">The <see cref="MonoUsbDeviceDescriptor"/> clas that will hold the data.</param>
        /// <returns>0 on success or a <see cref="MonoUsbError"/> code on failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_device_descriptor")]
        public static extern int GetDeviceDescriptor([In] MonoUsbProfileHandle deviceProfileHandle,
                                                              [Out] MonoUsbDeviceDescriptor deviceDescriptor);

        /// <summary>
        /// Get the USB configuration descriptor for the currently active configuration.
        /// </summary>
        /// <remarks>
        /// This is a non-blocking function; the device descriptor is cached in memory.
        /// </remarks>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="configHandle">A config handle.</param>
        /// <returns>0 on success or a <see cref="MonoUsbError"/> code on failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_active_config_descriptor")]
        internal static extern int GetActiveConfigDescriptor([In] MonoUsbProfileHandle deviceProfileHandle,
                                                                       [Out] out MonoUsbConfigHandle configHandle);


        /// <summary>
        /// Get a USB configuration descriptor based on its index. 
        /// </summary>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="config_index">The index of the configuration you wish to retrieve.</param>
        /// <param name="configHandle">A config handle.</param>
        /// <returns>0 on success or a <see cref="MonoUsbError"/> code on failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_config_descriptor")]
        public static extern int GetConfigDescriptor([In] MonoUsbProfileHandle deviceProfileHandle,
                                                              byte config_index,
                                                              [Out] out MonoUsbConfigHandle configHandle);

        /// <summary>
        /// Get a USB configuration descriptor with a specific bConfigurationValue.
        /// </summary>
        /// <param name="deviceProfileHandle">A device profile handle.</param>
        /// <param name="bConfigurationValue">The bConfigurationValue of the configuration you wish to retrieve.</param>
        /// <param name="configHandle">A config handle.</param>
        /// <returns>0 on success or a <see cref="MonoUsbError"/> code on failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_config_descriptor_by_value")]
        public static extern int GetConfigDescriptorByValue([In] MonoUsbProfileHandle deviceProfileHandle,
                                                                       byte bConfigurationValue,
                                                                       [Out] out MonoUsbConfigHandle configHandle);

        /// <summary>
        /// Free a configuration descriptor obtained from <see cref="GetActiveConfigDescriptor"/> or <see cref="GetConfigDescriptor"/>. 
        /// </summary>
        /// <remarks>
        /// It is safe to call this function with a NULL config parameter, in which case the function simply returns.
        /// </remarks>
        /// <param name="pConfigDescriptor">The configuration descriptor to free.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_free_config_descriptor")]
        internal static extern void FreeConfigDescriptor(IntPtr pConfigDescriptor);

        /// <summary>
        /// Retrieve a descriptor from the default control pipe. 
        /// </summary>
        /// <remarks>
        /// This is a convenience function which formulates the appropriate control message 
        /// to retrieve the descriptor.
        /// </remarks>
        /// <param name="deviceHandle">Retrieve a descriptor from the default control pipe.</param>
        /// <param name="desc_type">The descriptor type, <see cref="DescriptorType"/></param>
        /// <param name="desc_index">The index of the descriptor to retrieve.</param>
        /// <param name="pData">Output buffer for descriptor.</param>
        /// <param name="length">Size of data buffer.</param>
        /// <returns>Number of bytes returned in data, or <see cref="MonoUsbError"/> code on failure.</returns>
        public static int GetDescriptor(MonoUsbDeviceHandle deviceHandle, byte desc_type, byte desc_index, IntPtr pData, int length)
        {
            return ControlTransfer(deviceHandle,
                                           (byte) UsbEndpointDirection.EndpointIn,
                                           (byte) UsbStandardRequest.GetDescriptor,
                                           (short) ((desc_type << 8) | desc_index),
                                           0,
                                           pData,
                                           (short) length,
                                           1000);
        }

        /// <summary>
        /// Retrieve a descriptor from the default control pipe. 
        /// </summary>
        /// <remarks>
        /// This is a convenience function which formulates the appropriate control message 
        /// to retrieve the descriptor.
        /// </remarks>
        /// <param name="deviceHandle">Retrieve a descriptor from the default control pipe.</param>
        /// <param name="desc_type">The descriptor type, <see cref="DescriptorType"/></param>
        /// <param name="desc_index">The index of the descriptor to retrieve.</param>
        /// <param name="data">Output buffer for descriptor. This object is pinned using <see cref="PinnedHandle"/>.</param>
        /// <param name="length">Size of data buffer.</param>
        /// <returns>Number of bytes returned in data, or <see cref="MonoUsbError"/> code on failure.</returns>
        public static int GetDescriptor(MonoUsbDeviceHandle deviceHandle, byte desc_type, byte desc_index, object data, int length)
        {
            PinnedHandle p = new PinnedHandle(data);
            return GetDescriptor(deviceHandle, desc_type, desc_index, p.Handle, length);
        }

        #endregion

        #region API LIBRARY FUNCTIONS - Asynchronous device I/O

        /// <summary>
        /// Allocate a libusb transfer with a specified number of isochronous packet descriptors 
        /// </summary>
        /// <remarks>This function will fire off the USB transfer and then return immediately.
        /// <para>The returned transfer is pre-initialized for you. When the new transfer is no longer needed, it should be freed with <see cref="FreeTransfer"/>.</para>
        /// <para>Transfers intended for non-isochronous endpoints (e.g. control, bulk, interrupt) should specify an iso_packets count of zero.</para>
        /// <para>For transfers intended for isochronous endpoints, specify an appropriate number of packet descriptors to be allocated as part of the transfer. The returned transfer is not specially initialized for isochronous I/O; you are still required to set the <see cref="MonoUsbTransfer.NumIsoPackets"/> and <see cref="MonoUsbTransfer.Type"/> fields accordingly.</para>
        /// <para>It is safe to allocate a transfer with some isochronous packets and then use it on a non-isochronous endpoint. If you do this, ensure that at time of submission, <see cref="MonoUsbTransfer.NumIsoPackets"/> is 0 and that type is set appropriately.</para>
        /// </remarks>
        /// <param name="iso_packets">number of isochronous packet descriptors to allocate.</param>
        /// <returns>A newly allocated <see cref="MonoUsbTransfer"/>.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_alloc_transfer")]
        internal static extern IntPtr AllocTransfer(int iso_packets);

        /// <summary>
        /// Free a transfer structure. 
        /// </summary>
        /// <remarks>
        /// <para>This should be called for all transfers allocated with AllocTransfer().</para>
        /// <para>If the <see cref="MonoUsbTransferFlags.LIBUSB_TRANSFER_FREE_BUFFER"/> flag is set and the transfer buffer is non-NULL, this function will also free the transfer buffer using the standard system memory allocator (e.g. free()).</para>
        /// <para>It is legal to call this function with a NULL transfer. In this case, the function will simply return safely.</para>
        /// <para>It is not legal to free an active transfer (one which has been submitted and has not yet completed).</para>
        /// </remarks>
        /// <param name="pTransfer">The <see cref="MonoUsbTransfer"/> to free.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_free_transfer")]
        internal static extern void FreeTransfer(IntPtr pTransfer);

        /// <summary>
        /// Submit a transfer. 
        /// </summary>
        /// <remarks>This function will fire off the USB transfer and then return immediately.</remarks>
        /// <param name="pTransfer">The <see cref="MonoUsbTransfer"/> to submit.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_submit_transfer")]
        internal static extern int SubmitTransfer(MonoUsbTransfer pTransfer);

        /// <summary>
        /// Asynchronously cancel a previously submitted transfer. 
        /// </summary>
        /// <remarks>
        /// <para>This function returns immediately, but this does not indicate cancellation is complete. Your callback function will be invoked at some later time with a transfer status of <see cref="MonoUsbError.LIBUSB_ERROR_IO_CANCELLED"/>.</para>
        /// </remarks>
        /// <param name="pTransfer">The <see cref="MonoUsbTransfer"/> to cancel.</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_cancel_transfer")]
        internal static extern int CancelTransfer(MonoUsbTransfer pTransfer);

        #endregion

        #region API LIBRARY FUNCTIONS - Polling and timing

        /// <summary>
        /// Attempt to acquire the event handling lock.
        /// </summary>
        /// <remarks>
        /// <para>This lock is used to ensure that only one thread is monitoring libusb event sources at any one time.</para>
        /// <para>You only need to use this lock if you are developing an application which calls poll() or select() on libusb's file descriptors directly. If you stick to libusb's event handling loop functions (e.g. <see  cref="HandleEvents">libusb_handle_events</see>) then you do not need to be concerned with this locking.</para>
        /// <para>While holding this lock, you are trusted to actually be handling events. If you are no longer handling events, you must call <see  cref="UnlockEvents">libusb_unlock_events</see> as soon as possible.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 if the lock was obtained successfully.</item>
        /// <item>1 if the lock was not obtained. (i.e. another thread holds the lock)</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_try_lock_events")]
        public static extern int TryLockEvents([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Acquire the event handling lock, blocking until successful acquisition if it is contended. 
        /// </summary>
        /// <remarks>
        /// <para>This lock is used to ensure that only one thread is monitoring libusb event sources at any one time.</para>
        /// <para>You only need to use this lock if you are developing an application which calls poll() or select() on libusb's file descriptors directly. If you stick to libusb's event handling loop functions (e.g. <see  cref="HandleEvents">libusb_handle_events</see>) then you do not need to be concerned with this locking.</para>
        /// <para>While holding this lock, you are trusted to actually be handling events. If you are no longer handling events, you must call <see  cref="UnlockEvents">libusb_unlock_events</see> as soon as possible.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_lock_events")]
        public static extern void LockEvents([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Release the lock previously acquired with <see  cref="TryLockEvents">libusb_try_lock_events</see> or <see  cref="LockEvents">libusb_lock_events</see>. 
        /// </summary>
        /// <remarks>
        /// <para>Releasing this lock will wake up any threads blocked on <see  cref="WaitForEvent">libusb_wait_for_event</see>.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_unlock_events")]
        public static extern void UnlockEvents([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Determine if it is still OK for this thread to be doing event handling. 
        /// </summary>
        /// <remarks>
        /// <para>Sometimes, libusb needs to temporarily pause all event handlers, and this is the function you should use before polling file descriptors to see if this is the case.</para>
        /// <para>If this function instructs your thread to give up the events lock, you should just continue the usual logic that is documented in Multi-threaded applications and asynchronous I/O. On the next iteration, your thread will fail to obtain the events lock, and will hence become an event waiter.</para>
        /// <para>This function should be called while the events lock is held: you don't need to worry about the results of this function if your thread is not the current event handler.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>1 if event handling can start or continue.</item>
        /// <item>0 if this thread must give up the events lock.</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_event_handling_ok")]
        public static extern int EventHandlingOk([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Determine if an active thread is handling events (i.e. if anyone is holding the event handling lock).
        /// </summary>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>1 if a thread is handling events.</item>
        /// <item>0 if there are no threads currently handling events.</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_event_handler_active")]
        public static extern int EventHandlerActive([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Acquire the event waiters lock.
        /// </summary>
        /// <remarks>
        /// <para>This lock is designed to be obtained under the situation where you want to be aware when events are completed, but some other thread is event handling so calling <see  cref="HandleEvents">libusb_handle_events</see> is not allowed.</para>
        /// <para>You then obtain this lock, re-check that another thread is still handling events, then call <see  cref="WaitForEvent">libusb_wait_for_event</see>.</para>
        /// <para>You only need to use this lock if you are developing an application which calls poll() or select() on libusb's file descriptors directly, and may potentially be handling events from 2 threads simultaenously. If you stick to libusb's event handling loop functions (e.g. <see  cref="HandleEvents">libusb_handle_events</see>) then you do not need to be concerned with this locking.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_lock_event_waiters")]
        public static extern void LockEventWaiters([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Release the event waiters lock. 
        /// </summary>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_unlock_event_waiters")]
        public static extern void UnlockEventWaiters([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Wait for another thread to signal completion of an event. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function will block until any of the following conditions are met:
        /// <list type="numbered">
        /// <item>The timeout expires.</item>
        /// <item>A transfer completes.</item>
        /// <item>A thread releases the event handling lock through <see  cref="UnlockEvents">libusb_unlock_events</see>.</item>
        /// </list>
        /// </para>
        /// <para>Condition 1 is obvious. Condition 2 unblocks your thread after the callback for the transfer has completed. Condition 3 is important because it means that the thread that was previously handling events is no longer doing so, so if any events are to complete, another thread needs to step up and start event handling.</para>
        /// <para>This function releases the event waiters lock before putting your thread to sleep, and reacquires the lock as it is being woken up.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="timeval">Maximum timeout for this blocking function.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 after a transfer completes or another thread stops event handling.</item>
        /// <item>1 if the timeout expired.</item>
        /// </list>
        /// </returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_wait_for_event")]
        public static extern int WaitForEvent([In]MonoUsbSessionHandle sessionHandle, ref UnixNativeTimeval timeval);

        /// <summary>
        /// Handle any pending events. 
        /// </summary>
        /// <remarks>
        /// <para>libusb determines "pending events" by checking if any timeouts have expired and by checking the set of file descriptors for activity.</para>
        /// <para>If a non-zero timeval is passed and no events are currently pending, this function will block waiting for events to handle up until the specified timeout. If an event arrives or a signal is raised, this function will return early.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="tv">The maximum time to block waiting for events, or zero for non-blocking mode</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_handle_events_timeout")]
        public static extern int HandleEventsTimeout([In]MonoUsbSessionHandle sessionHandle, ref UnixNativeTimeval tv);

        /// <summary>
        /// Handle any pending events in blocking mode with a sensible timeout.
        /// </summary>
        /// <remarks>
        /// <para>This timeout is currently hardcoded at 2 seconds but we may change this if we decide other values are more sensible. For finer control over whether this function is blocking or non-blocking, or the maximum timeout, use <see  cref="HandleEventsTimeout">libusb_handle_events_timeout</see> instead.</para>
        /// </remarks>
        /// <param name="sessionHandle"></param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_handle_events")]
        public static extern int HandleEvents([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Handle any pending events by polling file descriptors, without checking if any other threads are already doing so. 
        /// </summary>
        /// <remarks>
        /// <para>Must be called with the event lock held, see <see  cref="LockEvents">libusb_lock_events</see>.</para>
        /// <para>This function is designed to be called under the situation where you have taken the event lock and are calling poll()/select() directly on libusb's file descriptors (as opposed to using <see  cref="HandleEvents">libusb_handle_events</see> or similar). You detect events on libusb's descriptors, so you then call this function with a zero timeout value (while still holding the event lock).</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="tv">The maximum time to block waiting for events, or zero for non-blocking mode</param>
        /// <returns>0 on success, or a <see cref="MonoUsbError"/> code on other failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_handle_events_locked")]
        public static extern int HandleEventsLocked([In]MonoUsbSessionHandle sessionHandle, ref UnixNativeTimeval tv);

        /// <summary>
        /// Determines whether your application must apply special timing considerations when monitoring libusb's file descriptors. 
        /// </summary>
        /// <remarks>
        /// <para>This function is only useful for applications which retrieve and poll libusb's file descriptors in their own main loop (The more advanced option).</para>
        /// <para>Ordinarily, libusb's event handler needs to be called into at specific moments in time (in addition to times when there is activity on the file descriptor set). The usual approach is to use <see  cref="GetNextTimeout">libusb_get_next_timeout</see> to learn about when the next timeout occurs, and to adjust your poll()/select() timeout accordingly so that you can make a call into the library at that time.</para>
        /// <para>Some platforms supported by libusb do not come with this baggage - any events relevant to timing will be represented by activity on the file descriptor set, and <see  cref="GetNextTimeout">libusb_get_next_timeout</see> will always return 0. This function allows you to detect whether you are running on such a platform.</para>
        /// <para>Since v1.0.5.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>0 if you must call into libusb at times determined by <see  cref="GetNextTimeout">libusb_get_next_timeout</see>, or 1 if all timeout events are handled internally or through regular activity on the file descriptors.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_pollfds_handle_timeouts")]
        public static extern int PollfdsHandleTimeouts([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Determine the next internal timeout that libusb needs to handle. 
        /// </summary>
        /// <remarks>
        /// <para>You only need to use this function if you are calling poll() or select() or similar on libusb's file descriptors yourself - you do not need to use it if you are calling <see  cref="HandleEvents">libusb_handle_events</see> or a variant directly.</para>
        /// <para>You should call this function in your main loop in order to determine how long to wait for select() or poll() to return results. libusb needs to be called into at this timeout, so you should use it as an upper bound on your select() or poll() call.</para>
        /// <para>When the timeout has expired, call into <see  cref="HandleEventsTimeout">libusb_handle_events_timeout</see> (perhaps in non-blocking mode) so that libusb can handle the timeout.</para>
        /// <para>This function may return 1 (success) and an all-zero timeval. If this is the case, it indicates that libusb has a timeout that has already expired so you should call <see  cref="HandleEventsTimeout">libusb_handle_events_timeout</see> or similar immediately. A return code of 0 indicates that there are no pending timeouts.</para>
        /// <para>On some platforms, this function will always returns 0 (no pending timeouts). See Notes on time-based events.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="tv">The maximum time to block waiting for events, or zero for non-blocking mode</param>
        /// <returns>0 if there are no pending timeouts, 1 if a timeout was returned, or <see cref="MonoUsbError.LIBUSB_ERROR_OTHER"/> on failure.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_next_timeout")]
        public static extern int GetNextTimeout([In]MonoUsbSessionHandle sessionHandle, ref UnixNativeTimeval tv);

        /// <summary>
        /// Register notification functions for file descriptor additions/removals. 
        /// </summary>
        /// <remarks>
        /// <para>To remove notifiers, pass NULL values for the function pointers.</para>
        /// <para>Note that file descriptors may have been added even before you register these notifiers (e.g. at <see cref="Init"/> time).</para>
        /// <para>Additionally, note that the removal notifier may be called during <see cref="Exit"/> (e.g. when it is closing file descriptors that were opened and added to the poll set at <see cref="Init"/> time). If you don't want this, remove the notifiers immediately before calling <see cref="Exit"/>.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="addedDelegate">Function delegate for addition notifications.</param>
        /// <param name="removedDelegate">Function delegate for removal notifications.</param>
        /// <param name="user_data">User data to be passed back to callbacks (useful for passing sessionHandle information).</param>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_set_pollfd_notifiers")]
        public static extern void SetPollfdNotifiers([In]MonoUsbSessionHandle sessionHandle,
                                                              PollfdAddedDelegate addedDelegate,
                                                              PollfdRemovedDelegate removedDelegate,
                                                              IntPtr user_data);


        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_get_pollfds")]
        private static extern IntPtr GetPollfdsInternal([In]MonoUsbSessionHandle sessionHandle);

        /// <summary>
        /// Retrieve a list of file descriptors that should be polled by your main loop as libusb event sources. 
        /// </summary>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>A list of PollfdItem structures, or null on error.</returns>
        public static List<PollfdItem> GetPollfds(MonoUsbSessionHandle sessionHandle)
        {
            List<PollfdItem> rtnList = new List<PollfdItem>();
            IntPtr pList = GetPollfdsInternal(sessionHandle);
            if (pList == IntPtr.Zero) return null;

            IntPtr pNext = pList;
            IntPtr pPollfd;
            while ((((pNext != IntPtr.Zero))) && (pPollfd = Marshal.ReadIntPtr(pNext)) != IntPtr.Zero)
            {
                PollfdItem pollfdItem = new PollfdItem(pPollfd);
                rtnList.Add(pollfdItem);
                pNext = new IntPtr(pNext.ToInt64() + IntPtr.Size);
            }
            Marshal.FreeHGlobal(pList);

            return rtnList;
        }

        #endregion

        #region API LIBRARY FUNCTIONS - Synchronous device I/O

        /// <summary>
        /// Perform a USB control transfer.
        /// </summary>
        /// <remarks>
        /// The direction of the transfer is inferred from the bmRequestType field of the setup packet. 
        /// The wValue, wIndex and wLength fields values should be given in host-endian byte order.
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="request_type">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="pData">A suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType).</param>
        /// <param name="dataLength">The length field for the setup packet. The data buffer should be at least this size.</param>
        /// <param name="timeout">timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>on success, the number of bytes actually transferred, Other wise a <see cref="MonoUsbError"/>.</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_control_transfer")]
        public static extern int ControlTransfer([In] MonoUsbDeviceHandle deviceHandle,
                                                         byte request_type,
                                                         byte request,
                                                         short value,
                                                         short index,
                                                         IntPtr pData,
                                                         short dataLength,
                                                         int timeout);

        /// <summary>
        /// Perform a USB control transfer.
        /// </summary>
        /// <remarks>
        /// The direction of the transfer is inferred from the bmRequestType field of the setup packet. 
        /// The wValue, wIndex and wLength fields values should be given in host-endian byte order.
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="request_type">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="data">
        /// <para>A suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType).</para>
        /// This value can be:
        /// <list type="bullet">
        /// <item>An <see cref="Array"/> of bytes or other <a href="http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx">blittable</a> types.</item>
        /// <item>An already allocated, pinned <see cref="GCHandle"/>. In this case <see cref="GCHandle.AddrOfPinnedObject"/> is used for the buffer address.</item>
        /// <item>An <see cref="IntPtr"/>.</item>
        /// </list>
        /// </param>
        /// <param name="dataLength">The length field for the setup packet. The data buffer should be at least this size.</param>
        /// <param name="timeout">timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>on success, the number of bytes actually transferred, Other wise a <see cref="MonoUsbError"/>.</returns>
        public static int ControlTransfer([In] MonoUsbDeviceHandle deviceHandle,
                                                  byte request_type,
                                                  byte request,
                                                  short value,
                                                  short index,
                                                  object data,
                                                  short dataLength,
                                                  int timeout)
        {
            PinnedHandle p = new PinnedHandle(data);
            int ret = ControlTransfer(deviceHandle, request_type, request, value, index, p.Handle, dataLength, timeout);
            p.Dispose();
            return ret;
        }

#if NOT_USED
    /// <summary>
    /// Perform a USB control transfer.
    /// </summary>
    /// <remarks>
    /// The direction of the transfer is inferred from the bmRequestType field of the setup packet. 
    /// The wValue, wIndex and wLength fields values should be given in host-endian byte order.
    /// </remarks>
    /// <param name="deviceHandle">A handle for the device to communicate with.</param>
    /// <param name="setupPacket">The setup packet.</param>
    /// <param name="pData">A suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType).</param>
    /// <param name="dataLength">The length field for the setup packet. The data buffer should be at least this size.</param>
    /// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
    /// <returns>on success, the number of bytes actually transferred, Other wise a <see cref="MonoUsbError"/>.</returns>
        public static int libusb_control_transfer(MonoUsbDeviceHandle deviceHandle,
                                                  ref libusb_control_setup setupPacket,
                                                  IntPtr pData,
                                                  short dataLength,
                                                  int timeout)
        {
            return libusb_control_transfer(deviceHandle,
                                           setupPacket.bmRequestType,
                                           setupPacket.bRequest,
                                           setupPacket.wValue,
                                           setupPacket.wIndex,
                                           pData,
                                           dataLength,
                                           timeout);
        }
#endif

        /// <summary>
        /// Perform a USB bulk transfer. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction of the transfer is inferred from the direction bits of the endpoint address.
        /// </para><para>
        /// For bulk reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para><para>
        /// You should also check the transferred parameter for bulk writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="pData">
        /// A suitably-sized data buffer for either input or output (depending on endpoint).</param>
        /// <param name="length">For bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actual_length">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>0 on success (and populates transferred).</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_bulk_transfer")]
        public static extern int BulkTransfer([In] MonoUsbDeviceHandle deviceHandle,
                                                      byte endpoint,
                                                      IntPtr pData,
                                                      int length,
                                                      out int actual_length,
                                                      int timeout);

        /// <summary>
        /// Perform a USB bulk transfer. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction of the transfer is inferred from the direction bits of the endpoint address.
        /// </para><para>
        /// For bulk reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para><para>
        /// You should also check the transferred parameter for bulk writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="data">
        /// <para>A suitably-sized data buffer for either input or output (depending on endpoint).</para>
        /// This value can be:
        /// <list type="bullet">
        /// <item>An <see cref="Array"/> of bytes or other <a href="http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx">blittable</a> types.</item>
        /// <item>An already allocated, pinned <see cref="GCHandle"/>. In this case <see cref="GCHandle.AddrOfPinnedObject"/> is used for the buffer address.</item>
        /// <item>An <see cref="IntPtr"/>.</item>
        /// </list>
        /// </param>
        /// <param name="length">For bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actual_length">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>0 on success (and populates transferred).</returns>
        public static int BulkTransfer([In] MonoUsbDeviceHandle deviceHandle,
                                               byte endpoint,
                                               object data,
                                               int length,
                                               out int actual_length,
                                               int timeout)
        {
            PinnedHandle p = new PinnedHandle(data);
            int ret = BulkTransfer(deviceHandle, endpoint, p.Handle, length, out actual_length, timeout);
            p.Dispose();
            return ret;
        }

        /// <summary>
        /// Perform a USB interrupt transfer. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction of the transfer is inferred from the direction bits of the endpoint address.
        /// </para><para>
        /// For bulk reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para><para>
        /// You should also check the transferred parameter for bulk writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="pData">A suitably-sized data buffer for either input or output (depending on endpoint).</param>
        /// <param name="length">For interrupt writes, the number of bytes from data to be sent. for interrupt reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actual_length">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>0 on success (and populates transferred).</returns>
        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_interrupt_transfer")]
        public static extern int InterruptTransfer([In] MonoUsbDeviceHandle deviceHandle,
                                                           byte endpoint,
                                                           IntPtr pData,
                                                           int length,
                                                           out int actual_length,
                                                           int timeout);


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
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="data">
        /// <para>A suitably-sized data buffer for either input or output (depending on endpoint).</para>
        /// This value can be:
        /// <list type="bullet">
        /// <item>An <see cref="Array"/> of bytes or other <a href="http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx">blittable</a> types.</item>
        /// <item>An already allocated, pinned <see cref="GCHandle"/>. In this case <see cref="GCHandle.AddrOfPinnedObject"/> is used for the buffer address.</item>
        /// <item>An <see cref="IntPtr"/>.</item>
        /// </list>
        /// </param>
        /// <param name="length">For interrupt writes, the number of bytes from data to be sent. for interrupt reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actual_length">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>0 on success (and populates transferred).</returns>
        public static int InterruptTransfer([In] MonoUsbDeviceHandle deviceHandle,
                                                    byte endpoint,
                                                    object data,
                                                    int length,
                                                    out int actual_length,
                                                    int timeout)
        {
            PinnedHandle p = new PinnedHandle(data);
            int ret = InterruptTransfer(deviceHandle, endpoint, p.Handle, length, out actual_length, timeout);
            p.Dispose();
            return ret;
        }

        #endregion

        #region API LIBRARY FUNCTIONS - Misc

        [DllImport(LIBUSB_DLL, CallingConvention = CC, SetLastError = false, EntryPoint = "libusb_strerror")]
        private static extern IntPtr StrError(int errcode);

        /// <summary>
        /// Get a string describing a <see cref="MonoUsbError"/>.
        /// </summary>
        /// <param name="errcode">The <see cref="MonoUsbError"/> code to retrieve a description for.</param>
        /// <returns>A string describing the <see cref="MonoUsbError"/> code.</returns>
        public static string StrError(MonoUsbError errcode)
        {
            IntPtr pErrStr = StrError((int) errcode);
            return Marshal.PtrToStringAnsi(pErrStr);
        }

        #endregion

        /// <summary>
        /// Converts a <see cref="MonoUsbTansferStatus"/> enum to a <see cref="MonoUsbError"/> enum.
        /// </summary>
        /// <param name="status">the <see cref="MonoUsbTansferStatus"/> to convert.</param>
        /// <returns>A <see cref="MonoUsbError"/> that represents <paramref name="status"/>.</returns>
        public static MonoUsbError MonoLibUsbErrorFromTransferStatus(MonoUsbTansferStatus status)
        {
            switch (status)
            {
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_COMPLETED:
                    return MonoUsbError.LIBUSB_SUCCESS;
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_ERROR:
                    return MonoUsbError.LIBUSB_ERROR_PIPE;
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_TIMED_OUT:
                    return MonoUsbError.LIBUSB_ERROR_TIMEOUT;
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_CANCELLED:
                    return MonoUsbError.LIBUSB_ERROR_IO_CANCELLED;
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_STALL:
                    return MonoUsbError.LIBUSB_ERROR_PIPE;
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_NO_DEVICE:
                    return MonoUsbError.LIBUSB_ERROR_NO_DEVICE;
                case MonoUsbTansferStatus.LIBUSB_TRANSFER_OVERFLOW:
                    return MonoUsbError.LIBUSB_ERROR_OVERFLOW;
                default:
                    return MonoUsbError.LIBUSB_ERROR_OTHER;
            }
        }

        /// <summary>
        /// Calls <see cref="MonoUsbEventHandler.Init()"/> and <see cref="MonoUsbEventHandler.Start"/> if <see cref="MonoUsbEventHandler.IsStopped"/> = true.
        /// </summary>
        internal static void InitAndStart()
        {
            if (!MonoUsbEventHandler.IsStopped) return;
            MonoUsbEventHandler.Init();
            MonoUsbEventHandler.Start();
        }

        /// <summary>
        /// Calls <see cref="MonoUsbEventHandler.Stop"/> and <see cref="MonoUsbEventHandler.Exit"/>.
        /// </summary>
        internal static void StopAndExit()
        {
#if LIBUSBDOTNET
            if (LibUsbDotNet.MonoLibUsb.MonoUsbDevice.mMonoUSBProfileList != null) LibUsbDotNet.MonoLibUsb.MonoUsbDevice.mMonoUSBProfileList.Free();
            LibUsbDotNet.MonoLibUsb.MonoUsbDevice.mMonoUSBProfileList = null;
#endif
            MonoUsbEventHandler.Stop(true);
            MonoUsbEventHandler.Exit();
        }


#if LIBUSBDOTNET

        internal static ErrorCode ErrorCodeFromLibUsbError(int ret, out string description)
        {
            description = string.Empty;
            if (ret == 0) return ErrorCode.Success;

            switch ((MonoUsbError) ret)
            {
                case MonoUsbError.LIBUSB_SUCCESS:
                    description += "Success";
                    return ErrorCode.Success;
                case MonoUsbError.LIBUSB_ERROR_IO:
                    description += "Input/output error";
                    return ErrorCode.IoSyncFailed;
                case MonoUsbError.LIBUSB_ERROR_INVALID_PARAM:
                    description += "Invalid parameter";
                    return ErrorCode.InvalidParam;
                case MonoUsbError.LIBUSB_ERROR_ACCESS:
                    description += "Access denied (insufficient permissions)";
                    return ErrorCode.AccessDenied;
                case MonoUsbError.LIBUSB_ERROR_NO_DEVICE:
                    description += "No such device (it may have been disconnected)";
                    return ErrorCode.DeviceNotFound;
                case MonoUsbError.LIBUSB_ERROR_BUSY:
                    description += "Resource busy";
                    return ErrorCode.ResourceBusy;
                case MonoUsbError.LIBUSB_ERROR_TIMEOUT:
                    description += "Operation timed out";
                    return ErrorCode.IoTimedOut;
                case MonoUsbError.LIBUSB_ERROR_OVERFLOW:
                    description += "Overflow";
                    return ErrorCode.Overflow;
                case MonoUsbError.LIBUSB_ERROR_PIPE:
                    description += "Pipe error or endpoint halted";
                    return ErrorCode.PipeError;
                case MonoUsbError.LIBUSB_ERROR_INTERRUPTED:
                    description += "System call interrupted (perhaps due to signal)";
                    return ErrorCode.Interrupted;
                case MonoUsbError.LIBUSB_ERROR_NO_MEM:
                    description += "Insufficient memory";
                    return ErrorCode.InsufficientMemory;
                case MonoUsbError.LIBUSB_ERROR_IO_CANCELLED:
                    description += "Transfer was canceled";
                    return ErrorCode.IoCancelled;
                case MonoUsbError.LIBUSB_ERROR_NOT_SUPPORTED:
                    description += "Operation not supported or unimplemented on this platform";
                    return ErrorCode.NotSupported;
                default:
                    description += "Unknown error:" + ret;
                    return ErrorCode.UnknownError;
            }
        }
#endif
    }
}