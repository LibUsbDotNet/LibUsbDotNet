//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Copyright © 2006-2010 Travis Robinson. All rights reserved.
// Copyright © 2011-2023 LibUsbDotNet contributors. All rights reserved.
// 
// website: http://github.com/libusbdotnet/libusbdotnet
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

namespace LibUsbDotNet
{
    /// <summary>
    ///  A structure representing the standard USB interface descriptor. This
    ///  descriptor is documented in section 9.6.5 of the USB 3.0 specification.
    ///  All multiple-byte fields are represented in host-endian format.
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = NativeMethods.Pack)]
    public unsafe struct InterfaceDescriptor
    {
        /// <summary>
        ///  Size of this descriptor (in bytes)
        /// </summary>
        public byte Length;

        /// <summary>
        ///  Descriptor type. Will have value
        ///  in this context.
        /// </summary>
        public byte DescriptorType;

        /// <summary>
        ///  Number of this interface
        /// </summary>
        public byte InterfaceNumber;

        /// <summary>
        ///  Value used to select this alternate setting for this interface
        /// </summary>
        public byte AlternateSetting;

        /// <summary>
        ///  Number of endpoints used by this interface (excluding the control
        ///  endpoint).
        /// </summary>
        public byte NumEndpoints;

        /// <summary>
        ///  USB-IF class code for this interface. See
        /// </summary>
        public byte InterfaceClass;

        /// <summary>
        ///  USB-IF subclass code for this interface, qualified by the
        ///  bInterfaceClass value
        /// </summary>
        public byte InterfaceSubClass;

        /// <summary>
        ///  USB-IF protocol code for this interface, qualified by the
        ///  bInterfaceClass and bInterfaceSubClass values
        /// </summary>
        public byte InterfaceProtocol;

        /// <summary>
        ///  Index of string descriptor describing this interface
        /// </summary>
        public byte Interface;

        /// <summary>
        ///  Array of endpoint descriptors. This length of this array is determined
        ///  by the bNumEndpoints field.
        /// </summary>
        public EndpointDescriptor* Endpoint;

        /// <summary>
        ///  Extra descriptors. If libusb encounters unknown interface descriptors,
        ///  it will store them here, should you wish to parse them.
        /// </summary>
        public byte* Extra;

        /// <summary>
        ///  Length of the extra descriptors, in bytes.
        /// </summary>
        public int ExtraLength;

    }
}
