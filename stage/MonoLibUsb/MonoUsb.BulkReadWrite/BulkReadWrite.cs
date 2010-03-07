using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;
using MonoLibUsb.Transfer;
using Usb = MonoLibUsb.MonoLibUsbApi;

namespace MonoLibUsb.ShowInfo
{
    internal enum TestMode
    {
        Sync,
        Async
    }

    internal class BulkReadWrite
    {
        private const int MY_CONFIG = 1;
        private const byte MY_EP_READ = 0x81;
        private const byte MY_EP_WRITE = 0x01;
        private const int MY_INTERFACE = 0;
        private const short MY_PID = 0x0053;
        private const int MY_TIMEOUT = 2000;
        private const short MY_VID = 0x04d8;
        private const int TEST_LOOP_COUNT = 1;

        private const int TEST_READ_LEN = 64;

        private const bool TEST_REST_DEVICE = true;
        private const int TEST_WRITE_LEN = 8;

        private static TestMode TEST_MODE = TestMode.Async;

        private static MonoUsbSessionHandle sessionHandle  = null;

        private static void fillTestData(byte[] data, int len)
        {
            int i;
            for (i = 0; i < len; i++)
                data[i] = (byte) (65 + (i & 0xf));
        }

        private static void memset(byte[] data, int value, int len)
        {
            int i;
            for (i = 0; i < len; i++)
                data[i] = (byte) (value);
        }

        private static void printHexByteString(byte[] data, int len)
        {
            int i;
            for (i = 0; i < len; i++)
                Console.Write("{0}", (char) data[i]);

            Console.WriteLine();
        }

        private static void bulk_transfer_cb(MonoUsbTransfer transfer)
        {
            Marshal.WriteInt32(transfer.PtrUserData, 1);
            /* caller interprets results and frees transfer */
        }

        private static MonoUsbError do_sync_bulk_transfer(MonoUsbDeviceHandle dev_handle,
                                                          byte endpoint,
                                                          byte[] buffer,
                                                          int length,
                                                          out int transferred,
                                                          int timeout,
                                                          byte type)
        {
            transferred = 0;
            MonoUsbTransfer transfer = new MonoUsbTransfer(0);
            if (transfer.IsInvalid) return MonoUsbError.LIBUSB_ERROR_NO_MEM;

            MonoUsbTransferDelegate monoUsbTransferCallbackDelegate = bulk_transfer_cb;
            int[] userCompleted = new int[] {0};
            GCHandle gcUserCompleted = GCHandle.Alloc(userCompleted, GCHandleType.Pinned);

            MonoUsbError e;
            GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            transfer.Fill(
                dev_handle,
                endpoint,
                gcBuffer.AddrOfPinnedObject(),
                length,
                monoUsbTransferCallbackDelegate,
                gcUserCompleted.AddrOfPinnedObject(),
                timeout,
                (EndpointType) type);

            e = transfer.Submit();
            if ((int) e < 0)
            {
                transfer.Free();
                return e;
            }
            int r;
            Console.WriteLine("Transfer Submitted..");
            while (userCompleted[0] == 0)
            {
                e = (MonoUsbError) (r = Usb.HandleEvents(sessionHandle));
                if (r < 0)
                {
                    if (e == MonoUsbError.LIBUSB_ERROR_INTERRUPTED)
                        continue;
                    transfer.Cancel();
                    while (userCompleted[0] == 0)
                        if (Usb.HandleEvents(sessionHandle) < 0)
                            break;
                    transfer.Free();
                    return e;
                }
            }

            transferred = transfer.ActualLength;
            e = MonoLibUsbApi.MonoLibUsbErrorFromTransferStatus(transfer.Status);
            ;
            return e;
        }

        public static int Main(string[] args)
        {
            MonoUsbDeviceHandle device_handle = null;

            int r = 0;
            int transferred;
            byte[] testWriteData = new byte[TEST_WRITE_LEN];
            byte[] testReadData = new byte[TEST_READ_LEN];

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "sync":
                        TEST_MODE = TestMode.Sync;
                        break;
                    case "async":
                        TEST_MODE = TestMode.Async;
                        break;
                }
            }

            fillTestData(testWriteData, TEST_WRITE_LEN);
            memset(testReadData, 0, TEST_READ_LEN);

            int loopCount = 0;
            do
            {
                try
                {
                    do
                    {
                        sessionHandle=new MonoUsbSessionHandle();
                        if (sessionHandle.IsInvalid) throw new Exception("Invalid session handle.");

                        Console.WriteLine("Opening Device..");
                        device_handle = MonoLibUsbApi.OpenDeviceWithVidPid(sessionHandle, MY_VID, MY_PID);
                        if ((device_handle == null) || device_handle.IsInvalid) break;
                        if (TEST_REST_DEVICE)
                        {
                            MonoLibUsbApi.ResetDevice(device_handle);
                            device_handle.Close();
                            device_handle = MonoLibUsbApi.OpenDeviceWithVidPid(sessionHandle, MY_VID, MY_PID);
                            if ((device_handle == null) || device_handle.IsInvalid) break;
                        }
                        Console.WriteLine("Set Config..");
                        r = MonoLibUsbApi.SetConfiguration(device_handle, MY_CONFIG);
                        if (r != 0) break;

                        Console.WriteLine("Set Interface..");
                        r = MonoLibUsbApi.ClaimInterface(device_handle, MY_INTERFACE);
                        if (r != 0) break;

                        // Write test data
                        int packetCount = 0;
                        int transferredTotal = 0;
                        do
                        {
                            Console.WriteLine("Sending test data..");
                            if (TEST_MODE == TestMode.Async)
                            {
                                r = (int) do_sync_bulk_transfer(device_handle,
                                                                MY_EP_WRITE,
                                                                testWriteData,
                                                                TEST_WRITE_LEN,
                                                                out transferred,
                                                                MY_TIMEOUT,
                                                                (byte) EndpointType.Bulk);
                            }
                            else
                            {
                                r = MonoLibUsbApi.BulkTransfer(device_handle,
                                                                       MY_EP_WRITE,
                                                                       testWriteData,
                                                                       TEST_WRITE_LEN,
                                                                       out transferred,
                                                                       MY_TIMEOUT);
                            }
                            if (r == 0)
                            {
                                packetCount++;
                                transferredTotal += transferred;
                            }
                        } while (r == 0 && packetCount < 5);

                        if (r == (int) MonoUsbError.LIBUSB_ERROR_TIMEOUT)
                        {
                            Console.WriteLine("Write Timed Out. {0} packet(s) written ({1} bytes)", packetCount, transferredTotal);
                        }
                        else if (r != (int) MonoUsbError.LIBUSB_ERROR_TIMEOUT && r != 0)
                        {
                            Console.WriteLine("Write failed:{0}", (MonoUsbError) r);
                            break;
                        }
                        // Read test data
                        Console.WriteLine("Reading test data..");
                        packetCount = 0;
                        transferredTotal = 0;
                        do
                        {
                            if (TEST_MODE == TestMode.Async)
                            {
                                r = (int) do_sync_bulk_transfer(device_handle,
                                                                MY_EP_READ,
                                                                testReadData,
                                                                TEST_READ_LEN,
                                                                out transferred,
                                                                MY_TIMEOUT,
                                                                (byte) EndpointType.Bulk);
                            }
                            else
                            {
                                r = MonoLibUsbApi.BulkTransfer(device_handle,
                                                                       MY_EP_READ,
                                                                       testReadData,
                                                                       TEST_READ_LEN,
                                                                       out transferred,
                                                                       MY_TIMEOUT);
                            }
                            if (r == (int) MonoUsbError.LIBUSB_ERROR_TIMEOUT)
                            {
                                Console.WriteLine("Read Timed Out. {0} packet(s) read ({1} bytes)", packetCount, transferredTotal);
                            }
                            else if (r != 0)
                                Console.WriteLine("Read failed:{0}", (MonoUsbError) r);
                            else
                            {
                                transferredTotal += transferred;
                                packetCount++;

                                // Display test data.
                                Console.Write("Received: ");
                                printHexByteString(testReadData, transferred);
                            }
                        } while (r == 0);
                    } while (false);
                }
                finally
                {
                    if (device_handle != null)
                    {
                        if (!device_handle.IsInvalid)
                        {
                            MonoLibUsbApi.ReleaseInterface(device_handle, MY_INTERFACE);
                            device_handle.Close();
                        }
                    }
                    if (sessionHandle!=null)
                    {
                        sessionHandle.Close();
                        sessionHandle = null;
                    }
                }
            } while (++loopCount < TEST_LOOP_COUNT);

            Console.WriteLine("\nDone!  [Press any key to exit]");
            Console.ReadKey();

            return r;
        }
    }
}