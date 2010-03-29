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
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
    internal partial class BenchmarkConsole
    {
        private static void showResults(ConType conType)
        {
            ConWriteLine(conType);
            ConWriteLine(conType, "Benchmark Results:");
            EndpointRunningStatus status = NeedsReadThread ? mStatusReader : mStatusWriter;

            lock (mStatusReader.mStatusLock)
            {
                lock (mStatusWriter.mStatusLock)
                {
                    DateTime dtStart = status.StartDateTime;
                    DateTime dtStop = status.StopDateTime;

                    if (dtStart == DateTime.MinValue) dtStart = DateTime.Now;
                    if (dtStop == DateTime.MinValue) dtStop = DateTime.Now;

                    TimeSpan totalTime = dtStop - dtStart;

                    ConWriteLine(conType, "\tTotal Packets   : {0}", status.PacketsTotal);
                    ConWriteLine(conType, "\tElapsed Time    : {0}", totalTime);
                    ConWriteLine(conType, "\tBytes per second: {0:#,###,###.00}", Math.Round(status.BytesTotal/totalTime.TotalSeconds, 2));

                    ConWriteLine(conType);
                    ConWriteLine(conType, "\tRead timeouts : {0}", mStatusReader.TimeoutCount);
                    ConWriteLine(conType, "\tWrite timeouts: {0}", mStatusWriter.TimeoutCount);

                    ConWriteLine(conType, "\tShort write packets (with bytes < {0}): {1}", mTestTransferSize, mStatusWriter.ShortPacketCount);
                }
            }
        }

        private static void showTestInfo(ConType conType)
        {
            ConWriteLine(conType);
            ConWriteLine(conType, "Test Information:");
            ConWriteLine(conType, "\tVid / Pid       : 0x{0:X4} / 0x{1:X4}", mVid, mPid);
            ConWriteLine(conType, "\tTest Mode       : {0}", mTestMode);
            ConWriteLine(conType, "\tPriority        : {0}", mThreadPriority);
            ConWriteLine(conType, "\tTransfer Size   : {0}", mTestTransferSize);
            ConWriteLine(conType, "\tDriver Mode     : {0}", mDriverMode);
            ConWriteLine(conType, "\tRetry Count     : {0}", mTimeoutRetryCount);
            ConWriteLine(conType, "\tDisplay Refresh : {0} (ms)", mDisplayUpdateInterval);
            ConWriteLine(conType, "\tTransfer Timeout: {0} (ms)", mTransferTimeout);
        }

        private static void showRunningStatus(EndpointRunningStatus endpointStatus)
        {
            ulong packets;
            double pps;
            double bps;

            if (endpointStatus.GetStatus(out packets, out pps, out bps))
            {
                // Display some packet status
                ConWriteLine(ConType.Status, "packets/sec: {0,-9:##,###.00}  bytes/sec: {1,-12:#,###,###} packets: {2}", pps, bps, packets);
            }
            else
            {
                ConWriteLine(ConType.Info, "Synchronizing {0}..", packets);
            }
        }

        private static void showDeviceSelection(out UsbRegistry selectedProfile)
        {
            selectedProfile = null;
            UsbRegDeviceList deviceProfiles = UsbDevice.AllDevices;
            if (deviceProfiles.Count == 0) throw new BenchmarkException("No devices were detected.");

            ConWriteLine(ConType.Info);
            ConWriteLine(ConType.Info, "Select benchmark device from available devices:");
            ConWriteLine(ConType.Info);
            for (int i = 0; i < deviceProfiles.Count; i++)
            {
                ConWrite(ConType.Info, (i + 1) + ". ");
                ConWrite(ConType.Info,
                         "{0:X4}:{1:X4} {2} (rev. {3})",
                         (ushort) deviceProfiles[i].Vid,
                         (ushort) deviceProfiles[i].Pid,
                         deviceProfiles[i].Name,
                         deviceProfiles[i].Rev);
                ConWriteLine(ConType.Info);
            }
            ConWriteLine(ConType.Info);

            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;
            Console.BackgroundColor = fg;
            Console.ForegroundColor = bg;
            ConWrite(ConType.Info, "Select (1-{0}) :", deviceProfiles.Count);
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            string input = Console.ReadLine();
            ushort inputValue;
            if (!ushort.TryParse(input, out inputValue))
            {
                ConWriteLine(ConType.Info);
                ConWriteLine(ConType.Info, "Invalid device selection. Exiting..");
                return;
            }
            selectedProfile = deviceProfiles[inputValue - 1];
            if (!selectedProfile.Open(out mUsbDevice))
                selectedProfile = null;
            else
            {
                mPid = (ushort) mUsbDevice.Info.Descriptor.ProductID;
                mVid = (ushort) mUsbDevice.Info.Descriptor.VendorID;
            }
        }

        private static void showBenchmarkStartTest()
        {
            ConWriteLine(ConType.Info);
            ConWriteLine(ConType.Info, "Press 'q' at any time to stop the test or now to abort.");
            ConWriteLine(ConType.Info, "Press 'i' any time during the test to display test information.");
            ConWriteLine(ConType.Info, "Press 'r' any time during the test to display overall results.");
            ConWriteLine(ConType.Info);
            ConWrite(ConType.Info, "[Press any other key to begin]");
            while (Console.KeyAvailable) Console.ReadKey(true);
            if (Console.ReadKey().KeyChar.ToString().ToLower() == "q") return;
            ConWriteLine(ConType.Info);
            ConWriteLine(ConType.Info, "Starting benchmark test..");
        }

        private static void ConWriteLine(ConType conType) { Console.WriteLine(); }
        private static void ConWriteLine(ConType conType, string text) { Console.WriteLine(text); }
        private static void ConWriteLine(ConType conType, string text, params object[] args) { Console.WriteLine(text, args); }
        private static void ConWrite(ConType conType, string text) { Console.Write(text); }
        private static void ConWrite(ConType conType, string text, params object[] args) { Console.Write(text, args); }
    }

    internal enum ConType
    {
        Info,
        Status,
        Error,
    }
}