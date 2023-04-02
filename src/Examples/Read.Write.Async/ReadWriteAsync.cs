using System;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.LibUsb;

namespace Examples;

internal class ReadWriteAsync
{
    public static IUsbDevice MyUsbDevice;

    #region SET YOUR USB Vendor and Product ID!

    public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1234, 1);

    #endregion

    public static async Task Main(string[] args)
    {
        Error ec = Error.Success;

        using (UsbContext context = new UsbContext())
        {
            try
            {
                // Find and open the usb device.
                MyUsbDevice = context.Find(MyUsbFinder);
                MyUsbDevice.Open();

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

                byte[] bytesToSend = Encoding.Default.GetBytes(testWriteString);
                byte[] readBuffer = new byte[1024];
                int testCount = 0;
                do
                {
                    // Create and submit transfer
                    var usbReadTransfer = reader.ReadAsync(readBuffer, 0, readBuffer.Length, 100);
                    var usbWriteTransfer = writer.WriteAsync(bytesToSend, 0, bytesToSend.Length, 100);
                        
                    // Await both transfers
                    var transfers = await Task.WhenAll(usbReadTransfer, usbWriteTransfer);
                        
                    // TODO: Cancellation not supported yet
                    // if (!usbWriteTransfer.IsCompleted) usbWriteTransfer.Cancel();
                    // if (!usbReadTransfer.IsCompleted) usbReadTransfer.Cancel();

                    Console.WriteLine("Read  :{0} Error:{1}", transfers[0].transferLength, transfers[0].error);
                    Console.WriteLine("Write :{0} Error:{1}", transfers[1].transferLength, transfers[1].error);
                    Console.WriteLine("Data  :" + Encoding.Default.GetString(readBuffer, 0, transfers[0].transferLength));
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