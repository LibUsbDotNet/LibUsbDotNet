using System;
using System.Collections.Generic;
using LibUsbDotNet.Internal;
using Microsoft.Win32;

namespace LibUsbDotNet.Main
{
    internal static class SetupApiRegistry
    {
        private class MasterItem : Dictionary<string, object>
        {
            public Dictionary<Guid,List<string>> DevicePaths=new Dictionary<Guid, List<string>>();
        }
        private class MasterList : List<MasterItem>
        {

        }
        private static readonly MasterList mMasterSetupApiDeviceList = new MasterList();
        private static readonly Object mLockSetupApiRegistry = new object();
        private static DateTime mLastRefreshTime = DateTime.MinValue;
        public static bool NeedsRefresh
        {
            get
            {
                lock (mLockSetupApiRegistry)
                {
                    if ((DateTime.Now - mLastRefreshTime).TotalMilliseconds >= 1000)
                        return true;
                    return false;
                }
            }

        }

        public static bool FillDeviceProperties(UsbRegistry usbRegistry, UsbDevice usbDevice)
        {
            if (NeedsRefresh) BuildMasterList();

            lock (mLockSetupApiRegistry)
            {
                string fakeHwId = LegacyUsbRegistry.GetRegistryHardwareID((ushort)usbDevice.Info.Descriptor.VendorID,
                                                                          (ushort)usbDevice.Info.Descriptor.ProductID,
                                                                          (ushort)usbDevice.Info.Descriptor.BcdDevice);
                bool bFound = false;
                string hwIdToFind = fakeHwId.ToLower();
                foreach (MasterItem masterItem in mMasterSetupApiDeviceList)
                {
                    string[] hwIds = masterItem[SPDRP.HardwareId.ToString()] as string[];
                    if (ReferenceEquals(hwIds,null)) continue;
                    foreach (string hwID in hwIds)
                    {
                        if (hwID.ToLower().Contains(hwIdToFind))
                        {
                            usbRegistry.mDeviceProperties = masterItem;
                            bFound = true;
                            break;
                        }
                    }
                    if (bFound) break;
                }
                return bFound;   
            }

        }
        public static void BuildMasterList()
        {
            lock (mLockSetupApiRegistry)
            {

                mMasterSetupApiDeviceList.Clear();
                SetupApi.EnumClassDevs(null, SetupApi.DICFG.PRESENT | SetupApi.DICFG.ALLCLASSES, BuildMasterCallback, mMasterSetupApiDeviceList);
                mLastRefreshTime = DateTime.Now;
            }
        }

        private static bool BuildMasterCallback(IntPtr deviceInfoSet, int deviceindex, ref SetupApi.SP_DEVINFO_DATA deviceInfoData, object userData)
        {

            MasterList deviceList = userData as MasterList;
            MasterItem deviceItem = new MasterItem();

            deviceList.Add(deviceItem);

            RegistryValueKind propertyType;
            byte[] propBuffer = new byte[256];
            int requiredSize;
            bool bSuccess = SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet,
                                                                    ref deviceInfoData,
                                                                    UsbRegistry.DEVICE_INTERFACE_GUIDS,
                                                                    SetupApi.DICUSTOMDEVPROP.NONE,
                                                                    out propertyType,
                                                                    propBuffer,
                                                                    propBuffer.Length,
                                                                    out requiredSize);
            if (bSuccess)
            {
                string[] devInterfaceGuids = UsbRegistry.GetAsStringArray(propBuffer, requiredSize);

                deviceItem.Add(UsbRegistry.DEVICE_INTERFACE_GUIDS, devInterfaceGuids);
                foreach (string s in devInterfaceGuids)
                {
                    Guid g = new Guid(s);
                    List<string> devicePathList;
                    if (SetupApi.GetDevicePath(g, out devicePathList))
                    {

                        deviceItem.DevicePaths.Add(g, devicePathList);
                    }
                }

            }
            else
            {

                bSuccess = SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet,
                                                                   ref deviceInfoData,
                                                                   UsbRegistry.LIBUSB_INTERFACE_GUIDS,
                                                                   SetupApi.DICUSTOMDEVPROP.NONE,
                                                                   out propertyType,
                                                                   propBuffer,
                                                                   propBuffer.Length,
                                                                   out requiredSize);
                if (bSuccess)
                {
                    string[] devInterfaceGuids = UsbRegistry.GetAsStringArray(propBuffer, requiredSize);

                    deviceItem.Add(UsbRegistry.LIBUSB_INTERFACE_GUIDS, devInterfaceGuids);
                    foreach (string s in devInterfaceGuids)
                    {
                        Guid g = new Guid(s);
                        List<string> devicePathList;
                        if (SetupApi.GetDevicePath(g, out devicePathList))
                        {
                            deviceItem.DevicePaths.Add(g, devicePathList);
                        }
                    }

                }
            }

            bSuccess =
                SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet,
                                                        ref deviceInfoData,
                                                        UsbRegistry.SYMBOLIC_NAME_KEY,
                                                        SetupApi.DICUSTOMDEVPROP.NONE,
                                                        out propertyType,
                                                        propBuffer,
                                                        propBuffer.Length,
                                                        out requiredSize);
            if (bSuccess)
            {
                string symbolicName = UsbRegistry.GetAsString(propBuffer, requiredSize);
                deviceItem.Add(UsbRegistry.SYMBOLIC_NAME_KEY, symbolicName);
            }
            SetupApi.getSPDRPProperties(deviceInfoSet, ref deviceInfoData, deviceItem);

            return false;
        }
    }
}