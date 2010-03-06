using System;
using System.Collections.Generic;
using System.Text;

namespace Benchmark
{
    internal class StopWatch
    {
        private bool mbStarted=false;
        private DateTime mStartDateTime;
        private ulong mBytesRecv = 0;
        private ulong mPacketsRecv = 0;
        public ulong PacketErrorCount = 0;

        public bool IsStarted { get { return mbStarted; } }

        public ulong ByteCount
        {
            get
            {
                return mBytesRecv;
            }
        }
        public ulong PacketCount
        {
            get
            {
                return mPacketsRecv;
            }
        }

        public double BytesPerSecond
        {
            get
            {
                double rtn = mBytesRecv/DiffWithNow().TotalSeconds;
                return rtn;
            }
        }

        public void Reset()
        {
            mbStarted=false;
            mBytesRecv = 0;
            mPacketsRecv = 0;
            PacketErrorCount = 0;
        }
        public  void AddPacket(ulong byteCount)
        {
            mPacketsRecv++;
            mBytesRecv += byteCount;
        }
        public TimeSpan Diff(DateTime currentDateTime)
        {
            if (!mbStarted)
            {
                mbStarted=true;
                mStartDateTime=DateTime.Now;
            }

            return currentDateTime - mStartDateTime;
        }
        public TimeSpan DiffWithNow()
        {
            return Diff(DateTime.Now);
        }
    }
}
