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
using System.Runtime.InteropServices;
using System.Text;
using WinApiNet;
using ECODE = WinApiNet.SetupApi.ErrorCodes;

namespace InfWizard
{
    internal static class Wdi
    {
        /// <summary>
        /// Force re-enumeration of a device (force installation)
        /// TODO: allow root re-enum
        /// </summary>
        public static int UpdateDriver(string deviceHardwareID)
        {
            int devIndex;
            int refreshed = 0;
            bool bSuccess;

            // Initialize the SP_DEVINFO_DATA structure
            SetupApi.SP_DEVINFO_DATA devInfoData = new SetupApi.SP_DEVINFO_DATA();

            // List all connected USB devices
            IntPtr pDevInfo = SetupApi.SetupDiGetClassDevs(0, "USB", IntPtr.Zero, SetupApi.DICFG.ALLCLASSES);
            if (pDevInfo == IntPtr.Zero || pDevInfo == new IntPtr(-1))
            {
                return refreshed;
            }
            for (devIndex = 0;; devIndex++)
            {
                devInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupApi.SP_DEVINFO_DATA));
                bSuccess = SetupApi.SetupDiEnumDeviceInfo(pDevInfo, devIndex, ref devInfoData);

                // Reached the end of the deviceInfo list.
                if (!bSuccess) break;

                // Find the hardware ID
                string[] saHardwareIDs;
                bSuccess = SetupApi.SetupDiGetDeviceRegistryProperty(out saHardwareIDs,
                                                                     pDevInfo,
                                                                     ref devInfoData,
                                                                     SetupApi.SPDRP.HARDWAREID);

                // Failed getting hardware id
                if (!bSuccess) break;

                // Failed getting hardware id
                if (saHardwareIDs.Length == 0) continue;

                // Check all hardwareids for a match 
                bool bFound = false;
                foreach (string s in saHardwareIDs)
                {
                    if (s.Trim().ToLower() == deviceHardwareID.Trim().ToLower())
                    {
                        bFound = true;
                        break;
                    }
                }

                // Hardware did not match; goto next device
                if (!bFound) continue;

                // Re-enumerate the device node
                SetupApi.CR status = SetupApi.CM_Reenumerate_DevNode((int) devInfoData.DevInst,
                                                                     SetupApi.CM.REENUMERATE_RETRY_INSTALLATION);
                if (status == SetupApi.CR.SUCCESS)
                {
                    InfWizardStatus.Log(CategoryType.RefreshDriver, StatusType.Success, "re-enumeration of {0} succeeded...", deviceHardwareID);
                }
                else if (status == SetupApi.CR.INVALID_DEVNODE)
                {
                     continue;
                }
                else
                {
                    InfWizardStatus.Log(CategoryType.RefreshDriver, StatusType.Warning, "failed to re-enumerate device node: CR code {0}", status);
                    continue;
                }
                


                refreshed++;
            }
            // return the number of devices that were re-enumerated.
            return refreshed;
        }

        /// <summary>
        /// Flag phantom/removed devices for reinstallation. 
        /// See: http://msdn.microsoft.com/en-us/library/aa906206.aspx
        /// </summary>
        public static int CheckRemoved(string deviceHardwareID)
        {
            int devIndex;
            int removed = 0;
            bool bSuccess;

            // Initialize the SP_DEVINFO_DATA structure
            SetupApi.SP_DEVINFO_DATA devInfoData = new SetupApi.SP_DEVINFO_DATA();
            devInfoData.cbSize = (uint) Marshal.SizeOf(typeof (SetupApi.SP_DEVINFO_DATA));

            // List all connected USB devices
            IntPtr pDevInfo = SetupApi.SetupDiGetClassDevs(0, "USB", IntPtr.Zero, SetupApi.DICFG.ALLCLASSES);
            if (pDevInfo == IntPtr.Zero || pDevInfo == new IntPtr(-1))
            {
                return removed;
            }
            for (devIndex = 0;; devIndex++)
            {
                bSuccess = SetupApi.SetupDiEnumDeviceInfo(pDevInfo, devIndex, ref devInfoData);

                // Reached the end of the deviceInfo list.
                if (!bSuccess) break;

                // Find the hardware ID
                string[] saHardwareIDs;
                bSuccess = SetupApi.SetupDiGetDeviceRegistryProperty(out saHardwareIDs,
                                                                     pDevInfo,
                                                                     ref devInfoData,
                                                                     SetupApi.SPDRP.HARDWAREID);

                // Failed getting hardware id
                if (!bSuccess) break;

                // Failed getting hardware id
                if (saHardwareIDs.Length == 0) continue;

                // Check all hardwareids for a match 
                bool bFound = false;
                foreach (string s in saHardwareIDs)
                {
                    if (s.Trim().ToLower() == deviceHardwareID.Trim().ToLower())
                    {
                        bFound = true;
                        break;
                    }
                }

                // Hardware did not match; goto next device
                if (!bFound) continue;

                uint status;
                uint pbmNumber;

                // If not Unplugged.
                bSuccess = (SetupApi.CM_Get_DevNode_Status(out status, out pbmNumber, devInfoData.DevInst, 0) == SetupApi.CR.NO_SUCH_DEVNODE);
                if (!bSuccess) continue;

                // Flag for reinstall on next plugin
                uint configFlags;
                bSuccess = SetupApi.SetupDiGetDeviceRegistryProperty(out configFlags, pDevInfo, ref devInfoData, SetupApi.SPDRP.CONFIGFLAGS);
                if (!bSuccess)
                {
                    InfWizardStatus.Log(CategoryType.CheckRemoved,
                                        StatusType.Warning,
                                        "could not read SPDRP_CONFIGFLAGS for phantom device {0}",
                                        deviceHardwareID);
                    continue;
                }

                // Mark for re-installation
                configFlags |= (uint) SetupApi.CONFIGFLAG.REINSTALL;
                uint[] flags = new uint[] {configFlags};
                bSuccess = SetupApi.SetupDiSetDeviceRegistryProperty(pDevInfo,
                                                                     ref devInfoData,
                                                                     SetupApi.SPDRP.CONFIGFLAGS,
                                                                     flags,
                                                                     Marshal.SizeOf(typeof (uint)));
                if (!bSuccess)
                {
                    InfWizardStatus.Log(CategoryType.CheckRemoved,
                                        StatusType.Warning,
                                        "could not write SPDRP_CONFIGFLAGS for phantom device {0}",
                                        deviceHardwareID);
                    continue;
                }
                removed++;
            }

            if (removed > 0)
            {
                InfWizardStatus.Log(CategoryType.CheckRemoved, StatusType.Info, "flagged {0} removed devices for reinstallation", removed);
            }

            SetupApi.SetupDiDestroyDeviceInfoList(pDevInfo);

            return removed;
        }

        public static bool InstallSetupPackage(string hardwareId, string infPath)
        {
            bool bSuccess = SetupApi.UpdateDriverForPlugAndPlayDevices(IntPtr.Zero,
                                                                       hardwareId,
                                                                       infPath,
                                                                       SetupApi.INSTALLFLAG.FORCE,
                                                                       IntPtr.Zero);
            if (bSuccess)
            {
                InfWizardStatus.Log(CategoryType.InstallSetupPackage, StatusType.Success, "driver update completed");
                UpdateDriver(hardwareId);
                return true;
            }

            // UpdateDriverForPlugAndPlayDevices FAILED
            ECODE setupError = (ECODE) Marshal.GetLastWin32Error();
            if (setupError != ECODE.NO_SUCH_DEVINST)
            {
                InfWizardStatus.Log(CategoryType.InstallSetupPackage, StatusType.Win32Error, "UpdateDriverForPlugAndPlayDevices failed");
                return false;
            }
            // UpdateDriverForPlugAndPlayDevices = NO_SUCH_DEVINST
            InfWizardStatus.Log(CategoryType.InstallSetupPackage,
                                StatusType.Warning,
                                "device not detected (copying driver files for next time device is plugged in)");

            StringBuilder sbDestInfFilename = new StringBuilder(1024);
            uint requiredSize;
            bSuccess = SetupApi.SetupCopyOEMInf(infPath,
                                                null,
                                                SetupApi.SPOST.SPOST_PATH,
                                                0,
                                                sbDestInfFilename,
                                                (uint) sbDestInfFilename.Capacity,
                                                out requiredSize,
                                                null);

            if (!bSuccess)
            {
                InfWizardStatus.Log(CategoryType.InstallSetupPackage,
                                    StatusType.Win32Error,
                                    "SetupCopyOEMInf failed");
            }
            else
            {
                InfWizardStatus.Log(CategoryType.InstallSetupPackage, StatusType.Success, "copied inf to {0}", sbDestInfFilename.ToString());
            }
            CheckRemoved(hardwareId);
            return bSuccess;
        }
    }
}