using System;
using System.Text;
using System.Text.RegularExpressions;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Examples
{
    internal class ReadWrite
    {
        public static UsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1234, 1);

        #endregion

        public static void Main(string[] args)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

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
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                // open write endpoint 1.
                UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                // Remove the exepath/startup filename text from the begining of the CommandLine.
                string cmdLine = Regex.Replace(
                    Environment.CommandLine, "^\".+?\"^.*? |^.*? ", "", RegexOptions.Singleline);

                if (!String.IsNullOrEmpty(cmdLine))
                {
                    int bytesWritten;
                    ec = writer.Write(Encoding.Default.GetBytes(cmdLine), 2000, out bytesWritten);
                    if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);

                    byte[] readBuffer = new byte[1024];
                    while (ec == ErrorCode.None)
                    {
                        int bytesRead;

                        // If the device hasn't sent data in the last 100 milliseconds,
                        // a timeout error (ec = IoTimedOut) will occur. 
                        ec = reader.Read(readBuffer, 100, out bytesRead);

                        if (bytesRead == 0) throw new Exception("No more bytes!");

                        // Write that output to the console.
                        Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));
                    }

                    Console.WriteLine("\r\nDone!\r\n");
                }
                else
                    throw new Exception("Nothing to do.");
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
    }
}