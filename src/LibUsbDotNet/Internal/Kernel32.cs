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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace LibUsbDotNet.Internal
{
#if !NETSTANDARD1_5 && !NETSTANDARD1_6
    [SuppressUnmanagedCodeSecurity]
#endif
    internal static class Kernel32
    {
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private static readonly StringBuilder m_sbSysMsg = new StringBuilder(1024);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int FormatMessage(int dwFlags,
                                                IntPtr lpSource,
                                                int dwMessageId,
                                                int dwLanguageId,
                                                [Out] StringBuilder lpBuffer,
                                                int nSize,
                                                IntPtr lpArguments);

        public static string FormatSystemMessage(int dwMessageId)
        {
            lock (m_sbSysMsg)
            {
                int ret = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM,
                                        IntPtr.Zero,
                                        dwMessageId,
#if !NETSTANDARD1_5 && !NETSTANDARD1_6
                                        CultureInfo.CurrentCulture.LCID,
#else
                                        0,
#endif
                                        m_sbSysMsg,
                                        m_sbSysMsg.Capacity - 1,
                                        IntPtr.Zero);

                if (ret > 0) return m_sbSysMsg.ToString(0, ret);
                return null;
            }
        }
    }
}
