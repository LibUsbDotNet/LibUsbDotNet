using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Main;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.LibUsb;

namespace Examples
{
    internal class ReadWriteAsync
    {
        public static UsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1234, 1);

        #endregion

        public static void Main(string[] args)
        {
            Error ec = Error.Success;

            using (UsbContext context = new UsbContext())
            {
                try
                {
                    // Find and open the usb device.
                    MyUsbDevice = context.Find(MyUsbFinder);

                    // If the device is open and ready
                    if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                    // If this is a "whole" usb device (libusb-win32, linux libusb)
                    // it will have an IUsbDevice interface. If not (WinUSB) the 
                    // variable will be null indicating this is an interface of a 
                    // device.
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
                    var reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                    // open write endpoint 1.
                    var writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                    // the write test data.
                    string testWriteString = "ABCDEFGH";

                    Error ecWrite;
                    Error ecRead;
                    int transferredOut;
                    int transferredIn;
                    UsbTransfer usbWriteTransfer;
                    UsbTransfer usbReadTransfer;
                    byte[] bytesToSend = Encoding.Default.GetBytes(testWriteString);
                    byte[] readBuffer = new byte[1024];
                    int testCount = 0;
                    do
                    {
                        // Create and submit transfer
                        ecRead = reader.SubmitAsyncTransfer(readBuffer, 0, readBuffer.Length, 100, out usbReadTransfer);
                        if (ecRead != Error.Success) throw new Exception("Submit Async Read Failed.");

                        ecWrite = writer.SubmitAsyncTransfer(bytesToSend, 0, bytesToSend.Length, 100, out usbWriteTransfer);
                        if (ecWrite != Error.Success)
                        {
                            usbReadTransfer.Dispose();
                            throw new Exception("Submit Async Write Failed.");
                        }

                        WaitHandle.WaitAll(new WaitHandle[] { usbWriteTransfer.AsyncWaitHandle, usbReadTransfer.AsyncWaitHandle }, 200, false);
                        if (!usbWriteTransfer.IsCompleted) usbWriteTransfer.Cancel();
                        if (!usbReadTransfer.IsCompleted) usbReadTransfer.Cancel();

                        ecWrite = usbWriteTransfer.Wait(out transferredOut);
                        ecRead = usbReadTransfer.Wait(out transferredIn);

                        usbWriteTransfer.Dispose();
                        usbReadTransfer.Dispose();

                        Console.WriteLine("Read  :{0} Error:{1}", transferredIn, ecRead);
                        Console.WriteLine("Write :{0} Error:{1}", transferredOut, ecWrite);
                        Console.WriteLine("Data  :" + Encoding.Default.GetString(readBuffer, 0, transferredIn));
                        testCount++;
                    } while (testCount < 5);
                    Console.WriteLine("\r\nDone!\r\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine((ec != Error.Success ? ec + ":" : String.Empty) + ex.Message);
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
                    }

                    // Wait for user input..
                    Console.ReadKey();
                }
            }
        }
    }
}