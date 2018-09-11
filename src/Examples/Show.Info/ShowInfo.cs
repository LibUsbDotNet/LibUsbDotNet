using System;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System.Collections.ObjectModel;

namespace Examples
{
    internal class ShowInfo
    {
        public static void Main(string[] args)
        {
            // Dump all devices and descriptor information to console output.
            var allDevices = UsbDevice.AllDevices;
            foreach (var usbRegistry in allDevices)
            {
                Console.WriteLine(usbRegistry.Info.ToString());

                if (usbRegistry.Open())
                {
                    for (int iConfig = 0; iConfig < usbRegistry.Configs.Count; iConfig++)
                    {
                        UsbConfigInfo configInfo = usbRegistry.Configs[iConfig];
                        Console.WriteLine(configInfo.ToString());

                        ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
                        for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                        {
                            UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                            Console.WriteLine(interfaceInfo.ToString());

                            ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.EndpointInfoList;
                            for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                            {
                                Console.WriteLine(endpointList[iEndpoint].ToString());
                            }
                        }
                    }

                    usbRegistry.Close();
                }
            }


            // Free usb resources.
            // This is necessary for libusb-1.0 and Linux compatibility.
            UsbDevice.Exit();

            // Wait for user input..
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}