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
using LibUsbDotNet.Info;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibUsbDotNet.LibUsb
{
    // Contains the functionality related to kernel driver support.
    public partial class UsbDevice
    {
        public unsafe bool SupportsDetachKernelDriver()
        {
            int result = NativeMethods.HasCapability((uint)Capability.SupportsDetachKernelDriver);
            return result == 1;
        }
        public unsafe bool IsKernelDriverActive(int interfaceNumber)
        {
            EnsureOpen();
            int result = NativeMethods.KernelDriverActive(this.DeviceHandle, interfaceNumber);
            return result == 1;
         }

        public unsafe bool DetachKernelDriver(int interfaceNumber)
        {
            EnsureOpen();
            var result = NativeMethods.DetachKernelDriver(this.DeviceHandle, interfaceNumber);
            return result == Error.Success;
        }

        public unsafe bool AttachKernelDriver(int interfaceNumber)
        {
            EnsureOpen();
            var result = NativeMethods.AttachKernelDriver(this.DeviceHandle, interfaceNumber);
            return result == Error.Success;
        }

        public unsafe bool SetAutoDetachKernelDriver(bool autoDetach)
        {
            EnsureOpen();
            var result = NativeMethods.SetAutoDetachKernelDriver(this.DeviceHandle, autoDetach ? 1 : 0);
            return result == Error.Success;
        }
    }
}
