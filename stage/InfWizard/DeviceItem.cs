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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using DynamicProps;
using InfWizard.Properties;

namespace InfWizard.WizardClassHelpers
{
    [Serializable]
    public class DeviceItemProfile
    {
        private readonly DeviceItem deviceItem;

        public DeviceItemProfile(DeviceItem deviceItem) { this.deviceItem = deviceItem; }

        public DeviceItem DeviceItem
        {
            get { return deviceItem; }
        }
    }

    [Serializable]
    public class DeviceItem
    {
        private static BinaryFormatter __binFormatter;

        protected static char[] TRIM_CHARS = new char[] {'_', '.', '-'};
        private string mBaseFilename = "New_Device";
        private bool mCreateDriverDirectory = true;

        internal string mDeviceDescription = String.Empty;
        private string mDeviceInterfaceGuid = Guid.NewGuid().ToString();
        private DateTime mDriverDate = DateTime.Now.Date;
        internal Dictionary<string, object> mDriverRegistryList;
        private string mDriverVersion = "1.0.0.0";
        private string mManufacturer = String.Empty;
        internal int mMI = -1;
        internal int mPid;
        private string mSaveDirectory = "";
        internal int mVid;

        internal string mLastDriverName;
        
        #region NON SERIALIZED

        [NonSerialized] internal string mDeviceId;
        [NonSerialized] internal bool mDriverless;
        [NonSerialized] internal bool mIsConnected;
        [NonSerialized] private bool mIsDirty;
        [NonSerialized] internal string mServiceName;
        [NonSerialized] internal bool mIsSkipServiceName;

        #endregion

        #region USER SETTINGS

        internal static StringCollection mUserDeviceDescriptions;
        internal static StringCollection mUserDeviceInterfaceGUIDs;
        internal static StringCollection mUserDeviceManufacturers;
        internal static StringCollection mUserSaveDirectorys;

        #endregion

        internal DeviceItem() { }

        public DeviceItem(int vid,
                          int pid,
                          string deviceDescription,
                          string manufacturer,
                          Dictionary<string, object> driverRegistryList)
        {
            mVid = vid;
            mDriverRegistryList = driverRegistryList;
            mManufacturer = manufacturer;
            mDeviceDescription = deviceDescription;
            mPid = pid;
            mBaseFilename = GetDosFilename(deviceDescription);
        }

        [Browsable(false)]
        public bool IsDirty
        {
            get { return mIsDirty; }
        }

        [Browsable(false)]
        public bool IsConnected
        {
            get { return mIsConnected; }
        }

        [Browsable(false)]
        public bool IsDriverless
        {
            get { return mDriverless; }
        }

        private static BinaryFormatter BinFormatter
        {
            get
            {
                if (ReferenceEquals(__binFormatter, null))
                    __binFormatter = new BinaryFormatter();
                return __binFormatter;
            }
        }

        //[PropertySort(41)]
        //[Category("4 - WinUsb")]
        //[DisplayName("KMDF Version")]
        //[Description("KMDF Version used by this WinUSB device.")]
        //public KmdfVersionType KmdfVersion
        //{
        //    get { return mKmdfVersion; }
        //    set { mKmdfVersion = value; }
        //}

        [PropertySort(1)]
        [Category("1 - General")]
        [Description("The base name for the generated INF and CAT files.  The appropriate extensions will be automatically appended.")]
        [DisplayName("Base Filename")]
        public string BaseFilename
        {
            get { return mBaseFilename; }
            set { mBaseFilename = GetDosFilename(value).TrimEnd(TRIM_CHARS); }
        }

        [PropertySort(2)]
        [Editor(typeof (FolderBrowserUITypeEditor), typeof (UITypeEditor))]
        [Category("1 - General")]
        [Description("The directory in which all generated files will be placed.  If this directoy does not exist, it will be automatically created.")]
        [DisplayName("Save Directory")]
        //       [EditorAttribute(typeof(StringDropDownUIEditor), typeof(UITypeEditor))]
            public string SaveDirectory
        {
            get
            {
                //              StringDropDownUIEditor.Strings = mUserSaveDirectorys;
                return mSaveDirectory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    mSaveDirectory = "";
                    return;
                }

                DirectoryInfo dtemp = new DirectoryInfo(value);
                mSaveDirectory = dtemp.FullName;
            }
        }

        [PropertySort(3)]
        [Category("1 - General")]
        [Description("Creates a sub-drectory inside the Save Directory with the Base Filename for the generated files.")]
        [DisplayName("Create Driver Directory")]
        public bool CreateDriverDirectory
        {
            get { return mCreateDriverDirectory; }
            set { mCreateDriverDirectory = value; }
        }

        [PropertySort(20)]
        [Category("2 - Device Information")]
        [DisplayName("Vendor ID")]
        [Description("The USBIF assigned vendor ID.")]
        public string VendorID
        {
            get { return "0x" + mVid.ToString("X4"); }
            set
            {
                mIsDirty = true;
                if (!String.IsNullOrEmpty(value))
                {
                    if (value.ToLower().StartsWith("0x"))
                        mVid = ushort.Parse(value.Substring(2), NumberStyles.HexNumber);
                    else
                        mVid = ushort.Parse(value, NumberStyles.HexNumber);

                    return;
                }
                mVid = 0;
            }
        }

        [PropertySort(21)]
        [Category("2 - Device Information")]
        [DisplayName("Product ID")]
        [Description("The vendor assigned product ID.")]
        public string ProductID
        {
            get { return "0x" + mPid.ToString("X4"); }
            set
            {
                mIsDirty = true;
                if (!String.IsNullOrEmpty(value))
                {
                    if (value.ToLower().StartsWith("0x"))
                        mPid = ushort.Parse(value.Substring(2), NumberStyles.HexNumber);
                    else
                        mPid = ushort.Parse(value, NumberStyles.HexNumber);

                    return;
                }
                mPid = 0;
            }
        }

        /*
        [PropertySort(22)]
        [Category("2 - Device Information")]
        [DisplayName("Revision (bcdDevice)")]
        [Description("The revision code (Not required)")]
        public string Rev
        {
            get
            {
                if ((mRev < 0) || (mRev > 9999)) mRev = 0;
                return mRev.ToString("0000");
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    ushort uValue;
                    if (ushort.TryParse(value, out uValue))
                        mRev = uValue;
                    else
                        mRev = 0;

                    if ((mRev < 0) || (mRev > 9999)) mRev = 0;

                    return;
                }
                mRev = 0;
            }
        }
        */

        [PropertySort(23)]
        [Category("2 - Device Information")]
        [DisplayName("Device Description")]
        [Description("The friendly name used to easily distinguish this USB device from other USB devices.")]
        [Editor(typeof (StringDropDownUIEditor), typeof (UITypeEditor))]
        public string DeviceDescription
        {
            get
            {
                StringDropDownUIEditor.Strings = mUserDeviceDescriptions;
                return mDeviceDescription;
            }
            set { mDeviceDescription = value; }
        }

        [PropertySort(24)]
        [Category("2 - Device Information")]
        [Description("The manufacturer of this USB device.")]
        [Editor(typeof (StringDropDownUIEditor), typeof (UITypeEditor))]
        public string Manufacturer
        {
            get
            {
                StringDropDownUIEditor.Strings = mUserDeviceManufacturers;
                return mManufacturer;
            }
            set { mManufacturer = value; }
        }

        [PropertySort(25)]
        [Category("2 - Device Information")]
        [DisplayName("Interface Number (MI)")]
        [Description("If the interface number is >= 0, it will be included in the hardware id for the inf file.")]
        public string MI
        {
            get
            {
                if (mMI < 0) return String.Empty;
                return mMI.ToString("X2");
            }
            set
            {
                mIsDirty = true;
                if (!String.IsNullOrEmpty(value))
                {
                    string s = value;
                    byte bValue;
                    if (byte.TryParse(value, NumberStyles.HexNumber, null, out bValue))
                    {
                        mMI = bValue;
                        return;
                    }
                }
                mMI = -1;
            }
        }

        [PropertySort(30)]
        [Category("3 - Driver Information")]
        [DisplayName("Driver Version")]
        [Description("Version of the driver used for this USB device.")]
        public string DriverVersion
        {
            get { return mDriverVersion; }
            set
            {
                string temp = "";
                String[] verParts = value.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                if (verParts.Length != 4)
                    throw new InvalidDataException("Version string must be on the format: major.minor.revision.buid.");

                foreach (string s in verParts)
                {
                    ushort.Parse(s);
                    temp += s + ".";
                }
                temp = temp.TrimEnd(new char[] {'.'});
                if (temp.Length == 0)
                    throw new InvalidDataException("Version string must be on the format: major.minor.revision.buid.");
                mDriverVersion = temp;
            }
        }

        [PropertySort(31)]
        [Category("3 - Driver Information")]
        [DisplayName("Driver Date")]
        [Description("The date this USB devices driver was created or modified.")]
        public DateTime DriverDate
        {
            get { return mDriverDate; }
            set { mDriverDate = value.Date; }
        }


        [PropertySort(40)]
        [Category("3 - Driver Information")]
        [Description("Identifies a USB devices with a unique GUID string.")]
        [DisplayName("Device Interface Guid")]
        [Editor(typeof (StringDropDownUIEditor), typeof (UITypeEditor))]
        public string DeviceInterfaceGuid
        {
            get
            {
                StringDropDownUIEditor.Strings = mUserDeviceInterfaceGUIDs;
                return mDeviceInterfaceGuid;
            }
            set { mDeviceInterfaceGuid = value; }
        }

        [PropertySort(41)]
        [ReadOnly(true)]
        [Category("Info")]
        [DisplayName("Driver Information")]
        [Description("USB Driver registry key/value pairs of an installed device.")]
        public Dictionary<string, object> DriverRegistryList
        {
            get { return mDriverRegistryList; }
        }

        internal void FillUserSettings(Settings settings)
        {
            mUserDeviceManufacturers = settings.UserDeviceManufacturers;
            mUserDeviceInterfaceGUIDs = settings.UserDeviceInterfaceGUIDs;
            mUserDeviceDescriptions = settings.UserDeviceDescriptions;
            mUserSaveDirectorys = settings.UserSaveDirectorys;

            mCreateDriverDirectory = settings.Last_CreateDriverDirectory;

            if ((mUserSaveDirectorys != null) && mUserSaveDirectorys.Count > 0)
                mSaveDirectory = mUserSaveDirectorys[mUserSaveDirectorys.Count - 1];
        }

        public static StringCollection SaveToStringCollection(StringCollection collection, string value, int max)
        {
            if (collection == null) collection = new StringCollection();
            if (!string.IsNullOrEmpty(value))
            {
                if (!collection.Contains(value))
                {
                    collection.Add(value);
                    while (collection.Count > max)
                        collection.RemoveAt(0);
                }
            }

            return collection;
        }

        internal void SaveUserSettings(Settings settings)
        {
            settings.UserDeviceManufacturers = SaveToStringCollection(settings.UserDeviceManufacturers, mManufacturer, 5);
            settings.UserDeviceDescriptions = SaveToStringCollection(settings.UserDeviceDescriptions, mDeviceDescription, 5);
            settings.UserDeviceInterfaceGUIDs = SaveToStringCollection(settings.UserDeviceInterfaceGUIDs, mDeviceInterfaceGuid, 5);
            settings.UserSaveDirectorys = SaveToStringCollection(settings.UserSaveDirectorys, mSaveDirectory, 5);

            settings.Last_CreateDriverDirectory = mCreateDriverDirectory;
        }

        public void ResetDirty() { mIsDirty = false; }

        public static void Save(DeviceItem deviceItem, Stream streamDest)
        {
            DeviceItemProfile deviceItemProfile = new DeviceItemProfile(deviceItem);
            BinFormatter.Serialize(streamDest, deviceItemProfile);
        }

        public static DeviceItemProfile Load(Stream streamSrc) { return BinFormatter.Deserialize(streamSrc) as DeviceItemProfile; }


        private static string DosFilenameEvaluator(Match match)
        {
            switch (match.Value)
            {
                case ".":
                case " ":
                    return "_";
                default:
                    return "";
            }
        }

        private static string GetDosFilename(string displayName)
        {
            return Regex.Replace(displayName, "[^a-zA-Z0-9_-]", new MatchEvaluator(DosFilenameEvaluator));
        }

        public string BuildInfHardwareID()
        {
            string s = String.Format(@"USB\VID_{0:X4}&PID_{1:X4}", mVid, mPid);
            if (mMI >= 0) s += String.Format("&MI_{0}", mMI.ToString("X2"));
            return s;
        }

        public static bool FindDriverlessPredicate(DeviceItem obj)
        {
            return obj.mDriverless;
        }
        public static bool FindSkipServicePredicate(DeviceItem obj)
        {
            return obj.mIsSkipServiceName;
        }
    }
}