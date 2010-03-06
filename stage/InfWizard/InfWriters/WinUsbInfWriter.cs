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
using InfWizard.Properties;
using InfWizard.WizardClassHelpers;
using TagNReplace;

namespace InfWizard.InfWriters
{
    public partial class WinUsbInfWriter : InfBaseWriter
    {
        private static readonly string[] mCatFileExts = new string[] {".cat"};
        private static readonly Settings userSettings = new Settings();

        public static readonly string VERSION = "6.0.6001.18002";

        internal WinUsbInfWriter(DeviceItem deviceItem)
            : base(deviceItem)
        {
        }

        public override string[] CatFileExts
        {
            get
            {
                return mCatFileExts;
            }
        }

        public override string InfBody
        {
            get
            {
                switch (mDeviceItem.KmdfVersion)
                {
                    case KmdfVersionType.v1_5:
                        return Resources.WINUSB_INF_BODY;
                    case KmdfVersionType.v1_7:
                        return Resources.WINUSB_INF_BODY;
                    case KmdfVersionType.v1_9:
                        return Resources.WINUSB2_INF_BODY;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override string CatBody
        {
            get
            {
                return Resources.WINUSB_CAT_BODY;
            }
        }

        public override string SpawnDriverPath
        {
            get
            {
                return userSettings.WinUsbSpawnDriverSourcePath;
            }
            set
            {
                userSettings.WinUsbSpawnDriverSourcePath = value;
            }
        }

        public override void AddAdditionalExpandStrings(TagNReplaceString tagger)
        {
            tagger.Tags[eAdditionalTags.SPAWNDRIVER_INFO_TEXT.ToString()] = resSpawnDriverWinusb.SPAWNDRIVER_INFO_TEXT;
        }

        public override void SaveDefaults()
        {
            userSettings.Save();
        }

        public override bool Write()
        {
            bool bRtn = (WriteInfFile() && WriteCatFiles());
            return bRtn;
        }
    }
}