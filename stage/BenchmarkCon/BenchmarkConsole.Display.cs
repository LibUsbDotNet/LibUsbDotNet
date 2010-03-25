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
    internal partial class BenchmarkConsole
    {
        private static void showResults()
        {
            Console.WriteLine();
            Console.WriteLine("Benchmark Results:");
            showTestInfo();
            EndpointRunningStatus status;

            if (mTestMode == UsbTestType.ReadFromDevice || mTestMode == UsbTestType.Loop)
                status = mStatusReader;
            else
                status = mStatusWriter;

            DateTime dtStart = status.StartDateTime;
            DateTime dtStop = status.StopDateTime;

            if (dtStart == DateTime.MinValue) dtStart = DateTime.Now;
            if (dtStop == DateTime.MinValue) dtStop = DateTime.Now;

            TimeSpan totalTime = dtStop - dtStart;

            Console.WriteLine("\tTotal Packets   : {0}", status.PacketsTotal);
            Console.WriteLine("\tElapsed Time    : {0}", totalTime);
            Console.WriteLine("\tBytes per second: {0:#,###,###.00}", Math.Round(status.BytesTotal/totalTime.TotalSeconds, 2));

            Console.WriteLine();
            Console.WriteLine("\tRead timeouts : {0}", mStatusReader.TimeoutCount);
            Console.WriteLine("\tWrite timeouts: {0}", mStatusWriter.TimeoutCount);

            Console.WriteLine("\tShort write packets (with bytes < {0}): {1}", mTestTransferSize, mStatusWriter.ShortPacketCount);
        }

        private static void showTestInfo()
        {
            Console.WriteLine();
            Console.WriteLine("\tVid / Pid       : 0x{0:X4} / 0x{1:X4}", mVid, mPid);
            Console.WriteLine("\tTest Mode       : {0}", mTestMode);
            Console.WriteLine("\tPriority        : {0}", mThreadPriority);
            Console.WriteLine("\tTransfer Size   : {0}", mTestTransferSize);
            Console.WriteLine("\tDriver Mode     : {0}", mDriverMode);
            Console.WriteLine("\tRetry Count     : {0}", mTimeoutRetryCount);
            Console.WriteLine("\tDisplay Refresh : {0} (ms)", mDisplayUpdateInterval);
            Console.WriteLine("\tTransfer Timeout: {0} (ms)", mTransferTimeout);
        }

        private static void showRunningStatus(EndpointRunningStatus endpointStatus)
        {
            ulong packets;
            double pps;
            double bps;

            if (endpointStatus.GetStatus(out packets, out pps, out bps))
            {
                // Display some packet status
                Console.WriteLine("packets/sec: {0,-9:##,###.00}  bytes/sec: {1,-12:#,###,###} packets: {2}", pps, bps, packets);
            }
            else
            {
                Console.WriteLine("Synchronizing {0}..", packets);
            }
        }
    }
}