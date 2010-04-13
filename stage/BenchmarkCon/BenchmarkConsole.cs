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
using System.Threading;
using System.Windows.Forms;
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
    internal partial class BenchmarkConsole
    {
        #region Benchmark Device Configuration

        private const int MY_CONFIG = 1;
        private const EndpointType MY_ENDPOINT_TYPE = EndpointType.Bulk;

        /// <summary>Custom vendor request implemented in the test firmware.</summary>
        /// <remarks>Gets the test type byte to the user allocated data buffer.</remarks>
        private static UsbSetupPacket mGetTestTypePacket =
            new UsbSetupPacket((byte) (UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte) PICFW_COMMANDS.GET_TEST,
                               0,
                               0,
                               1);


        /// <summary>Custom vendor request implemented in the test firmware.</summary>
        /// <remarks>
        /// Sets the test type using the value parameter of the setup packet.
        /// Gets the test type byte to the user allocated data buffer.
        /// </remarks>
        private static UsbSetupPacket mSetTestTypePacket =
            new UsbSetupPacket((byte) (UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte) PICFW_COMMANDS.SET_TEST,
                               (short) UsbTestType.None,
                               0,
                               1);

        #endregion

        #region User Configurable Fields and Defaults

        /// <summary>The display update interval (ms).</summary>
        private static int mDisplayUpdateInterval = 1000;

        private static UsbDevice.DriverModeType mDriverMode;

        /// <summary>The benchmark usb device vendor id.</summary>
        private static ushort mPid;

        private static bool mShowDeviceList;

        /// <summary>The type of test to run.</summary>
        private static UsbTestType mTestMode = UsbTestType.Loop;

        /// <summary>The transfer size used for reading and writing. This should not be greater than 65536.</summary>
        private static int mTestTransferSize = 4096;

        /// <summary>Priority for all usb threads (including handle events)</summary>
        private static ThreadPriority mThreadPriority = ThreadPriority.AboveNormal;

        /// <summary>If the sync read/write functions return a timeout error the transfer is tried this many times.</summary>
        private static int mTimeoutRetryCount = 1;

        /// <summary>Thee transfer read/write timeout.</summary>
        private static int mTransferTimeout = 5000;

        /// <summary>The benchmark usb device product id.</summary>
        private static ushort mVid = 0x04d8;

        private static byte mEndpointID = 0x01;
        private static byte mInterfaceID = 0x00;

        private static bool mNoTestSelect;

        #endregion

        #region Internal Benchmark Fields 

        private static readonly ManualResetEvent mCancelTestEvent = new ManualResetEvent(false);

        private static readonly EndpointRunningStatus mStatusReader = new EndpointRunningStatus();
        private static readonly EndpointRunningStatus mStatusWriter = new EndpointRunningStatus();

        private static UsbEndpointReader mReader;
        private static UsbDevice mUsbDevice;
        private static UsbEndpointWriter mWriter;

        private static Thread mReadThread;
        private static Thread mWriteThread;

        #endregion
        internal static ReadEndpointID ReadEndpoint
        {
            get
            {
                return (ReadEndpointID)(mEndpointID | 0x80);
            }
        }
        internal static WriteEndpointID WriteEndpoint
        {
            get
            {
                return (WriteEndpointID)(mEndpointID);
            }
        }
        protected static bool IsTestRunning
        {
            get
            {
                switch (mTestMode)
                {
                    case UsbTestType.ReadFromDevice:
                        return (mReadThread.IsAlive);
                    case UsbTestType.WriteToDevice:
                        return (mWriteThread.IsAlive);
                    case UsbTestType.Loop:
                        return (mReadThread.IsAlive && mWriteThread.IsAlive);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static bool NeedsReadThread
        {
            get { return (mTestMode == UsbTestType.Loop || mTestMode == UsbTestType.ReadFromDevice); }
        }

        private static bool NeedsWriteThread
        {
            get { return (mTestMode == UsbTestType.Loop || mTestMode == UsbTestType.WriteToDevice); }
        }

        private static void fillTestData(byte[] data, int len)
        {
            int i;
            for (i = 0; i < len; i++)
                data[i] = (byte) (65 + (i & 0xf));
        }


        /// <summary>
        /// Sends the control request to get the test mode
        /// </summary>
        private static bool GetTestType(out byte testType)
        {
            int transferred;
            byte[] dataBuffer = new byte[1];
            mGetTestTypePacket.Index = mInterfaceID;
            bool success = mUsbDevice.ControlTransfer(ref mGetTestTypePacket, dataBuffer, dataBuffer.Length, out transferred);
            testType = dataBuffer[0];

            return success;
        }

        /// <summary>
        /// Sends the control request to set the test mode
        /// </summary>
        private static bool SetTestType(ref UsbTestType testType)
        {
            int transferred;
            byte[] dataBuffer = new byte[1];

            mSetTestTypePacket.Value = (short) testType;
            mSetTestTypePacket.Index = mInterfaceID;

            bool success = mUsbDevice.ControlTransfer(ref mSetTestTypePacket, dataBuffer, dataBuffer.Length, out transferred);
            testType = (UsbTestType) dataBuffer[0];

            return success;
        }

        public static void Main(string[] args)
        {
            Console.Clear();

            try
            {
                if (args.Length == 0)
                {
                    ConWriteLine(ConType.Info, resBenchmark.ShowHelp);
                    return;
                }
                string argErrors;
                if (!parseArguments(args, out argErrors))
                    throw new BenchmarkArgumentException(argErrors);

                if (mNoTestSelect)
                    mTestMode = UsbTestType.Loop;

                Thread.CurrentThread.Priority = mThreadPriority;
                UsbRegistry deviceProfile = null;

                if (mPid == 0 || mShowDeviceList)
                {
                    showDeviceSelection(out deviceProfile);
                }
                else
                {
                    UsbDeviceFinder finder = new UsbDeviceFinder(mVid, mPid);
                    UsbRegDeviceList deviceProfiles = UsbDevice.AllDevices.FindAll(finder);

                    if (deviceProfiles.Count == 0) throw new BenchmarkException("Benchmark test device not connected.");

                    foreach (UsbRegistry availProfile in deviceProfiles)
                    {
                        if (availProfile is WinUsb.WinUsbRegistry)
                        {
                            WinUsb.WinUsbRegistry winUSBRegistry = (WinUsb.WinUsbRegistry) availProfile;
                            if (winUSBRegistry.InterfaceID != mInterfaceID) continue;
                        }
                        if (availProfile.Open(out mUsbDevice))
                        {
                            deviceProfile = availProfile;
                            break;
                        }
                    }
                }

                if (ReferenceEquals(deviceProfile, null)) throw new BenchmarkException("Benchmark test device could not be opened.");

                mDriverMode = mUsbDevice.DriverMode;

                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                IUsbDevice wholeUsbDevice = mUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    if (!wholeUsbDevice.SetConfiguration(MY_CONFIG))
                        throw new Exception(String.Format("Failed set configuration!\n{0}", UsbDevice.LastErrorString));

                    // Claim interface #0.
                    if (!wholeUsbDevice.ClaimInterface(mInterfaceID))
                        throw new Exception(String.Format("Failed claim interface!\n{0}", UsbDevice.LastErrorString));
                }
                if (!mNoTestSelect)
                {
                    byte testType;
                    if (!GetTestType(out testType))
                        throw new BenchmarkException("Failed getting test type, {0:X4}:{1:X4} doesn't appear to be a benchmark device.\n{2}",
                                                     mVid,
                                                     mPid,
                                                     UsbDevice.LastErrorString);

                    // Make sure the device is in loop mode.
                    if (testType != (byte) mTestMode)
                        if (!SetTestType(ref mTestMode))
                            throw new BenchmarkException("Failed setting test type, {0:X4}:{1:X4} may not be a benchmark device.\n{2}",
                                                         mVid,
                                                         mPid,
                                                         UsbDevice.LastErrorString);

                }
                showTestInfo(ConType.Info);
                showBenchmarkStartTest();

                // Create the read/write threads
                mReadThread = new Thread(ReadThreadFn);
                mWriteThread = new Thread(WriteThreadFn);

                mReadThread.Priority = mThreadPriority;
                mWriteThread.Priority = mThreadPriority;


                // Start the read/write threads
                if (NeedsReadThread)
                {
                    mReader = mUsbDevice.OpenEndpointReader(ReadEndpoint, mTestTransferSize, MY_ENDPOINT_TYPE);
                    mReadThread.Start();
                }
                if (NeedsWriteThread)
                {
                    mWriter = mUsbDevice.OpenEndpointWriter(WriteEndpoint, MY_ENDPOINT_TYPE);
                    mWriteThread.Start();
                }
                Thread.Sleep(10);

                // The threads will keep running until 'q' is pressed, 
                // the maximum timeout retry count is reached or another error 
                // code is returned by the bulk read/write sync functions.
                while (IsTestRunning)
                {
                    if (NeedsReadThread)
                        showRunningStatus(mStatusReader);
                    else
                        showRunningStatus(mStatusWriter);

                    // Make sure 'q' has not been pressed.
                    Application.DoEvents();
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey();
                        if (key.KeyChar.ToString().ToLower() == "q")
                        {
                            mCancelTestEvent.Set();
                            if (NeedsReadThread)
                                mReader.Abort();

                            if (NeedsWriteThread)
                                mWriter.Abort();

                            ConWriteLine(ConType.Info, "\nStopping Test..");
                            break;
                        }

                        if (key.KeyChar.ToString().ToLower() == "i")
                        {
                            showTestInfo(ConType.Info);
                        }
                        else if (key.KeyChar.ToString().ToLower() == "r")
                        {
                            showResults(ConType.Status);
                        }
                        while (Console.KeyAvailable) Console.ReadKey(true);
                    }
                    Thread.Sleep(mDisplayUpdateInterval);
                }
                mStatusReader.Enabled = false;
                mStatusWriter.Enabled = false;
                // When both threads have exited the test is over.
                ConWriteLine(ConType.Info, "\nWaiting for threads..");
                while (mReadThread.IsAlive || mWriteThread.IsAlive)
                {
                    ConWriteLine(ConType.Info, "\treading:{0}, writing:{1}", mReadThread.IsAlive, mWriteThread.IsAlive);
                    Thread.Sleep(1000);
                }
            }
            catch (BenchmarkArgumentException ex)
            {
                ConWriteLine(ConType.Info, resBenchmark.ShowHelp);
                ConWriteLine(ConType.Error, "\n\nARGUMENT ERROR!");
                ConWriteLine(ConType.Error, ex.Message);
            }
            catch (BenchmarkException ex)
            {
                ConWriteLine(ConType.Error, "\n\nBENCHMARK ERROR!");
                ConWriteLine(ConType.Error, ex.Message);
            }
            catch (Exception ex)
            {
                ConWriteLine(ConType.Error, "\n\nBENCHMARK EXCEPTION!");
                ConWriteLine(ConType.Error, ex.ToString());
            }
            finally
            {
                if ((mUsbDevice != null) && mUsbDevice.IsOpen)
                {
                    // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                    // it exposes an IUsbDevice interface. If not (WinUSB) the 
                    // 'wholeUsbDevice' variable will be null indicating this is 
                    // an interface of a device; it does not require or support 
                    // configuration and interface selection.
                    IUsbDevice wholeUsbDevice = mUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);
                    }

                    mUsbDevice.Close();

                    ConWriteLine(ConType.Info, "\nDone!\n");
                    showTestInfo(ConType.Status);
                    showResults(ConType.Status);
                }
                mUsbDevice = null;
                UsbDevice.Exit();


#if DEBUG
                Console.ReadKey();
#endif
            }
        }

        private static void ReadThreadFn()
        {
            byte[] dataBuffer = new byte[mTestTransferSize];
            int timeoutCount = 0;
            ErrorCode ec = ErrorCode.InvalidParam;
            do
            {
                int transferred;
                if (mCancelTestEvent.WaitOne(0, false)) break;
                ec = mReader.Read(dataBuffer, mTransferTimeout, out transferred);
                mStatusReader.AddPacket(transferred);
                Thread.Sleep(0);

                if (mTestMode == UsbTestType.Loop)
                {
                    // TODO: Verify Loop Data..
                }
                else if (mTestMode == UsbTestType.ReadFromDevice)
                {
                    // TODO: Verify Read Data..
                }

                if (ec == ErrorCode.Success)
                {
                    timeoutCount = 0;
                }
                if (ec == ErrorCode.IoTimedOut && !mCancelTestEvent.WaitOne(0, false))
                {
                    mStatusReader.TimeoutCount++;
                    if (timeoutCount < mTimeoutRetryCount)
                    {
                        ConWriteLine(ConType.Error, "ReadThreadFn   : BulkTransfer timeout count:{0}", timeoutCount);
                        ec = ErrorCode.Success;
                    }
                    timeoutCount++;
                }
            } while (ec == ErrorCode.Success && !mCancelTestEvent.WaitOne(0, false));

            // The thread is exiting
            if (ec != ErrorCode.Success && !mCancelTestEvent.WaitOne(0, false))
                ConWriteLine(ConType.Error, "ReadThreadFn   : BulkTransfer failed: {0}", ec);
            else
                ConWriteLine(ConType.Status, "ReadThreadFn   : Normal termination.");
        }

        private static void WriteThreadFn()
        {
            byte[] dataBuffer = new byte[mTestTransferSize];
            fillTestData(dataBuffer, dataBuffer.Length);
            ErrorCode ec = ErrorCode.InvalidParam;
            int timeoutCount = 0;

            do
            {
                int transferred;
                if (mCancelTestEvent.WaitOne(0, false)) break;
                ec = mWriter.Write(dataBuffer, mTransferTimeout, out transferred);
                mStatusWriter.AddPacket(transferred);

                Thread.Sleep(0);

                if (ec == ErrorCode.Success)
                {
                    timeoutCount = 0;
                }
                if (ec == ErrorCode.IoTimedOut && !mCancelTestEvent.WaitOne(0, false))
                {
                    mStatusWriter.TimeoutCount++;

                    if (timeoutCount < mTimeoutRetryCount)
                    {
                        ec = ErrorCode.Success;
                        ConWriteLine(ConType.Error, "WriteThreadFn  : BulkTransfer timeout count:{0}", timeoutCount);
                    }
                    timeoutCount++;
                }
                else if (ec == ErrorCode.Success && transferred != dataBuffer.Length && !mCancelTestEvent.WaitOne(0, false))
                {
                    mStatusWriter.ShortPacketCount++;
                    ConWriteLine(ConType.Error, "WriteThreadFn  : BulkTransfer short write ({0} bytes)", transferred);
                }
            } while (ec == ErrorCode.Success && !mCancelTestEvent.WaitOne(0, false));

            // The thread is exiting
            if (ec != ErrorCode.Success && !mCancelTestEvent.WaitOne(0, false))
                ConWriteLine(ConType.Error, "WriteThreadFn  : BulkTransfer failed: {0}", ec);
            else
                ConWriteLine(ConType.Status, "WriteThreadFn  : Normal termination.");
        }
    }

    internal class BenchmarkException : Exception
    {
        public BenchmarkException(string argErrors)
            : base(argErrors) { }

        public BenchmarkException(string format, params object[] args)
            : base(string.Format(format, args)) { }
    }

    internal class BenchmarkArgumentException : BenchmarkException
    {
        public BenchmarkArgumentException(string argErrors)
            : base(argErrors) { }

        public BenchmarkArgumentException(string format, params object[] args)
            : base(format, args) { }
    }
}