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
using System.IO;
using InfWizard.WizardClassHelpers;

namespace InfWizard.InfWriters
{
    partial class WinUsbInfWriter
    {
        private string[] __patternMatches;

        private string[] patternMatches
        {
            get
            {
                if (__patternMatches == null)
                {
                    switch (mDeviceItem.KmdfVersion)
                    {
                        case KmdfVersionType.v1_5:
                        case KmdfVersionType.v1_7:
                            __patternMatches = new string[]
                                           {
                                               Tagger.TagString("\\x86\\WinUSBCoInstaller.dll"),
                                               Tagger.TagString("\\amd64\\WinUSBCoInstaller.dll"),
                                               Tagger.TagString("\\ia64\\WinUSBCoInstaller.dll"),
                                               Tagger.TagString("\\x86\\WdfCoInstaller#KMDF_FILE_VER#.dll"),
                                               Tagger.TagString("\\amd64\\WdfCoInstaller#KMDF_FILE_VER#.dll"),
                                               Tagger.TagString("\\ia64\\WdfCoInstaller#KMDF_FILE_VER#.dll"),
                                           }; 
                                           break;
                        case KmdfVersionType.v1_9:
                            __patternMatches = new string[]
                                           {
                                               Tagger.TagString("\\x86\\WinUSBCoInstaller2.dll"),
                                               Tagger.TagString("\\amd64\\WinUSBCoInstaller2.dll"),
                                               Tagger.TagString("\\ia64\\WinUSBCoInstaller2.dll"),
                                               Tagger.TagString("\\x86\\WdfCoInstaller#KMDF_FILE_VER#.dll"),
                                               Tagger.TagString("\\amd64\\WdfCoInstaller#KMDF_FILE_VER#.dll"),
                                               Tagger.TagString("\\ia64\\WdfCoInstaller#KMDF_FILE_VER#.dll"),
                                           }; 
                                           break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }

                return __patternMatches;
            }
        }

        protected override void getSpawnDrivers(DirectoryInfo diParentDir, List<DriverFile> driverList)
        {
            try
            {


                List<FileInfo> parentFiles = new List<FileInfo>(diParentDir.GetFiles("*.*", SearchOption.AllDirectories));

                List<FileInfo> filesToCopy = parentFiles.FindAll(fndPatternMatches);

                if (filesToCopy.Count >= patternMatches.Length)
                {
                    foreach (string s in patternMatches)
                    {
                        FileInfo fileInfoFound = null;
                        foreach (FileInfo info in filesToCopy)
                        {
                            if (info.FullName.ToLower().EndsWith(s.ToLower()))
                            {
                                fileInfoFound = info;
                                break;
                            }
                        }

                        if (fileInfoFound == null)
                        {
                            driverList.Clear();
                            break;
                        }
                        driverList.Add(new DriverFile(s, fileInfoFound));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private bool fndPatternMatches(FileInfo obj)
        {
            foreach (string s in patternMatches)
            {
                if (OperationCancelled) return false;
                if (obj.FullName.ToLower().EndsWith(s.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}