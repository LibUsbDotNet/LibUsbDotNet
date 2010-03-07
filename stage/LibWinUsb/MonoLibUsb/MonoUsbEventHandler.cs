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
using System;
using System.Threading;
using LibUsbDotNet.Main;

namespace MonoLibUsb
{
    /// <summary>
    /// Manages the Libusb-1.0 session context and a static "handle_events" thread for simplified asynchronous IO.
    /// </summary>
    public static class MonoUsbEventHandler
    {
        private static readonly ManualResetEvent mIsStoppedEvent = new ManualResetEvent(true);
        private static bool mRunning;
        private static MonoUsbSessionHandle mSessionHandle;
        internal static Thread mUsbEventThread;
        private static UnixNativeTimeval mWaitUnixNativeTimeval;

        /// <summary>
        /// Gets the <see cref="MonoUsbSessionHandle"/> that was initialized in <see cref="Init()"/> with <see cref="MonoLibUsbApi.Init"/>.
        /// </summary>
        /// <remarks>
        /// This structure is needed for members that require a <see cref="MonoUsbSessionHandle"/> parameter when using the <see cref="MonoUsbEventHandler"/>.
        /// </remarks>
        public static MonoUsbSessionHandle SessionHandle
        {
            get { return mSessionHandle; }
        }

        /// <summary>
        /// False if the handle_events thread is running and periodically calling <see cref="MonoLibUsbApi.HandleEventsTimeout"/>.
        /// </summary>
        public static bool IsStopped
        {
            get { return mIsStoppedEvent.WaitOne(0, false); }
        }

        /// <summary>
        /// Stops the handle_events thread.
        /// Exits the <see cref="SessionHandle"/> by calling <see cref="MonoLibUsbApi.Exit"/>.
        /// </summary>
        public static void Exit()
        {
            Stop(true);
            if (mSessionHandle == null) return;

            if (mSessionHandle.IsInvalid) return;
            mSessionHandle.Close();
            mSessionHandle = null;
        }

        private static void HandleEventFn(object oHandle)
        {
            mIsStoppedEvent.Reset();
            while (mRunning)
                MonoLibUsbApi.HandleEventsTimeout(mSessionHandle, ref mWaitUnixNativeTimeval);

            mIsStoppedEvent.Set();
        }


        /// <summary>
        /// Calls <see cref="MonoLibUsbApi.Init"/> and initialize "handle_events" thread. 
        /// </summary>
        /// <param name="tvSec">handle_events service interval seconds.</param>
        /// <param name="tvUsec">handle_events service interval milliseconds.</param>
        public static void Init(long tvSec, long tvUsec) { Init(new UnixNativeTimeval(tvSec, tvUsec)); }

        /// <summary>
        /// Calls <see cref="MonoLibUsbApi.Init"/> and initialize "handle_events" thread. 
        /// </summary>
        public static void Init() { Init(UnixNativeTimeval.Default); }

        private static void Init(UnixNativeTimeval unixNativeTimeval)
        {
            if (IsStopped && !mRunning && mSessionHandle==null)
            {
                mWaitUnixNativeTimeval = unixNativeTimeval;
                mSessionHandle=new MonoUsbSessionHandle();
                if (mSessionHandle.IsInvalid)
                {
                    mSessionHandle = null;
                    throw new UsbException(typeof (MonoLibUsbApi), String.Format("Init:libusb_init Failed:Invalid Session Handle"));
                }
            }
        }

        /// <summary>
        /// Starts the handle_events thread executing <see cref="MonoLibUsbApi.HandleEventsTimeout"/>.
        /// </summary>
        /// <returns>
        /// True if the thread is started.
        /// False if the thread is allready running.
        /// </returns>
        public static bool Start()
        {
            if (IsStopped && !mRunning && mSessionHandle!=null)
            {
                mRunning = true;
                mUsbEventThread = new Thread(HandleEventFn);
                mUsbEventThread.Priority = Helper.IsLinux ? ThreadPriority.Normal : ThreadPriority.Lowest;
                mUsbEventThread.Start(mSessionHandle);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops the handle_events thread.
        /// </summary>
        /// <remarks>
        /// <alert class="warning">Must be called before your application exits.</alert>
        /// </remarks>
        /// <param name="bWait">If true, wait for the thread to exit before returning.</param>
        public static void Stop(bool bWait)
        {
            if (!IsStopped && mRunning)
            {
                mRunning = false;

                if (bWait) mIsStoppedEvent.WaitOne(2100, false);
            }
        }
    }
}