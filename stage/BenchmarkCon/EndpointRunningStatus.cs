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

namespace LibUsbDotNet
{
    internal class EndpointRunningStatus
    {
        private readonly object mStatusLock = new object();

        private ulong mBytesDisplay;
        private ulong mBytesTotal;
        private DateTime mDisplayDateTime = DateTime.MinValue;
        private bool mEnabled = true;

        private ulong mPacketsDisplay;
        private ulong mPacketsTotal;
        private ulong mShortPacketCount;

        private DateTime mStartDateTime = DateTime.MinValue;
        private DateTime mStopDateTime = DateTime.MinValue;

        private ulong mTimeoutCount;

        public DateTime StopDateTime
        {
            get
            {
                lock (mStatusLock)
                {
                    return mStopDateTime;
                }
            }
        }

        public bool Enabled
        {
            get { return mEnabled; }
            set { lock (mStatusLock) mEnabled = value; }
        }

        public ulong TimeoutCount
        {
            get { return mTimeoutCount; }
            set { mTimeoutCount = value; }
        }

        public ulong BytesTotal
        {
            get { return mBytesTotal; }
            set { mBytesTotal = value; }
        }

        public ulong PacketsTotal
        {
            get { return mPacketsTotal; }
            set { mPacketsTotal = value; }
        }

        public DateTime StartDateTime
        {
            get { return mStartDateTime; }
            set { mStartDateTime = value; }
        }

        public ulong ShortPacketCount
        {
            get { return mShortPacketCount; }
            set { mShortPacketCount = value; }
        }

        public void AddPacket(int transferred)
        {
            if (transferred <= 0) return;
            if (!mEnabled) return;
            lock (mStatusLock)
            {
                if (mStartDateTime == DateTime.MinValue)
                {
                    mPacketsTotal++;
                    if (mPacketsTotal >= 6)
                    {
                        mPacketsTotal = 0;
                        mStartDateTime = DateTime.Now;
                    }
                    return;
                }

                if (mDisplayDateTime == DateTime.MinValue)
                    mDisplayDateTime = mStartDateTime;

                mPacketsTotal++;
                mPacketsDisplay++;
                mBytesTotal += (ulong) transferred;
                mBytesDisplay += (ulong) transferred;
            }
        }

        public bool GetStatus(out ulong packets, out double pps, out double bps)
        {
            lock (mStatusLock)
            {
                if (mDisplayDateTime != DateTime.MinValue)
                {
                    mStopDateTime = DateTime.Now;
                    Double elapsedSecs = (mStopDateTime - mDisplayDateTime).TotalSeconds;
                    if (elapsedSecs > 0)
                    {
                        mDisplayDateTime = mStopDateTime;

                        packets = mPacketsDisplay;
                        mPacketsDisplay = 0;

                        pps = packets/elapsedSecs;

                        bps = mBytesDisplay/elapsedSecs;
                        mBytesDisplay = 0;

                        return true;
                    }
                    packets = 0;
                }
                else
                {
                    packets = mPacketsTotal;
                }
            }
            pps = 0;
            bps = 0;
            return false;
        }
    }
}