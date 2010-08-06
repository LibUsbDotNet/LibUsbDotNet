// Copyright © 2006-2010 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
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
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using LibUsbDotNet.Internal.UsbRegex;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using WinApiNet;

namespace InfWizard.WizardClassHelpers
{
    internal static class DeviceSelectionHelper
    {
        private static readonly string[] mSkipServiceNames = new string[]
                                                                 {
                                                                     "usbhub", 
                                                                     "usbccgp",
                                                                 };

        internal static string GetAsAutoString(byte[] buffer)
        {
            GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            string s = Marshal.PtrToStringAuto(gc.AddrOfPinnedObject());
            if (ReferenceEquals(s, null)) s = string.Empty;
            gc.Free();

            return s;
        }

        internal static string GetAsAutoString(byte[] buffer, int len)
        {
            GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            string s = Marshal.PtrToStringAuto(gc.AddrOfPinnedObject(), len);
            if (ReferenceEquals(s, null)) s = string.Empty;
            gc.Free();

            return s;
        }

        internal static string[] GetAsAutoStringArray(byte[] buffer, int len)
        {
            string sNullDelimited = GetAsAutoString(buffer, len);

            string[] sArray = sNullDelimited.Split(new char[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);

            return sArray;
        }

        public static bool RemoveDevice(RemoveDeviceForm.RemoveDeviceOptions removeDeviceOptions,
                                        IntPtr hWnwd,
                                        DEIFlags flags)
        {
            DeviceRemoveInfo deInfo = new DeviceRemoveInfo(removeDeviceOptions, flags, hWnwd);
            EnumerateDevices(deInfo);
            
            if (deInfo.Removed > 0)
                Wdi.UpdateDriver(removeDeviceOptions.DeviceItem.BuildInfHardwareID());
            
            return deInfo.Removed > 0;
        }

        public static int EnumerateDevices(DeviceEnumeratorInfo deviceEnumeratorInfo)
        {
            int devIndex;

            const uint CM_PROB_PHANTOM = (0x0000002D);   // The devinst currently exists only in the registry

            // Initialize the SP_DEVINFO_DATA structure
            SetupApi.SP_DEVINFO_DATA devInfoData = new SetupApi.SP_DEVINFO_DATA();
            devInfoData.cbSize = (uint) Marshal.SizeOf(typeof (SetupApi.SP_DEVINFO_DATA));

            // Used to parse the DeviceID tokens.
            RegHardwareID regHardwareID = RegHardwareID.GlobalInstance;

            // Used as a buffer for:
            // * SetupDiGetDeviceRegistryProperty
            // * CM_Get_Device_ID
            // * SetupDiGetCustomDeviceProperty
            // * SetupDiGetDeviceProperty
            byte[] propBuffer = new byte[1024];

            // List all connected USB devices
            IntPtr pDevInfo = SetupApi.SetupDiGetClassDevs(0, "USB", deviceEnumeratorInfo.Hwnd, deviceEnumeratorInfo.DICFGFlags);
            if (pDevInfo == IntPtr.Zero || pDevInfo == new IntPtr(-1))
            {
                InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Win32Error, "SetupDiGetClassDevs Failed!");
                return -1;
            }

            for (devIndex = 0;; devIndex++)
            {
                if (!SetupApi.SetupDiEnumDeviceInfo(pDevInfo, devIndex, ref devInfoData))
                {
                    // Reached the end of the eviceInfo list.
                    InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Info, "device enumeration complete.");
                    break;
                }
                DeviceItem deviceItem = new DeviceItem();

                // SPDRP_DRIVER seems to do a better job at detecting driverless devices than
                // SPDRP_INSTALL_STATE
                RegistryValueKind propertyType;
                int requiredSize;
                if (SetupApi.SetupDiGetDeviceRegistryProperty(pDevInfo,
                                                              ref devInfoData,
                                                              SetupApi.SPDRP.DRIVER,
                                                              out propertyType,
                                                              propBuffer,
                                                              propBuffer.Length,
                                                              out requiredSize))
                {
                    deviceItem.mDriverless = false;

                    // Read all string values from the registry driver key
                    IntPtr hKey = SetupApi.SetupDiOpenDevRegKey(pDevInfo,
                                                                ref devInfoData,
                                                                1,
                                                                0,
                                                                DevKeyType.DRV,
                                                                (int) RegistryKeyPermissionCheck.ReadSubTree);
                    if (hKey != IntPtr.Zero && hKey != new IntPtr(-1))
                    {
                        int index = 0;
                        int nameLength = 255;
                        int dataLength = 1023;

                        deviceItem.mDriverRegistryList = new Dictionary<string, object>();

                        StringBuilder sbName = new StringBuilder(nameLength + 1);
                        StringBuilder sbValue = new StringBuilder(dataLength + 1);
                        RegistryValueKind regValueType;

                        while (SetupApi.RegEnumValue(hKey, index, sbName, ref nameLength, IntPtr.Zero, out regValueType, sbValue, ref dataLength) == 0)
                        {
                            if (regValueType == RegistryValueKind.String)
                                deviceItem.mDriverRegistryList.Add(sbName.ToString(), sbValue.ToString());

                            // Get next key/value index
                            index++;

                            // Reset max lengths
                            nameLength = 255;
                            dataLength = 1023;
                        }
                        SetupApi.RegCloseKey(hKey);
                    }
                }
                else
                {
                    deviceItem.mDriverless = true;
                }

                // [trobinson] patch
                uint status;
                uint pbmNumber;
                deviceItem.mIsConnected = (SetupApi.CM_Get_DevNode_Status(out status, out pbmNumber, devInfoData.DevInst, 0) != SetupApi.CR.NO_SUCH_DEVNODE);
                if (deviceItem.mIsConnected)
                    deviceItem.mIsConnected = ((pbmNumber & CM_PROB_PHANTOM) != CM_PROB_PHANTOM);

                //if (deviceItem.mDriverless && !deviceItem.mIsConnected)
                //    deviceItem.mDriverless = false;

                // Find only the ones that are driverless
                if (deviceEnumeratorInfo.DriverlessOnly && !deviceItem.mDriverless)
                {
                    InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Info, "skipping non driverless device.");
                    continue;
                }

                // Driverless devices will return an error
                InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Info, "driverless device found.");


                // Eliminate USB hubs by checking the driver string
                if (!SetupApi.SetupDiGetDeviceRegistryProperty(pDevInfo,
                                                               ref devInfoData,
                                                               SetupApi.SPDRP.SERVICE,
                                                               out propertyType,
                                                               propBuffer,
                                                               propBuffer.Length,
                                                               out requiredSize))
                {
                    InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Warning, "failed getting SPDRP.SERVICE");
                    deviceItem.mServiceName = String.Empty;
                }
                else
                {
                    deviceItem.mServiceName = GetAsAutoString(propBuffer);
                }

                bool bContinue = true;
                foreach (string skipServiceName in mSkipServiceNames)
                {
                    if (deviceItem.mServiceName.Trim().ToLower() == skipServiceName)
                    {
                        bContinue = false;
                        break;
                    }
                }
                if (!bContinue && deviceEnumeratorInfo.SkipWindowsServices)
                    continue;
                //if (!bContinue)
                //    continue;

                deviceItem.mIsSkipServiceName = !bContinue;

                string[] saHardwareIDs;

                // Retrieve the hardware ID
                if (!SetupApi.SetupDiGetDeviceRegistryProperty(out saHardwareIDs,
                                                               pDevInfo,
                                                               ref devInfoData,
                                                               SetupApi.SPDRP.HARDWAREID))
                {
                    InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Win32Error, "failed getting SPDRP.HARDWAREID");
                    continue;
                }

                if (saHardwareIDs.Length == 0)
                {
                    InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Error, "device does not have any hardware ids");
                    continue;
                }

                for (int hwid = 0; hwid < saHardwareIDs.Length; hwid++)
                    InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Info, "found hardware ID ({0}/{1}): {2}", hwid + 1, saHardwareIDs.Length, saHardwareIDs[hwid]);

                // Get Device ID
                SetupApi.CR r = SetupApi.CM_Get_Device_ID(devInfoData.DevInst, propBuffer, propBuffer.Length, 0);
                if (r != SetupApi.CR.SUCCESS)
                {
                    InfWizardStatus.Log(CategoryType.EnumerateDevices,
                                        StatusType.Error,
                                        "CM_Get_Device_ID:Failed retrieving simple path for device index: {0} HWID:{1} CR error {2}",
                                        devIndex,
                                        saHardwareIDs[0],
                                        r);
                    continue;
                }
                deviceItem.mDeviceId = GetAsAutoString(propBuffer);

                InfWizardStatus.Log(CategoryType.EnumerateDevices,
                                    StatusType.Info,
                                    "{0} USB device {1}: {2}",
                                    deviceItem.mDriverless ? "Driverless" : deviceItem.mServiceName,
                                    devIndex,
                                    deviceItem.mDeviceId);


                string sDeviceDescription;
                if (SetupApi.WindowsVersion < WindowsVersionType.WINDOWS_7)
                {
                    // On Vista and earlier, we can use SPDRP_DEVICEDESC
                    bContinue = SetupApi.SetupDiGetDeviceRegistryProperty(out sDeviceDescription,
                                                                          pDevInfo,
                                                                          ref devInfoData,
                                                                          SetupApi.SPDRP.DEVICEDESC);

                    if (!bContinue) sDeviceDescription = string.Empty;
                }
                else
                {
                    // On Windows 7, the information we want ("Bus reported device description") is
                    // accessed through DEVPKEY_Device_BusReportedDeviceDesc
                    try
                    {
                        bContinue = SetupApi.SetupDiGetDeviceProperty(pDevInfo,
                                                                      ref devInfoData,
                                                                      SetupApi.DEVPKEY_Device_BusReportedDeviceDesc,
                                                                      out propertyType,
                                                                      propBuffer,
                                                                      propBuffer.Length,
                                                                      out requiredSize,
                                                                      0);
                    }
                    catch (DllNotFoundException)
                    {
                        //if (SetupDiGetDeviceProperty == NULL)
                        InfWizardStatus.Log(CategoryType.EnumerateDevices,
                                            StatusType.Warning,
                                            "Failed to locate SetupDiGetDeviceProperty() is Setupapi.dll");
                        bContinue = false;
                    }
                    if (bContinue)
                    {
                        sDeviceDescription = GetAsAutoString(propBuffer);
                    }
                    else
                    {
                        // fallback to SPDRP_DEVICEDESC (USB husb still use it)
                        bContinue = SetupApi.SetupDiGetDeviceRegistryProperty(out sDeviceDescription,
                                                                              pDevInfo,
                                                                              ref devInfoData,
                                                                              SetupApi.SPDRP.DEVICEDESC);
                        if (!bContinue) sDeviceDescription = string.Empty;
                    }
                }
                if (!bContinue)
                    InfWizardStatus.Log(CategoryType.EnumerateDevices,
                                        StatusType.Warning | StatusType.Win32Error,
                                        "Failed reading read device description for {0}: {1}",
                                        devIndex,
                                        deviceItem.mDeviceId);

                deviceItem.DeviceDescription = sDeviceDescription;
                deviceItem.BaseFilename = sDeviceDescription;
                MatchCollection matches = regHardwareID.Matches(saHardwareIDs[0]);
                foreach (Match match in matches)
                {
                    foreach (NamedGroup namedGroup in RegHardwareID.NAMED_GROUPS)
                    {
                        RegHardwareID.ENamedGroups groupEnum = (RegHardwareID.ENamedGroups) namedGroup.GroupNumber;
                        Group group = match.Groups[(int) groupEnum];
                        if (!group.Success) continue;

                        switch (groupEnum)
                        {
                            case RegHardwareID.ENamedGroups.Vid:
                                deviceItem.VendorID = group.Value;
                                break;
                            case RegHardwareID.ENamedGroups.Pid:
                                deviceItem.ProductID = group.Value;
                                break;
                            case RegHardwareID.ENamedGroups.Rev:
                                //deviceItem.Rev = group.Value;
                                break;
                            case RegHardwareID.ENamedGroups.MI:
                                deviceItem.MI = group.Value;
                                if (deviceItem.MI!=string.Empty)
                                    deviceItem.DeviceDescription += String.Format(" (Interface #{0})", deviceItem.mMI);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                if (deviceItem.mVid == 0 && deviceItem.mPid == 0) continue;

                string sManufacturer;
                if (!SetupApi.SetupDiGetDeviceRegistryProperty(out sManufacturer,
                                                               pDevInfo,
                                                               ref devInfoData,
                                                               SetupApi.SPDRP.MFG))
                {
                    sManufacturer = string.Empty;
                }
                deviceItem.Manufacturer = sManufacturer;

                string[] deviceInterfaceGuids=new string[0];
                if (SetupApi.SetupDiGetCustomDeviceProperty(pDevInfo,
                                                             ref devInfoData,
                                                             "DeviceInterfaceGUIDs",
                                                             SetupApi.DICUSTOMDEVPROP.NONE,
                                                             out propertyType,
                                                             propBuffer,
                                                             propBuffer.Length,
                                                             out requiredSize))
                {
                    deviceInterfaceGuids = GetAsAutoStringArray(propBuffer, requiredSize);
                }

                if (deviceInterfaceGuids.Length > 0)
                    deviceItem.DeviceInterfaceGuid = deviceInterfaceGuids[0];

                if (!deviceEnumeratorInfo.DeviceFound(deviceItem, pDevInfo, ref devInfoData))
                    break;

                InfWizardStatus.Log(CategoryType.EnumerateDevices, StatusType.Info, "device description: {0}", deviceItem.DeviceDescription);
            }

            SetupApi.SetupDiDestroyDeviceInfoList(pDevInfo);
            return devIndex;
        }
    }
    public delegate bool DeviceFoundDelegate(DeviceItem item, IntPtr pDeviceInfoSet, ref SetupApi.SP_DEVINFO_DATA DeviceInfoData);

    [Flags]
    public enum DEIFlags
    {
        /// <summary>
        /// Return only the device that is associated with the system default device interface, if one is set, for the specified device interface classes. 
        ///  only valid with <see cref="DEVICEINTERFACE"/>.
        /// </summary>
        DICFG_Default = 0x00000001,
        /// <summary>
        /// Return only devices that are currently present in a system. 
        /// </summary>
        DICFG_Present = 0x00000002,
        /// <summary>
        /// Return a list of installed devices for all device setup classes or all device interface classes. 
        /// </summary>
        DICFG_AllClasses = 0x00000004,
        /// <summary>
        /// Return only devices that are a part of the current hardware profile. 
        /// </summary>
        DICFG_Profile = 0x00000008,
        /// <summary>
        /// Return devices that support device interfaces for the specified device interface classes. 
        /// </summary>
        DICFG_DeviceInterface = 0x00000010,

        DICFG_MASK = 0x000000ff,

        Driverless = 0x00000100,

        IncludeWindowsServices = 0x00000200,
    }
    public class DeviceEnumeratorInfo
    {
        protected readonly List<DeviceItem> deviceList = new List<DeviceItem>();
        private readonly DEIFlags mFlags;
        private readonly IntPtr mHwnd;

        public DeviceEnumeratorInfo(DEIFlags flags, IntPtr hwnd)
        {
            mHwnd = hwnd;
            mFlags = flags;
        }

        public IntPtr Hwnd
        {
            get { return mHwnd; }
        }

        public SetupApi.DICFG DICFGFlags
        {
            get { return (SetupApi.DICFG) (mFlags & DEIFlags.DICFG_MASK); }
        }

        public bool SkipWindowsServices
        {
            get { return (mFlags & DEIFlags.IncludeWindowsServices)==0; }
        }

        public bool DriverlessOnly
        {
            get { return (mFlags & DEIFlags.Driverless) == DEIFlags.Driverless; }
        }

        public List<DeviceItem> DeviceList
        {
            get { return deviceList; }
        }

        #region IDeviceEnumeratorInfo Members

        public virtual bool DeviceFound(DeviceItem item,
                                        IntPtr pDeviceInfoSet,
                                        ref SetupApi.SP_DEVINFO_DATA DeviceInfoData)
        {
            deviceList.Add(item);
            return true; // true to continue enumerating
        }

        #endregion

    }
    public class DeviceRemoveInfo : DeviceEnumeratorInfo
    {
        private readonly RemoveDeviceForm.RemoveDeviceOptions mRemoveDeviceOptions;
        private int mRemoved;

        public DeviceRemoveInfo(RemoveDeviceForm.RemoveDeviceOptions removeDeviceOptions, DEIFlags flags, IntPtr hwnd) :
            base(flags, hwnd)
        {
            mRemoveDeviceOptions = removeDeviceOptions;
        }

        public int Removed
        {
            get { return mRemoved; }
        }

        #region IDeviceEnumeratorInfo Members

        public override bool DeviceFound(DeviceItem item, IntPtr pDeviceInfoSet, ref SetupApi.SP_DEVINFO_DATA DeviceInfoData)
        {
            if (mRemoveDeviceOptions.RemoveByVidPid)
            {
                if (item.VendorID.ToLower() != mRemoveDeviceOptions.DeviceItem.VendorID.ToLower() ||
                item.ProductID.ToLower() != mRemoveDeviceOptions.DeviceItem.ProductID.ToLower())
                    return true;
            }
            else
            {
                if (item.mDeviceId != mRemoveDeviceOptions.DeviceItem.mDeviceId) return true;
            }
            bool bUninstalled;
            if (SetupApi.WindowsVersion >= WindowsVersionType.WINDOWS_7)
            {
                if ((bUninstalled=SetupApi.DiUninstallDevice(IntPtr.Zero, pDeviceInfoSet, ref DeviceInfoData, 0, IntPtr.Zero))==true)
                    mRemoved++;
                else
                    InfWizardStatus.Log(CategoryType.RemoveDevice,  StatusType.Warning|StatusType.Win32Error, "failed uninstalling device.");
            }
            else
            {
                if ((bUninstalled=SetupApi.SetupDiRemoveDevice(pDeviceInfoSet, ref DeviceInfoData))==true)
                    mRemoved++;
                else
                    InfWizardStatus.Log(CategoryType.RemoveDevice, StatusType.Warning|StatusType.Win32Error, "failed uninstalling device.");
            }
            if (bUninstalled)
            {
                InfWizardStatus.Log(CategoryType.RemoveDevice, StatusType.Success, "device uninstall complete");  
            }
            object oInfFileName;
            if (item.mDriverRegistryList != null && !item.mIsSkipServiceName)
            {
                if (item.mDriverRegistryList.TryGetValue("InfPath", out oInfFileName))
                {
                    if (!(SetupApi.SetupUninstallOEMInf(oInfFileName.ToString(), SetupApi.SUOI.FORCEDELETE, IntPtr.Zero)))
                        InfWizardStatus.Log(CategoryType.RemoveDevice, StatusType.Warning | StatusType.Win32Error, "SetupUninstallOEMInf failed");
                }
            }
            return mRemoveDeviceOptions.RemoveByVidPid;
        }

        #endregion

    }
    public sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Methods
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeRegistryHandle()
            : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll")]
        private static extern int RegCloseKey(IntPtr hKey);
        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
                return (RegCloseKey(handle) == 0);
            SetHandleAsInvalid();
            return true;
        }
    }
}