// Copyright © 2006-2010 Travis Robinson. All rights reserved.
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
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using Debug = System.Diagnostics.Debug;

namespace LibUsbDotNet
{
    public abstract partial class UsbDevice
    {
        private static object mHasWinUsbDriver;
        private static object mHasLibUsbWinBackDriver;

        private static UsbKernelVersion mUsbKernelVersion;

        /// <summary>
        /// Gets a list of all available USB devices (WinUsb, LibUsb, Linux LibUsb v1.x).
        /// </summary>
        /// <remarks>
        /// Use this property to get a list of USB device that can be accessed by LibUsbDotNet.
        /// Using this property as opposed to <see cref="AllLibUsbDevices"/> and <see cref="AllWinUsbDevices"/>
        /// will ensure your source code is platform-independent.
        /// </remarks>
        public static List<MonoUsbDevice> AllDevices => MonoUsbDevice.MonoUsbDeviceList;

        /// <summary>
        /// Global static error event for all Usb errors.
        /// </summary>
        /// <example>
        /// Sample code to reset an endpoint if a critical error occurs.
        /// <code>
        /// // Hook the usb error handler function
        /// UsbGlobals.UsbErrorEvent += UsbErrorEvent;
        ///private void UsbErrorEvent(object sender, UsbError e)
        ///{
        /// // If the error is from a usb endpoint
        /// if (sender is UsbEndpointBase)
        /// {
        ///     // If the endpoint transfer failed
        ///     if (e.Win32ErrorNumber == 31)
        ///     {
        ///         // If the USB device is still open, connected, and valid
        ///         if (usb.IsOpen)
        ///         {
        ///             // Try to reset then endpoint
        ///             if (((UsbEndpointBase) sender).Reset())
        ///             {
        ///                 // Endpoint reset successful.
        ///                 // Tell LibUsbDotNet to ignore this error and continue.
        ///                 e.Handled = true;
        ///             }
        ///         }
        ///     }
        /// }
        /// }
        /// </code>
        /// </example>
        public static event EventHandler<UsbError> UsbErrorEvent;
    }
}