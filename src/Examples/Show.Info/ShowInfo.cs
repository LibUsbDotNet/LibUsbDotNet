using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using System;
using System.Collections.ObjectModel;

namespace Examples
{
    internal class ShowInfo
    {
        public static void Main(string[] args)
        {
            // Dump all devices and descriptor information to console output.
            using (UsbContext context = new UsbContext())
            {
                var allDevices = context.List();
                foreach (var usbRegistry in allDevices)
                {
                    Console.WriteLine(usbRegistry.Info.ToString());

                    if (usbRegistry.TryOpen())
                    {
                        for (int iConfig = 0; iConfig < usbRegistry.Configs.Count; iConfig++)
                        {
                            UsbConfigInfo configInfo = usbRegistry.Configs[iConfig];
                            Console.WriteLine(configInfo.ToString());

                            ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.Interfaces;
                            for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                            {
                                UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                                Console.WriteLine(interfaceInfo.ToString());

                                ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.Endpoints;
                                for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                                {
                                    Console.WriteLine(endpointList[iEndpoint].ToString());
                                }
                            }
                        }

                        usbRegistry.Close();
                    }
                }
            }

            // Wait for user input..
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}