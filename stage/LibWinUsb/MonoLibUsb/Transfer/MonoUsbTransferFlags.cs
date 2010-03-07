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
namespace MonoLibUsb.Transfer
{
    /// <summary>
    /// Transfer flags.
    /// </summary>
    public enum MonoUsbTransferFlags : byte
    {
        /// <summary>
        /// No transfer flags.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Report short frames as errors
        /// </summary>
        LIBUSB_TRANSFER_SHORT_NOT_OK = 1 << 0,

        /// <summary>
        /// Automatically free() transfer buffer during FreeTransfer()
        /// </summary>
        LIBUSB_TRANSFER_FREE_BUFFER = 1 << 1,

        /// <summary>
        /// Automatically call FreeTransfer() after callback returns.
        /// If this flag is set, it is illegal to call FreeTransfer()
        /// from your transfer callback, as this will result in a double-free
        /// when this flag is acted upon.
        /// </summary>
        LIBUSB_TRANSFER_FREE_TRANSFER = 1 << 2,
    } ;
}