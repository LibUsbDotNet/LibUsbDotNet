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
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.LibUsb
{
    /// <summary>
    /// Contains methods for retrieving data from a <see cref="EndpointType.Bulk"/> or <see cref="EndpointType.Interrupt"/> endpoint using the overloaded <see cref="Read(byte[],int,out int)"/> functions or a <see cref="DataReceived"/> event.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Before using the <see cref="DataReceived"/> event, the <see cref="DataReceivedEnabled"/> property must be set to true.</item>
    /// <item>While the <see cref="DataReceivedEnabled"/> property is True, the overloaded <see cref="Read(byte[],int,out int)"/> functions cannot be used.</item>
    /// </list>
    /// </remarks>
    public class UsbEndpointReader : UsbEndpointBase
    {
        private int mReadBufferSize;

        public UsbEndpointReader(IUsbDevice usbDevice, int readBufferSize, byte alternateInterfaceID, ReadEndpointID readEndpointID, EndpointType endpointType)
            : base(usbDevice, alternateInterfaceID, (byte)readEndpointID, endpointType)
        {
            this.mReadBufferSize = readBufferSize;
        }

        /// <summary>
        /// Default read buffer size when using the <see cref="DataReceived"/> event.
        /// </summary>
        /// <remarks>
        /// This value can be bypassed using the second parameter of the <see cref="UsbDevice.OpenEndpointReader(LibUsbDotNet.Main.ReadEndpointID,int)"/> method.
        /// The default is 4096.
        /// </remarks>
        public static int DefReadBufferSize { get; set; } = 4096;

        /// <summary>
        /// Reads data from the current <see cref="UsbEndpointReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the recieved data in.</param>
        /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
        /// <param name="transferLength">Number of bytes actually transferred.</param>
        /// <returns>
        /// <see cref="Error"/>.<see cref="Error.None"/> on success.
        /// </returns>
        public virtual Error Read(byte[] buffer, int timeout, out int transferLength)
        {
            return this.Read(buffer, 0, buffer.Length, timeout, out transferLength);
        }

        /// <summary>
        /// Reads data from the current <see cref="UsbEndpointReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the recieved data in.</param>
        /// <param name="offset">The position in buffer to start storing the data.</param>
        /// <param name="count">The maximum number of bytes to receive.</param>
        /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
        /// <param name="transferLength">Number of bytes actually transferred.</param>
        /// <returns>
        /// <see cref="Error"/>.<see cref="Error.None"/> on success.
        /// </returns>
        public virtual Error Read(IntPtr buffer, int offset, int count, int timeout, out int transferLength)
        {
            return this.Transfer(buffer, offset, count, timeout, out transferLength);
        }

        /// <summary>
        /// Reads data from the current <see cref="UsbEndpointReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the recieved data in.</param>
        /// <param name="offset">The position in buffer to start storing the data.</param>
        /// <param name="count">The maximum number of bytes to receive.</param>
        /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
        /// <param name="transferLength">Number of bytes actually transferred.</param>
        /// <returns>
        /// <see cref="Error"/>.<see cref="Error.None"/> on success.
        /// </returns>
        public virtual Error Read(byte[] buffer, int offset, int count, int timeout, out int transferLength)
        {
            return this.Transfer(buffer, offset, count, timeout, out transferLength);
        }

        /// <summary>
        /// Reads data from the current <see cref="UsbEndpointReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the recieved data in.</param>
        /// <param name="offset">The position in buffer to start storing the data.</param>
        /// <param name="count">The maximum number of bytes to receive.</param>
        /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
        /// <param name="transferLength">Number of bytes actually transferred.</param>
        /// <returns>
        /// <see cref="Error"/>.<see cref="Error.None"/> on success.
        /// </returns>
        public virtual Error Read(object buffer, int offset, int count, int timeout, out int transferLength)
        {
            return this.Transfer(buffer, offset, count, timeout, out transferLength);
        }

        /// <summary>
        /// Reads data from the current <see cref="UsbEndpointReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the recieved data in.</param>
        /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
        /// <param name="transferLength">Number of bytes actually transferred.</param>
        /// <returns>
        /// <see cref="Error"/>.<see cref="Error.None"/> on success.
        /// </returns>
        public virtual Error Read(object buffer, int timeout, out int transferLength)
        {
            return this.Transfer(buffer, 0, Marshal.SizeOf(buffer), timeout, out transferLength);
        }

        /// <summary>
        /// Reads/discards data from the enpoint until no more data is available.
        /// </summary>
        /// <returns>Alwats returns <see cref="Error.None"/> </returns>
        public virtual Error ReadFlush()
        {
            byte[] bufDummy = new byte[64];
            int iTransferred;
            int iBufCount = 0;
            while (this.Read(bufDummy, 10, out iTransferred) == Error.Success && iBufCount < 128)
            {
                iBufCount++;
            }

            return Error.Success;
        }
    }
}