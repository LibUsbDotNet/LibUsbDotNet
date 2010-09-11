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
using System.IO;
using System.Text;
using InfWizard.Properties;
using InfWizard.WizardClassHelpers;
using TagNReplace;

namespace InfWizard.InfWriters
{
    internal enum InfTagKeys
    {
        BASE_FILENAME,

        INF_FILENAME,
        CAT_FILENAME,
        DEVICE_HARDWARE_ID,

        DRIVER_DATE,
        DRIVER_VERSION,
        DEVICE_DESCRIPTION,
        VID,
        PID,
        DEVICE_MANUFACTURER,
        DEVICE_INTERFACE_GUID,
        HARDWAREID,
        MI
    }

    internal class InfWriter
    {
        private static readonly Settings mUserSettings = new Settings();
        private readonly DeviceItem mDeviceItem;
        private readonly DriverResource mDriverResource;
        private string mDriverDirectory;
        private TagNReplaceString mTagger;

        public InfWriter(DeviceItem deviceItem, DriverResource driverResource)
        {
            mDeviceItem = deviceItem;
            mDriverResource = driverResource;
        }

        public string DriverDirectory
        {
            get
            {
                if (ReferenceEquals(null, mDriverDirectory))
                {
                    if (mDeviceItem.CreateDriverDirectory)
                        mDriverDirectory = Path.Combine(mDeviceItem.SaveDirectory, mDeviceItem.BaseFilename);
                    else
                        mDriverDirectory = mDeviceItem.SaveDirectory;

                    if (!Directory.Exists(mDriverDirectory))
                        Directory.CreateDirectory(mDriverDirectory);
                }
                return mDriverDirectory;
            }
        }

        public TagNReplaceString Tagger
        {
            get
            {
                if (ReferenceEquals(null, mTagger))
                {
                    TagNReplaceString tnr = new TagNReplaceString();
                    tnr[InfTagKeys.BASE_FILENAME.ToString()] = mDeviceItem.BaseFilename;

                    tnr[InfTagKeys.INF_FILENAME.ToString()] = mDeviceItem.BaseFilename + ".inf";
                    tnr[InfTagKeys.CAT_FILENAME.ToString()] = mDeviceItem.BaseFilename + ".cat";
                    
                    tnr[InfTagKeys.DRIVER_DATE.ToString()] = mDeviceItem.DriverDate.Date.ToString("MM/dd/yyyy");
                    tnr[InfTagKeys.DRIVER_VERSION.ToString()] = mDeviceItem.DriverVersion;
                    tnr[InfTagKeys.DEVICE_DESCRIPTION.ToString()] = mDeviceItem.DeviceDescription;
                    tnr[InfTagKeys.VID.ToString()] = mDeviceItem.VendorID;
                    tnr[InfTagKeys.PID.ToString()] = mDeviceItem.ProductID;
                    tnr[InfTagKeys.MI.ToString()] = mDeviceItem.MI;
                    tnr[InfTagKeys.HARDWAREID.ToString()] = mDeviceItem.BuildInfHardwareID();
                    tnr[InfTagKeys.DEVICE_HARDWARE_ID.ToString()] = mDeviceItem.BuildInfHardwareID().Substring(4);

                    tnr[InfTagKeys.DEVICE_MANUFACTURER.ToString()] = mDeviceItem.Manufacturer;
                    tnr[InfTagKeys.DEVICE_INTERFACE_GUID.ToString()] = "{" + mDeviceItem.DeviceInterfaceGuid + "}";

                    tnr.Formatters.Add(new StandardTagValueFormatter("#", "#", false));
                    mTagger = tnr;
                }

                return mTagger;
            }
        }

        public void SaveDefaults() { mUserSettings.Save(); }

        public bool Write()
        {
            bool bRtn = (WriteStringFiles() && WriteDataFiles());
            return bRtn;
        }

        protected bool WriteStringFiles()
        {
            bool bRtn = true;

            string readmeHtmFile = Path.Combine(DriverDirectory, "README_" + mDeviceItem.BaseFilename + ".htm");

            try
            {
                // All strings without an extention are considered tokens.
                // They are added to the tagger instead of writing to file.
                foreach (KeyValuePair<string, string> pair in mDriverResource.Strings)
                    if (!pair.Key.Contains("."))
                        Tagger[pair.Key] = pair.Value;

                foreach (KeyValuePair<string, string> pair in mDriverResource.Strings)
                {
                    // Skip the token strings
                    if (!pair.Key.Contains(".")) continue;

                    string key = Tagger.TagString(pair.Key);
                    string filePath = Path.Combine(DriverDirectory, key);

                    if (filePath.ToLower() == readmeHtmFile.ToLower()) continue;
                    
                    InfWizardStatus.Log(CategoryType.InfWriter, StatusType.Info, "writing driver file {0}", key);

                    // The resource key is a path relative to the driver directory.
                    string fileDir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);
                    
                    if (File.Exists(filePath)) File.Delete(filePath);


                    // Tag the string and write the file.
                    string fileData = Tagger.TagString(pair.Value);
                    File.WriteAllText(filePath, fileData, Encoding.ASCII);
                }

                // Write the libusbdotnet InfWizard readme htm file.
                File.WriteAllText(readmeHtmFile,Tagger.TagString(resInfWizard.SETUP_PACKAGE_README_HTM));
            }
            catch (Exception ex)
            {
                InfWizardStatus.Log(CategoryType.InfWriter, StatusType.Error, "failed writing string driver resource {0}", ex);
                bRtn = false;
            }

            return bRtn;
        }

        public string GetDriverPathFilename(string extention) { return Path.Combine(DriverDirectory, string.Format("{0}.{1}", mDeviceItem.BaseFilename, extention.TrimStart(new char[] {'.'}))); }

        protected bool WriteDataFiles()
        {
            try
            {
                foreach (KeyValuePair<string, byte[]> pair in mDriverResource.Files)
                {
                    // The resource key is a path relative to the driver directory.
                    string filePath = Path.Combine(DriverDirectory, Tagger.TagString(pair.Key));
                    string fileDir = Path.GetDirectoryName(filePath);

                    InfWizardStatus.Log(CategoryType.InfWriter, StatusType.Info, "writing driver file {0}", filePath);

                    if (!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);
                    
                    if (File.Exists(filePath)) File.Delete(filePath);

                    File.WriteAllBytes(filePath, pair.Value);
                }
                return true;
            }
            catch (Exception ex)
            {
                InfWizardStatus.Log(CategoryType.InfWriter, StatusType.Error, "failed writing data file driver resource: {0}", ex);
            }
            return false;
        }
    }
}