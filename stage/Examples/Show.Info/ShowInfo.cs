using System;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace Examples
{
    internal class ShowInfo
    {
        public static UsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x04d8, 0x0053);

        #endregion

        public static void Main(string[] args)
        {
            // First list all known devices.
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    Console.WriteLine("{0:X4}h:{1:X4}h {2} ({3})",
                        MyUsbDevice.Info.Descriptor.VendorID,
                        MyUsbDevice.Info.Descriptor.ProductID,
                        MyUsbDevice.Info.ProductString,
                        MyUsbDevice.Info.ManufacturerString);

                    MyUsbDevice.Close();
                }
            }

            // Check for a valid & connected usb device by vendor and product id.
            // (see MyUsbFinder above)
            Console.WriteLine("Finding your device..");
            UsbRegistry myUsbRegistry = allDevices.Find(MyUsbFinder);

            if (ReferenceEquals(myUsbRegistry, null))
            {
                // The device is not connected or cannot be accessed by libusbdotnet
                Console.WriteLine("Device not connected!");
                ShowLastUsbError();
                return;
            }

            // Display the usb device description from the registry.
            // This is the description that was set by the install inf. 
            Console.WriteLine("Found device {0}", myUsbRegistry[SPDRP.DeviceDesc]);

            // Display the usb devices DeviceInterfaceGuids (if any).
            // These are set in the devices install inf.
            Guid[] deviceInterfaceGuids = myUsbRegistry.DeviceInterfaceGuids;
            foreach (Guid deviceInterfaceGuid in deviceInterfaceGuids)
                Console.WriteLine("Device Interface Guid: {0}", deviceInterfaceGuid);

            // Open this usb device.
            if (!myUsbRegistry.Open(out MyUsbDevice))
            {
                // If using libusb-win32 this should not happen.
                // If using WinUSB or libusb-1.0 this will occur if the 
                // device is in-use.
                Console.WriteLine("Failed opening device!");
                ShowLastUsbError();
                return;
            }

            // Get the usb DeviceDescriptor information from the usb device.
            // Up until this point, we have just been querying the windows registry,
            // nothing had actually been sent or received from the usb device.
            UsbDeviceInfo myDeviceInfo = MyUsbDevice.Info;

            // Dump the UsbDeviceDesciptor to console output.
            Console.WriteLine(myDeviceInfo.Descriptor.ToString());

            // Display device manufacturer string (if one exists)
            if (myDeviceInfo.Descriptor.ManufacturerStringIndex != 0)
                Console.WriteLine("Manufacturer: {0}", myDeviceInfo.ManufacturerString);

            // Display device product string (if one exists)
            if (myDeviceInfo.Descriptor.ProductStringIndex != 0)
                Console.WriteLine("Product: {0}", myDeviceInfo.ProductString);

            // Display device serial number string (if one exists)
            if (myDeviceInfo.Descriptor.SerialStringIndex != 0)
                Console.WriteLine("Serial Number: {0}", myDeviceInfo.SerialString);

            // Close the device.
            MyUsbDevice.Close();
            
            // Free usb resources.
            // This is necessary for libusb-1.0 and Linux compatibility.
            UsbDevice.Exit();

            // Wait for user input..
            Console.ReadKey();
        }

        private static void ShowLastUsbError()
        {
            Console.WriteLine("Error Number: {0}", UsbDevice.LastErrorNumber);
            Console.WriteLine(UsbDevice.LastErrorString);

            // Wait for user input..
            Console.ReadKey();
        }
    }
}