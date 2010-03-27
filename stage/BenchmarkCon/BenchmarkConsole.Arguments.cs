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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
    internal partial class BenchmarkConsole
    {
        private static Regex __cmdLineParser;

        internal static Regex CmdLineParser
        {
            get
            {
                if (ReferenceEquals(__cmdLineParser, null))
                    __cmdLineParser = new Regex(resBenchmark.CommandLineArgExp,
                                                RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                return __cmdLineParser;
            }
        }

        private static bool parseArguments(IEnumerable<string> args, out string argumentErrors)
        {
            StringBuilder sbArgErrors = new StringBuilder();

            foreach (string arg in args)
            {
                if (arg.ToLower().Trim() == "read")
                {
                    mTestMode = UsbTestType.ReadFromDevice;
                    continue;
                }
                if (arg.ToLower().Trim() == "list")
                {
                    mShowDeviceList = true;
                    continue;
                }
                if (arg.ToLower().Trim() == "write")
                {
                    mTestMode = UsbTestType.WriteToDevice;
                    continue;
                }
                if (arg.ToLower().Trim() == "loop")
                {
                    mTestMode = UsbTestType.Loop;
                    continue;
                }

                MatchCollection vidPidMatches = CmdLineParser.Matches(arg);
                foreach (Match match in vidPidMatches)
                {
                    if (match.Groups["Vid"].Success)
                        mVid = ushort.Parse(match.Groups["Vid"].Value, NumberStyles.HexNumber);
                    else if (match.Groups["Pid"].Success)
                        mPid = ushort.Parse(match.Groups["Pid"].Value, NumberStyles.HexNumber);
                    else if (match.Groups["Retry"].Success)
                        mTimeoutRetryCount = int.Parse(match.Groups["Retry"].Value);
                    else if (match.Groups["Size"].Success)
                        mTestTransferSize = int.Parse(match.Groups["Size"].Value);
                    else if (match.Groups["Timeout"].Success)
                        mTransferTimeout = int.Parse(match.Groups["Timeout"].Value);
                    else if (match.Groups["Priority"].Success)
                    {
                        string priority = match.Groups["Priority"].Value.ToLower();
                        Dictionary<string, int> threadEnums = Helper.GetEnumData(typeof (ThreadPriority));
                        foreach (KeyValuePair<string, int> threadEnum in threadEnums)
                        {
                            if (threadEnum.Key.ToLower() == priority)
                            {
                                mThreadPriority = (ThreadPriority) threadEnum.Value;
                                break;
                            }
                        }
                    }
                    else if (match.Groups["Refresh"].Success)
                    {
                        mDisplayUpdateInterval = int.Parse(match.Groups["Refresh"].Value);
                        if (mDisplayUpdateInterval < 10) mDisplayUpdateInterval = 10;
                        if (mDisplayUpdateInterval > 60000) mDisplayUpdateInterval = 60000;
                    }
                    else if (match.Groups["Driver"].Success)
                    {
                        switch (match.Groups["Driver"].Value.ToLower())
                        {
                            case "libusb10":
                                UsbDevice.ForceLibUsbWinBack = true;
                                break;
                            case "libusb-win32":
                            case "winusb":
                                UsbDevice.ForceLibUsbWinBack = false;
                                break;
                        }
                    }
                    else if (match.Groups["SwitchName"].Success)
                    {
                        if (match.Groups["SwitchValue"].Success)
                        {
                            sbArgErrors.AppendFormat("Invalid switch: {0} value: {1}\r\n",
                                                     match.Groups["SwitchName"].Value,
                                                     match.Groups["SwitchValue"].Value);
                        }
                        else
                            sbArgErrors.AppendFormat("Invalid argument: {0}\r\n", match.Groups["SwitchName"].Value);
                    }
                    else
                    {
                        throw new BenchmarkArgumentException("CommandLineArgs.txt is broke!");
                    }
                }
            }

            argumentErrors = sbArgErrors.ToString();

            return argumentErrors.Length == 0;
        }
    }
}