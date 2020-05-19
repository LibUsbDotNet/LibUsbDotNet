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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LibUsbDotNet.Info
{
    /// <summary> Describes a USB device interface.
    /// </summary>
    public class UsbInterfaceInfo : UsbBaseInfo
    {
        private List<UsbEndpointInfo> endpoints = new List<UsbEndpointInfo>();

        public static unsafe Collection<UsbInterfaceInfo> FromUsbInterface(LibUsb.UsbDevice device, Interface @interface)
        {
            var interfaces = (InterfaceDescriptor*)@interface.Altsetting;
            Collection<UsbInterfaceInfo> value = new Collection<UsbInterfaceInfo>();

            for (int i = 0; i < @interface.NumAltsetting; i++)
            {
                value.Add(FromUsbInterfaceDescriptor(device, interfaces[i]));
            }

            return value;
        }

        public static unsafe UsbInterfaceInfo FromUsbInterfaceDescriptor(LibUsb.UsbDevice device, InterfaceDescriptor descriptor)
        {
            Debug.Assert(descriptor.DescriptorType == (int)DescriptorType.Interface, "A config descriptor was expected");

            UsbInterfaceInfo value = new UsbInterfaceInfo();
            value.AlternateSetting = descriptor.AlternateSetting;

            var endpoints = (EndpointDescriptor*)descriptor.Endpoint;

            for (int i = 0; i < descriptor.NumEndpoints; i++)
            {
                if (endpoints[i].DescriptorType != 0)
                {
                    value.endpoints.Add(UsbEndpointInfo.FromUsbEndpointDescriptor(endpoints[i]));
                }
            }

            value.RawDescriptors = new byte[descriptor.ExtraLength];
            if (descriptor.ExtraLength > 0)
            {
                Span<byte> extra = new Span<byte>(descriptor.Extra, descriptor.ExtraLength);
                extra.CopyTo(value.RawDescriptors);
            }

            value.Interface = device.GetStringDescriptor(descriptor.Interface, failSilently: true);
            value.Class = (ClassCode)descriptor.InterfaceClass;
            value.Number = descriptor.InterfaceNumber;
            value.Protocol = descriptor.InterfaceProtocol;
            value.SubClass = descriptor.InterfaceSubClass;

            return value;
        }

        public virtual byte AlternateSetting { get; private set; }

        public virtual ClassCode Class { get; private set; }

        public virtual int Number { get; private set; }

        public virtual byte Protocol { get; private set; }

        public virtual string Interface { get; private set; }

        public virtual byte SubClass { get; private set; }

        /// <summary>
        /// Gets the collection of endpoint descriptors associated with this interface.
        /// </summary>
        public virtual ReadOnlyCollection<UsbEndpointInfo> Endpoints
        {
            get { return this.endpoints.AsReadOnly(); }
        }

        public override string ToString()
        {
            return this.Interface;
        }
    }
}