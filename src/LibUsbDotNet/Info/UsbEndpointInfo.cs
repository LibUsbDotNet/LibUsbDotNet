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
using System.Diagnostics;

namespace LibUsbDotNet.Info
{
    /// <summary> Contains Endpoint information for the current <see cref="T:LibUsbDotNet.Info.UsbConfigInfo"/>.
    /// </summary>
    public class UsbEndpointInfo : UsbBaseInfo
    {
        public static unsafe UsbEndpointInfo FromUsbEndpointDescriptor(EndpointDescriptor descriptor)
        {
            Debug.Assert(descriptor.DescriptorType == (int)DescriptorType.Endpoint, "An endpoint descriptor was expected");

            var value = new UsbEndpointInfo();
            value.Attributes = descriptor.Attributes;
            value.EndpointAddress = descriptor.EndpointAddress;

            value.RawDescriptors = new byte[descriptor.ExtraLength];
            if (descriptor.ExtraLength > 0)
            {
                Span<byte> extra = new Span<byte>(descriptor.Extra, descriptor.ExtraLength);
                extra.CopyTo(value.RawDescriptors);
            }

            value.Interval = descriptor.Interval;
            value.MaxPacketSize = descriptor.MaxPacketSize;
            value.Refresh = descriptor.Refresh;
            value.SyncAddress = descriptor.SynchAddress;

            return value;
        }

        public virtual byte Attributes { get; private set; }

        public virtual byte EndpointAddress { get; private set; }

        public virtual byte Interval { get; private set; }

        public virtual ushort MaxPacketSize { get; private set; }

        public virtual byte Refresh { get; private set; }

        public virtual byte SyncAddress { get; private set; }

        public override string ToString()
        {
            return $"{this.EndpointAddress}";
        }
    }
}