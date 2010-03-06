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
using System.IO;
using System.Windows.Forms;
using InfWizard.WizardClassHelpers;
using TagNReplace;

namespace InfWizard.InfWriters
{
    public class DriverFile
    {
        private readonly string mRelativeDestFile;
        private readonly FileInfo mSourceFile;

        public DriverFile(string relativeDestFile, FileInfo sourceFile)
        {
            mRelativeDestFile = relativeDestFile;
            mSourceFile = sourceFile;
        }

        public string RelativeDestFile
        {
            get
            {
                return mRelativeDestFile;
            }
        }

        public FileInfo SourceFile
        {
            get
            {
                return mSourceFile;
            }
        }
    }

    public abstract class InfBaseWriter
    {
        protected readonly DeviceItem mDeviceItem;
        private string mDriverDirectory;
        private TagNReplaceString mTagger;
        private BackgroundWorker mBackGroundWorker=null;

        public InfBaseWriter(DeviceItem mDeviceItem)
        {
            this.mDeviceItem = mDeviceItem;
        }

        public DeviceItem DeviceItem
        {
            get
            {
                return mDeviceItem;
            }
        }

        public abstract string[] CatFileExts
        {
            get;
        }

        public abstract string InfBody
        {
            get;
        }

        public abstract string CatBody
        {
            get;
        }

        public List<DriverFile> DriverFileList
        {
            get
            {
                List<DriverFile> rtn = new List<DriverFile>();
                if (SpawnDriverPathIsValid)
                {
                    DirectoryInfo diSpawnDriverPath = new DirectoryInfo(SpawnDriverPath);
                    getSpawnDrivers(diSpawnDriverPath, rtn);
                    if (rtn.Count >0)
                    {
                        SpawnDriverPath = getOptimalSpawnPath(rtn);
                    }
                }
                return rtn;
            }
        }

        private string getOptimalSpawnPath(List<DriverFile> driverFiles) 
        {
            string optimalPath = null;
            foreach (DriverFile file in driverFiles)
            {
                if (optimalPath==null)
                {
                    optimalPath = file.SourceFile.Directory.FullName;
                    continue;
                    
                }
                while (!file.SourceFile.Directory.FullName.Contains(optimalPath))
                    optimalPath = optimalPath.Remove(optimalPath.Length - 1);

            }
            return new DirectoryInfo(optimalPath).Parent.FullName;
        }

        public abstract string SpawnDriverPath
        {
            get;
            set;
        }

        public virtual bool SpawnDriverPathIsValid
        {
            get
            {
                bool bDriverPathIsValid;
                try
                {
                    DirectoryInfo d = new DirectoryInfo(SpawnDriverPath);
                    bDriverPathIsValid = d.Exists;
                    return bDriverPathIsValid;
                }
                catch
                {
                    bDriverPathIsValid = false;
                }

                return bDriverPathIsValid;
            }
        }

        protected string DriverDirectory
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

                    tnr[InfTagKeys.DRIVER_CAT_FILENAME.ToString()] = mDeviceItem.BaseFilename;
                    tnr[InfTagKeys.DRIVER_DATE.ToString()] = mDeviceItem.DriverDate.Date.ToString("MM/dd/yyyy");
                    tnr[InfTagKeys.DRIVER_VERSION.ToString()] = mDeviceItem.DriverVersion;
                    tnr[InfTagKeys.DEVICE_DESCRIPTION.ToString()] = mDeviceItem.DeviceDescription;
                    tnr[InfTagKeys.VID.ToString()] = mDeviceItem.VendorID;
                    tnr[InfTagKeys.PID.ToString()] = mDeviceItem.ProductID;
                    tnr[InfTagKeys.DEVICE_MANUFACTURER.ToString()] = mDeviceItem.Manufacturer;
                    tnr[InfTagKeys.DEVICE_INTERFACE_GUID.ToString()] = mDeviceItem.DeviceInterfaceGuid;
                    tnr[InfTagKeys.KMDF_VER.ToString()] = mDeviceItem.KmdfVersion.ToString().Replace("v","").Replace("_",".");
                    tnr[InfTagKeys.KMDF_FILE_VER.ToString()] = mDeviceItem.KmdfFileVersion;

                    AddAdditionalExpandStrings(tnr);

                    tnr.Formatters.Add(new StandardTagValueFormatter("#", "#", false));
                    mTagger = tnr;
                }

                return mTagger;
            }
        }
        protected bool OperationCancelled
        {
            get
            {
                if (mBackGroundWorker != null)
                    if (mBackGroundWorker.CancellationPending) return true;
                return false;
            }
        }
        public BackgroundWorker BackGroundWorker
        {
            get { return mBackGroundWorker; }
            set { mBackGroundWorker = value; }
        }

        public abstract void AddAdditionalExpandStrings(TagNReplaceString tagger);
        protected abstract void getSpawnDrivers(DirectoryInfo diParentDir, List<DriverFile> driverList);
        public abstract void SaveDefaults();


        public abstract bool Write();

        public virtual bool SpawnDriver()
        {
            List<DriverFile> driverFiles = DriverFileList;
            if ((!ReferenceEquals(driverFiles, null)) && driverFiles.Count > 0)
            {
                foreach (DriverFile driverFile in driverFiles)
                {
                    string sDestFile = DriverDirectory.TrimEnd(new char[] {'\\', '/'}) + "\\" +
                                       driverFile.RelativeDestFile.TrimStart(new char[] {'\\', '/'});

                    FileInfo fiDestFile = new FileInfo(sDestFile);
                    if (fiDestFile.Exists)
                        fiDestFile.Delete();

                    if (!fiDestFile.Directory.Exists) fiDestFile.Directory.Create();

                    fiDestFile = driverFile.SourceFile.CopyTo(fiDestFile.FullName, true);
                }

                return true;
            }
            return false;
        }


        protected bool WriteCatFiles()
        {
            bool bRtn = true;
            try
            {
                // Write Cat Files
                foreach (string catFileExt in CatFileExts)
                {
                    string sCatFilename = Path.Combine(DriverDirectory, mDeviceItem.BaseFilename + catFileExt);
                    FileStream fs = new FileStream(sCatFilename, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(CatBody);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed Generating Cat Files:" + GetType().FullName);
                bRtn = false;
            }

            return bRtn;
        }

        protected bool WriteInfFile()
        {
            bool bRtn = true;
            try
            {
                string infFilename = Path.Combine(DriverDirectory, mDeviceItem.BaseFilename + ".inf");
                string infFileData = Tagger.TagString(InfBody);

                // Write INF File
                FileStream fs = new FileStream(infFilename, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(infFileData);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed Generating Inf File:" + GetType().FullName);
                bRtn = false;
            }
            return bRtn;
        }

        #region Nested Types

        protected enum eAdditionalTags
        {
            SPAWNDRIVER_INFO_TEXT
        }

        #endregion
    }
}