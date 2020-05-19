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
    /// <summary> Contains all Configuration information for the current <see cref="T:LibUsbDotNet.UsbDevice"/>.
    /// </summary>
    public class UsbConfigInfo : UsbBaseInfo
    {
        private readonly List<UsbInterfaceInfo> interfaces = new List<UsbInterfaceInfo>();

        internal static unsafe UsbConfigInfo FromUsbConfigDescriptor(global::LibUsbDotNet.LibUsb.UsbDevice device, ConfigDescriptor descriptor)
        {
            Debug.Assert(descriptor.DescriptorType == (int)DescriptorType.Config, "A config descriptor was expected");

            UsbConfigInfo value = new UsbConfigInfo();
            value.Attributes = descriptor.Attributes;
            value.Configuration = device.GetStringDescriptor(descriptor.Configuration, failSilently: true);
            value.ConfigurationValue = descriptor.ConfigurationValue;

            value.RawDescriptors = new byte[descriptor.ExtraLength];
            if (descriptor.ExtraLength > 0)
            {
                Span<byte> extra = new Span<byte>(descriptor.Extra, descriptor.ExtraLength);
                extra.CopyTo(value.RawDescriptors);
            }

            var interfaces = (Interface*)descriptor.Interface;
            for (int i = 0; i < descriptor.NumInterfaces; i++)
            {
                var values = UsbInterfaceInfo.FromUsbInterface(device, interfaces[i]);
                value.interfaces.AddRange(values);
            }

            value.MaxPower = descriptor.MaxPower;

            return value;
        }

        public virtual string Configuration { get; protected set; }

        public virtual byte Attributes { get; protected set; }

        public virtual int ConfigurationValue { get; protected set; }

        public virtual byte MaxPower { get; protected set; }

        /// <summary>
        /// Gets the collection of USB device interfaces associated with this <see cref="UsbConfigInfo"/> instance.
        /// </summary>
        public virtual ReadOnlyCollection<UsbInterfaceInfo> Interfaces
        {
            get { return this.interfaces.AsReadOnly(); }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Configuration;
        }
    }
}