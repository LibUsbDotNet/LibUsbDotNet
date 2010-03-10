using System;
using System.Collections.Generic;
using LibUsbDotNet.Main;
using MonoLibUsb;
using MonoLibUsb.Descriptors;
using MonoLibUsb.Profile;
using Usb = MonoLibUsb.MonoUsbApi;

namespace MonoUsb.ShowConfig
{
    internal class ShowConfig
    {
        private static MonoUsbSessionHandle sessionHandle;

        // Predicate functions for finding only devices with the specified VendorID & ProductID.
        private static bool MyVidPidPredicate(MonoUsbProfile profile)
        {
            if (profile.DeviceDescriptor.VendorID == 0x04d8 && profile.DeviceDescriptor.ProductID == 0x0053)
                return true;
            return false;
        }

        public static void Main(string[] args)
        {
            // Initialize the context.
            sessionHandle = new MonoUsbSessionHandle();
            if (sessionHandle.IsInvalid)
                throw new Exception(String.Format("Failed intialized libusb context.\n{0}:{1}",
                                                  MonoUsbSessionHandle.LastErrorCode,
                                                  MonoUsbSessionHandle.LastErrorString));

            MonoUsbProfileList profileList = new MonoUsbProfileList();

            // The list is initially empty.
            // Each time refresh is called the list contents are updated. 
            int ret = profileList.Refresh(sessionHandle);
            if (ret < 0) throw new Exception("Failed to retrieve device list.");
            Console.WriteLine("{0} device(s) found.", ret);

            // Use the GetList() method to get a generic List of MonoUsbProfiles
            // Find all profiles that match in the MyVidPidPredicate.
            List<MonoUsbProfile> myVidPidList = profileList.GetList().FindAll(MyVidPidPredicate);

            // myVidPidList reresents a list of connected USB devices that matched
            // in MyVidPidPredicate.
            foreach (MonoUsbProfile profile in myVidPidList)
            {
                // Write the VendorID and ProductID to console output.
                Console.WriteLine("[Device] Vid:{0:X4} Pid:{1:X4}", profile.DeviceDescriptor.VendorID, profile.DeviceDescriptor.ProductID);
                
                // Loop through all of the devices configurations.
                for (byte i = 0; i < profile.DeviceDescriptor.ConfigurationCount; i++)
                {
                    // Get a handle to the configuration.
                    MonoUsbConfigHandle configHandle;
                    if (MonoUsbApi.GetConfigDescriptor(profile.ProfileHandle, i, out configHandle) < 0) continue;
                    if (configHandle.IsInvalid) continue;

                    // Create a MonoUsbConfigDescriptor instance for this config handle.
                    MonoUsbConfigDescriptor configDescriptor = new MonoUsbConfigDescriptor(configHandle);

                    // Write the bConfigurationValue to console output.
                    Console.WriteLine("  [Config] bConfigurationValue:{0}", configDescriptor.bConfigurationValue);

                    // Interate through the InterfaceList
                    foreach (MonoUsbInterface usbInterface in configDescriptor.InterfaceList)
                    {
                        // Interate through the AltInterfaceList
                        foreach (MonoUsbAltInterfaceDescriptor usbAltInterface in usbInterface.AltInterfaceList)
                        {
                            // Write the bInterfaceNumber and bAlternateSetting to console output.
                            Console.WriteLine("    [Interface] bInterfaceNumber:{0} bAlternateSetting:{1}",
                                              usbAltInterface.bInterfaceNumber,
                                              usbAltInterface.bAlternateSetting);

                            // Interate through the EndpointList
                            foreach (MonoUsbEndpointDescriptor endpoint in usbAltInterface.EndpointList)
                            {
                                // Write the bEndpointAddress, EndpointType, and wMaxPacketSize to console output.
                                Console.WriteLine("      [Endpoint] bEndpointAddress:{0:X2} EndpointType:{1} wMaxPacketSize:{2}",
                                                  endpoint.bEndpointAddress,
                                                  (EndpointType) (endpoint.bmAttributes & 0x3),
                                                  endpoint.wMaxPacketSize);
                            }
                        }
                    }
                    // Not neccessary, but good programming practice.
                    configHandle.Close();
                }
            }
            // Not neccessary, but good programming practice.
            profileList.Close();
            // Not neccessary, but good programming practice.
            sessionHandle.Close();
        }
    }
}