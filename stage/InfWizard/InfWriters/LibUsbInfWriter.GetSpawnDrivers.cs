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

namespace InfWizard.InfWriters
{
    partial class LibUsbInfWriter
    {
        private string[] __patternMatches;

        private string[] __patternMatches2;

        private string[] patternMatches
        {
            get
            {
                __patternMatches = null;
                if (__patternMatches == null)
                {
                    __patternMatches = new string[]
                                           {
                                               Tagger.TagString("\\libusb0.dll"),
                                               Tagger.TagString("\\libusb0_x64.dll"),
                                               Tagger.TagString("\\libusb0_ia64.dll"),
                                               Tagger.TagString("\\libusb0.sys"),
                                               Tagger.TagString("\\libusb0_x64.sys"),
                                               Tagger.TagString("\\libusb0_ia64.sys"),
                                           };
                }

                return __patternMatches;
            }
        }

        private string[] patternMatches2
        {
            get
            {
                if (__patternMatches2 == null)
                {
                    __patternMatches2 = new string[]
                                            {
                                                Tagger.TagString("\\x86\\libusb0.dll"),
                                                Tagger.TagString("\\x64\\libusb0.dll"),
                                                Tagger.TagString("\\i64\\libusb0.dll"),
                                                Tagger.TagString("\\x86\\libusb0.sys"),
                                                Tagger.TagString("\\x64\\libusb0.sys"),
                                                Tagger.TagString("\\i64\\libusb0.sys"),
                                            };
                }

                return __patternMatches2;
            }
        }

        protected override void getSpawnDrivers(DirectoryInfo diParentDir, List<DriverFile> driverList)
        {
            try
            {
                List<FileInfo> parentFiles = new List<FileInfo>(diParentDir.GetFiles("*.*", SearchOption.AllDirectories));

                List<FileInfo> filesToCopy;
                filesToCopy = parentFiles.FindAll(fndPatternMatches);
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

                    if (driverList.Count == patternMatches.Length) return;
                }

                driverList.Clear();
                filesToCopy = parentFiles.FindAll(fndPatternMatches2);
                if (filesToCopy.Count >= patternMatches2.Length)
                {
                    for (int iPatternMatch = 0; iPatternMatch < patternMatches2.Length; iPatternMatch++)
                    {
                        FileInfo fileInfoFound = null;
                        foreach (FileInfo info in filesToCopy)
                        {
                            if (info.FullName.ToLower().EndsWith(patternMatches2[iPatternMatch].ToLower()))
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
                        driverList.Add(new DriverFile(patternMatches[iPatternMatch], fileInfoFound));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private bool fndPatternMatches(FileInfo obj)
        {
            if (OperationCancelled) return false;
            foreach (string s in patternMatches)
            {
                if (obj.FullName.ToLower().EndsWith(s.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private bool fndPatternMatches2(FileInfo obj)
        {
            if (OperationCancelled) return false;
            foreach (string s in patternMatches2)
            {
                if (obj.FullName.ToLower().EndsWith(s.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}