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
        private const byte MY_EP_READ = 0x81;
        private const byte MY_EP_WRITE = 0x01;
        private const int MY_INTERFACE = 0;


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
        private static ushort mPid = 0x0000;

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

        #endregion

        #region Internal Benchmark Fields 

        private static readonly ManualResetEvent mCancelTestEvent = new ManualResetEvent(false);

        private static readonly EndpointRunningStatus mStatusReader = new EndpointRunningStatus();
        private static readonly EndpointRunningStatus mStatusWriter = new EndpointRunningStatus();

        private static UsbEndpointReader mReader;
        private static UsbDevice mUsbDevice;
        private static UsbEndpointWriter mWriter;

        private static Thread readThread;
        private static Thread writeThread;

        #endregion

        protected static bool IsTestRunning
        {
            get
            {
                switch (mTestMode)
                {
                    case UsbTestType.ReadFromDevice:
                        return (readThread.IsAlive);
                    case UsbTestType.WriteToDevice:
                        return (writeThread.IsAlive);
                    case UsbTestType.Loop:
                        return (readThread.IsAlive && writeThread.IsAlive);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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

            bool success = mUsbDevice.ControlTransfer(ref mSetTestTypePacket, dataBuffer, dataBuffer.Length, out transferred);
            testType = (UsbTestType) dataBuffer[0];

            return success;
        }

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine(resBenchmark.ShowHelp);
                    return;
                }
                string argErrors;
                if (!parseArguments(args, out argErrors))
                    throw new BenchmarkException(argErrors);

                if (mPid == 0)
                    throw new BenchmarkException("The benchmark device product id must be specified");

                Thread.CurrentThread.Priority = mThreadPriority;


                Console.Clear();

                UsbDeviceFinder finder = new UsbDeviceFinder(mVid, mPid);
                UsbRegistry deviceProfile = UsbDevice.AllDevices.Find(finder);
                if (deviceProfile == null) throw new BenchmarkException("Benchmark test device not connected.");

                if (!deviceProfile.Open(out mUsbDevice)) throw new BenchmarkException("Benchmark test device could not be opened.");

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
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                byte testType;
                if (!GetTestType(out testType))
                    throw new BenchmarkException("Failed getting test type, {0:X4}:{1:X4} doesn't appear to be a benchmark device.",
                                                 mVid,
                                                 mPid);

                // Make sure the device is in loop mode.
                if (testType != (byte) mTestMode)
                    if (!SetTestType(ref mTestMode))
                        throw new BenchmarkException("Failed setting test type, {0:X4}:{1:X4} may not be a benchmark device.",
                                                     mVid,
                                                     mPid);


                showTestInfo();
                Console.WriteLine();
                Console.WriteLine("Press 'q' at any time to stop the test or now to abort.");
                Console.Write("[Press any other key to begin]");
                if (Console.ReadKey().KeyChar.ToString().ToLower() == "q") return;
                Console.WriteLine();
                Console.WriteLine("Starting benchmark test..");

                // Create the read/write threads
                readThread = new Thread(ReadThreadFn);
                writeThread = new Thread(WriteThreadFn);

                readThread.Priority = mThreadPriority;
                writeThread.Priority = mThreadPriority;


                // Start the read/write threads
                if (mTestMode == UsbTestType.ReadFromDevice || mTestMode == UsbTestType.Loop)
                {
                    mReader = mUsbDevice.OpenEndpointReader((ReadEndpointID) MY_EP_READ);
                    readThread.Start();
                }
                if (mTestMode == UsbTestType.WriteToDevice || mTestMode == UsbTestType.Loop)
                {
                    mWriter = mUsbDevice.OpenEndpointWriter((WriteEndpointID) MY_EP_WRITE);
                    writeThread.Start();
                }
                Thread.Sleep(10);

                // The threads will keep running until 'q' is pressed, 
                // the maximum timeout retry count is reached or another error 
                // code is returned by the bulk read/write sync functions.
                while (IsTestRunning)
                {
                    if (mTestMode == UsbTestType.ReadFromDevice || mTestMode == UsbTestType.Loop)
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
                            Console.WriteLine("\nStopping Test..");
                            break;
                        }
                        if (key.KeyChar.ToString().ToLower() == "i")
                        {
                            showTestInfo();
                        }
                    }
                    Thread.Sleep(mDisplayUpdateInterval);
                }
                mStatusReader.Enabled = false;
                mStatusWriter.Enabled = false;
                // When both threads have exited the test is over.
                Console.WriteLine("\nWaiting for threads..");
                while (readThread.IsAlive || writeThread.IsAlive)
                {
                    Console.WriteLine("\treading:{0}, writing:{1}", readThread.IsAlive, writeThread.IsAlive);
                    Thread.Sleep(1000);
                }
            }
            catch (BenchmarkException ex)
            {
                if (ReferenceEquals(mUsbDevice, null))
                    Console.WriteLine(resBenchmark.ShowHelp);

                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

                    Console.WriteLine("\nDone!\n");
                    showResults();
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
            ErrorCode ec;
            do
            {
                int transferred;
                ec = mReader.Read(dataBuffer, mTransferTimeout, out transferred);
                mStatusReader.AddPacket(transferred);
                Thread.Sleep(0);

                if (ec == ErrorCode.Success)
                {
                    timeoutCount = 0;
                }
                if (ec == ErrorCode.IoTimedOut)
                {
                    mStatusReader.TimeoutCount++;
                    if (timeoutCount < mTimeoutRetryCount)
                    {
                        Console.WriteLine("ReadThreadFn  :BulkTransfer timeout count:{0}", timeoutCount);
                        ec = ErrorCode.Success;
                    }
                    timeoutCount++;
                }
            } while (ec == ErrorCode.Success && !mCancelTestEvent.WaitOne(0, false));

            // The thread is exiting
            if (ec != ErrorCode.Success)
                Console.WriteLine("ReadThreadFn  :BulkTransfer failed: {0}", ec);
            else
                Console.WriteLine("ReadThreadFn  : Normal termination.");
        }

        private static void WriteThreadFn()
        {
            byte[] dataBuffer = new byte[mTestTransferSize];
            fillTestData(dataBuffer, dataBuffer.Length);
            ErrorCode ec;
            int timeoutCount = 0;

            do
            {
                int transferred;
                ec = mWriter.Write(dataBuffer, mTransferTimeout, out transferred);
                mStatusWriter.AddPacket(transferred);

                Thread.Sleep(0);

                if (ec == ErrorCode.Success)
                {
                    timeoutCount = 0;
                }
                if (ec == ErrorCode.IoTimedOut)
                {
                    mStatusWriter.TimeoutCount++;

                    if (timeoutCount < mTimeoutRetryCount)
                    {
                        ec = ErrorCode.Success;
                        Console.WriteLine("WriteThreadFn : BulkTransfer timeout count:{0}", timeoutCount);
                    }
                    timeoutCount++;
                }
                else if (ec == ErrorCode.Success && transferred != dataBuffer.Length)
                {
                    mStatusWriter.ShortPacketCount++;
                    Console.WriteLine("WriteThreadFn : BulkTransfer short write ({0} bytes)", transferred);
                }
            } while (ec == ErrorCode.Success && !mCancelTestEvent.WaitOne(0, false));

            // The thread is exiting
            if (ec != ErrorCode.Success)
                Console.WriteLine("WriteThreadFn : BulkTransfer failed: {0}", ec);
            else
                Console.WriteLine("WriteThreadFn : Normal termination.");
        }
    }

    internal class BenchmarkException : Exception
    {
        public BenchmarkException(string argErrors)
            : base(argErrors) { }

        public BenchmarkException(string format, params object[] args)
            : base(string.Format(format, args)) { }
    }
}