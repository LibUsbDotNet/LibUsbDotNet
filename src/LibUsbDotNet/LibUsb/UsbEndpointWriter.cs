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
using LibUsbDotNet.LudnMonoLibUsb.Internal;
using LibUsbDotNet.Main;
using System;

namespace LibUsbDotNet.LibUsb
{
    public class UsbEndpointWriter : LibUsbDotNet.UsbEndpointWriter
    {
        private readonly UsbDevice usbDevice;

        internal UsbEndpointWriter(UsbDevice usbDevice, byte alternateInterfaceID, WriteEndpointID writeEndpointID, EndpointType endpointType)
            : base(usbDevice, alternateInterfaceID, writeEndpointID, endpointType)
        {
            this.usbDevice = usbDevice;
        }

        /// <summary>
        /// This method has no effect on write endpoints, andalways returs true.
        /// </summary>
        /// <returns>True</returns>
        public override bool Flush() { return true; }

        /// <summary>
        /// Cancels pending transfers and clears the halt condition on an enpoint.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Reset()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            Abort();
            NativeMethods.ClearHalt(this.usbDevice.DeviceHandle, EpNum).ThrowOnError();
            return true;
        }

        protected override UsbTransfer CreateTransferContext()
        {
            return new MonoUsbTransferContext(this);
        }
    }
}
