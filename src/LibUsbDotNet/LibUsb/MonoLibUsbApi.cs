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
#region API LIBRARY FUNCTIONS - USB descriptors

        [DllImport(NativeMethods.LibUsbNativeLibrary, CallingConvention = NativeMethods.LibUsbCallingConvention, SetLastError = false, EntryPoint = "libusb_free_config_descriptor")]
        internal static extern void FreeConfigDescriptor(IntPtr pConfigDescriptor);

#endregion
    }
}