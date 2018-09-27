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

namespace LibUsbDotNet.LibUsb
{
    /// <summary>
    /// Initializes a new libusb context. You can access most of libusb's functionality through context. Multiple
    /// contexts operate in isolation from each other.
    /// </summary>
    /// <seealso href="http://libusb.sourceforge.net/api-1.0/libusb_contexts.html"/>
    public interface IUsbContext : IDisposable
    {
    }
}
