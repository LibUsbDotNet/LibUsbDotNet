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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace WinApiNet
{
    public class SetupApi
    {
        #region Enumerations

        public enum CR
        {
            SUCCESS = (0x00000000),
            DEFAULT = (0x00000001),
            OUT_OF_MEMORY = (0x00000002),
            INVALID_POINTER = (0x00000003),
            INVALID_FLAG = (0x00000004),
            INVALID_DEVNODE = (0x00000005),
            INVALID_DEVINST = INVALID_DEVNODE,
            INVALID_RES_DES = (0x00000006),
            INVALID_LOG_CONF = (0x00000007),
            INVALID_ARBITRATOR = (0x00000008),
            INVALID_NODELIST = (0x00000009),
            DEVNODE_HAS_REQS = (0x0000000A),
            DEVINST_HAS_REQS = DEVNODE_HAS_REQS,
            INVALID_RESOURCEID = (0x0000000B),
            DLVXD_NOT_FOUND = (0x0000000C), // WIN 95 ONLY
            NO_SUCH_DEVNODE = (0x0000000D),
            NO_SUCH_DEVINST = NO_SUCH_DEVNODE,
            NO_MORE_LOG_CONF = (0x0000000E),
            NO_MORE_RES_DES = (0x0000000F),
            ALREADY_SUCH_DEVNODE = (0x00000010),
            ALREADY_SUCH_DEVINST = ALREADY_SUCH_DEVNODE,
            INVALID_RANGE_LIST = (0x00000011),
            INVALID_RANGE = (0x00000012),
            FAILURE = (0x00000013),
            NO_SUCH_LOGICAL_DEV = (0x00000014),
            CREATE_BLOCKED = (0x00000015),
            NOT_SYSTEM_VM = (0x00000016), // WIN 95 ONLY
            REMOVE_VETOED = (0x00000017),
            APM_VETOED = (0x00000018),
            INVALID_LOAD_TYPE = (0x00000019),
            BUFFER_SMALL = (0x0000001A),
            NO_ARBITRATOR = (0x0000001B),
            NO_REGISTRY_HANDLE = (0x0000001C),
            REGISTRY_ERROR = (0x0000001D),
            INVALID_DEVICE_ID = (0x0000001E),
            INVALID_DATA = (0x0000001F),
            INVALID_API = (0x00000020),
            DEVLOADER_NOT_READY = (0x00000021),
            NEED_RESTART = (0x00000022),
            NO_MORE_HW_PROFILES = (0x00000023),
            DEVICE_NOT_THERE = (0x00000024),
            NO_SUCH_VALUE = (0x00000025),
            WRONG_TYPE = (0x00000026),
            INVALID_PRIORITY = (0x00000027),
            NOT_DISABLEABLE = (0x00000028),
            FREE_RESOURCES = (0x00000029),
            QUERY_VETOED = (0x0000002A),
            CANT_SHARE_IRQ = (0x0000002B),
            NO_DEPENDENT = (0x0000002C),
            SAME_RESOURCES = (0x0000002D),
            NO_SUCH_REGISTRY_KEY = (0x0000002E),
            INVALID_MACHINENAME = (0x0000002F), // NT ONLY
            REMOTE_COMM_FAILURE = (0x00000030), // NT ONLY
            MACHINE_UNAVAILABLE = (0x00000031), // NT ONLY
            NO_CM_SERVICES = (0x00000032), // NT ONLY
            ACCESS_DENIED = (0x00000033), // NT ONLY
            CALL_NOT_IMPLEMENTED = (0x00000034),
            INVALID_PROPERTY = (0x00000035),
            DEVICE_INTERFACE_ACTIVE = (0x00000036),
            NO_SUCH_DEVICE_INTERFACE = (0x00000037),
            INVALID_REFERENCE_STRING = (0x00000038),
            INVALID_CONFLICT_LIST = (0x00000039),
            INVALID_INDEX = (0x0000003A),
            INVALID_STRUCTURE_SIZE = (0x0000003B),
            NUM_CR_RESULTS = (0x0000003C)
        }


        public enum DeviceInterfaceDataFlags : uint
        {
            Active = 0x00000001,
            Default = 0x00000002,
            Removed = 0x00000004
        }


        [Flags]
        public enum DICFG
        {
            /// <summary>
            /// Return only the device that is associated with the system default device interface, if one is set, for the specified device interface classes. 
            ///  only valid with <see cref="DEVICEINTERFACE"/>.
            /// </summary>
            DEFAULT = 0x00000001,
            /// <summary>
            /// Return only devices that are currently present in a system. 
            /// </summary>
            PRESENT = 0x00000002,
            /// <summary>
            /// Return a list of installed devices for all device setup classes or all device interface classes. 
            /// </summary>
            ALLCLASSES = 0x00000004,
            /// <summary>
            /// Return only devices that are a part of the current hardware profile. 
            /// </summary>
            PROFILE = 0x00000008,
            /// <summary>
            /// Return devices that support device interfaces for the specified device interface classes. 
            /// </summary>
            DEVICEINTERFACE = 0x00000010,
        }


        public enum DICUSTOMDEVPROP : uint
        {
            NONE = 0,
            MERGE_MULTISZ = 0x00000001,
        }


        public enum ErrorCodes
        {
            ERROR_NO_MORE_ITEMS = -259,
            ERROR_DEVICE_NOT_CONNECTED = -1167
        }

        public enum SPDIT
        {
            NODRIVER = 0x00000000,
            CLASSDRIVER = 0x00000001,
            COMPATDRIVER = 0x00000002,
        }


        /// <summary>
        ///
        /// Device registry property codes
        /// (Codes marked as read-only (R) may only be used for
        /// SetupDiGetDeviceRegistryProperty)
        ///
        /// These values should cover the same set of registry properties
        /// as defined by the CM_DRP codes in cfgmgr32.h.
        ///
        /// Note that SPDRP codes are zero based while CM_DRP codes are one based!
        /// </summary>
        public enum SPDRP
        {
            DEVICEDESC = (0x00000000), // DeviceDesc (R/W)
            HARDWAREID = (0x00000001), // HardwareID (R/W)
            COMPATIBLEIDS = (0x00000002), // CompatibleIDs (R/W)
            UNUSED0 = (0x00000003), // unused
            SERVICE = (0x00000004), // Service (R/W)
            UNUSED1 = (0x00000005), // unused
            UNUSED2 = (0x00000006), // unused
            CLASS = (0x00000007), // Class (R--tied to ClassGUID)
            CLASSGUID = (0x00000008), // ClassGUID (R/W)
            DRIVER = (0x00000009), // Driver (R/W)
            CONFIGFLAGS = (0x0000000A), // ConfigFlags (R/W)
            MFG = (0x0000000B), // Mfg (R/W)
            FRIENDLYNAME = (0x0000000C), // FriendlyName (R/W)
            LOCATION_INFORMATION = (0x0000000D), // LocationInformation (R/W)
            PHYSICAL_DEVICE_OBJECT_NAME = (0x0000000E), // PhysicalDeviceObjectName (R)
            CAPABILITIES = (0x0000000F), // Capabilities (R)
            UI_NUMBER = (0x00000010), // UiNumber (R)
            UPPERFILTERS = (0x00000011), // UpperFilters (R/W)
            LOWERFILTERS = (0x00000012), // LowerFilters (R/W)
            BUSTYPEGUID = (0x00000013), // BusTypeGUID (R)
            LEGACYBUSTYPE = (0x00000014), // LegacyBusType (R)
            BUSNUMBER = (0x00000015), // BusNumber (R)
            ENUMERATOR_NAME = (0x00000016), // Enumerator Name (R)
            SECURITY = (0x00000017), // Security (R/W, binary form)
            SECURITY_SDS = (0x00000018), // Security (W, SDS form)
            DEVTYPE = (0x00000019), // Device Type (R/W)
            EXCLUSIVE = (0x0000001A), // Device is exclusive-access (R/W)
            CHARACTERISTICS = (0x0000001B), // Device Characteristics (R/W)
            ADDRESS = (0x0000001C), // Device Address (R)
            UI_NUMBER_DESC_FORMAT = (0X0000001D), // UiNumberDescFormat (R/W)
            DEVICE_POWER_DATA = (0x0000001E), // Device Power Data (R)
            REMOVAL_POLICY = (0x0000001F), // Removal Policy (R)
            REMOVAL_POLICY_HW_DEFAULT = (0x00000020), // Hardware Removal Policy (R)
            REMOVAL_POLICY_OVERRIDE = (0x00000021), // Removal Policy Override (RW)
            INSTALL_STATE = (0x00000022), // Device Install State (R)
            MAXIMUM_PROPERTY = (0x00000023), // Upper bound on ordinals
        }

        #endregion

        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;


        public const int LINE_LEN = 512;

        public static string mLastErrorString = "";

        public static string LastErrorString
        {
            get
            {
                return mLastErrorString;
            }
        }

        private static string FormatSystemMessage(int dwMessageId)
        {
            StringBuilder sbSystemMessage = new StringBuilder(1024);

                int ret = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM,
                                        IntPtr.Zero,
                                        dwMessageId,
                                        CultureInfo.CurrentCulture.LCID,
                                        sbSystemMessage,
                                        sbSystemMessage.Capacity - 1,
                                        IntPtr.Zero);

                if (ret > 0) return sbSystemMessage.ToString(0, ret);
                return null;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int FormatMessage(int dwFlags,
                                                IntPtr lpSource,
                                                int dwMessageId,
                                                int dwLanguageId,
                                                [Out] StringBuilder lpBuffer,
                                                int nSize,
                                                IntPtr lpArguments);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
        /// <param name="Buffer">Address of a buffer to receive a device instance ID string. The required buffer size can be obtained by calling CM_Get_Device_ID_Size, then incrementing the received value to allow room for the string's terminating NULL. </param>
        /// <param name="BufferLen">Caller-supplied length, in characters, of the buffer specified by Buffer. </param>
        /// <param name="ulFlags">Not used. set to 0.</param>
        /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in cfgmgr32.h.</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern CR CM_Get_Device_ID(IntPtr dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);


        /// <summary>
        /// The CM_Get_Parent function obtains a device instance handle to the parent node of a specified device node, in the local machine's device tree.
        /// </summary>
        /// <param name="pdnDevInst">Caller-supplied pointer to the device instance handle to the parent node that this function retrieves. The retrieved handle is bound to the local machine.</param>
        /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine. </param>
        /// <param name="ulFlags">Not used. set to 0.</param>
        /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in cfgmgr32.h.</returns>
        [DllImport("setupapi.dll")]
        public static extern CR CM_Get_Parent(out IntPtr pdnDevInst, IntPtr dnDevInst, int ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto /*, SetLastError = true*/)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        /// <summary>
        /// The SetupDiEnumDeviceInterfaces function enumerates the device interfaces that are contained in a device information set. 
        /// </summary>
        /// <param name="hDevInfo">A pointer to a device information set that contains the device interfaces for which to return information. This handle is typically returned by SetupDiGetClassDevs. </param>
        /// <param name="devInfo">A pointer to an SP_DEVINFO_DATA structure that specifies a device information element in DeviceInfoSet. This parameter is optional and can be NULL. If this parameter is specified, SetupDiEnumDeviceInterfaces constrains the enumeration to the interfaces that are supported by the specified device. If this parameter is NULL, repeated calls to SetupDiEnumDeviceInterfaces return information about the interfaces that are associated with all the device information elements in DeviceInfoSet. This pointer is typically returned by SetupDiEnumDeviceInfo. </param>
        /// <param name="interfaceClassGuid">A pointer to a GUID that specifies the device interface class for the requested interface. </param>
        /// <param name="memberIndex">A zero-based index into the list of interfaces in the device information set. The caller should call this function first with MemberIndex set to zero to obtain the first interface. Then, repeatedly increment MemberIndex and retrieve an interface until this function fails and GetLastError returns ERROR_NO_MORE_ITEMS.  If DeviceInfoData specifies a particular device, the MemberIndex is relative to only the interfaces exposed by that device.</param>
        /// <param name="deviceInterfaceData">A pointer to a caller-allocated buffer that contains, on successful return, a completed SP_DEVICE_INTERFACE_DATA structure that identifies an interface that meets the search parameters. The caller must set DeviceInterfaceData.cbSize to sizeof(SP_DEVICE_INTERFACE_DATA) before calling this function. </param>
        /// <returns></returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr hDevInfo,
                                                                 ref SP_DEVINFO_DATA devInfo,
                                                                 ref Guid interfaceClassGuid,
                                                                 UInt32 memberIndex,
                                                                 ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr hDevInfo,
                                                                 [MarshalAs(UnmanagedType.AsAny)] object devInfo,
                                                                 ref Guid interfaceClassGuid,
                                                                 UInt32 memberIndex,
                                                                 ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDriverInfo(IntPtr DeviceInfoSet,
                                                        IntPtr DeviceInfoData,
                                                        SPDIT DriverType,
                                                        int MemberIndex,
                                                        ref SP_DRVINFO_DATA DriverInfoData);

        /// <summary>
        /// The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local machine. 
        /// </summary>
        /// <param name="ClassGuid">A pointer to the GUID for a device setup class or a device interface class. This pointer is optional and can be NULL. For more information about how to set ClassGuid, see the following Comments section. </param>
        /// <param name="Enumerator">A pointer to a NULL-terminated string that supplies the name of a Plug and Play (PnP) enumerator or a PnP device instance identifier. This pointer is optional and can be NULL. For more information about how to set the Enumerator value, see the following Comments section. </param>
        /// <param name="hwndParent">A handle of the top-level window to be used for a user interface that is associated with installing a device instance in the device information set. This handle is optional and can be NULL. </param>
        /// <param name="Flags">A variable of type DWORD that specifies control options that filter the device information elements that are added to the device information set. This parameter can be a bitwise OR of zero or more of the following flags.</param>
        /// <returns></returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetClassDevsA")]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid,
                                                        [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
                                                        IntPtr hwndParent,
                                                        DICFG Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetClassDevsA")]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, int Enumerator, IntPtr hwndParent, DICFG Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetClassDevsA")]
        public static extern IntPtr SetupDiGetClassDevs(int ClassGuid, string Enumerator, IntPtr hwndParent, DICFG Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetCustomDeviceProperty(IntPtr DeviceInfoSet,
                                                                 ref SP_DEVINFO_DATA DeviceInfoData,
                                                                 string CustomPropertyName,
                                                                 DICUSTOMDEVPROP Flags,
                                                                 out RegistryValueKind PropertyRegDataType,
                                                                 Byte[] PropertyBuffer,
                                                                 int PropertyBufferSize,
                                                                 out int RequiredSize);

        /// <summary>
        /// The SetupDiGetDeviceInstanceId function retrieves the device instance ID that is associated with a device information element.
        /// </summary>
        /// <param name="DeviceInfoSet">A handle to the device information set that contains the device information element that represents the device for which to retrieve a device instance ID. </param>
        /// <param name="DeviceInfoData">A pointer to an SP_DEVINFO_DATA structure that specifies the device information element in DeviceInfoSet. </param>
        /// <param name="DeviceInstanceId">A pointer to the character buffer that will receive the NULL-terminated device instance ID for the specified device information element. For information about device instance IDs, see Device Identification Strings.</param>
        /// <param name="DeviceInstanceIdSize">The size, in characters, of the DeviceInstanceId buffer. </param>
        /// <param name="RequiredSize">A pointer to the variable that receives the number of characters required to store the device instance ID.</param>
        /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved with a call to GetLastError.</returns>
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetDeviceInstanceIdA")]
        public static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet,
                                                             ref SP_DEVINFO_DATA DeviceInfoData,
                                                             StringBuilder DeviceInstanceId,
                                                             int DeviceInstanceIdSize,
                                                             out int RequiredSize);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo,
                                                                     ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                                                                     DEVICE_INTERFACE_DETAIL_HANDLE deviceInterfaceDetailData,
                                                                     UInt32 deviceInterfaceDetailDataSize,
                                                                     out UInt32 requiredSize,
                                                                     [MarshalAs(UnmanagedType.AsAny)] object deviceInfoData);

        /// <summary>
        /// The SetupDiGetDeviceRegistryProperty function retrieves the specified device property.
        /// This handle is typically returned by the SetupDiGetClassDevs or SetupDiGetClassDevsEx function.
        /// </summary>
        /// <param Name="DeviceInfoSet">Handle to the device information set that contains the interface and its underlying device.</param>
        /// <param Name="DeviceInfoData">Pointer to an SP_DEVINFO_DATA structure that defines the device instance.</param>
        /// <param Name="Property">Device property to be retrieved. SEE MSDN</param>
        /// <param Name="PropertyRegDataType">Pointer to a variable that receives the registry data Type. This parameter can be NULL.</param>
        /// <param Name="PropertyBuffer">Pointer to a buffer that receives the requested device property.</param>
        /// <param Name="PropertyBufferSize">Size of the buffer, in bytes.</param>
        /// <param Name="RequiredSize">Pointer to a variable that receives the required buffer size, in bytes. This parameter can be NULL.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet,
                                                                   ref SP_DEVINFO_DATA DeviceInfoData,
                                                                   SPDRP Property,
                                                                   out RegistryValueKind PropertyRegDataType,
                                                                   byte[] PropertyBuffer,
                                                                   int PropertyBufferSize,
                                                                   out int RequiredSize);

        /*BOOL  SetupDiRemoveDevice(IN HDEVINFO  DeviceInfoSet, IN OUT PSP_DEVINFO_DATA  DeviceInfoData); */

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiRemoveDevice(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);


        public static int GetDevicePath(Guid InterfaceGuid, out String DevicePath)
        {
            bool bResult = false;
            DevicePath = null;

            IntPtr deviceInfo = IntPtr.Zero;

            SP_DEVICE_INTERFACE_DATA interfaceData = SP_DEVICE_INTERFACE_DATA.Empty;
            DeviceInterfaceDetailHelper detailHelper;

            // [1]
            deviceInfo = SetupDiGetClassDevs(ref InterfaceGuid, null, IntPtr.Zero, DICFG.PRESENT | DICFG.DEVICEINTERFACE);
            if (deviceInfo != IntPtr.Zero)
            {

                bResult = SetupDiEnumDeviceInterfaces(deviceInfo, null, ref InterfaceGuid, 0, ref interfaceData);
                if (bResult)
                {
                    uint length = 1024;
                    detailHelper = new DeviceInterfaceDetailHelper(length);
                    bResult = SetupDiGetDeviceInterfaceDetail(deviceInfo, ref interfaceData, detailHelper.Handle, length, out length, null);
                    if (bResult)
                    {
                        DevicePath = detailHelper.DevicePath;
                    }
                }
                SetupDiDestroyDeviceInfoList(deviceInfo);
            }
            if (!bResult)
                return ShowWin32Error("GetDevicePath");

            return 0;
        }

        public static bool SetupDiGetDeviceInterfaceDetailLength(IntPtr hDevInfo,
                                                                 ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                                                                 out uint requiredLength)
        {
            DEVICE_INTERFACE_DETAIL_HANDLE tmp = new DEVICE_INTERFACE_DETAIL_HANDLE();
            return SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, tmp, 0, out requiredLength, null);
        }

        public static bool SetupDiGetDeviceRegistryProperty(out byte[] regBytes,
                                                            IntPtr DeviceInfoSet,
                                                            ref SP_DEVINFO_DATA DeviceInfoData,
                                                            SPDRP Property)
        {
            regBytes = null;
            byte[] tmp = new byte[1024];
            int iReqSize;
            RegistryValueKind regValueType;
            if (!SetupDiGetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, Property, out regValueType, tmp, tmp.Length, out iReqSize))
            {
                //usb_error("usb_registry_match_no_hubs(): getting hardware id failed");
                return false;
            }
            regBytes = new byte[iReqSize];
            Array.Copy(tmp, regBytes, regBytes.Length);
            return true;
        }

        public static bool SetupDiGetDeviceRegistryProperty(out string regSZ, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property)
        {
            regSZ = null;
            byte[] tmp;
            if (SetupDiGetDeviceRegistryProperty(out tmp, DeviceInfoSet, ref DeviceInfoData, Property))
            {
                regSZ = Encoding.Unicode.GetString(tmp).TrimEnd(new char[] {'\0'});
                return true;
            }
            return false;
        }

        public static bool SetupDiGetDeviceRegistryProperty(out string[] regMultiSZ,
                                                            IntPtr DeviceInfoSet,
                                                            ref SP_DEVINFO_DATA DeviceInfoData,
                                                            SPDRP Property)
        {
            regMultiSZ = null;
            string tmp;
            if (SetupDiGetDeviceRegistryProperty(out tmp, DeviceInfoSet, ref DeviceInfoData, Property))
            {
                regMultiSZ = tmp.Split(new char[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            return false;
        }
        public static bool GetLastWin32ErrorDetails(string text, out string errorString)
        {
            errorString = text;
            int errorNumber = Marshal.GetLastWin32Error();
            if (errorNumber == 0) return false;

            errorString = FormatSystemMessage(errorNumber);
            if (errorString == null) return false;

            errorString = String.Format("{0}\r\n{1}:{2}",text,errorNumber,errorString);
            Debug.Print(errorString);
            return true;
        }

        public static int ShowWin32Error(string funcName)
        {
            int iError = Marshal.GetLastWin32Error();
            mLastErrorString = funcName + "\r\n" + FormatSystemMessage(iError);
            Debug.Print(String.Format("{0}:{1} - {2}", funcName, iError, mLastErrorString));
            return -Math.Abs(iError);
        }

        #region Nested Types

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVICE_INTERFACE_DETAIL_HANDLE
        {
            private IntPtr mPtr;

            internal DEVICE_INTERFACE_DETAIL_HANDLE(IntPtr ptrInit)
            {
                mPtr = ptrInit;
            }
        }

        public class DeviceInterfaceDetailHelper
        {
            public static readonly uint SIZE = (uint) (Marshal.SizeOf(typeof (uint)) + Marshal.SystemDefaultCharSize);
            private IntPtr mpDevicePath;
            private IntPtr mpStructure;

            public DeviceInterfaceDetailHelper(uint maximumLength)
            {
                mpStructure = Marshal.AllocHGlobal((int) maximumLength);
                mpDevicePath = new IntPtr(mpStructure.ToInt64() + Marshal.SizeOf(typeof (uint)));
            }

            public DEVICE_INTERFACE_DETAIL_HANDLE Handle
            {
                get
                {
                    Marshal.WriteInt32(mpStructure, (int) SIZE);
                    return new DEVICE_INTERFACE_DETAIL_HANDLE(mpStructure);
                }
            }

            public string DevicePath
            {
                get
                {
                    return Marshal.PtrToStringAuto(mpDevicePath);
                }
            }


            public void Free()
            {
                if (mpStructure != IntPtr.Zero)
                    Marshal.FreeHGlobal(mpStructure);

                mpDevicePath = IntPtr.Zero;
                mpStructure = IntPtr.Zero;
            }

            ~DeviceInterfaceDetailHelper()
            {
                Free();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public static readonly SP_DEVICE_INTERFACE_DATA Empty = new SP_DEVICE_INTERFACE_DATA(Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA)));

            public UInt32 cbSize;
            public Guid interfaceClassGuid;
            public UInt32 flags;
            private UIntPtr reserved;

            private SP_DEVICE_INTERFACE_DATA(int size)
            {
                cbSize = (uint)size;
                reserved = UIntPtr.Zero;
                flags = 0;
                interfaceClassGuid = Guid.Empty;
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public static readonly SP_DEVINFO_DATA Empty = new SP_DEVINFO_DATA(Marshal.SizeOf(typeof(SP_DEVINFO_DATA)));

            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;

            private SP_DEVINFO_DATA(int size)
            {
                cbSize = (uint)size;
                ClassGuid = Guid.Empty;
                DevInst = 0;
                Reserved = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DRVINFO_DATA
        {
            public static SP_DRVINFO_DATA New = new SP_DRVINFO_DATA(Marshal.SizeOf(typeof (SP_DRVINFO_DATA)));

            private SP_DRVINFO_DATA(int size)
            {
                cbSize = size;
                //DriverVersion = 0L;
                //DriverDate = new FILETIME();
                bProviderName = new byte[LINE_LEN];
                bMfgName = new byte[LINE_LEN];
                bDescription = new byte[LINE_LEN];
                Reserved = new IntPtr();
                DriverType = 0;
            }

            private int cbSize;
            public readonly int DriverType;
            private IntPtr Reserved;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LINE_LEN)] private readonly byte[] bDescription;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LINE_LEN)] private readonly byte[] bMfgName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LINE_LEN)] private readonly byte[] bProviderName;

            //public readonly FILETIME DriverDate;
            //public readonly Int64 DriverVersion;

            public string Description
            {
                get
                {
                    return Marshal.PtrToStringUni(Marshal.UnsafeAddrOfPinnedArrayElement(bDescription, 0));
                }
            }

            public string MfgName
            {
                get
                {
                    return Marshal.PtrToStringUni(Marshal.UnsafeAddrOfPinnedArrayElement(bMfgName, 0));
                }
            }

            public string ProviderName
            {
                get
                {
                    return Marshal.PtrToStringUni(Marshal.UnsafeAddrOfPinnedArrayElement(bProviderName, 0));
                }
            }
        }

        #endregion
    }
}