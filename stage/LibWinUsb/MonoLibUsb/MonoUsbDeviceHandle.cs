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
using LibUsbDotNet.Main;

namespace MonoLibUsb
{
    /// <summary>
    /// Wraps a device handle into a <see cref="System.Runtime.ConstrainedExecution.CriticalFinalizerObject"/>
    /// </summary>
    /// <remarks>
    /// The <see cref="MonoUsbDeviceHandle"/> class ensures all devices get closed and 
    /// freed regardless of abnormal program terminations or coding errors.
    /// </remarks>

    public class MonoUsbDeviceHandle : SafeContextHandle
    {
        /// <summary>
        /// Creates an empty <see cref="MonoUsbDeviceHandle"/> instance.
        /// </summary>
        public MonoUsbDeviceHandle()
            : base(IntPtr.Zero) { }

        /// <summary>
        /// Wraps a raw usb device handle pointer in a <see cref="MonoUsbDeviceHandle"/> class.
        /// </summary>
        /// <param name="deviceHandle">the device handle to wrap.</param>
        public MonoUsbDeviceHandle(IntPtr deviceHandle)
            : base(deviceHandle) { }

        ///<summary>
        ///Closes the <see cref="MonoUsbDeviceHandle"/>.
        ///</summary>
        ///<returns>
        ///true if the <see cref="MonoUsbDeviceHandle"/> is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a ReleaseHandleFailed Managed Debugging Assistant.
        ///</returns>
        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                MonoLibUsbApi.Close(handle);
                SetHandleAsInvalid();
                return true;
            }
            return false;
        }
    }
}