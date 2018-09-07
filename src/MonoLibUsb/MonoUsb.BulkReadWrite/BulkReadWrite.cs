using System;
using System.Runtime.InteropServices;
using MonoLibUsb.Transfer;
using Usb = MonoLibUsb.MonoUsbApi;

namespace MonoLibUsb.ShowInfo
{
    internal enum TestMode
    {
        Sync,
        Async
    }

    internal class BulkReadWrite
    {
        #region DEVICE SETUP
        private const int MY_CONFIG = 1;
        private const byte MY_EP_READ = 0x81;
        private const byte MY_EP_WRITE = 0x01;
        private const int MY_INTERFACE = 0;
        private const short MY_PID = 0x0053;
        private const short MY_VID = 0x04d8;
        #endregion

        #region TEST SETUP
        private const int MY_TIMEOUT = 2000;
        private const int TEST_LOOP_COUNT = 1;

        private const int TEST_READ_LEN = 64;

        private const bool TEST_REST_DEVICE = true;
        private const int TEST_WRITE_LEN = 8;

        private static TestMode TEST_MODE = TestMode.Async;
        #endregion

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

        // This function originated from bulk_transfer_cb()
        // in sync.c of the Libusb-1.0 source code.
        private static void bulkTransferCB(MonoUsbTransfer transfer)
        {
            Marshal.WriteInt32(transfer.PtrUserData, 1);
            /* caller interprets results and frees transfer */
        }

        // This function originated from do_sync_bulk_transfer()
        // in sync.c of the Libusb-1.0 source code.
        private static MonoUsbError doBulkAsyncTransfer(MonoUsbDeviceHandle dev_handle,
                                                          byte endpoint,
                                                          byte[] buffer,
                                                          int length,
                                                          out int transferred,
                                                          int timeout)
        {
            transferred = 0;
            MonoUsbTransfer transfer = new MonoUsbTransfer(0);
            if (transfer.IsInvalid) return MonoUsbError.ErrorNoMem;

            MonoUsbTransferDelegate monoUsbTransferCallbackDelegate = bulkTransferCB;
            int[] userCompleted = new int[] {0};
            GCHandle gcUserCompleted = GCHandle.Alloc(userCompleted, GCHandleType.Pinned);

            MonoUsbError e;
            GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            transfer.FillBulk(
                dev_handle,
                endpoint,
                gcBuffer.AddrOfPinnedObject(),
                length,
                monoUsbTransferCallbackDelegate,
                gcUserCompleted.AddrOfPinnedObject(),
                timeout);

            e = transfer.Submit();
            if ((int) e < 0)
            {
                transfer.Free();
                gcUserCompleted.Free();
                return e;
            }
            int r;
            Console.WriteLine("Transfer Submitted..");
            while (userCompleted[0] == 0)
            {
                e = (MonoUsbError) (r = Usb.HandleEvents(sessionHandle));
                if (r < 0)
                {
                    if (e == MonoUsbError.ErrorInterrupted)
                        continue;
                    transfer.Cancel();
                    while (userCompleted[0] == 0)
                        if (Usb.HandleEvents(sessionHandle) < 0)
                            break;
                    transfer.Free();
                    gcUserCompleted.Free();
                    return e;
                }
            }

            transferred = transfer.ActualLength;
            e = MonoUsbApi.MonoLibUsbErrorFromTransferStatus(transfer.Status);
            transfer.Free();
            gcUserCompleted.Free();
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
                        device_handle = MonoUsbApi.OpenDeviceWithVidPid(sessionHandle, MY_VID, MY_PID);
                        if ((device_handle == null) || device_handle.IsInvalid) break;

                        // If TEST_REST_DEVICE = True, reset the device and re-open
                        if (TEST_REST_DEVICE)
                        {
                            MonoUsbApi.ResetDevice(device_handle);
                            device_handle.Close();
                            device_handle = MonoUsbApi.OpenDeviceWithVidPid(sessionHandle, MY_VID, MY_PID);
                            if ((device_handle == null) || device_handle.IsInvalid) break;
                        }

                        // Set configuration
                        Console.WriteLine("Set Config..");
                        r = MonoUsbApi.SetConfiguration(device_handle, MY_CONFIG);
                        if (r != 0) break;

                        // Claim interface
                        Console.WriteLine("Set Interface..");
                        r = MonoUsbApi.ClaimInterface(device_handle, MY_INTERFACE);
                        if (r != 0) break;
                        
                        /////////////////////
                        // Write test data //
                        /////////////////////
                        int packetCount = 0;
                        int transferredTotal = 0;
                        do
                        {
                            Console.WriteLine("Sending test data..");

                            // If the Async TEST_MODE enumeration is set, use
                            // the internal transfer function
                            if (TEST_MODE == TestMode.Async)
                            {
                                r = (int)doBulkAsyncTransfer(device_handle,
                                                                MY_EP_WRITE,
                                                                testWriteData,
                                                                TEST_WRITE_LEN,
                                                                out transferred,
                                                                MY_TIMEOUT);
                            }
                            else
                            {
                                // Use the sync bulk transfer API function 
                                r = MonoUsbApi.BulkTransfer(device_handle,
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
                            // Keep writing data until an error occurs or
                            // 4 packets have been sent.
                        } while (r == 0 && packetCount < 5);

                        
                        if (r == (int) MonoUsbError.ErrorTimeout)
                        {
                            // This is considered normal operation
                            Console.WriteLine("Write Timed Out. {0} packet(s) written ({1} bytes)", packetCount, transferredTotal);
                        }
                        else if (r != (int) MonoUsbError.ErrorTimeout && r != 0)
                        {
                            // An error, other than ErrorTimeout was received. 
                            Console.WriteLine("Write failed:{0}", (MonoUsbError) r);
                            break;
                        }
                        ////////////////////
                        // Read test data //
                        ////////////////////
                        Console.WriteLine("Reading test data..");
                        packetCount = 0;
                        transferredTotal = 0;
                        do
                        {
                            // If the Async TEST_MODE enumeration is set, use
                            // the internal transfer function
                            if (TEST_MODE == TestMode.Async)
                            {
                                r = (int) doBulkAsyncTransfer(device_handle,
                                                                MY_EP_READ,
                                                                testReadData,
                                                                TEST_READ_LEN,
                                                                out transferred,
                                                                MY_TIMEOUT);
                            }
                            else
                            {
                                // Use the sync bulk transfer API function 
                                r = MonoUsbApi.BulkTransfer(device_handle,
                                                                       MY_EP_READ,
                                                                       testReadData,
                                                                       TEST_READ_LEN,
                                                                       out transferred,
                                                                       MY_TIMEOUT);
                            }
                            if (r == (int) MonoUsbError.ErrorTimeout)
                            {
                                // This is considered normal operation
                                Console.WriteLine("Read Timed Out. {0} packet(s) read ({1} bytes)", packetCount, transferredTotal);
                            }
                            else if (r != 0)
                            {
                                // An error, other than ErrorTimeout was received. 
                                Console.WriteLine("Read failed:{0}", (MonoUsbError)r);
                            }
                            else
                            {
                                transferredTotal += transferred;
                                packetCount++;

                                // Display test data.
                                Console.Write("Received: ");
                                Console.WriteLine(System.Text.Encoding.Default.GetString(testReadData, 0, transferred));
                            }
                            // Keep reading data until an error occurs, (ErrorTimeout)
                        } while (r == 0);
                    } while (false);
                }
                finally
                {
                    // Free and close resources
                    if (device_handle != null)
                    {
                        if (!device_handle.IsInvalid)
                        {
                            MonoUsbApi.ReleaseInterface(device_handle, MY_INTERFACE);
                            device_handle.Close();
                        }
                    }
                    if (sessionHandle!=null)
                    {
                        sessionHandle.Close();
                        sessionHandle = null;
                    }
                }
                // Run the entire test TEST_LOOP_COUNT times.
            } while (++loopCount < TEST_LOOP_COUNT);

            Console.WriteLine("\nDone!  [Press any key to exit]");
            Console.ReadKey();

            return r;
        }
    }
}