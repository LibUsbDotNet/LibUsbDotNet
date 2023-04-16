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
                foreach (UsbDevice device in allDevices)
                {
                    bool openedDevice = device.TryOpen();

                    Console.WriteLine(device.Info.ToString());
                    
                    if (!openedDevice)
                        continue;
                    
                    foreach (var configInfo in device.Configs)
                    {
                        Console.WriteLine($"\t{configInfo.ToString().ReplaceLineEndings("\n\t")}");

                        ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.Interfaces;
                        for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                        {
                            UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                            Console.WriteLine($"\t\t{interfaceInfo.ToString().ReplaceLineEndings("\n\t\t")}");

                            ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.Endpoints;
                            for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                            {
                                Console.WriteLine($"\t\t\tEndpoint: {iEndpoint}");
                                Console.WriteLine($"\t\t\t{endpointList[iEndpoint].ToString().ReplaceLineEndings("\n\t\t\t")}");
                            }
                        }
                    }

                    device.Close();
                }
            }

            // Wait for user input..
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}