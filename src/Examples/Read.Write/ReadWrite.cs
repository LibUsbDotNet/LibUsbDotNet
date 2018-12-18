using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using System.Linq;

namespace Examples
{
    internal class ReadWrite
    {
        //Put your Product Id Here
        private const int ProductId = 0x0001;

        //Put your Vendor Id Here
        private const int VendorId = 0x0001;

        public static void Main(string[] args)
        {
            using (var context = new UsbContext())
            {
                context.SetDebugLevel(LogLevel.Info);

                //Get a list of all connected devices
                var usbDeviceCollection = context.List();

                //Narrow down the device by vendor and pid
                var selectedDevice = usbDeviceCollection.FirstOrDefault(d => d.ProductId == ProductId && d.VendorId == VendorId);

                //Open the device
                selectedDevice.Open();

                //Get the first config number of the interface
                selectedDevice.ClaimInterface(selectedDevice.Configs[0].Interfaces[0].Number);

                //Open up the endpoints
                var writeEndpoint = selectedDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                var readEnpoint = selectedDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                //Create a buffer with some data in it
                var buffer = new byte[64];
                buffer[0] = 0x3f;
                buffer[1] = 0x23;
                buffer[2] = 0x23;

                //Write three bytes
                writeEndpoint.Write(buffer, 3000, out var bytesWritten);

                var readBuffer = new byte[64];

                //Read some data
                readEnpoint.Read(readBuffer, 3000, out var readBytes);
            }
        }
    }
}