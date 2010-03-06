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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DynamicProps;
using Gui.Wizard;
using InfWizard.InfWriters;
using InfWizard.Properties;
using InfWizard.WizardClassHelpers;
using WinApiNet;

namespace InfWizard
{
    public partial class fUsbInfWizard : Form
    {
        #region Enumerations

        public enum ShowCommands
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }

        #endregion

        private readonly DynamicPropWrapper mpgDeviceConfigurationWrapper = new DynamicPropWrapper();
        private readonly Settings mSettings = new Settings();
        private DeviceItem mCurrentDeviceItem;
        private List<DeviceItem> mDeviceList;

        public fUsbInfWizard()
        {
            InitializeComponent();
            InitializeDisplayStrings();
        }


        [DllImport("shell32.dll")]
        private static extern IntPtr ShellExecute(IntPtr hwnd,
                                                  string lpOperation,
                                                  string lpFile,
                                                  string lpParameters,
                                                  string lpDirectory,
                                                  ShowCommands nShowCmd);


        private static GridItem FindGridItem(string sLabel, GridItem gridItemToSearch)
        {
            /*
            // Searches through gridItemToSearch and all of its child nodes
            // Returns the first GridItem that has a Label equal to the sLabel parameter.
            // NOTE:
            // You could use any matching criteria you wanted, the Label property is what
            // worked for me because it always equals the property name. You could also
            // use the gridItemToSearch.PropertyDescriptor
            */
            //            if (gridItemToSearch.PropertyDescriptor != null && gridItemToSearch.PropertyDescriptor.Name == sLabel)
            if (gridItemToSearch.Label != null && gridItemToSearch.Label == sLabel)
                return gridItemToSearch;
            if (gridItemToSearch.GridItems.Count > 0)
            {
                for (int iGrid = 0; iGrid < gridItemToSearch.GridItems.Count; iGrid++)
                {
                    GridItem check = FindGridItem(sLabel, gridItemToSearch.GridItems[iGrid]);
                    if (check != null)
                        return check;
                }
            }

            return null;
        }

        private void gbDeviceSelection_Enter(object sender, EventArgs e)
        {
        }

        private static GridItem GetParentGridItem(GridItem gItem)
        {
            // Takes a GridItem as a parameter and find the root(top most) GridItem
            GridItem gridItemFind = gItem;
            GridItem gridItemLast = null;

            while (gridItemFind != null)
            {
                gridItemLast = gridItemFind;
                gridItemFind = gridItemFind.Parent;
            }
            return gridItemLast;
        }

        private void InitializeDisplayStrings()
        {
            lMainText.Text = Resources.MAINTEXT;
            
            headerDeviceSelection.Title = Resources.DEVICE_SELECTION_TITLE;
            headerDeviceSelection.Description = Resources.DEVICE_SELECTION_DESCRIPTION;

            headerDeviceConfiguration.Title = Resources.DEVICE_CONFIGURATION_TITLE;
            headerDeviceConfiguration.Description = Resources.DEVICE_CONFIGURATION_DESCRIPTION;
        }

        private void lvDeviceSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvDeviceSelection.SelectedIndices.Count > 0)
            {
                mCurrentDeviceItem = (DeviceItem) lvDeviceSelection.SelectedItems[0].Tag;
            }
            wizMain.NextEnabled = lvDeviceSelection.SelectedIndices.Count > 0;
            cmdRemoveDevice.Enabled = wizMain.NextEnabled;
        }

        private void pgDeviceConfiguration_PropertySortChanged(object sender, EventArgs e)
        {
            if (pgDeviceConfiguration.PropertySort == PropertySort.CategorizedAlphabetical)
                pgDeviceConfiguration.PropertySort = PropertySort.Categorized;
            refreshDeviceConfigurationView(false);
        }

        private void pgDeviceConfiguration_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pgDeviceConfiguration_UpdateNext();
        }

        private void pgDeviceConfiguration_UpdateNext()
        {
            if (mCurrentDeviceItem.SaveDirectory.Length > 0 && mCurrentDeviceItem.BaseFilename.Length > 0 &&
                mCurrentDeviceItem.DeviceDescription.Length > 0)
                wizMain.NextEnabled = true;
            else
                wizMain.NextEnabled = false;
        }

        private static void pgDeviceConfigurationPostGridFocus(Object o)
        {
            PropertyGrid grid = (PropertyGrid) o;
            grid.Focus();
            SendKeys.Send("{TAB}");
        }

        private void rbDeviceSelection_AllDevices_CheckedChanged(object sender, EventArgs e)
        {
            refreshDeviceSelectionView();
        }

        private void rbDeviceSelection_OnlyConnected_CheckedChanged(object sender, EventArgs e)
        {
            refreshDeviceSelectionView();
        }

        private void refreshDeviceConfigurationView(bool bInitial)
        {
            mpgDeviceConfigurationWrapper.Instance = mCurrentDeviceItem;

            if (rbDriverType_LibUsb.Checked)
            {
                toolTip1.SetToolTip(cmdFindFrivers, resSpawnDriverLibusb.SPAWNDRIVER_INFO_TEXT);
                if (mCurrentDeviceItem.DriverVersion == WinUsbInfWriter.VERSION)
                    mCurrentDeviceItem.DriverVersion = LibUsbInfWriter.VERSION;

                mpgDeviceConfigurationWrapper["DeviceInterfaceGuid"].Overrides.Add(new ReadOnlyAttribute(false));
                mpgDeviceConfigurationWrapper["KmdfVersion"].Overrides.Add(new ReadOnlyAttribute(true));
            }
            else
            {
                toolTip1.SetToolTip(cmdFindFrivers, resSpawnDriverWinusb.SPAWNDRIVER_INFO_TEXT);
                if (mCurrentDeviceItem.DriverVersion == LibUsbInfWriter.VERSION)
                    mCurrentDeviceItem.DriverVersion = WinUsbInfWriter.VERSION;
                mpgDeviceConfigurationWrapper["DeviceInterfaceGuid"].Overrides.Add(new ReadOnlyAttribute(false));
                mpgDeviceConfigurationWrapper["KmdfVersion"].Overrides.Add(new ReadOnlyAttribute(false));
            }
            pgDeviceConfiguration.SelectedObject = mpgDeviceConfigurationWrapper.DynamicDescriptor;
            if (bInitial)
            {
                mCurrentDeviceItem.SaveDirectory = mSettings.Last_SaveDirectory;
                mCurrentDeviceItem.CreateDriverDirectory = mSettings.Last_CreateDriverDirectory;
                mCurrentDeviceItem.SpawnDriverFiles = mSettings.Last_SpawnDriverFiles;
                GridItem gridToSel = FindGridItem("Save Directory", GetParentGridItem(pgDeviceConfiguration.SelectedGridItem));
                if (gridToSel != null) pgDeviceConfiguration.SelectedGridItem = gridToSel;

                mpgDeviceConfigurationWrapper.PropertySortType = DynPropertySortTypes.UsePropertySortAttributes;
                SynchronizationContext.Current.Post(pgDeviceConfigurationPostGridFocus, pgDeviceConfiguration);
            }
            pgDeviceConfiguration_UpdateNext();
            pgDeviceConfiguration.Refresh();
        }

        private void refreshDeviceSelectionView()
        {
            SetupApi.DICFG flags = SetupApi.DICFG.ALLCLASSES;

            if (rbDeviceSelection_OnlyConnected.Checked)
                flags = flags | SetupApi.DICFG.PRESENT;

            DeviceSelectionHelper.FindRealDevices(out mDeviceList, Handle, flags);
            lvDeviceSelection.Items.Clear();
            DeviceSelectionHelper.FindRealDevices(out mDeviceList, Handle, flags);
            foreach (DeviceItem deviceItem in mDeviceList)
            {
                ListViewItem lvKey = lvDeviceSelection.Items.Add("0x" + deviceItem.VendorID);
                lvKey.Tag = deviceItem;
                lvKey.SubItems.Add("0x" + deviceItem.ProductID);
                lvKey.SubItems.Add(deviceItem.DeviceDescription);
                lvKey.SubItems.Add(deviceItem.Manufacturer);
            }
            wizMain.NextEnabled = false;
        }

        private InfBaseWriter NewWriter()
        {
            InfBaseWriter infWriter = null;

            if (rbDriverType_LibUsb.Checked)
            {
                infWriter = new LibUsbInfWriter(mCurrentDeviceItem);
            }
            else if (rbDriverType_WinUsb.Checked)
            {
                infWriter = new WinUsbInfWriter(mCurrentDeviceItem);
            }
            return infWriter;
        }

        private DialogResult ShowFindDriversDialog(InfBaseWriter infWriter)
        {
            fSpawnDrivers fSpawnDriversFind = new fSpawnDrivers(infWriter);
            return fSpawnDriversFind.ShowDialog(this);
        }

        private void wizDeviceConfiguration_CloseFromNext(object sender, PageEventArgs e)
        {
            e.CloseInFinsh = false;
            try
            {
                bool bSuccess = false;
                InfBaseWriter infWriter = NewWriter();

                if (infWriter != null) bSuccess = infWriter.Write();

                if (bSuccess)
                {
                    if (mCurrentDeviceItem.SpawnDriverFiles)
                    {
                        if (infWriter.DriverFileList.Count == 0)
                        {
                            if (ShowFindDriversDialog(infWriter) == DialogResult.OK)
                            {
                                infWriter.SpawnDriver();
                            }
                        }
                        else
                        {
                            infWriter.SpawnDriver();
                        }
                    }
                    mSettings.Last_SaveDirectory = mCurrentDeviceItem.SaveDirectory;
                    mSettings.Save();

                    if (
                        MessageBox.Show(this,
                                        "Usb Inf generation was successful!\r\n\r\n Would you like to generate another?",
                                        "InfWizard Complete",
                                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        e.Page = wizPageMain;
                    }
                    else
                    {
                        ShellExecute(IntPtr.Zero, "open", mCurrentDeviceItem.SaveDirectory, null, null, ShowCommands.SW_SHOWNORMAL);
                        e.CloseInFinsh = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "InfWizard Error");
            }
            finally
            {
            }
        }

        private void wizDeviceConfiguration_ShowFromNext(object sender, EventArgs e)
        {
            refreshDeviceConfigurationView(true);
        }

        private void wizDeviceSelection_ShowFromNext(object sender, EventArgs e)
        {
            refreshDeviceSelectionView();
        }


        private void rbDriverType_WinUsb_CheckedChanged(object sender, EventArgs e)
        {
            refreshDeviceConfigurationView(false);
        }

        private void rbDriverType_LibUsb_CheckedChanged(object sender, EventArgs e)
        {
            refreshDeviceConfigurationView(false);
        }

        private void wizDeviceConfiguration_CloseFromBack(object sender, PageEventArgs e)
        {
            mSettings.Last_CreateDriverDirectory = mCurrentDeviceItem.CreateDriverDirectory;
            mSettings.Last_SaveDirectory = mCurrentDeviceItem.SaveDirectory;
            mSettings.Last_SpawnDriverFiles = mCurrentDeviceItem.SpawnDriverFiles;
        }

        private void cmdRemoveDevice_Click(object sender, EventArgs e)
        {
            fRemoveDevice f = new fRemoveDevice(mCurrentDeviceItem, rbDeviceSelection_OnlyConnected.Checked);
            if (f.ShowDialog(this)==System.Windows.Forms.DialogResult.OK)
            {
                refreshDeviceSelectionView();
            }
        }

        private void cmdFindFrivers_Click(object sender, EventArgs e)
        {
            ShowFindDriversDialog(NewWriter());
        }

        private void cmdSaveProfile_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.SupportMultiDottedExtensions = true;
            d.DefaultExt = "Bin.UsbWiz";
            d.Filter = "USB Wizard device profile (*.Bin.UsbWiz)|*.Bin.UsbWizard|All files (*.*)|*.*";
            d.AddExtension = true;
            d.OverwritePrompt = true;

            if (d.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    FileInfo fiDest = new FileInfo(d.FileName);

                    if (fiDest.Exists) fiDest.Delete();

                    FileStream fsDest = fiDest.Create();
                    DeviceItem.Save(mCurrentDeviceItem, getDriverName(), fsDest);

                    fsDest.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed saving profile.");
                }
            }
        }

        private void setDriverByName(string driverName)
        {
            switch (driverName)
            {
                case "LibUsb":
                    rbDriverType_LibUsb.Checked = true;
                    break;
                case "WinUsb":
                    rbDriverType_WinUsb.Checked = true;
                    break;
            }
        }

        private string getDriverName()
        {
            if (rbDriverType_LibUsb.Checked)
            {
                return "LibUsb";
            }
            if (rbDriverType_WinUsb.Checked)
            {
                return "WinUsb";
            }
            return "";
        }

        private void cmdLoadProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.SupportMultiDottedExtensions = true;
            d.DefaultExt = "Bin.UsbWiz";
            d.Filter = "USB Wizard device profile (*.Bin.UsbWiz)|*.Bin.UsbWizard|All files (*.*)|*.*";
            d.AddExtension = true;
            d.CheckFileExists = true;

            if (d.ShowDialog(this) == DialogResult.OK)
            {
                loadProfile(d.FileName);

            }
        }

        private void loadProfile(string fileName)
        {
            loadProfile(fileName, false);
        }
        private void loadProfile(string fileName, bool quiet) 
        {
            try
            {
                FileInfo fiDest = new FileInfo(fileName);

                FileStream fsDest = fiDest.Open(FileMode.Open, FileAccess.Read);
                DeviceItemProfile deviceItemProfile = DeviceItem.Load(fsDest);
                mCurrentDeviceItem = deviceItemProfile.DeviceItem;
                fsDest.Close();

                wizMain.PageIndex = wizMain.Pages.IndexOf(wizDeviceConfiguration);

                setDriverByName(deviceItemProfile.DriverName);
                refreshDeviceConfigurationView(true);

            }
            catch (Exception ex)
            {
                if (!quiet)
                    MessageBox.Show(ex.ToString(), "Failed saving profile.");
            }
        }

        private void fUsbInfWizard_Load(object sender, EventArgs e)
        {
            // Remove the exepath/startup filename text from the begining of the CommandLine.
            string cmdLine = Regex.Replace(Environment.CommandLine, "^\".+?\"^.*? |^.*? ", "", RegexOptions.Singleline).Trim();
            if(!String.IsNullOrEmpty(cmdLine))
            {
                loadProfile(cmdLine, true);
            }
        }
    }
}