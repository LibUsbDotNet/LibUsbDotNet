// Copyright © 2006-2009 Travis Robinson. All rights reserved.
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

namespace MonoLibUsb
{
    /// <summary>
    /// Error codes.
    /// Most libusb functions return 0 on success or one of these codes on failure. 
    /// </summary>
    public enum MonoUsbError
    {
        /// <summary>
        /// Success (no error) 
        /// </summary>
        LIBUSB_SUCCESS = 0,

        /// <summary>
        /// Input/output error 
        /// </summary>
        LIBUSB_ERROR_IO = -1,

        /// <summary>
        /// Invalid parameter 
        /// </summary>
        LIBUSB_ERROR_INVALID_PARAM = -2,

        /// <summary>
        /// Access denied (insufficient permissions) 
        /// </summary>
        LIBUSB_ERROR_ACCESS = -3,

        /// <summary>
        /// No such device (it may have been disconnected) 
        /// </summary>
        LIBUSB_ERROR_NO_DEVICE = -4,

        /// <summary>
        /// Entity not found 
        /// </summary>
        LIBUSB_ERROR_NOT_FOUND = -5,

        /// <summary>
        /// Resource busy 
        /// </summary>
        LIBUSB_ERROR_BUSY = -6,

        /// <summary>
        /// Operation timed out 
        /// </summary>
        LIBUSB_ERROR_TIMEOUT = -7,

        /// <summary>
        /// Overflow 
        /// </summary>
        LIBUSB_ERROR_OVERFLOW = -8,

        /// <summary>
        /// Pipe error 
        /// </summary>
        LIBUSB_ERROR_PIPE = -9,

        /// <summary>
        /// System call interrupted (perhaps due to signal) 
        /// </summary>
        LIBUSB_ERROR_INTERRUPTED = -10,

        /// <summary>
        /// Insufficient memory 
        /// </summary>
        LIBUSB_ERROR_NO_MEM = -11,

        /// <summary>
        /// Operation not supported or unimplemented on this platform 
        /// </summary>
        LIBUSB_ERROR_NOT_SUPPORTED = -12,

        /// <summary>
        /// Cancel IO failed.
        /// </summary>
        LIBUSB_ERROR_IO_CANCELLED = -13,

        /// <summary>
        /// Other error 
        /// </summary>
        LIBUSB_ERROR_OTHER = -99,
    } ;
}