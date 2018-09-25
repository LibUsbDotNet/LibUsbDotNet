using System;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace Examples
{
    internal class ReadPolling
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
                    var reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);


                    byte[] readBuffer = new byte[1024];
                    while (ec == Error.Success)
                    {
                        int bytesRead;

                        // If the device hasn't sent data in the last 5 seconds,
                        // a timeout error (ec = IoTimedOut) will occur. 
                        ec = reader.Read(readBuffer, 5000, out bytesRead);

                        if (bytesRead == 0) throw new Exception(string.Format("{0}:No more bytes!", ec));
                        Console.WriteLine("{0} bytes read", bytesRead);

                        // Write that output to the console.
                        Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));
                    }

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