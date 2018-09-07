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
using InfWizard.WizardClassHelpers;
using Microsoft.Win32;

namespace WinApiNet
{
    public enum WindowsVersionType {
	WINDOWS_UNDEFINED,
	WINDOWS_UNSUPPORTED,
	WINDOWS_2K,
	WINDOWS_XP,
	WINDOWS_VISTA,
	WINDOWS_7
    }
    [Flags]
    public enum DevKeyType
    {
        DEV = 0x00000001,         // Open/Create/Delete device key
        DRV = 0x00000002,         // Open/Create/Delete driver key
        BOTH = 0x00000004,         // Delete both driver and Device key

    }
    public class SetupApi
    {
        #region Enumerations
        /// <summary>
        /// Define OEM Source Type values for use in SetupCopyOEMInf.
        /// </summary>
        public enum SPOST:uint 
        {
            SPOST_NONE = 0,
            SPOST_PATH = 1,
            SPOST_URL = 2,
            SPOST_MAX = 3,
        }

        [Flags]
        public enum CONFIGFLAG
        {
            REINSTALL=0x00000020
        }
        [Flags]
        public enum CM
        {
            REENUMERATE_NORMAL = 0x00000000,
            REENUMERATE_SYNCHRONOUS = 0x00000001,
            REENUMERATE_RETRY_INSTALLATION = 0x00000002,
            REENUMERATE_ASYNCHRONOUS = 0x00000004,
            REENUMERATE_BITS = 0x00000007,
        }

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


        public enum ErrorCodes:uint
        {
            APPLICATION_ERROR_MASK=0x20000000,

            ERROR_SEVERITY_SUCCESS = 0x00000000,
            ERROR_SEVERITY_INFORMATIONAL = 0x40000000,
            ERROR_SEVERITY_WARNING = 0x80000000,
            ERROR_SEVERITY_ERROR = 0xC0000000,

            //
            // Setupapi-specific error codes
            //
            // Inf parse outcomes
            //
            EXPECTED_SECTION_NAME = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0),
            BAD_SECTION_NAME_LINE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 1),
            SECTION_NAME_TOO_LONG = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 2),
            GENERAL_SYNTAX = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 3),
            //
            // Inf runtime errors
            //
            WRONG_INF_STYLE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x100),
            SECTION_NOT_FOUND = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x101),
            LINE_NOT_FOUND = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x102),
            NO_BACKUP = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x103),
            //
            // Device Installer/other errors
            //
            NO_ASSOCIATED_CLASS = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x200),
            CLASS_MISMATCH = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x201),
            DUPLICATE_FOUND = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x202),
            NO_DRIVER_SELECTED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x203),
            KEY_DOES_NOT_EXIST = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x204),
            INVALID_DEVINST_NAME = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x205),
            INVALID_CLASS = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x206),
            DEVINST_ALREADY_EXISTS = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x207),
            DEVINFO_NOT_REGISTERED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x208),
            INVALID_REG_PROPERTY = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x209),
            NO_INF = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x20A),
            NO_SUCH_DEVINST = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x20B),
            CANT_LOAD_CLASS_ICON = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x20C),
            INVALID_CLASS_INSTALLER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x20D),
            DI_DO_DEFAULT = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x20E),
            DI_NOFILECOPY = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x20F),
            INVALID_HWPROFILE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x210),
            NO_DEVICE_SELECTED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x211),
            DEVINFO_LIST_LOCKED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x212),
            DEVINFO_DATA_LOCKED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x213),
            DI_BAD_PATH = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x214),
            NO_CLASSINSTALL_PARAMS = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x215),
            FILEQUEUE_LOCKED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x216),
            BAD_SERVICE_INSTALLSECT = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x217),
            NO_CLASS_DRIVER_LIST = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x218),
            NO_ASSOCIATED_SERVICE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x219),
            NO_DEFAULT_DEVICE_INTERFACE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x21A),
            DEVICE_INTERFACE_ACTIVE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x21B),
            DEVICE_INTERFACE_REMOVED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x21C),
            BAD_INTERFACE_INSTALLSECT = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x21D),
            NO_SUCH_INTERFACE_CLASS = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x21E),
            INVALID_REFERENCE_STRING = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x21F),
            INVALID_MACHINENAME = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x220),
            REMOTE_COMM_FAILURE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x221),
            MACHINE_UNAVAILABLE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x222),
            NO_CONFIGMGR_SERVICES = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x223),
            INVALID_PROPPAGE_PROVIDER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x224),
            NO_SUCH_DEVICE_INTERFACE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x225),
            DI_POSTPROCESSING_REQUIRED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x226),
            INVALID_COINSTALLER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x227),
            NO_COMPAT_DRIVERS = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x228),
            NO_DEVICE_ICON = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x229),
            INVALID_INF_LOGCONFIG = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x22A),
            DI_DONT_INSTALL = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x22B),
            INVALID_FILTER_DRIVER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x22C),
            NON_WINDOWS_NT_DRIVER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x22D),
            NON_WINDOWS_DRIVER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x22E),
            NO_CATALOG_FOR_OEM_INF = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x22F),
            DEVINSTALL_QUEUE_NONNATIVE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x230),
            NOT_DISABLEABLE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x231),
            CANT_REMOVE_DEVINST = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x232),
            INVALID_TARGET = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x233),
            DRIVER_NONNATIVE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x234),
            IN_WOW64 = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x235),
            SET_SYSTEM_RESTORE_POINT = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x236),

            SCE_DISABLED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x238),
            UNKNOWN_EXCEPTION = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x239),
            PNP_REGISTRY_ERROR = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x23A),
            REMOTE_REQUEST_UNSUPPORTED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x23B),
            NOT_AN_INSTALLED_OEM_INF = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x23C),
            INF_IN_USE_BY_DEVICES = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x23D),
            DI_FUNCTION_OBSOLETE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x23E),
            NO_AUTHENTICODE_CATALOG = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x23F),
            AUTHENTICODE_DISALLOWED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x240),
            AUTHENTICODE_TRUSTED_PUBLISHER = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x241),
            AUTHENTICODE_TRUST_NOT_ESTABLISHED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x242),
            AUTHENTICODE_PUBLISHER_NOT_TRUSTED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x243),
            SIGNATURE_OSATTRIBUTE_MISMATCH = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x244),
            ONLY_VALIDATE_VIA_AUTHENTICODE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x245),
            DEVICE_INSTALLER_NOT_READY = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x246),
            DRIVER_STORE_ADD_FAILED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x247),
            DEVICE_INSTALL_BLOCKED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x248),
            DRIVER_INSTALL_BLOCKED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x249),
            WRONG_INF_TYPE = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x24A),
            FILE_HASH_NOT_IN_CATALOG = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x24B),
            DRIVER_STORE_DELETE_FAILED = (APPLICATION_ERROR_MASK | ERROR_SEVERITY_ERROR | 0x24C),

        }

        public enum SPDIT
        {
            NODRIVER = 0x00000000,
            CLASSDRIVER = 0x00000001,
            COMPATDRIVER = 0x00000002,
        }

        /// <summary>
        ///  Flags for UpdateDriverForPlugAndPlayDevices
        /// </summary>
        [Flags]
        public enum INSTALLFLAG : uint
        {
            /// <summary>
            /// Force the installation of the specified driver
            /// </summary>
            FORCE = 0x00000001,
            /// <summary>
            /// Do a read-only install (no file copy)
            /// </summary>
            READONLY = 0x00000002,
            /// <summary>
            /// No UI shown at all. API will fail if any UI must be shown.
            /// </summary>
            NONINTERACTIVE = 0x00000004,
            /// <summary>
            /// Mask all flag bits.
            /// </summary>
            BITS = 0x00000007
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

        [StructLayout(LayoutKind.Sequential)]
        internal class DEVPROPKEY
        {
            public DEVPROPKEY(Guid fmtid,ulong pid )
            {
                this.fmtid = fmtid;
                 this.pid = pid;
           }

            private Guid fmtid;
            private ulong pid;
        }

        internal static readonly DEVPROPKEY DEVPKEY_Device_BusReportedDeviceDesc =
            new DEVPROPKEY(new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), 4);

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

        private static WindowsVersionType _windowsVersion = WindowsVersionType.WINDOWS_UNDEFINED;
        public static WindowsVersionType WindowsVersion
        {
            get
            {
                if (_windowsVersion == WindowsVersionType.WINDOWS_UNDEFINED)
                {
                    _windowsVersion = WindowsVersionType.WINDOWS_UNSUPPORTED;
                    OperatingSystem os_version = Environment.OSVersion;
                    if (os_version.Platform == PlatformID.Win32NT)
                    {
                        if ((os_version.Version.Major == 5) && (os_version.Version.Minor == 0))
                        {
                            _windowsVersion = WindowsVersionType.WINDOWS_2K;
                        }
                        else if ((os_version.Version.Major == 5) && (os_version.Version.Minor == 1))
                        {
                            _windowsVersion = WindowsVersionType.WINDOWS_XP;
                        }
                        else if (os_version.Version.Major >= 6)
                        {
                            if (os_version.Version.Build < 7000)
                            {
                                _windowsVersion = WindowsVersionType.WINDOWS_VISTA;
                            }
                            else
                            {
                                _windowsVersion = WindowsVersionType.WINDOWS_7;
                            }
                        }
                    }
                }
                return _windowsVersion;
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


        [DllImport("newdev.dll", CharSet = CharSet.Auto , SetLastError = true)]
        public static extern bool UpdateDriverForPlugAndPlayDevices(IntPtr hwndParent,[In,MarshalAs(UnmanagedType.LPTStr)] string HardwareId,[In,MarshalAs(UnmanagedType.LPTStr)] string FullInfPath,INSTALLFLAG InstallFlags,IntPtr bRebootRequired);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
        /// <param name="Buffer">Address of a buffer to receive a device instance ID string. The required buffer size can be obtained by calling CM_Get_Device_ID_Size, then incrementing the received value to allow room for the string's terminating NULL. </param>
        /// <param name="BufferLen">Caller-supplied length, in characters, of the buffer specified by Buffer. </param>
        /// <param name="ulFlags">Not used. set to 0.</param>
        /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in cfgmgr32.h.</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern CR CM_Get_Device_ID(uint dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern CR CM_Get_Device_ID(uint dnDevInst, byte[] Buffer, int BufferLen, int ulFlags);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern CR CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, int ulFlags);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern CR CM_Get_Device_ID_Size(out ulong size, uint dnDevInst, int ulFlags);



        /// <summary>
        /// The CM_Get_Parent function obtains a device instance handle to the parent node of a specified device node, in the local machine's device tree.
        /// </summary>
        /// <param name="pdnDevInst">Caller-supplied pointer to the device instance handle to the parent node that this function retrieves. The retrieved handle is bound to the local machine.</param>
        /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine. </param>
        /// <param name="ulFlags">Not used. set to 0.</param>
        /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in cfgmgr32.h.</returns>
        [DllImport("setupapi.dll")]
        public static extern CR CM_Get_Parent(out IntPtr pdnDevInst, IntPtr dnDevInst, int ulFlags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern CR CM_Locate_DevNode(ref int pdnDevInst, string pDeviceID, int ulFlags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern CR CM_Reenumerate_DevNode(int devInst, SetupApi.CM flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern CR CM_Request_Device_Eject(int devInst,IntPtr pVetoType,IntPtr  pszVetoName,uint ulNameLength,uint ulFlags);

       [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern CR CM_Get_DevNode_Status(out uint ulStatus,out uint pulProblemNumber,uint dnDevInst,uint ulFlags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int CMP_WaitNoPendingInstallEvents(uint timeoutMs);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto /*, SetLastError = true*/)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetupDiGetDeviceProperty(IntPtr DeviceInfoSet,ref SP_DEVINFO_DATA DeviceInfoData,[In] DEVPROPKEY PropertyKey,out RegistryValueKind PropertyType,byte[] PropertyBuffer,int PropertyBufferSize,out int RequiredSize,int Flags);


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

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet,
                                                                   ref SP_DEVINFO_DATA DeviceInfoData,
                                                                   SPDRP Property,
                                                                   out RegistryValueKind PropertyRegDataType,
                                                                   uint[] PropertyBuffer,
                                                                   int PropertyBufferSize,
                                                                   out int RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiSetDeviceRegistryProperty(IntPtr DeviceInfoSet,
                                                                   ref SP_DEVINFO_DATA DeviceInfoData,
                                                                   SPDRP Property,
                                                                   [MarshalAs(UnmanagedType.AsAny),In] object PropertyBuffer,
                                                                   int PropertyBufferSize);

        /*BOOL  SetupDiRemoveDevice(IN HDEVINFO  DeviceInfoSet, IN OUT PSP_DEVINFO_DATA  DeviceInfoData); */

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiRemoveDevice(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);

        [Flags]
        public enum SUOI:uint
        {
           FORCEDELETE=0x0001
        }
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupUninstallOEMInf(string InfFileName, SUOI Flags, IntPtr Reserved);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupCopyOEMInf([In] String SourceInfFileName, [In] String OEMSourceMediaLocation, SetupApi.SPOST OEMSourceMediaType, uint CopyStyle, [Out] StringBuilder DestinationInfFileName, uint DestinationInfFileNameSize, out uint RequiredSize, [Out] StringBuilder DestinationInfFileNameComponent);

        [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, int scope, int hwProfile, DevKeyType keyType, int samDesired);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegEnumValue(IntPtr hKey,int index,StringBuilder lpValueName,ref int lpcValueName,IntPtr lpReserved,out RegistryValueKind lpType,byte[] data,ref int dataLength);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegEnumValue(IntPtr hKey, int index, StringBuilder lpValueName, ref int lpcValueName, IntPtr lpReserved, out RegistryValueKind lpType, StringBuilder data, ref int dataLength);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegCloseKey (IntPtr hKey);

        [DllImport("newdev.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DiUninstallDevice(IntPtr hwndParent,IntPtr DeviceInfoSet,ref SP_DEVINFO_DATA DeviceInfoData,uint Flags,IntPtr pbNeedReboot);


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
        public static bool SetupDiGetDeviceRegistryProperty(out uint value,
                                                    IntPtr DeviceInfoSet,
                                                    ref SP_DEVINFO_DATA DeviceInfoData,
                                                    SPDRP Property)
        {
            value = 0;
            uint[] tmp = new uint[1];
            int iReqSize;
            RegistryValueKind regValueType;
            if (!SetupDiGetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, Property, out regValueType, tmp,tmp.Length*Marshal.SizeOf(typeof(uint)), out iReqSize))
            {
                //usb_error("usb_registry_match_no_hubs(): getting hardware id failed");
                return false;
            }
            value = tmp[0];
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