// Copyright � 2006-2010 Travis Robinson. All rights reserved.
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
#if !NETSTANDARD1_5 && !NETSTANDARD1_6
using LibUsbDotNet.DeviceNotify.Linux;

namespace LibUsbDotNet.DeviceNotify
{
    /// <summary>
    /// Platform independent class for linux/windows device notification.
    /// </summary>
    /// <code source="..\Examples\Device.Notification\DeviceNotification.cs" lang="cs"/>
    public static class DeviceNotifier
    {
        /// <summary>
        /// Creates a new instance of a device notifier class.
        /// </summary>
        /// <returns>A <see cref="WindowsDeviceNotifier"/> under windows and a <see cref="LinuxDeviceNotifier"/> under linux.</returns>
        public static IDeviceNotifier OpenDeviceNotifier()
        {
            if (UsbDevice.IsLinux)
            {
                return new LinuxDeviceNotifier();
            }
            else
            {
                return new WindowsDeviceNotifier();
            }
        }
    }
}
#endif
