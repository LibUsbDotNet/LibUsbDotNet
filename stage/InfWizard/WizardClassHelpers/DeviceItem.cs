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
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using DynamicProps;
using InfWizard.InfWriters;

namespace InfWizard.WizardClassHelpers
{
    public enum KmdfVersionType
    {
        v1_5,
        v1_7,
        v1_9,
    }

    [Serializable]
    public class DeviceItemProfile
    {
        private readonly DeviceItem deviceItem;
        private readonly string driverName = "LibUsb";

        public DeviceItemProfile(string driverName, DeviceItem deviceItem)
        {
            this.driverName = driverName;
            this.deviceItem = deviceItem;
        }

        public DeviceItem DeviceItem
        {
            get
            {
                return deviceItem;
            }
        }

        public string DriverName
        {
            get
            {
                return driverName;
            }
        }
    }

    [Serializable]
    public class DeviceItem : IEquatable<DeviceItem>
    {
        private static BinaryFormatter __binFormatter;

        protected static char[] TRIM_CHARS = new char[] {'_', '.', '-'};
        private readonly Dictionary<string, object> driverRegistryList;
        private string mBaseFilename = "New_Device";
        private bool mCreateDriverDirectory = true;

        private string mDeviceDescription;
        private Guid mDeviceInterfaceGuid = Guid.NewGuid();
        [NonSerialized] private int mdevIndex = -1;
        private DateTime mDriverDate;
        private string mDriverVersion;
        private KmdfVersionType mKmdfVersion = KmdfVersionType.v1_9;
        private string mManufacturer;
        private int mPid;
        private string mSaveDriectory = "";
        private bool mSpawnDriverFiles = false;
        private int mVid;

        public DeviceItem(int vid,
                          int pid,
                          string deviceDescription,
                          string manufacturer,
                          int devIndex,
                          Dictionary<string, object> driverRegistryList)
        {
            mVid = vid;
            this.driverRegistryList = driverRegistryList;
            mManufacturer = manufacturer;
            mDeviceDescription = deviceDescription;
            mPid = pid;
            mDriverVersion = LibUsbInfWriter.VERSION;
            mDriverDate = DateTime.Now.Date;
            mBaseFilename = GetDosFilename(deviceDescription);
            mdevIndex = devIndex;
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

        [Browsable(false)]
        internal int DevIndex
        {
            get
            {
                return mdevIndex;
            }
        }

        [PropertySort(41)]
        [Category("4 - WinUsb")]
        [DisplayName("KMDF Version")]
        [Description("KMDF Version used by this WinUSB device.")]
        public KmdfVersionType KmdfVersion
        {
            get
            {
                return mKmdfVersion;
            }
            set
            {
                mKmdfVersion = value;
            }
        }

        [PropertySort(1)]
        [Category("1 - General")]
        [Description("The base name for the generated INF and CAT files.  The appropriate extensions will be automatically appended.")]
        [DisplayName("Base Filename")]
        public string BaseFilename
        {
            get
            {
                return mBaseFilename;
            }
            set
            {
                mBaseFilename = GetDosFilename(value).TrimEnd(TRIM_CHARS);
            }
        }

        [PropertySort(2)]
        [Editor(typeof (FileDialogUITypeEditor), typeof (UITypeEditor))]
        [Category("1 - General")]
        [Description("The directory in which all generated files will be placed.  If this directoy does not exist, it will be automatically created.")
        ]
        [DisplayName("Save Directory")]
        public string SaveDirectory
        {
            get
            {
                return mSaveDriectory;
            }
            set
            {
                if (value == null || value == "")
                {
                    mSaveDriectory = "";
                    return;
                }

                DirectoryInfo dtemp = new DirectoryInfo(value);
                mSaveDriectory = dtemp.FullName;
            }
        }

        [PropertySort(3)]
        [Category("1 - General")]
        [Description("Creates a sub-drectory inside the Save Directory with the Base Filename for the generated files.")]
        [DisplayName("Create Driver Directory")]
        public bool CreateDriverDirectory
        {
            get
            {
                return mCreateDriverDirectory;
            }
            set
            {
                mCreateDriverDirectory = value;
            }
        }

        [PropertySort(4)]
        [Category("1 - General")]
        [Description("Include all neccessary driver (sys/dll) files.")]
        [DisplayName("Spawn Driver Files")]
        public bool SpawnDriverFiles
        {
            get
            {
                return mSpawnDriverFiles;
            }
            set
            {
                mSpawnDriverFiles = value;
            }
        }

        [PropertySort(20)]
        [Category("2 - Device Information")]
        [DisplayName("Vendor ID")]
        [Description("The USBIF assigned vendor ID.")]
        public string VendorID
        {
            get
            {
                return mVid.ToString("X4");
            }
            set
            {
                short.Parse(value, NumberStyles.HexNumber);
            }
        }

        [PropertySort(21)]
        [Category("2 - Device Information")]
        [DisplayName("Product ID")]
        [Description("The vendor assigned product ID.")]
        public string ProductID
        {
            get
            {
                return mPid.ToString("X4");
            }
            set
            {
                mPid = short.Parse(value, NumberStyles.HexNumber);
            }
        }

        [PropertySort(22)]
        [Category("2 - Device Information")]
        [DisplayName("Device Description")]
        [Description("The friendly name used to easily distinguish this USB device from other USB devices.")]
        public string DeviceDescription
        {
            get
            {
                return mDeviceDescription;
            }
            set
            {
                mDeviceDescription = value;
            }
        }

        [PropertySort(23)]
        [Category("2 - Device Information")]
        [Description("The manufacturer of this USB device.")]
        public string Manufacturer
        {
            get
            {
                return mManufacturer;
            }
            set
            {
                mManufacturer = value;
            }
        }

        [PropertySort(30)]
        [Category("3 - Driver Information")]
        [DisplayName("Driver Version")]
        [Description("Version of the driver used for this USB device.")]
        public string DriverVersion
        {
            get
            {
                return mDriverVersion;
            }
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
            get
            {
                return mDriverDate;
            }
            set
            {
                mDriverDate = value.Date;
            }
        }


        [PropertySort(40)]
        [Category("3 - Driver Information")]
        [Description("Identifies a USB devices with a unique GUID string.")]
        [DisplayName("Device Interface Guid")]
        public String DeviceInterfaceGuid
        {
            get
            {
                return mDeviceInterfaceGuid.ToString();
            }
            set
            {
                mDeviceInterfaceGuid = new Guid(value);
            }
        }

        [PropertySort(41)]
        [ReadOnly(true)]
        [Category("Info")]
        [DisplayName("Driver Information")]
        [Description("USB Driver registry key/value pairs of an installed device.")]
        public Dictionary<string, object> DriverRegistryList
        {
            get
            {
                return driverRegistryList;
            }
        }

        [Browsable(false)]
        public string KmdfFileVersion
        {
            get
            {
                string sRtn = "01009";

                String[] verParts = mKmdfVersion.ToString().Replace("v","").Replace("_",".") .Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                if (verParts.Length == 2)
                {
                    ushort major;
                    ushort minor;
                    if (ushort.TryParse(verParts[0], out major))
                    {
                        if (ushort.TryParse(verParts[1], out minor))
                        {
                            sRtn = major.ToString("00") + minor.ToString("000");
                        }
                    }
                }
                return sRtn;
            }
        }

        #region IEquatable<DeviceItem> Members

        public bool Equals(DeviceItem deviceItem)
        {
            if (deviceItem == null) return false;
            if (mVid != deviceItem.mVid) return false;
            if (mPid != deviceItem.mPid) return false;
            //if (!Equals(mDeviceDescription, deviceItem.mDeviceDescription)) return false;
            //if (!Equals(mManufacturer, deviceItem.mManufacturer)) return false;
            return true;
        }

        #endregion

        public static void Save(DeviceItem deviceItem, string driverName, Stream streamDest)
        {
            DeviceItemProfile deviceItemProfile = new DeviceItemProfile(driverName, deviceItem);
            BinFormatter.Serialize(streamDest, deviceItemProfile);
        }

        public static DeviceItemProfile Load(Stream streamSrc)
        {
            return BinFormatter.Deserialize(streamSrc) as DeviceItemProfile;
        }


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
            string dosFileName = Regex.Replace(displayName, "[^a-zA-Z0-9_-]", new MatchEvaluator(DosFilenameEvaluator));
            return dosFileName;
        }


        public static bool operator !=(DeviceItem deviceItem1, DeviceItem deviceItem2)
        {
            return !Equals(deviceItem1, deviceItem2);
        }

        public static bool operator ==(DeviceItem deviceItem1, DeviceItem deviceItem2)
        {
            return Equals(deviceItem1, deviceItem2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as DeviceItem);
        }

        public override int GetHashCode()
        {
            int result = mVid;
            result = 29*result + mPid;
            result = 29*result + (mDeviceDescription != null ? mDeviceDescription.GetHashCode() : 0);
            result = 29*result + (mManufacturer != null ? mManufacturer.GetHashCode() : 0);
            return result;
        }
    }
}