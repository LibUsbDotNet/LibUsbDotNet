using System;
using System.Collections.Generic;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.MonoLibUsb;

namespace Examples
{
    internal class LibUsbWinBackReadWrite
    {
        public static MonoUsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x04d8, 0x0053);

        #endregion

        public static void Main(string[] args)
        {
            ErrorCode ec = ErrorCode.None;
            byte[] testWriteData = new byte[8];

            for (int i = 0; i < testWriteData.Length; i++)
                testWriteData[i] = (byte)(65 + (i & 0xf));

            try
            {
                // Find the usb device.
                List<MonoUsbDevice> deviceList = MonoUsbDevice.MonoUsbDeviceList;
                MyUsbDevice = deviceList.Find(MyUsbFinder.Check);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // Add a handler to the UsbErrorEvent for detailed error messages.
                UsbDevice.UsbErrorEvent += (OnUsbError);

                if (!MyUsbDevice.Open()) throw new Exception("Open Device Failed.");

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

                int transferred;
                int packetCount = 0;
                do
                {
                    ec = writer.Write(testWriteData, 1000, out transferred);
                    if (ec == ErrorCode.None) packetCount++;
                } while (ec == ErrorCode.None && packetCount < 2);
                if (ec != ErrorCode.None && ec != ErrorCode.IoTimedOut) throw new Exception(UsbDevice.LastErrorString);

                StringBuilder sbLoopData = new StringBuilder();
                byte[] readBuffer = new byte[1024];
                do
                {
                    // If the device hasn't sent data in the last 100 milliseconds,
                    // a timeout error (ec = IoTimedOut) will occur. 
                    ec = reader.Read(readBuffer, 1000, out transferred);

                    if (ec != ErrorCode.None) break;

                    sbLoopData.Append(Encoding.Default.GetString(readBuffer, 0, transferred));
                } while ((ec == ErrorCode.None));
                // Write that output to the console.
                Console.Clear();
                Console.Write("Loopback data: ");
                Console.WriteLine(sbLoopData.ToString());
                Console.WriteLine();
                Console.WriteLine();
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

                }
                MonoUsbDevice.Exit();
                // Wait for user input..
                
                Console.ReadKey();
            }
        }

        private static void OnUsbError(object sender, UsbError e)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.ToString());
            Console.ForegroundColor = temp;

        }
    }
}