// Copyright © 2009 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  trobinso@users.sourceforge.net
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using LibUsbDotNet.Main;
using Microsoft.Win32;
using WinApiNet;

namespace InfWizard.WizardClassHelpers
{
    public delegate bool DeviceFoundDelegate(
        DeviceItem item, IntPtr pDeviceInfoSet, ref SetupApi.SP_DEVINFO_DATA DeviceInfoData);

    public interface IDeviceEnumeratorInfo
    {
        #region REQUIRED Override Members

        bool DeviceFound(DeviceItem item, IntPtr pDeviceInfoSet, ref SetupApi.SP_DEVINFO_DATA DeviceInfoData);

        #endregion
    }

    public class DeviceEnumeratorInfo : IDeviceEnumeratorInfo
    {
        #region OPTIONAL Override Members

        public virtual bool DeviceFound(DeviceItem item,
                                        IntPtr pDeviceInfoSet,
                                        ref SetupApi.SP_DEVINFO_DATA DeviceInfoData)
        {
            if (!deviceList.Contains(item))
            {
                deviceList.Add(item);
            }
            return true; // true to continue enumerating
        }

        #endregion

        protected readonly List<DeviceItem> deviceList = new List<DeviceItem>();

        public List<DeviceItem> DeviceList
        {
            get { return deviceList; }
        }
    }

    public class DeviceRemoveInfo : IDeviceEnumeratorInfo
    {
        private readonly fRemoveDevice.RemoveDeviceOptions mRemoveDeviceOptions;
        private bool mRemoved;

        public DeviceRemoveInfo(fRemoveDevice.RemoveDeviceOptions removeDeviceOptions) { mRemoveDeviceOptions = removeDeviceOptions; }

        public bool Removed
        {
            get { return mRemoved; }
        }

        #region IDeviceEnumeratorInfo Members

        public bool DeviceFound(DeviceItem item, IntPtr pDeviceInfoSet, ref SetupApi.SP_DEVINFO_DATA DeviceInfoData)
        {
            bool bContinue = true;
            if (item.Equals(mRemoveDeviceOptions.DeviceItem))
            {
                bContinue = SetupApi.SetupDiRemoveDevice(pDeviceInfoSet, ref DeviceInfoData);
                mRemoved = bContinue;
                if (mRemoved)
                {
                    removeOemInfFile(item);
                }
            }

            return bContinue; // true to continue enumerating
        }

        #endregion

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetWindowsDirectory(StringBuilder lpBuffer,
                                                       uint uSize);

        private static void removeOemInfFile(DeviceItem item)
        {
            if (item.DriverRegistryList != null)
            {
                object oValue;
                if (item.DriverRegistryList.TryGetValue("InfPath", out oValue))
                {
                    if (!ReferenceEquals(oValue, null))
                    {
                        if (oValue is string)
                        {
                            string infFile = oValue as string;
                            infFile = infFile.ToLower();
                            if (infFile.StartsWith("oem") && infFile.EndsWith(".inf"))
                            {
                                StringBuilder winDirBuilder = new StringBuilder(100);
                                uint strLen = GetWindowsDirectory(winDirBuilder, 100);
                                if (strLen > 0 && strLen == winDirBuilder.Length)
                                {
                                    string oemInfFile =
                                        string.Format("{0}\\inf\\{1}",
                                                      winDirBuilder.ToString().TrimEnd(new char[] {'\\', '/'}),
                                                      infFile);
                                    FileInfo fiOemInfFile = new FileInfo(oemInfFile);
                                    if (fiOemInfFile.Exists)
                                        fiOemInfFile.Delete();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal static class DeviceSelectionHelper
    {
        internal static string GetAsString(byte[] buffer, int len)
        {
            if (len > 2) return Encoding.Unicode.GetString(buffer, 0, len).TrimEnd('\0');

            return "";
        }

        internal static string[] GetAsStringArray(byte[] buffer, int len) { return GetAsString(buffer, len).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries); }

        internal static Int32 GetAsStringInt32(byte[] buffer, int len)
        {
            Int32 iRtn = 0;
            if (len == 4)
                iRtn = buffer[0] | ((buffer[1]) << 8) | ((buffer[2]) << 16) | ((buffer[3]) << 24);
            return iRtn;
        }
        public static bool RemoveDevice(fRemoveDevice.RemoveDeviceOptions removeDeviceOptions, IntPtr hWnwd, SetupApi.DICFG dicfgFlags)
        {
            DeviceRemoveInfo deInfo = new DeviceRemoveInfo(removeDeviceOptions);
            EnumerateDevices(deInfo, hWnwd, dicfgFlags);

            return deInfo.Removed;
        }

        public static int FindRealDevices(out List<DeviceItem> deviceList, IntPtr hWnwd) { return FindRealDevices(out deviceList, hWnwd, SetupApi.DICFG.ALLCLASSES | SetupApi.DICFG.PRESENT); }

        public static int FindRealDevices(out List<DeviceItem> deviceList, IntPtr hWnwd, SetupApi.DICFG dicfgFlags)
        {
            DeviceEnumeratorInfo deInfo = new DeviceEnumeratorInfo();
            deviceList = deInfo.DeviceList;
            EnumerateDevices(deInfo, hWnwd, dicfgFlags);

            return deInfo.DeviceList.Count;
        }

        public static int EnumerateDevices(IDeviceEnumeratorInfo deviceEnumeratorInfo,
                                           IntPtr hWnwd,
                                           SetupApi.DICFG dicfgFlags)
        {
            IntPtr dev_info = IntPtr.Zero;
            SetupApi.SP_DEVINFO_DATA dev_info_data = new SetupApi.SP_DEVINFO_DATA();
            int dev_index = 0;
            Dictionary<string, object> driverRegistryList;


            dev_info_data.cbSize = (uint) Marshal.SizeOf(typeof (SetupApi.SP_DEVINFO_DATA));
            dev_index = 0;

            dev_info = SetupApi.SetupDiGetClassDevs(0, "USB", hWnwd, dicfgFlags);


            if (dev_info == IntPtr.Zero || dev_info.ToInt64() == -1)
            {
                return -1;
            }
            bool bSuccess;
            while (SetupApi.SetupDiEnumDeviceInfo(dev_info, dev_index, ref dev_info_data))
            {
                driverRegistryList = null;

                if (usb_registry_match(dev_info, ref dev_info_data))
                {
                    string[] saHardwareIDs;
                    string sDeviceDescription;
                    string sManufacturer;
                    string sPhyisicalObjectName;

                    SetupApi.SetupDiGetDeviceRegistryProperty(out saHardwareIDs,
                                                              dev_info,
                                                              ref dev_info_data,
                                                              SetupApi.SPDRP.HARDWAREID);
                    SetupApi.SetupDiGetDeviceRegistryProperty(out sDeviceDescription,
                                                              dev_info,
                                                              ref dev_info_data,
                                                              SetupApi.SPDRP.DEVICEDESC);
                    SetupApi.SetupDiGetDeviceRegistryProperty(out sManufacturer,
                                                              dev_info,
                                                              ref dev_info_data,
                                                              SetupApi.SPDRP.MFG);
                    SetupApi.SetupDiGetDeviceRegistryProperty(out sPhyisicalObjectName,
                                          dev_info,
                                          ref dev_info_data,
                                          SetupApi.SPDRP.PHYSICAL_DEVICE_OBJECT_NAME);

                    Debug.Print("PDO:" + sPhyisicalObjectName);
                    RegistryValueKind propertyType;
                    byte[] propBuffer = new byte[1024];
                    int requiredSize;


                    bSuccess =
                        SetupApi.SetupDiGetCustomDeviceProperty(dev_info,
                                                                ref dev_info_data,
                                                                "DeviceInterfaceGUIDs",
                                                                SetupApi.DICUSTOMDEVPROP.NONE,
                                                                out propertyType,
                                                                propBuffer,
                                                                propBuffer.Length,
                                                                out requiredSize);
                    string[] deviceInterfaceGuid = null;
                    string driverKeyString;
                    if (bSuccess)
                    {
                        driverRegistryList = new Dictionary<string, object>();
                        
                        deviceInterfaceGuid = GetAsStringArray(propBuffer, requiredSize);
                    }
                    {
                    bSuccess =
                            SetupApi.SetupDiGetDeviceRegistryProperty(dev_info,
                                                                      ref dev_info_data,
                                                                      SetupApi.SPDRP.DRIVER,
                                                                      out propertyType,
                                                                      propBuffer,
                                                                      propBuffer.Length,
                                                                      out requiredSize);
                        if (bSuccess)
                        {
                            driverRegistryList = new Dictionary<string, object>();
                            driverKeyString =
                                Encoding.Unicode.GetString(propBuffer, 0, requiredSize).TrimEnd(new char[] {'\0'});
                            string[] driverClassAndNumber = driverKeyString.Split(new char[] {'\\'});
                            string sKeyToOpen = @"SYSTEM\CurrentControlSet\Control\Class\" + driverClassAndNumber[0] +
                                                "\\" + driverClassAndNumber[1];
                            RegistryKey regdriverKey = Registry.LocalMachine.OpenSubKey(sKeyToOpen, false);
                            string[] multiSz;
                            string[] keyNames = regdriverKey.GetValueNames();
                            foreach (string keyName in keyNames)
                            {
                                object oValue = regdriverKey.GetValue(keyName);

                                driverRegistryList.Add(keyName, oValue);
                                if (oValue is string)
                                {
                                    Debug.Print(keyName + ":" + (String) (oValue));
                                }
                                else if (oValue is string[])
                                {
                                    Debug.Print(keyName + ":");
                                    multiSz = (string[]) oValue;
                                    foreach (string sz in multiSz)
                                    {
                                        Debug.Print("\t\t" + sz);
                                    }
                                }
                                else if (oValue is Int32)
                                {
                                    Debug.Print(keyName + ":" + (Int32) (oValue));
                                }
                                else if (oValue is UInt32)
                                {
                                    Debug.Print(keyName + ":" + (UInt32) (oValue));
                                }
                            }
                            Debug.Print(driverKeyString);
                        }
                    }
                    //int iDriverIndex = 0;
                    //SetupApi.SP_DRVINFO_DATA drvinfo_data = SetupApi.SP_DRVINFO_DATA.New;
                    //while (SetupApi.SetupDiEnumDriverInfo(dev_info, IntPtr.Zero, SetupApi.SPDIT.CLASSDRIVER, iDriverIndex, ref drvinfo_data))
                    //{
                    //    iDriverIndex++;
                    //}

                    bSuccess =
                        SetupApi.SetupDiGetCustomDeviceProperty(dev_info,
                                                                ref dev_info_data,
                                                                "SymbolicName",
                                                                SetupApi.DICUSTOMDEVPROP.NONE,
                                                                out propertyType,
                                                                propBuffer,
                                                                propBuffer.Length,
                                                                out requiredSize);
                    string symbolicName;
                    if (bSuccess)
                    {
                        symbolicName = Encoding.Unicode.GetString(propBuffer, 0, (int) requiredSize);
                        Debug.Print(symbolicName);
                    }
                    if (saHardwareIDs != null)
                    {
                        UsbSymbolicName usbSymbolicName = UsbSymbolicName.Parse(saHardwareIDs[0]);

                        if (usbSymbolicName.Vid != 0 && usbSymbolicName.Pid != 0)
                        {
                            DeviceItem deviceItem =
                                new DeviceItem(usbSymbolicName.Vid,
                                               usbSymbolicName.Pid,
                                               sDeviceDescription,
                                               sManufacturer,
                                               dev_index,
                                               driverRegistryList);
                            

                            bool bContinue = deviceEnumeratorInfo.DeviceFound(deviceItem, dev_info, ref dev_info_data);
                            if (!bContinue) break;
                        }
                    }
                }
                dev_index++;
            }

            SetupApi.SetupDiDestroyDeviceInfoList(dev_info);
            return dev_index;
        }


        public static int FindRealInterfaces(out List<DeviceItem> deviceList, IntPtr hWnwd, SetupApi.DICFG dicfgFlags)
        {
            dicfgFlags = dicfgFlags | SetupApi.DICFG.DEVICEINTERFACE;

            deviceList = new List<DeviceItem>();
            IntPtr dev_info = IntPtr.Zero;
            SetupApi.SP_DEVINFO_DATA dev_info_data = new SetupApi.SP_DEVINFO_DATA();
            SetupApi.SP_DEVICE_INTERFACE_DATA dev_interface_data = new SetupApi.SP_DEVICE_INTERFACE_DATA();
            int dev_index = 0;


            dev_info_data.cbSize = (uint) Marshal.SizeOf(typeof (SetupApi.SP_DEVINFO_DATA));
            dev_index = 0;

            dev_info = SetupApi.SetupDiGetClassDevs(0, "USB", hWnwd, dicfgFlags);

            if (dev_info == IntPtr.Zero || dev_info.ToInt64() == -1)
            {
                return -1;
            }
            while (SetupApi.SetupDiEnumDeviceInfo(dev_info, dev_index, ref dev_info_data))
            {
                if (usb_registry_match(dev_info, ref dev_info_data))
                {
                    string classGuidBytes;
                    bool bSuccess =
                        SetupApi.SetupDiGetDeviceRegistryProperty(out classGuidBytes,
                                                                  dev_info,
                                                                  ref dev_info_data,
                                                                  SetupApi.SPDRP.CLASSGUID);
                    int DeviceInterfaceIndex = 0;
                    if (bSuccess)
                    {
                        Guid classGuid = new Guid(classGuidBytes);
                        while (
                            SetupApi.SetupDiEnumDeviceInterfaces(dev_info,
                                                                 ref dev_info_data,
                                                                 ref classGuid,
                                                                 (uint) DeviceInterfaceIndex,
                                                                 ref dev_interface_data))
                        {
                            string devicePath;
                            SetupApi.GetDevicePath(dev_interface_data.interfaceClassGuid, out devicePath);
                            DeviceInterfaceIndex++;
                            Debug.Print("devicePath:" + devicePath);
                        }
                    }
                }
                dev_index++;
            }

            SetupApi.SetupDiDestroyDeviceInfoList(dev_info);
            return dev_index;
        }


        private static bool usb_registry_match(IntPtr dev_info, ref SetupApi.SP_DEVINFO_DATA dev_info_data)
        {
            string[] saHardwareIDs;
            if (
                SetupApi.SetupDiGetDeviceRegistryProperty(out saHardwareIDs,
                                                          dev_info,
                                                          ref dev_info_data,
                                                          SetupApi.SPDRP.HARDWAREID))
            {
                foreach (string hardwareID in saHardwareIDs)
                {
                    if (hardwareID.ToLower().IndexOf("&mi_") != -1 || hardwareID.ToLower().IndexOf("root_hub") != -1)
                        return false;
                }
            }
            return true;
        }
    }
}