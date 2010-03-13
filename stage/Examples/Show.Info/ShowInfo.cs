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
            // Check for a valid & connected usb device by vendor and product id.
            Console.WriteLine("Finding your device..");
            UsbRegistry myUsbRegistry = UsbDevice.AllDevices.Find(MyUsbFinder);

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

            // Display the usb devices DeviceInterfaceGuids.
            // This set by the install inf.

            Guid[] deviceInterfaceGuids = myUsbRegistry.DeviceInterfaceGuids;
            foreach (Guid deviceInterfaceGuid in deviceInterfaceGuids)
                Console.WriteLine("Device Interface Guid: {0}", deviceInterfaceGuid);

            // Open this usb device.
            if (!myUsbRegistry.Open(out MyUsbDevice))
            {
                // If a UsbRegistry class is obtained this should never happen with libusb-win32.
                // It will happen with WinUsb only of the device is being used by another process.
                Console.WriteLine("Failed opening device!");
                ShowLastUsbError();
                return;
            }

            // Get the REAL usb DeviceDescriptor information from the usb device.
            // Up until this point, we have just been querying the windows registry,
            // nothing had actually been sent or received from the usb device.
            UsbDeviceInfo myDeviceInfo = MyUsbDevice.Info;

            // Dump the UsbDeviceDesciptor to console output.
            Console.WriteLine(myDeviceInfo.Descriptor.ToString());

            // Display REAL Manufacturer String (if one exists)
            if (myDeviceInfo.Descriptor.ManufacturerStringIndex != 0)
                Console.WriteLine("Manufacturer: {0}", myDeviceInfo.ManufacturerString);

            // Display REAL Product String (if one exists)
            if (myDeviceInfo.Descriptor.ProductStringIndex != 0)
                Console.WriteLine("Product: {0}", myDeviceInfo.ProductString);

            // Display REAL Serial Number String (if one exists)
            if (myDeviceInfo.Descriptor.SerialStringIndex != 0)
                Console.WriteLine("Serial Number: {0}", myDeviceInfo.SerialString);

            // Close the device.
            // When a UsbDevice class is closed, it is disposed and all resources 
            // are freed. It cannot be re-open; you must create a new instance.
            MyUsbDevice.Close();

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