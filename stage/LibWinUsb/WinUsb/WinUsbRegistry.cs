// Copyright © 2006-2009 Travis Robinson. All rights reserved.
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
using LibUsbDotNet.Internal;
using LibUsbDotNet.Main;
using Microsoft.Win32;

namespace LibUsbDotNet.WinUsb
{
    /// <summary> WinUsb specific members for device registry settings.
    /// </summary> 
    public class WinUsbRegistry : UsbRegistry
    {
        internal WinUsbRegistry() { }

        private WinUsbRegistry(WinUsbRegistry winUSBRegistry, string devicePath)
        {
            foreach (KeyValuePair<string, object> deviceProperty in winUSBRegistry.mDeviceProperties)
            {
                mDeviceProperties.Add(deviceProperty.Key, deviceProperty.Value);
            }
            mDeviceProperties[SYMBOLIC_NAME_KEY] = devicePath;
        }

        /// <summary>
        /// Gets a list of available LibUsb devices.
        /// </summary>
        public static List<WinUsbRegistry> DeviceList
        {
            get
            {
                List<WinUsbRegistry> deviceList = new List<WinUsbRegistry>();
                SetupApi.EnumClassDevs(null, SetupApi.DICFG.ALLCLASSES | SetupApi.DICFG.PRESENT, WinUsbRegistryCallBack, deviceList);

                List<WinUsbRegistry> addList = new List<WinUsbRegistry>();
                List<WinUsbRegistry> delList = new List<WinUsbRegistry>();

                foreach (WinUsbRegistry winUsbRegistry in deviceList)
                {
                    // WinUSB device interfaces from a composite device do not have a 
                    // SynbolicName which LibsUsbDotNet uses to Open WinUsb devices.
                    if (winUsbRegistry.SymbolicName == String.Empty)
                    {
                        delList.Add(winUsbRegistry);
                        foreach (Guid g in winUsbRegistry.DeviceInterfaceGuids)
                        {
                            List<String> devicePaths;
                            if (SetupApi.GetDevicePath(g, out devicePaths))
                            {
                                foreach (string devicePath in devicePaths)
                                {
                                    // Use the DevicePath as the SymbolicName
                                    addList.Add(new WinUsbRegistry(winUsbRegistry, devicePath));
                                }
                            }
                        }
                    }
                }
                foreach (WinUsbRegistry delWinUSBRegistry in delList)
                {
                    deviceList.Remove(delWinUSBRegistry);
                }
                foreach (WinUsbRegistry addWinUSBRegistry in addList)
                {
                    bool bFound = false;
                    foreach (WinUsbRegistry winUSBRegistry in deviceList)
                    {
                        if (winUSBRegistry.SymbolicName == addWinUSBRegistry.SymbolicName)
                        {
                            bFound = true;
                            break;
                        }
                    }
                    if (!bFound)
                    {
                        deviceList.Add(addWinUSBRegistry);
                    }
                }
                return deviceList;
            }
        }

        /// <summary>
        /// Gets a collection of DeviceInterfaceGuids that are associated with this WinUSB device.
        /// </summary>
        public override Guid[] DeviceInterfaceGuids
        {
            get
            {
                if (ReferenceEquals(mDeviceInterfaceGuids, null))
                {
                    if (!mDeviceProperties.ContainsKey(DEVICE_INTERFACE_GUIDS)) return new Guid[0];

                    string[] saDeviceInterfaceGuids = (string[]) mDeviceProperties[DEVICE_INTERFACE_GUIDS];
                    mDeviceInterfaceGuids = new Guid[saDeviceInterfaceGuids.Length];
                    for (int i = 0; i < saDeviceInterfaceGuids.Length; i++)
                    {
                        string sGuid = saDeviceInterfaceGuids[i].Trim(new char[] {' ', '{', '}', '[', ']', '\0'});
                        mDeviceInterfaceGuids[i] = new Guid(sGuid);
                    }
                }
                return mDeviceInterfaceGuids;
            }
        }

        /// <summary>
        /// Check this value to determine if the usb device is still connected to the bus and ready to open.
        /// </summary>
        /// <remarks>
        /// Uses the symbolic name as a unique id to determine if this device instance is still attached.
        /// </remarks>
        /// <exception cref="UsbException">An exception is thrown if the <see cref="UsbRegistry.SymbolicName"/> property is null or empty.</exception>
        public override bool IsAlive
        {
            get
            {
                if (String.IsNullOrEmpty(SymbolicName)) throw new UsbException(this, "A symbolic name is required for this property.");

                List<WinUsbRegistry> deviceList = DeviceList;
                foreach (WinUsbRegistry registry in deviceList)
                {
                    if (String.IsNullOrEmpty(registry.SymbolicName)) continue;

                    if (registry.SymbolicName == SymbolicName)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Opens the USB device for communucation.
        /// </summary>
        /// <returns>Return a new instance of the <see cref="UsbDevice"/> class.
        /// If the device fails to open a null refrence is return. For extended error
        /// information use the <see cref="UsbDevice.UsbErrorEvent"/>.
        ///  </returns>
        public override UsbDevice Device
        {
            get
            {
                WinUsbDevice winUsbDevice;
                Open(out winUsbDevice);
                return winUsbDevice;
            }
        }

        /// <summary>
        /// Opens the USB device for communucation.
        /// </summary>
        /// <param name="usbDevice">The newly created UsbDevice.</param>
        /// <returns>True on success.</returns>
        public override bool Open(out UsbDevice usbDevice)
        {
            usbDevice = null;
            WinUsbDevice winUsbDevice;
            bool bSuccess = Open(out winUsbDevice);
            if (bSuccess)
                usbDevice = winUsbDevice;
            return bSuccess;
        }

        /// <summary>
        /// Opens the USB device for communucation.
        /// </summary>
        /// <param name="usbDevice">Returns an opened WinUsb device on success, null on failure.</param>
        /// <returns>True on success.</returns>
        public bool Open(out WinUsbDevice usbDevice)
        {
            usbDevice = null;

            if (String.IsNullOrEmpty(SymbolicName)) return false;
            String[] devInterfaceGuids = (string[]) mDeviceProperties[DEVICE_INTERFACE_GUIDS];

            UsbSymbolicName symbolicNameToMatch = UsbSymbolicName.Parse(SymbolicName);

            if (devInterfaceGuids != null && devInterfaceGuids.Length > 0)
            {
                foreach (string s in devInterfaceGuids)
                {
                    Guid guidInterface = new Guid(s);
                    List<string> devicePathList;
                    bool bSuccess = SetupApi.GetDevicePath(guidInterface, out devicePathList);
                    if (bSuccess)
                    {
                        foreach (string devicePath in devicePathList)
                        {
                            UsbSymbolicName symbolicName = UsbSymbolicName.Parse(devicePath);
                            if (symbolicNameToMatch.SerialNumber == symbolicName.SerialNumber)
                            {
                                if (WinUsbDevice.Open(devicePath, out usbDevice))
                                {
                                    usbDevice.mUsbRegistry = this;
                                    return true;
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }


        private static bool WinUsbRegistryCallBack(IntPtr deviceInfoSet,
                                                   int deviceIndex,
                                                   ref SetupApi.SP_DEVINFO_DATA deviceInfoData,
                                                   object classEnumeratorCallbackParam1)
        {
            List<WinUsbRegistry> deviceList = (List<WinUsbRegistry>) classEnumeratorCallbackParam1;

            RegistryValueKind propertyType;
            byte[] propBuffer = new byte[256];
            int requiredSize;
            bool bSuccess = SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet,
                                                                    ref deviceInfoData,
                                                                    DEVICE_INTERFACE_GUIDS,
                                                                    SetupApi.DICUSTOMDEVPROP.NONE,
                                                                    out propertyType,
                                                                    propBuffer,
                                                                    propBuffer.Length,
                                                                    out requiredSize);
            if (bSuccess)
            {
                string[] devInterfaceGuids = GetAsStringArray(propBuffer, requiredSize);

                WinUsbRegistry regInfo = new WinUsbRegistry();
                regInfo.mDeviceProperties.Add(DEVICE_INTERFACE_GUIDS, devInterfaceGuids);

                bSuccess =
                    SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet,
                                                            ref deviceInfoData,
                                                            SYMBOLIC_NAME_KEY,
                                                            SetupApi.DICUSTOMDEVPROP.NONE,
                                                            out propertyType,
                                                            propBuffer,
                                                            propBuffer.Length,
                                                            out requiredSize);
                string symbolicName = String.Empty;
                if (!bSuccess)
                {
                }
                else
                {
                    symbolicName = GetAsString(propBuffer, requiredSize);
                }
                regInfo.mDeviceProperties.Add(SYMBOLIC_NAME_KEY, symbolicName);
                SetupApi.getSPDRPProperties(deviceInfoSet, ref deviceInfoData, regInfo.mDeviceProperties);
                deviceList.Add(regInfo);
            }

            return false;
        }
    }
}