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

namespace LibUsbDotNet.Internal
{
    internal class SafeOverlapped : SafeContextHandle
    {
        // Find the structural starting positions in the NativeOverlapped structure.
        private static readonly int FieldOffsetEventHandle = Marshal.OffsetOf(typeof (NativeOverlapped), "EventHandle").ToInt32();
        private static readonly int FieldOffsetInternalHigh = Marshal.OffsetOf(typeof (NativeOverlapped), "InternalHigh").ToInt32();
        private static readonly int FieldOffsetInternalLow = Marshal.OffsetOf(typeof (NativeOverlapped), "InternalLow").ToInt32();
        private static readonly int FieldOffsetOffsetHigh = Marshal.OffsetOf(typeof (NativeOverlapped), "OffsetHigh").ToInt32();
        private static readonly int FieldOffsetOffsetLow = Marshal.OffsetOf(typeof (NativeOverlapped), "OffsetLow").ToInt32();

        // this needs to go in global memory or it'll have trouble with 64bit
        public SafeOverlapped() : base(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeOverlapped)))){}

        public IntPtr InternalLow
        {
            get { return Marshal.ReadIntPtr(DangerousGetHandle(), FieldOffsetInternalLow); }
            set { Marshal.WriteIntPtr(DangerousGetHandle(), FieldOffsetInternalLow, value); }
        }

        public IntPtr InternalHigh
        {
            get { return Marshal.ReadIntPtr(DangerousGetHandle(), FieldOffsetInternalHigh); }
            set { Marshal.WriteIntPtr(DangerousGetHandle(), FieldOffsetInternalHigh, value); }
        }

        public int OffsetLow
        {
            get { return Marshal.ReadInt32(DangerousGetHandle(), FieldOffsetOffsetLow); }
            set { Marshal.WriteInt32(DangerousGetHandle(), FieldOffsetOffsetLow, value); }
        }

        public int OffsetHigh
        {
            get { return Marshal.ReadInt32(DangerousGetHandle(), FieldOffsetOffsetHigh); }
            set { Marshal.WriteInt32(DangerousGetHandle(), FieldOffsetOffsetHigh, value); }
        }

        /// <summary>
        /// The overlapped event wait hande.
        /// </summary>
        public IntPtr EventHandle
        {
            get { return Marshal.ReadIntPtr(DangerousGetHandle(), FieldOffsetEventHandle); }
            set { Marshal.WriteIntPtr(DangerousGetHandle(), FieldOffsetEventHandle, value); }
        }

        /// <summary>
        /// Pass this into the DeviceIoControl and GetOverlappedResult APIs
        /// </summary>
        public IntPtr GlobalOverlapped
        {
            get { return DangerousGetHandle(); }
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
                SetHandleAsInvalid();
            }
            return true;
        }

        /// <summary>
        /// Sets the overlapped event handle and zeros the structure.
        /// </summary>
        /// <param name="hEventOverlapped"></param>
        public void Init(IntPtr hEventOverlapped)
        {
            EventHandle = hEventOverlapped;
            InternalLow = IntPtr.Zero;
            InternalHigh = IntPtr.Zero;
            OffsetLow = 0;
            OffsetHigh = 0;
        }
    }
}
