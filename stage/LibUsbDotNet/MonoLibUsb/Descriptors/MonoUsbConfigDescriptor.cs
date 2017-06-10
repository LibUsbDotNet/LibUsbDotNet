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
using System.Runtime.InteropServices;


using LibUsbDotNet.Descriptors;
using MonoLibUsb.Profile;

namespace MonoLibUsb.Descriptors
{
    ///<summary>A structure representing the standard USB configuration descriptor. 
    ///This descriptor is documented in section 9.6.3 of the USB 2.0 specification. 
    ///All multiple-byte fields are represented in host-endian format.</summary>
    /// <example><code source="..\MonoLibUsb\MonoUsb.ShowConfig\ShowConfig.cs" lang="cs"/></example>
    [StructLayout(LayoutKind.Sequential, Pack = MonoUsbApi.LIBUSB_PACK)]
    public class MonoUsbConfigDescriptor: UsbConfigDescriptor
    {
        ///<summary> Array of interfaces supported by this configuration. The length of this array is determined by the bNumInterfaces field.</summary>
        private readonly IntPtr pInterfaces;

        ///<summary> Extra descriptors. If libusb encounters unknown configuration descriptors, it will store them here, should you wish to parse them.</summary>
        private readonly IntPtr pExtraBytes;

        ///<summary> Length of the extra descriptors, in bytes.</summary>
        public readonly int ExtraLength;

        ///<summary> Extra descriptors. If libusb encounters unknown configuration descriptors, it will store them here, should you wish to parse them.</summary>
        public byte[] ExtraBytes
        {
            get
            {
                byte[] bytes = new byte[ExtraLength];
                Marshal.Copy(pExtraBytes, bytes, 0, bytes.Length);
                return bytes;
            }
        }

        ///<summary> Array of interfaces supported by this configuration. The length of this array is determined by the bNumInterfaces field.</summary>
        public List<MonoUsbInterface> InterfaceList
        {
            get
            {
                List<MonoUsbInterface> interfaceList = new List<MonoUsbInterface>();
                int iInterface;
                for (iInterface = 0; iInterface < InterfaceCount; iInterface++)
                {
                    IntPtr pNextInterface = new IntPtr(pInterfaces.ToInt64() + (Marshal.SizeOf(typeof (MonoUsbInterface))*iInterface));
                    MonoUsbInterface monoUsbInterface = new MonoUsbInterface();
                    Marshal.PtrToStructure(pNextInterface, monoUsbInterface);
                    interfaceList.Add(monoUsbInterface);
                }

                return interfaceList;
            }
        }
    }
}