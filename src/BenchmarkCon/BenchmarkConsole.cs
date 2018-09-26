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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

// ReSharper disable InconsistentNaming

namespace LibUsbDotNet
{
    internal class BenchmarkConsole2
    {
        private const int MAX_OUTSTANDING_TRANSFERS = 10;
        private static readonly string[] EndpointTypeDisplayString = new string[] {"Control", "Isochronous", "Bulk", "Interrupt", null};
        private static readonly string[] TestDisplayString = new string[] {"None", "Read", "Write", "Loop", null};
        private static object DisplayCriticalSection = new object();

        // This is used only in VerifyData() for display information
        // about data validation mismatches.
        public static void CONVDAT(string format, params object[] args) { Console.Write("vdat:" + format, args); }

        public static void CONERR(string format, params object[] args) { Console.Write("err:" + format, args); }
        public static void CONMSG(string format, params object[] args) { Console.Write(format, args); }
        public static void CONWRN(string format, params object[] args) { Console.Write("wrn:" + format, args); }
        public static void CONDBG(string format, params object[] args) { Console.Write("dbg:" + format, args); }

        public static void CONERR0(string message) { CONERR("{0}", message); }
        public static void CONMSG0(string message) { CONMSG("{0}", message); }
        public static void CONWRN0(string message) { CONWRN("{0}", message); }
        public static void CONDBG0(string message) { CONDBG("{0}", message); }


        // Custom vendor requests that must be implemented in the benchmark firmware.
        // Test selection can be bypassed with the "notestselect" argument.
        //
        private enum BENCHMARK_DEVICE_COMMANDS
        {
            SET_TEST = 0x0E,
            GET_TEST = 0x0F,
        } ;

        // Tests supported by the official benchmark firmware.
        //
        [Flags]
        private enum BENCHMARK_DEVICE_TEST_TYPE
        {
            TestTypeNone = 0x00,
            TestTypeRead = 0x01,
            TestTypeWrite = 0x02,
            TestTypeLoop = TestTypeRead | TestTypeWrite,
        } ;

// This software was mainly created for testing the libusb-win32 kernel & user driver.
        private enum BENCHMARK_TRANSFER_MODE
        {
            // Tests for the libusb-win32 sync transfer function.
            TRANSFER_MODE_SYNC,

            // Test for async function, iso transfers, and queued transfers
            TRANSFER_MODE_ASYNC,
        } ;

// Holds all of the information about a test.
        private class BENCHMARK_TEST_PARAM
        {
            // User configurable value set from the command line.
            //
            public int BufferCount; // Number of outstanding asynchronous transfers
            public int BufferSize; // Number of bytes to transfer
            public UsbDevice Device;
            public int Ep; // Endpoint number (1-15)
            public int Intf; // Interface number
            public int Altf; // Alternate setting Interface number
            public bool IsCancelled;
            public int IsoPacketSize; // Isochronous packet size (defaults to the endpoints max packet size)
            public bool IsUserAborted;
            public bool NoTestSelect; // If true, don't send control message to select the test type.
            public int Pid; // Porduct ID
            public ThreadPriority Priority; // Priority to run this thread at.
            public int Refresh; // Refresh interval (ms)
            public int Retry; // Number for times to retry a timed out transfer before aborting
            public BENCHMARK_DEVICE_TEST_TYPE TestType; // The benchmark test type.
            public int Timeout; // Transfer timeout (ms)
            public BENCHMARK_TRANSFER_MODE TransferMode; // Sync or Async
            public bool UseList; // Show the user a device list and let them choose a benchmark device. 
            public bool Verify; // Only for loop and read test. If true, verifies data integrity. 

            // Internal value use during the test.
            //

            public byte[] VerifyBuffer; // Stores the verify test pattern for 1 packet.
            public int VerifyBufferSize; // Size of VerifyBuffer
            public bool VerifyDetails; // If true, prints detailed information for each invalid byte.
            public int Vid; // Vendor ID
        } ;

// The benchmark transfer context used for asynchronous transfers.  see TransferAsync().
        public class BENCHMARK_TRANSFER_HANDLE
        {
            public UsbTransfer Context;
            public byte[] Data;
            public int DataMaxLength;
            public bool InUse;
        } ;

// Holds all of the information about a transfer.
        private class BENCHMARK_TRANSFER_PARAM
        {
            public readonly BENCHMARK_TRANSFER_HANDLE[] TransferHandles = new BENCHMARK_TRANSFER_HANDLE[MAX_OUTSTANDING_TRANSFERS];

            // Placeholder for end of structure; this is where the raw data for the
            // transfer buffer is allocated.
            //
            public byte[][] Buffer;
            public UsbEndpointBase Ep;
            public int IsoPacketSize;
            public bool IsRunning;
            public long LastStartTick;
            public long LastTick;

            public long LastTransferred;
            public int OutstandingTransferCount;

            public int Packets;
            public int RunningErrorCount;
            public int RunningTimeoutCount;

            public int ShortTransferCount;
            public long StartTick;
            public BENCHMARK_TEST_PARAM Test;

            public Thread ThreadHandle;
            public int TotalErrorCount;
            public int TotalTimeoutCount;
            public long TotalTransferred;

            public int TransferHandleNextIndex;
            public int TransferHandleWaitIndex;
        } ;


// Critical section for running status. 
        private static string TRANSFER_DISPLAY(BENCHMARK_TRANSFER_PARAM TransferParam, string ReadingString, string WritingString) { return ((TransferParam.Ep.EndpointInfo.EndpointAddress & 0x80) == 0x80 ? ReadingString : WritingString); }

        private static void INC_ROLL(ref int IncField, int RollOverValue)
        {
            if ((++IncField) >= RollOverValue)
                IncField = 0;
        }

        private static byte ENDPOINT_TYPE(BENCHMARK_TRANSFER_PARAM TransferParam) { return (byte) (TransferParam.Ep.EndpointInfo.Attributes & 3); }

        private static void SetTestDefaults(BENCHMARK_TEST_PARAM test)
        {
            test.Ep = 0x00;
            test.Vid = 1234;
            test.Pid = 5678;
            test.Refresh = 1000;
            test.Timeout = 5000;
            test.TestType = BENCHMARK_DEVICE_TEST_TYPE.TestTypeLoop;
            test.BufferSize = 4096;
            test.BufferCount = 1;
            test.Priority = ThreadPriority.Normal;
        }

        private static UsbInterfaceInfo usb_find_interface(UsbConfigInfo config_descriptor, int interface_number, int alt_interface_number, out UsbInterfaceInfo first_interface)
        {
            first_interface = null;

            if (ReferenceEquals(config_descriptor, null)) return null;
            ReadOnlyCollection<UsbInterfaceInfo> interfaces = config_descriptor.Interfaces;
            for (int intfIndex = 0; intfIndex < interfaces.Count; intfIndex++)
            {
                if (ReferenceEquals(first_interface, null))
                    first_interface = interfaces[intfIndex];
                if (interfaces[intfIndex].Number == interface_number &&
                    (alt_interface_number==-1 || interfaces[intfIndex].AlternateSetting == alt_interface_number))
                {
                    return interfaces[intfIndex];
                }
            }

            return null;
        }
        private static UsbDevice Bench_Open(BENCHMARK_TEST_PARAM test)
        {
            UsbDevice foundDevice;
            using (UsbContext context = new UsbContext())
            {
                var devices = context.List();

                foreach (var usbRegDevice in devices)
                {
                    if (usbRegDevice.Info.VendorId == test.Vid && usbRegDevice.Info.ProductId == test.Pid)
                    {
                        if (usbRegDevice.TryOpen())
                        {
                            if (usbRegDevice.Info.NumConfigurations > 0)
                            {
                                UsbInterfaceInfo firstInterface;
                                UsbInterfaceInfo foundInterface = usb_find_interface(usbRegDevice.Configs[0], test.Intf, test.Altf, out firstInterface);
                                if (!ReferenceEquals(foundInterface, null))
                                {
                                    return usbRegDevice.Clone();
                                }
                            }

                            usbRegDevice.Close();
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Sends the control request to get the test mode
        /// </summary>
        private int Bench_GetTestType(UsbDevice dev, out BENCHMARK_DEVICE_TEST_TYPE testType, int intf)
        {
            int transferred;
            byte[] dataBuffer = new byte[1];
            UsbSetupPacket getTestTypePacket =
                new UsbSetupPacket((byte) (UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                                   (byte) BENCHMARK_DEVICE_COMMANDS.GET_TEST,
                                   0,
                                   (short) intf,
                                   1);
            transferred = dev.ControlTransfer(getTestTypePacket, dataBuffer, 0, dataBuffer.Length);
            testType = (BENCHMARK_DEVICE_TEST_TYPE) dataBuffer[0];

            return transferred;
        }

        /// <summary>
        /// Sends the control request to set the test mode
        /// </summary>
        private static int Bench_SetTestType(UsbDevice dev, BENCHMARK_DEVICE_TEST_TYPE testType, int intf)
        {
            int transferred;
            byte[] dataBuffer = new byte[1];
            UsbSetupPacket setTestTypePacket =
                new UsbSetupPacket((byte) (UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                                   (byte)BENCHMARK_DEVICE_COMMANDS.SET_TEST,
                                   (short) testType,
                                   (short) intf,
                                   1);

            transferred = dev.ControlTransfer(setTestTypePacket, dataBuffer, 0, dataBuffer.Length);
            return transferred;
        }


        private static int VerifyData(BENCHMARK_TRANSFER_PARAM transferParam, byte[] data, int dataLength)
        {
            int verifyDataSize = transferParam.Test.VerifyBufferSize;
            byte[] verifyData = transferParam.Test.VerifyBuffer;
            byte keyC = 0;
            bool seedKey = true;
            int dataLeft = dataLength;
            int dataIndex = 0;
            int packetIndex = 0;
            int verifyIndex = 0;

            while (dataLeft > 1)
            {
                verifyDataSize = dataLeft > transferParam.Test.VerifyBufferSize ? transferParam.Test.VerifyBufferSize : dataLeft;

                if (seedKey)
                    keyC = data[dataIndex + 1];
                else
                {
                    if (data[dataIndex + 1] == 0)
                        keyC = 0;
                    else
                    {
                        keyC++;
                    }
                }
                seedKey = false;
                // Index 0 is always 0.
                // The key is always at index 1
                verifyData[1] = keyC;
                if (memcmp(data, dataIndex, verifyData, verifyDataSize) != 0)
                {
                    // Packet verification failed.

                    // Reset the key byte on the next packet.
                    seedKey = true;

                    CONVDAT("data mismatch packet-index={0} data-index={1}\n", packetIndex, dataIndex);

                    if (transferParam.Test.VerifyDetails)
                    {
                        for (verifyIndex = 0; verifyIndex < verifyDataSize; verifyIndex++)
                        {
                            if (verifyData[verifyIndex] == data[dataIndex + verifyIndex])
                                continue;

                            CONVDAT("packet-offset={0} expected {1:X2}h got {2:X2}h\n",
                                    verifyIndex,
                                    verifyData[verifyIndex],
                                    data[dataIndex + verifyIndex]);
                        }
                    }
                }

                // Move to the next packet.
                packetIndex++;
                dataLeft -= verifyDataSize;
                dataIndex += verifyDataSize;
            }

            return 0;
        }

        private static int memcmp(byte[] srcBytes, int srcStartOffset, byte[] compareBytes, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (srcBytes[i + srcStartOffset] != compareBytes[i])
                    return -1;
            }
            return 0;
        }

        private static Error TransferSync(BENCHMARK_TRANSFER_PARAM transferParam, out int transferred)
        {
            var handle = GCHandle.Alloc(transferParam.Buffer[0]);

            var ret = transferParam.Ep.Transfer(handle.AddrOfPinnedObject(),
                                             0,
                                             transferParam.Test.BufferSize,
                                             transferParam.Test.Timeout, out transferred);
            handle.Free();

            return ret;
        }

        private static Error TransferAsync(BENCHMARK_TRANSFER_PARAM transferParam, out BENCHMARK_TRANSFER_HANDLE handleRef, out int transferred)
        {
            BENCHMARK_TRANSFER_HANDLE handle;
            handleRef = null;
            Error ret = Error.Success;

            transferred = 0;
            // Submit transfers until the maximum number of outstanding transfer(s) is reached.
            while (transferParam.OutstandingTransferCount < transferParam.Test.BufferCount)
            {
                if (ReferenceEquals(transferParam.TransferHandles[transferParam.TransferHandleNextIndex],null))
                {
                    transferParam.TransferHandles[transferParam.TransferHandleNextIndex]=new BENCHMARK_TRANSFER_HANDLE();
                }
                // Get the next available benchmark transfer handle.
                handleRef = handle = transferParam.TransferHandles[transferParam.TransferHandleNextIndex];

                // If a libusb-win32 transfer context hasn't been setup for this benchmark transfer
                // handle, do it now.
                //
                if (ReferenceEquals(handle.Context, null))
                {
                    // Data buffer(s) are located at the end of the transfer param.
                    handle.Data = transferParam.Buffer[transferParam.TransferHandleNextIndex];
                    handle.DataMaxLength = transferParam.Test.BufferSize;

                    handle.Context = transferParam.Ep.NewAsyncTransfer();
                    handle.Context.Fill(handle.Data,
                                        0,
                                        handle.DataMaxLength,
                                        transferParam.Test.Timeout,
                                        transferParam.IsoPacketSize > 0 ? transferParam.IsoPacketSize : transferParam.Ep.EndpointInfo.MaxPacketSize);
                }


                // Submit this transfer now.
                handle.Context.Reset();
                ret = handle.Context.Submit();
                if (ret != Error.Success) goto Done;

                // Mark this handle has InUse.
                handle.InUse = true;

                // When transfers ir successfully submitted, OutstandingTransferCount goes up; when
                // they are completed it goes down.
                //
                transferParam.OutstandingTransferCount++;

                // Move TransferHandleNextIndex to the next available transfer.
                INC_ROLL(ref transferParam.TransferHandleNextIndex, transferParam.Test.BufferCount);
            }

            // If the number of outstanding transfers has reached the limit, wait for the 
            // oldest outstanding transfer to complete.
            //
            if (transferParam.OutstandingTransferCount == transferParam.Test.BufferCount)
            {
                // TransferHandleWaitIndex is the index of the oldest outstanding transfer.
                handleRef = handle = transferParam.TransferHandles[transferParam.TransferHandleWaitIndex];
                ret = handle.Context.Wait(out transferred, false);
                if (ret != Error.Success)
                    goto Done;

                // Mark this handle has no longer InUse.
                handle.InUse = false;

                // When transfers ir successfully submitted, OutstandingTransferCount goes up; when
                // they are completed it goes down.
                //
                transferParam.OutstandingTransferCount--;

                // Move TransferHandleWaitIndex to the oldest outstanding transfer.
                INC_ROLL(ref transferParam.TransferHandleWaitIndex, transferParam.Test.BufferCount);
            }

            Done:
            return ret;
        }

        private static void TransferThreadProc(object state)
        {
            int transferred;
            byte[] data;
            int i;
            Error ret;
            BENCHMARK_TRANSFER_HANDLE handle;
            BENCHMARK_TRANSFER_PARAM transferParam = (BENCHMARK_TRANSFER_PARAM) state;

            transferParam.IsRunning = true;

            while (!transferParam.Test.IsCancelled)
            {
                data = null;
                handle = null;
                transferred = 0;

                if (transferParam.Test.TransferMode == BENCHMARK_TRANSFER_MODE.TRANSFER_MODE_SYNC)
                {
                    ret = TransferSync(transferParam, out transferred);
                    data = transferParam.Buffer[0];
                }
                else if (transferParam.Test.TransferMode == BENCHMARK_TRANSFER_MODE.TRANSFER_MODE_ASYNC)
                {
                    ret = TransferAsync(transferParam, out handle, out transferred);
                    if (!ReferenceEquals(handle, null)) data = handle.Data;
                }
                else
                {
                    CONERR("invalid transfer mode {0}\n", transferParam.Test.TransferMode);
                    goto Done;
                }
                if (ret != Error.Success)
                {
                    // The user pressed 'Q'.
                    if (transferParam.Test.IsUserAborted) break;

                    // Transfer timed out
                    if (ret == Error.Timeout)
                    {
                        transferParam.TotalTimeoutCount++;
                        transferParam.RunningTimeoutCount++;
                        CONWRN("Timeout #{0} {1} on Ep{2:X2}h..\n",
                               transferParam.RunningTimeoutCount,
                               TRANSFER_DISPLAY(transferParam, "reading", "writing"),
                               transferParam.Ep.EndpointInfo.EndpointAddress);

                        if (transferParam.RunningTimeoutCount > transferParam.Test.Retry)
                            break;
                    }
                    else
                    {
                        // An error (other than a timeout) occured.

                        // usb_strerror()is not thread safe and should not be used
                        // in a multi-threaded app.  It's used here because
                        // this is a test program.
                        //

                        transferParam.TotalErrorCount++;
                        transferParam.RunningErrorCount++;
                        CONERR("failed {0}! {1} of {2} ret={3}: {4}\n",
                               TRANSFER_DISPLAY(transferParam, "reading", "writing"),
                               transferParam.RunningErrorCount,
                               transferParam.Test.Retry + 1,
                               ret,
                               null);

                        transferParam.Ep.Reset();

                        if (transferParam.RunningErrorCount > transferParam.Test.Retry)
                            break;
                    }
                    ret = 0;
                }
                else
                {
                    if (transferred < transferParam.Test.BufferSize && !transferParam.Test.IsCancelled)
                    {
                        if (transferred > 0)
                        {
                            transferParam.ShortTransferCount++;
                            CONWRN("Short transfer on Ep{0:X2}h expected {1} got {2}.\n",
                                   transferParam.Ep.EndpointInfo.EndpointAddress,
                                   transferParam.Test.BufferSize,
                                   transferred);
                        }
                        else
                        {
                            CONWRN("Zero-length transfer on Ep{0:X2}h expected {1}.\n",
                                   transferParam.Ep.EndpointInfo.EndpointAddress,
                                   transferParam.Test.BufferSize);

                            transferParam.TotalErrorCount++;
                            transferParam.RunningErrorCount++;
                            if (transferParam.RunningErrorCount > transferParam.Test.Retry)
                                break;
                        }
                    }
                    else
                    {
                        transferParam.RunningErrorCount = 0;
                        transferParam.RunningTimeoutCount = 0;
                    }

                    if ((transferParam.Test.Verify) &&
                        (transferParam.Ep.EndpointInfo.EndpointAddress & 0x80) != 0)
                    {
                        VerifyData(transferParam, data, transferred);
                    }
                }

                lock (DisplayCriticalSection)
                {
                    if (transferParam.StartTick == 0 && transferParam.Packets >= 0)
                    {
                        transferParam.StartTick = DateTime.Now.Ticks;
                        transferParam.LastStartTick = transferParam.StartTick;
                        transferParam.LastTick = transferParam.StartTick;

                        transferParam.LastTransferred = 0;
                        transferParam.TotalTransferred = 0;
                        transferParam.Packets = 0;
                    }
                    else
                    {
                        if (transferParam.LastStartTick == 0)
                        {
                            transferParam.LastStartTick = transferParam.LastTick;
                            transferParam.LastTransferred = 0;
                        }
                        transferParam.LastTick = DateTime.Now.Ticks;

                        transferParam.LastTransferred += transferred;
                        transferParam.TotalTransferred += transferred;
                        transferParam.Packets++;
                    }
                }
            }

            Done:

            for (i = 0; i < transferParam.Test.BufferCount; i++)
            {
                if (!ReferenceEquals(transferParam.TransferHandles[i], null))
                {
                    if (transferParam.TransferHandles[i].InUse)
                    {
                        if (!transferParam.TransferHandles[i].Context.IsCompleted)
                        {
                            transferParam.Ep.Abort();
                            Thread.Sleep(1);
                        }
                        transferParam.TransferHandles[i].InUse = false;
                        transferParam.TransferHandles[i].Context.Dispose();
                    }
                }
            }

            transferParam.IsRunning = false;
            return;
        }

        private static string GetParamStrValue(string src, string paramName)
        {
            if (src.StartsWith(paramName)) return src.Substring(paramName.Length);
            return null;
        }

        private static bool GetParamIntValue(string src, string paramName, ref int returnValue)
        {
            string value = GetParamStrValue(src, paramName);
            if (!String.IsNullOrEmpty(value))
            {
                NumberStyles style = NumberStyles.Integer;
                if (value.ToLower().StartsWith("0x"))
                {
                    value = value.Substring(2);
                    style = NumberStyles.HexNumber;
                }
                int testValue;
                if (int.TryParse(value, style, null, out testValue))
                {
                    returnValue = testValue;
                    return true;
                }
            }
            return false;
        }

        private static int ValidateBenchmarkArgs(BENCHMARK_TEST_PARAM testParam)
        {
            if (testParam.BufferCount < 1 || testParam.BufferCount > MAX_OUTSTANDING_TRANSFERS)
            {
                CONERR("Invalid BufferCount argument {0}. BufferCount must be greater than 0 and less than or equal to {1}.\n",
                       testParam.BufferCount,
                       MAX_OUTSTANDING_TRANSFERS);
                return -1;
            }

            return 0;
        }

        private static int ParseBenchmarkArgs(BENCHMARK_TEST_PARAM testParams, int argc, string[] argv)
        {
            string arg;
            string value;
            int iarg;

            for (iarg = 0; iarg < argc; iarg++)
            {
                arg = argv[iarg].ToLower();

                if (GetParamIntValue(arg, "vid=", ref testParams.Vid))
                {
                }
                else if (GetParamIntValue(arg, "pid=", ref testParams.Pid))
                {
                }
                else if (GetParamIntValue(arg, "retry=", ref testParams.Retry))
                {
                }
                else if (GetParamIntValue(arg, "buffercount=", ref testParams.BufferCount))
                {
                    if (testParams.BufferCount > 1)
                        testParams.TransferMode = BENCHMARK_TRANSFER_MODE.TRANSFER_MODE_ASYNC;
                }
                else if (GetParamIntValue(arg, "buffersize=", ref testParams.BufferSize))
                {
                }
                else if (GetParamIntValue(arg, "size=", ref testParams.BufferSize))
                {
                }
                else if (GetParamIntValue(arg, "timeout=", ref testParams.Timeout))
                {
                }
                else if (GetParamIntValue(arg, "intf=", ref testParams.Intf))
                {
                }
                else if (GetParamIntValue(arg, "altf=", ref testParams.Altf))
                {
                }
                else if (GetParamIntValue(arg, "ep=", ref testParams.Ep))
                {
                    testParams.Ep &= 0xf;
                }
                else if (GetParamIntValue(arg, "refresh=", ref testParams.Refresh))
                {
                }
                else if ((value = GetParamStrValue(arg, "mode=")) != null)
                {
                    if (GetParamStrValue(value, "sync") != null)
                    {
                        testParams.TransferMode = BENCHMARK_TRANSFER_MODE.TRANSFER_MODE_SYNC;
                    }
                    else if (GetParamStrValue(value, "async") != null)
                    {
                        testParams.TransferMode = BENCHMARK_TRANSFER_MODE.TRANSFER_MODE_ASYNC;
                    }
                    else
                    {
                        // Invalid EndpointType argument.
                        CONERR("invalid transfer mode argument! {0}\n", argv[iarg]);
                        return -1;
                    }
                }
                else if ((value = GetParamStrValue(arg, "priority=")) != null)
                {
                    if (GetParamStrValue(value, "lowest") != null)
                    {
                        testParams.Priority = ThreadPriority.Lowest;
                    }
                    else if (GetParamStrValue(value, "belownormal") != null)
                    {
                        testParams.Priority = ThreadPriority.BelowNormal;
                    }
                    else if (GetParamStrValue(value, "normal") != null)
                    {
                        testParams.Priority = ThreadPriority.Normal;
                    }
                    else if (GetParamStrValue(value, "abovenormal") != null)
                    {
                        testParams.Priority = ThreadPriority.AboveNormal;
                    }
                    else if (GetParamStrValue(value, "highest") != null)
                    {
                        testParams.Priority = ThreadPriority.Highest;
                    }
                    else
                    {
                        CONERR("invalid priority argument! {0}\n", argv[iarg]);
                        return -1;
                    }
                }
                else if (GetParamIntValue(arg, "packetsize=", ref testParams.IsoPacketSize))
                {
                }
                else if (GetParamStrValue(arg, "notestselect") != null)
                {
                    testParams.NoTestSelect = true;
                }
                else if (GetParamStrValue(arg, "read") != null)
                {
                    testParams.TestType = BENCHMARK_DEVICE_TEST_TYPE.TestTypeRead;
                }
                else if (GetParamStrValue(arg, "write") != null)
                {
                    testParams.TestType = BENCHMARK_DEVICE_TEST_TYPE.TestTypeWrite;
                }
                else if (GetParamStrValue(arg, "loop") != null)
                {
                    testParams.TestType = BENCHMARK_DEVICE_TEST_TYPE.TestTypeLoop;
                }
                else if (GetParamStrValue(arg, "list") != null)
                {
                    testParams.UseList = true;
                }
                else if (GetParamStrValue(arg, "verifydetails") != null)
                {
                    testParams.VerifyDetails = true;
                    testParams.Verify = true;
                }
                else if (GetParamStrValue(arg, "verify") != null)
                {
                    testParams.Verify = true;
                }
                else
                {
                    CONERR("invalid argument! {0}\n", argv[iarg]);
                    return -1;
                }
            }
            return ValidateBenchmarkArgs(testParams);
        }

        private static int CreateVerifyBuffer(BENCHMARK_TEST_PARAM testParam, ushort endpointMaxPacketSize)
        {
            int i;
            byte indexC = 0;
            testParam.VerifyBuffer = new byte[endpointMaxPacketSize];

            testParam.VerifyBufferSize = endpointMaxPacketSize;

            for (i = 0; i < endpointMaxPacketSize; i++)
            {
                testParam.VerifyBuffer[i] = indexC++;
                if (indexC == 0) indexC = 1;
            }

            return 0;
        }

        private static void FreeTransferParam(ref BENCHMARK_TRANSFER_PARAM testTransferRef)
        {
            if (ReferenceEquals(testTransferRef, null)) return;

            if (testTransferRef.ThreadHandle != null)
            {
                testTransferRef.ThreadHandle = null;
            }

            testTransferRef = null;
        }

        private static BENCHMARK_TRANSFER_PARAM CreateTransferParam(BENCHMARK_TEST_PARAM test, int endpointID)
        {
            BENCHMARK_TRANSFER_PARAM transferParam;
            UsbInterfaceInfo testInterface;
            UsbInterfaceInfo firstInterface;
            int i;

            transferParam = new BENCHMARK_TRANSFER_PARAM();
            transferParam.Test = test;
            transferParam.Buffer = new byte[transferParam.Test.BufferCount][];
            for (i = 0; i < transferParam.Test.BufferCount; i++)
                transferParam.Buffer[i] = new byte[transferParam.Test.BufferSize];

RefetchAltInterface:
            if (ReferenceEquals((testInterface = usb_find_interface(test.Device.Configs[0], test.Intf, test.Altf, out firstInterface)), null))
            {
                CONERR("failed locating interface {0:X2}h!\n", test.Intf);
                FreeTransferParam(ref transferParam);
                goto Done;
            }

            for (i = 0; i < testInterface.Endpoints.Count ; i++)
            {
                if ((endpointID & 0x80) == 0x80)
                {
                    // Use first endpoint that matches the direction
                    if ((testInterface.Endpoints[i].EndpointAddress & 0x80) == 0x80)
                    {
                        transferParam.Ep = test.Device.OpenEndpointReader(
                            (ReadEndpointID) testInterface.Endpoints[i].EndpointAddress,
                            0,
                            (EndpointType) (testInterface.Endpoints[i].Attributes & 0x3));
                        break;
                    }
                }
                else
                {
                    if ((testInterface.Endpoints[i].EndpointAddress & 0x80) == 0x00)
                    {
                        transferParam.Ep = test.Device.OpenEndpointWriter(
                            (WriteEndpointID) testInterface.Endpoints[i].EndpointAddress,
                            (EndpointType) (testInterface.Endpoints[i].Attributes & 0x3));
                        break;
                    }
                }
            }
            if (ReferenceEquals(transferParam.Ep, null))
            {
                CONERR("failed locating EP{0:X2}h!\n", endpointID);
                FreeTransferParam(ref transferParam);
                goto Done;
            }

            if (transferParam.Ep.EndpointInfo.MaxPacketSize==0)
            {
                transferParam.Test.Altf++;
                goto RefetchAltInterface;
            }
            if ((transferParam.Test.BufferSize%transferParam.Ep.EndpointInfo.MaxPacketSize) > 0)
            {
                CONERR("buffer size {0} is not an interval of EP{1:X2}h maximum packet size of {2}!\n",
                       transferParam.Test.BufferSize,
                       transferParam.Ep.EndpointInfo.EndpointAddress,
                       transferParam.Ep.EndpointInfo.MaxPacketSize);

                FreeTransferParam(ref transferParam);
                goto Done;
            }

            if (test.IsoPacketSize != 0)
                transferParam.IsoPacketSize = test.IsoPacketSize;
            else
                transferParam.IsoPacketSize = transferParam.Ep.EndpointInfo.MaxPacketSize;

            if (ENDPOINT_TYPE(transferParam) == (byte) EndpointType.Isochronous)
                transferParam.Test.TransferMode = BENCHMARK_TRANSFER_MODE.TRANSFER_MODE_ASYNC;

            ResetRunningStatus(transferParam);

            transferParam.ThreadHandle = new Thread(TransferThreadProc);

            // If verify mode is on, this is a loop test, and this is a write endpoint, fill
            // the buffers with the same test data sent by a benchmark device when running
            // a read only test.
            if (transferParam.Test.Verify &&
                transferParam.Test.TestType == BENCHMARK_DEVICE_TEST_TYPE.TestTypeLoop &&
                (transferParam.Ep.EndpointInfo.EndpointAddress & 0x80) == 0)
            {
                // Data Format:
                // [0][KeyByte] 2 3 4 5 ..to.. wMaxPacketSize (if data byte rolls it is incremented to 1)
                // Increment KeyByte and repeat
                //
                byte indexC = 0;
                int bufferIndex = 0;
                int transferIndex = 0;
                UInt16 dataIndex;
                int packetIndex;
                int packetCount = ((transferParam.Test.BufferCount*transferParam.Test.BufferSize)/transferParam.Ep.EndpointInfo.MaxPacketSize);
                for (packetIndex = 0; packetIndex < packetCount; packetIndex++)
                {
                    indexC = 2;
                    for (dataIndex = 0; dataIndex < transferParam.Ep.EndpointInfo.MaxPacketSize; dataIndex++)
                    {
                        if (dataIndex == 0) // Start
                            transferParam.Buffer[transferIndex][bufferIndex] = 0;
                        else if (dataIndex == 1) // Key
                            transferParam.Buffer[transferIndex][bufferIndex] = (byte) (packetIndex & 0xFF);
                        else // Data
                            transferParam.Buffer[transferIndex][bufferIndex] = indexC++;

                        // if wMaxPacketSize is > 255, indexC resets to 1.
                        if (indexC == 0) indexC = 1;

                        bufferIndex++;
                        if (transferParam.Test.BufferSize == bufferIndex)
                        {
                            bufferIndex = 0;
                            transferIndex++;
                        }
                    }
                }
            }

            Done:
            if (ReferenceEquals(transferParam, null))
                CONERR0("failed creating transfer param!\n");

            return transferParam;
        }

        private static void GetAverageBytesSec(BENCHMARK_TRANSFER_PARAM transferParam, out double bps)
        {
            double ticksSec;
            if ((transferParam.StartTick == 0) ||
                (transferParam.StartTick >= transferParam.LastTick) ||
                transferParam.TotalTransferred == 0)
            {
                bps = 0;
            }
            else
            {
                ticksSec = (transferParam.LastTick - transferParam.StartTick)/10000000.0;
                bps = (transferParam.TotalTransferred/ticksSec);
            }
        }

        private static void GetCurrentBytesSec(BENCHMARK_TRANSFER_PARAM transferParam, out double bps)
        {
            double ticksSec;
            if ((transferParam.StartTick == 0) ||
                (transferParam.LastStartTick == 0) ||
                (transferParam.LastTick <= transferParam.LastStartTick) ||
                transferParam.LastTransferred == 0)
            {
                bps = 0;
            }
            else
            {
                ticksSec = (transferParam.LastTick - transferParam.LastStartTick)/10000000.0;
                bps = transferParam.LastTransferred/ticksSec;
            }
        }

        private static void ShowRunningStatus(BENCHMARK_TRANSFER_PARAM transferParam)
        {
            BENCHMARK_TRANSFER_PARAM temp = transferParam;
            double bpsOverall;
            double bpsLastTransfer;

            // LOCK the display critical section
            lock (DisplayCriticalSection)
            {
                // UNLOCK the display critical section

                if ((temp.StartTick == 0) || (temp.StartTick >= temp.LastTick))
                {
                    CONMSG("Synchronizing {0}..\n", Math.Abs(transferParam.Packets));
                }
                else
                {
                    GetAverageBytesSec(temp, out bpsOverall);
                    GetCurrentBytesSec(temp, out bpsLastTransfer);
                    transferParam.LastStartTick = 0;
                    CONMSG("Avg. Bytes/s: {0:F2} Transfers: {1} Bytes/s: {2:F2}\n",
                           bpsOverall,
                           temp.Packets,
                           bpsLastTransfer);
                }
            }
        }

        private static void ShowTransferInfo(BENCHMARK_TRANSFER_PARAM transferParam)
        {
            double bpsAverage;
            double bpsCurrent;
            double elapsedSeconds;

            if (ReferenceEquals(transferParam, null)) return;

            CONMSG("{0} {1} (Ep{2:X2}h) max packet size: {3}\n",
                   EndpointTypeDisplayString[ENDPOINT_TYPE(transferParam)],
                   TRANSFER_DISPLAY(transferParam, "Read", "Write"),
                   transferParam.Ep.EndpointInfo.EndpointAddress,
                   transferParam.Ep.EndpointInfo.MaxPacketSize);

            if (transferParam.StartTick != 0)
            {
                GetAverageBytesSec(transferParam, out bpsAverage);
                GetCurrentBytesSec(transferParam, out bpsCurrent);
                CONMSG("\tTotal Bytes     : {0}\n", transferParam.TotalTransferred);
                CONMSG("\tTotal Transfers : {0}\n", transferParam.Packets);

                if (transferParam.ShortTransferCount > 0)
                {
                    CONMSG("\tShort Transfers : {0}\n", transferParam.ShortTransferCount);
                }
                if (transferParam.TotalTimeoutCount > 0)
                {
                    CONMSG("\tTimeout Errors  : {0}\n", transferParam.TotalTimeoutCount);
                }
                if (transferParam.TotalErrorCount > 0)
                {
                    CONMSG("\tOther Errors    : {0}\n", transferParam.TotalErrorCount);
                }

                CONMSG("\tAvg. Bytes/sec  : {0:F2}\n", bpsAverage);

                if (transferParam.StartTick != 0 && transferParam.StartTick < transferParam.LastTick)
                {
                    elapsedSeconds = (transferParam.LastTick - transferParam.StartTick) / 10000000.0;

                    CONMSG("\tElapsed Time    : {0:F2} seconds\n", elapsedSeconds);
                }

                CONMSG0("\n");
            }
        }

        private static void ShowTestInfo(BENCHMARK_TEST_PARAM testParam)
        {
            if (ReferenceEquals(testParam, null)) return;

            CONMSG("{0} Test Information\n", TestDisplayString[(byte) testParam.TestType & 3]);
            CONMSG("\tVid / Pid       : {0:X4}h / {1:X4}h\n", testParam.Vid, testParam.Pid);
            CONMSG("\tInterface #     : {0:X2}h\n", testParam.Intf);
            CONMSG("\tAlt Interface # : {0:X2}h\n", testParam.Altf);
            CONMSG("\tPriority        : {0}\n", testParam.Priority);
            CONMSG("\tBuffer Size     : {0}\n", testParam.BufferSize);
            CONMSG("\tBuffer Count    : {0}\n", testParam.BufferCount);
            CONMSG("\tDisplay Refresh : {0} (ms)\n", testParam.Refresh);
            CONMSG("\tTransfer Timeout: {0} (ms)\n", testParam.Timeout);
            CONMSG("\tRetry Count     : {0}\n", testParam.Retry);
            CONMSG("\tVerify Data     : {0}{1}\n",
                   testParam.Verify ? "On" : "Off",
                   (testParam.Verify && testParam.VerifyDetails) ? " (Detailed)" : "");

            CONMSG0("\n");
        }

        private static void WaitForTestTransfer(BENCHMARK_TRANSFER_PARAM transferParam)
        {
            while (!ReferenceEquals(transferParam, null))
            {
                if (!transferParam.IsRunning)
                {
                    if (!transferParam.ThreadHandle.IsAlive)
                    {
                        CONMSG("stopped Ep{0:X2}h thread.\n",
                               transferParam.Ep.EndpointInfo.EndpointAddress);
                        break;
                    }
                }
                Thread.Sleep(100);
                CONMSG("waiting for Ep{0:X2}h thread..\n", transferParam.Ep.EndpointInfo.EndpointAddress);
            }
        }

        private static void ResetRunningStatus(BENCHMARK_TRANSFER_PARAM transferParam)
        {
            if (ReferenceEquals(transferParam, null)) return;

            transferParam.StartTick = 0;
            transferParam.TotalTransferred = 0;
            transferParam.Packets = -2;
            transferParam.LastTick = 0;
            transferParam.RunningTimeoutCount = 0;
        }


        private static int GetTestDeviceFromList(BENCHMARK_TEST_PARAM testParam)
        {
            using (UsbContext context = new UsbContext())
            {
                var allRegDevices = context.List();
                UsbInterfaceInfo firstInterface;

                int ret = -1;

                for (int i = 0; i < allRegDevices.Count; i++)
                {

                    {
                        var usbRegDevice = allRegDevices[i];

                        CONMSG("{0}. {1:X4}:{2:X4} {3}\n",
                               i + 1,
                               usbRegDevice.Info.VendorId,
                               usbRegDevice.Info.ProductId,
                               usbRegDevice.Info.Product);
                    }
                }

                if (allRegDevices.Count == 0)
                {
                    CONERR0("No devices where found!\n");
                    ret = -1;
                    goto Done;
                }

                CONMSG("\nSelect device (1-{0}) :", allRegDevices.Count);
                string userInputS = Console.ReadLine();
                int userInput;
                if (int.TryParse(userInputS, out userInput))
                    ret = 1;
                else
                    ret = -1;
                if (ret != 1 || userInput < 1)
                {
                    CONMSG0("\n");
                    CONMSG0("Aborting..\n");
                    ret = -1;
                    goto Done;
                }
                CONMSG0("\n");
                userInput--;
                if (userInput >= 0 && userInput < allRegDevices.Count)
                {
                    testParam.Device = allRegDevices[userInput].Clone();

                    if (!ReferenceEquals(testParam.Device, null))
                    {
                        testParam.Vid = testParam.Device.Info.VendorId;
                        testParam.Pid = testParam.Device.Info.ProductId;

                        if (usb_find_interface(testParam.Device.Configs[0], testParam.Intf, testParam.Altf, out firstInterface) == null)
                        {
                            // the specified (or default) interface didn't exist, use the first one.
                            if (firstInterface != null)
                            {
                                testParam.Intf = firstInterface.Number;
                            }
                            else
                            {
                                CONERR("device {0:X4}:{1:X4} does not have any interfaces!\n",
                                       testParam.Vid,
                                       testParam.Pid);
                                ret = -1;
                                goto Done;
                            }
                        }
                        ret = 0;
                    }
                }

                Done:
                return ret;
            }
        }

        private static int Main(string[] argv)
        {
            BENCHMARK_TEST_PARAM Test = new BENCHMARK_TEST_PARAM();
            BENCHMARK_TRANSFER_PARAM ReadTest = null;
            BENCHMARK_TRANSFER_PARAM WriteTest = null;


            if (argv.Length == 0)
            {
                ShowHelp();
                return -1;
            }

            ShowCopyright();

            SetTestDefaults(Test);

            // Load the command line arguments.
            if (ParseBenchmarkArgs(Test, argv.Length, argv) < 0)
                return -1;

            if (Test.UseList)
            {
                if (GetTestDeviceFromList(Test) < 0)
                    goto Done;
            }
            else
            {
                // Open a benchmark device. see Bench_Open().
                Test.Device = Bench_Open(Test);
            }
            if (Test.Device == null)
            {
                CONERR("device {0:X4}:{1:X4} not found!\n", Test.Vid, Test.Pid);
                goto Done;
            }

            // If "NoTestSelect" appears in the command line then don't send the control
            // messages for selecting the test type.
            //
            if (!Test.NoTestSelect)
            {
                if (Bench_SetTestType(Test.Device, Test.TestType, Test.Intf) != 1)
                {
                    CONERR("setting bechmark test type #{0}!\n{1}\n", Test.TestType, null);
                    goto Done;
                }
            }

            CONMSG("Benchmark device {0:X4}:{1:X4} opened..\n", Test.Vid, Test.Pid);

            // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
            // it exposes an IUsbDevice interface. If not (WinUSB) the 
            // 'wholeUsbDevice' variable will be null indicating this is 
            // an interface of a device; it does not require or support 
            // configuration and interface selection.
            IUsbDevice wholeUsbDevice = Test.Device as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                // This is a "whole" USB device. Before it can be used, 
                // the desired configuration and interface must be selected.

                // Select config #1
                wholeUsbDevice.SetConfiguration(1);
                // Claim interface #0.
                wholeUsbDevice.ClaimInterface(Test.Intf);
            }

            Test.Device.SetAltInterface(Test.Altf);

            // If reading from the device create the read transfer param. This will also create
            // a thread in a suspended state.
            //
            if ((Test.TestType & BENCHMARK_DEVICE_TEST_TYPE.TestTypeRead) != 0)
            {
                ReadTest = CreateTransferParam(Test, Test.Ep | 0x80);
                if (ReadTest == null) goto Done;
            }

            // If writing to the device create the write transfer param. This will also create
            // a thread in a suspended state.
            //
            if ((Test.TestType & BENCHMARK_DEVICE_TEST_TYPE.TestTypeWrite) != 0)
            {
                WriteTest = CreateTransferParam(Test, Test.Ep);
                if (WriteTest == null) goto Done;
            }

            if (Test.Verify)
            {
                if (ReadTest != null && WriteTest != null)
                {
                    if (CreateVerifyBuffer(Test, (ushort) WriteTest.Ep.EndpointInfo.MaxPacketSize) < 0)
                        goto Done;
                }
                else if (ReadTest != null)
                {
                    if (CreateVerifyBuffer(Test, (ushort) ReadTest.Ep.EndpointInfo.MaxPacketSize) < 0)
                        goto Done;
                }
                else
                {
                    // This is a write only test; nothing to do here.
                }
            }

            ShowTestInfo(Test);
            ShowTransferInfo(ReadTest);
            ShowTransferInfo(WriteTest);

            CONMSG0("\nWhile the test is running:\n");
            CONMSG0("Press 'Q' to quit\n");
            CONMSG0("Press 'T' for test details\n");
            CONMSG0("Press 'I' for status information\n");
            CONMSG0("Press 'R' to reset averages\n");
            CONMSG0("\nPress 'Q' to exit, any other key to begin..");
            char key = Console.ReadKey().KeyChar;
            CONMSG0("\n");

            if (key == 'Q' || key == 'q') goto Done;

            // Set the thread priority and start it.
            if (ReadTest != null)
            {
                ReadTest.ThreadHandle.Priority = Test.Priority;
                ReadTest.ThreadHandle.Start(ReadTest);
            }

            // Set the thread priority and start it.
            if (WriteTest != null)
            {
                WriteTest.ThreadHandle.Priority = Test.Priority;
                WriteTest.ThreadHandle.Start(WriteTest);
            }

            while (!Test.IsCancelled)
            {
                Thread.Sleep(Test.Refresh);

                if (Console.KeyAvailable)
                {
                    // A key was pressed.
                    key = Console.ReadKey().KeyChar;
                    switch (key)
                    {
                        case 'Q':
                        case 'q':
                            Test.IsUserAborted = true;
                            Test.IsCancelled = true;
                            break;
                        case 'T':
                        case 't':
                            ShowTestInfo(Test);
                            break;
                        case 'I':
                        case 'i':
                            // LOCK the display critical section
                            lock (DisplayCriticalSection)
                            {
                                // Print benchmark test details.
                                ShowTransferInfo(ReadTest);
                                ShowTransferInfo(WriteTest);
                            }

                            break;

                        case 'R':
                        case 'r':
                            // LOCK the display critical section
                            lock (DisplayCriticalSection)
                            {
                                // Reset the running status.
                                ResetRunningStatus(ReadTest);
                                ResetRunningStatus(WriteTest);

                                // UNLOCK the display critical section
                            }
                            break;
                    }

                    // Only one key at a time.
                    while (Console.KeyAvailable) Console.ReadKey(true);
                }

                // If the read test should be running and it isn't, cancel the test.
                if ((ReadTest != null) && !ReadTest.IsRunning)
                {
                    Test.IsCancelled = true;
                    break;
                }

                // If the write test should be running and it isn't, cancel the test.
                if ((WriteTest != null) && !WriteTest.IsRunning)
                {
                    Test.IsCancelled = true;
                    break;
                }

                // Print benchmark stats
                if (ReadTest != null)
                    ShowRunningStatus(ReadTest);
                else
                    ShowRunningStatus(WriteTest);
            }

            // Wait for the transfer threads to complete gracefully if it
            // can be done in 10ms. All of the code from this point to
            // WaitForTestTransfer() is not required.  It is here only to
            // improve response time when the test is cancelled.
            //
            Thread.Sleep(10);

            // If the thread is still running, abort and reset the endpoint.
            if ((ReadTest != null) && ReadTest.IsRunning)
                ReadTest.Ep.Abort();

            // If the thread is still running, abort and reset the endpoint.
            if ((WriteTest != null) && WriteTest.IsRunning)
                WriteTest.Ep.Abort();

            // Small delay incase usb_resetep() was called.
            Thread.Sleep(10);

            // WaitForTestTransfer will not return until the thread
            // has exited.
            WaitForTestTransfer(ReadTest);
            WaitForTestTransfer(WriteTest);

            // Print benchmark detailed stats
            ShowTestInfo(Test);
            if (ReadTest != null) ShowTransferInfo(ReadTest);
            if (WriteTest != null) ShowTransferInfo(WriteTest);


            Done:
            if (Test.Device != null)
            {
                Test.Device.Close();
                Test.Device = null;
            }

            FreeTransferParam(ref ReadTest);
            FreeTransferParam(ref WriteTest);

            CONMSG0("Press any key to exit..");
            Console.ReadKey();
            CONMSG0("\n");

            return 0;
        }

        private static void ShowHelp()
        {
            ShowCopyright();

            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.ToLower().EndsWith("benchmarkhelp.txt"))
                {
                    Stream helpTextStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    StreamReader helpTextStreamReader = new StreamReader(helpTextStream);
                    CONMSG("{0}", helpTextStreamReader.ReadToEnd());
                    helpTextStreamReader.Close();
                }
            }
        }
        public static string Version
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] assemblyKvp = assembly.FullName.Split(',');
                foreach (string s in assemblyKvp)
                {
                    string[] sKeyPair = s.Split('=');
                    if (sKeyPair[0].ToLower().Trim() == "version")
                    {
                        return sKeyPair[1].Trim();
                    }
                }
                return null;
            }
        }

        private static void ShowCopyright()
        {
            CONMSG("LibUsbDotNet USB Benchmark v{0}\n",Version);
            CONMSG0("Copyright (c) 2010 Travis Robinson. <libusbdotnet@gmail.com>\n");
            CONMSG0("website: http://sourceforge.net/projects/libusbdotnet\n");
        }
    }
}