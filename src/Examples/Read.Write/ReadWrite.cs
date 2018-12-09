using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Examples
{
    internal class ReadWrite
    {
        public static IUsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1234, 1);

        #endregion

        public static void Main(string[] args)
        {
            var ec = Error.Success;

            using (var context = new UsbContext())
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
                    var wholeUsbDevice = MyUsbDevice as IUsbDevice;
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

                    // Remove the exepath/startup filename text from the begining of the CommandLine.
                    var cmdLine = Regex.Replace(
                        Environment.CommandLine, "^\".+?\"^.*? |^.*? ", "", RegexOptions.Singleline);

                    if (!string.IsNullOrEmpty(cmdLine))
                    {
                        ec = writer.Write(Encoding.Default.GetBytes(cmdLine), 2000, out var bytesWritten);
                        if (ec != Error.Success) throw new Exception($"The command line {cmdLine} failed with an error of {ec}.");

                        var readBuffer = new byte[1024];
                        while (ec == Error.Success)
                        {

                            // If the device hasn't sent data in the last 100 milliseconds,
                            // a timeout error (ec = IoTimedOut) will occur. 
                            ec = reader.Read(readBuffer, 100, out var bytesRead);

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
                    Console.WriteLine((ec != Error.Success ? ec + ":" : string.Empty) + ex.Message);
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
                            var wholeUsbDevice = MyUsbDevice as IUsbDevice;
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