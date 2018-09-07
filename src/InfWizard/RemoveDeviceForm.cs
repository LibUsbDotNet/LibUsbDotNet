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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using InfWizard.WizardClassHelpers;

namespace InfWizard
{
    public partial class RemoveDeviceForm : Form
    {
        public static RemoveDeviceOptions DefaultRemoveOptions;
        private readonly DeviceItem mDeviceItem;
        private DialogResult mRemoveDialogResult=DialogResult.Cancel;

        public RemoveDeviceForm() { InitializeComponent(); }

        public RemoveDeviceForm(DeviceItem deviceItem, bool onlyConnectedDevices)
            : this()
        {
            mDeviceItem = deviceItem;
            DefaultRemoveOptions = new RemoveDeviceOptions(mDeviceItem);
            DefaultRemoveOptions.OnlyConnectedDevices = onlyConnectedDevices;
            warningLabel.Text = resInfWizard.REMOVE_DEVICE_WARNING;
            removeDeviceOptionGrid.SelectedObject = DefaultRemoveOptions;
        }

        public class RemoveDeviceOptions
        {
            private readonly DeviceItem mDeviceItem;

            private bool mDeepClean;
            private bool mOnlyConnectedDevices;
            private bool mRemoveByVidPid;
            public RemoveDeviceOptions(DeviceItem deviceItem) { mDeviceItem = deviceItem; }

            [DisplayName("Deep Clean INF Directory")]
            [Description("Scans the inf directory for all inf files using this usb devices VID and PID.")]
            [Category("Remove Options")]
            public bool DeepClean
            {
                get { return mDeepClean; }
                set { mDeepClean = value; }
            }

            [DisplayName("Remove Device By vendor and product id")]
            [Description("Removes all devices matching this devices vid and pid.")]
            [Category("Remove Options")]
            public bool RemoveByVidPid
            {
                get { return mRemoveByVidPid; }
                set { mRemoveByVidPid = value; }
            }

            [DisplayName("Connected Devices Only")]
            [Description("Only search for usb device that are currently connect to the host.")]
            [Category("Remove Options")]
            public bool OnlyConnectedDevices
            {
                get { return mOnlyConnectedDevices; }
                set { mOnlyConnectedDevices = value; }
            }

            [Category("USB Device Information")]
            [DisplayName("Description")]
            public string DeviceDescription
            {
                get { return mDeviceItem.DeviceDescription; }
            }

            [Category("USB Device Information")]
            public string Manufacturer
            {
                get { return mDeviceItem.Manufacturer; }
            }

            [Category("USB Device Information")]
            public string VendorID
            {
                get { return mDeviceItem.VendorID; }
            }

            [Category("USB Device Information")]
            public string ProductID
            {
                get { return mDeviceItem.ProductID; }
            }

            [Browsable(false)]
            public DeviceItem DeviceItem
            {
                get { return mDeviceItem; }
            }
        }

        private void Remove()
        {
            DEIFlags flags = DEIFlags.DICFG_AllClasses|DEIFlags.IncludeWindowsServices;
            Cursor = Cursors.WaitCursor;

            rtfRemoveDeviceStatus.StatusFilter = RemoveStatusFilter;
            removeDeviceOptionGrid.Enabled = false;
            removeCommand.Enabled = false;

            if (DefaultRemoveOptions.OnlyConnectedDevices)
                flags = flags | DEIFlags.DICFG_Present;

            try
            {
                mRemoveDialogResult = DialogResult.OK;
                rtfRemoveDeviceStatus.LoggingEnabled = true;
                InfWizardStatus.Log(CategoryType.RemoveDevice, StatusType.Info, "enumerating devices..");
                DeviceSelectionHelper.RemoveDevice(DefaultRemoveOptions, Handle, flags);

                if (DefaultRemoveOptions.DeepClean)
                {
                     DeepClean();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Device Removal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                rtfRemoveDeviceStatus.LoggingEnabled = false;
                Cursor = Cursors.Default;
                removeDeviceOptionGrid.Enabled = true;
            }
        }

        private static bool RemoveStatusFilter(StatusArgs statusargs) 
        {
            if (statusargs.Category == CategoryType.EnumerateDevices)
                return false;
            return true;
        }

        private static bool DeepClean()
        {
            try
            {
                // %USB\PicFW_Bootloader.DeviceDesc% =PicFW_Bootloader_Install, USB\VID_5472&PID_0002
                string sVidPidScan = DefaultRemoveOptions.DeviceItem.BuildInfHardwareID().ToLower();
                DirectoryInfo diWindows = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System)).Parent;
                DirectoryInfo diWindowsInf = new DirectoryInfo(diWindows.FullName + Path.DirectorySeparatorChar + "inf");

                List<FileInfo> foundInfFileList = new List<FileInfo>();

                FileInfo[] infFiles = diWindowsInf.GetFiles("*.inf", SearchOption.TopDirectoryOnly);
                foreach (FileInfo infFile in infFiles)
                {
                    StreamReader sr = new StreamReader(infFile.OpenRead());
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line != null)
                        {
                            if (line.ToLower().Contains(sVidPidScan))
                            {
                                foundInfFileList.Add(infFile);
                                break;
                            }
                        }
                    }
                    sr.Close();
                }

                if (foundInfFileList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (FileInfo fileInfo in foundInfFileList) sb.AppendLine(fileInfo.Name);

                    if (MessageBox.Show(sb.ToString(), String.Format("Deep Clean: Remove {0} inf files?", foundInfFileList.Count), MessageBoxButtons.YesNo) == DialogResult.OK)
                    {
                        foreach (FileInfo fileInfo in foundInfFileList) fileInfo.Delete();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void removeCommand_Click(object sender, EventArgs e) { Remove(); }

        private void RemoveDeviceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = mRemoveDialogResult;
        }
    }
}