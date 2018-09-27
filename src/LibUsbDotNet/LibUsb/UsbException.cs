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

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace LibUsbDotNet.LibUsb
{
    [Serializable]
    public class UsbException : Exception
    {
        public UsbException()
        {
        }

        public UsbException(Error errorCode)
            : this(GetErrorMessage(errorCode))
        {
            this.ErrorCode = errorCode;
            this.HResult = (int)errorCode;
        }

        public UsbException(string message)
            : base(message)
        {
        }

        public UsbException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UsbException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Error ErrorCode
        {
            get;
            private set;
        }

        private static string GetErrorMessage(Error errorCode)
        {
            IntPtr errorString = NativeMethods.StrError(errorCode);

            if (errorString != IntPtr.Zero)
            {
                // From the documentation: 'The caller must not free() the returned string.'
                return Marshal.PtrToStringAnsi(errorString);
            }
            else
            {
                return $"An unknown error with code {(int)errorCode} has occurred.";
            }
        }
    }
}
