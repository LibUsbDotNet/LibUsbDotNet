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
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Examples
{
    internal class ReadIsochronous
    {
        public static UsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x04d8, 0x0F02);

        #endregion

        private static readonly int NUM_OF_TEST_TRANSFERS = 30;
        private static readonly int ISO_NUMBER_OF_PACKETS = 128;
        private static readonly int ISO_PACKET_SIZE = 64;
        private static int transferCount = 0;

        public static void Main(string[] args)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                UsbRegDeviceList regList = UsbDevice.AllDevices.FindAll(MyUsbFinder);
                if (regList.Count == 0) throw new Exception("Device Not Found.");

                regList[regList.Count - 2].Open(out MyUsbDevice);
                // MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                UsbEndpointReader reader = null;
                foreach (LibUsbDotNet.Info.UsbEndpointInfo endpointInfo in MyUsbDevice.Configs[0].InterfaceInfoList[0].EndpointInfoList)
                {
                    if ((endpointInfo.Descriptor.EndpointID & 0x80) != 0)
                    {
                        reader = MyUsbDevice.OpenEndpointReader(
                            (ReadEndpointID)endpointInfo.Descriptor.EndpointID, 
                            0, 
                            (EndpointType)(endpointInfo.Descriptor.Attributes & 0x3));
                        break;
                    }
                }

                if (ReferenceEquals(reader, null))
                {
                    throw new Exception("Failed locating read endpoint.");
                }
                reader.Reset();
                int readBufferSize = ISO_PACKET_SIZE * ISO_NUMBER_OF_PACKETS;


                byte[] bufferEven = new byte[readBufferSize];
                UsbTransfer transferEven = reader.NewAsyncTransfer();
                transferEven.Fill(bufferEven, 0, readBufferSize, 5000, ISO_PACKET_SIZE);

                byte[] bufferOdd = new byte[readBufferSize];
                UsbTransfer transferOdd = reader.NewAsyncTransfer();
                transferOdd.Fill(bufferOdd, 0, readBufferSize, 5000, ISO_PACKET_SIZE);

                byte[] bufferEnd = new byte[readBufferSize];
                UsbTransfer transferEnd = reader.NewAsyncTransfer();
                transferEnd.Fill(bufferEnd, 0, readBufferSize, 5000, ISO_PACKET_SIZE);

                ec = transferEven.Submit();
                if (ec != ErrorCode.Success) throw new Exception("Failed submitting transfer");
                Thread.Sleep(0);

                ec = transferOdd.Submit();
                if (ec != ErrorCode.Success) throw new Exception("Failed submitting transfer");
                Thread.Sleep(0);

                ec = transferEnd.Submit();
                if (ec != ErrorCode.Success) throw new Exception("Failed submitting transfer");
                Thread.Sleep(0);

                int transferred;
                UsbTransfer transferCurrent = transferEven;
                byte[] bufferCurrent = bufferEven;

                do
                {
                    int waitRet = WaitHandle.WaitAny(new WaitHandle[]
                                                         {
                                                             transferCurrent.AsyncWaitHandle
                                                         }, 5000, true);
                    if (waitRet == WaitHandle.WaitTimeout)
                        throw new Exception("Read time out");

                    ec = transferCurrent.Wait(out transferred);
                    if (ec != ErrorCode.Success)
                        throw new Exception("Failed getting async result");

                    showTransfer(transferCurrent, bufferCurrent, transferred, transferCount);

                    if (transferCount + 3 < NUM_OF_TEST_TRANSFERS)
                    {
                        transferCurrent.Reset();
                        ec = transferCurrent.Submit();
                        Thread.Sleep(0);

                        if (ec != ErrorCode.Success)
                            throw new Exception("Failed submitting transfer");
                    }
                    if (ReferenceEquals(transferCurrent, transferEven))
                    {
                        transferCurrent = transferOdd;
                        bufferCurrent = bufferOdd;
                    }
                    else if (ReferenceEquals(transferCurrent, transferOdd))
                    {
                        transferCurrent = transferEnd;
                        bufferCurrent = bufferEnd;
                    }
                    else if (ReferenceEquals(transferCurrent, transferEnd))
                    {
                        transferCurrent = transferEven;
                        bufferCurrent = bufferEven;
                    }
                } while (++transferCount < NUM_OF_TEST_TRANSFERS);
/*
                for (int i = 0; i < readBufferSize; i++)
                {
                    Console.Write("{0}, {1}", bufferEven[i], bufferOdd[i]);
                }
*/
                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();
                }

                // Wait for user input..
                Console.ReadKey();
            }
        }

        static DateTime dtLast = DateTime.MinValue;
        static double mTotalBytes = 0.0;
        static object oShowTransferLOCK=new object();
        private static void showTransfer(UsbTransfer transfer, byte[] bufferCurrent, int transferred, int transferIndex)
        {
            double bytesSec;
            double totalBytes;
            lock (oShowTransferLOCK)
            {
                if (dtLast == DateTime.MinValue)
                {
                    dtLast = DateTime.Now;
                    return;
                } 
                mTotalBytes += transferred;
                totalBytes = mTotalBytes;
            }
            bytesSec = totalBytes / (DateTime.Now - dtLast).TotalSeconds;
            Console.WriteLine("#{0} complete. {1} bytes/sec ({2} bytes total)", 
                transferIndex,
                Math.Round(bytesSec,2), 
                transferred);
        }
    }
}