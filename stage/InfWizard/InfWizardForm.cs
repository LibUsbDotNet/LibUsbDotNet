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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
    public partial class InfWizardForm : Form
    {
        private readonly DynamicPropWrapper mDeviceConfigGridWrapper = new DynamicPropWrapper();
        private readonly Settings mSettings = new Settings();
        private DeviceItem mCurrentDeviceItem;
        private DriverResource mCurrentDriverResource;
        private InfWriter mCurrentInfWriter;
        private List<DeviceItem> mDeviceList;
        private bool mSetupPackageWritten;
        private bool mSkipToDriverResourceLocator;

        public InfWizardForm()
        {
            InitializeComponent();
        }

        internal enum ShowCommands
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


        [DllImport("shell32.dll")]
        private static extern IntPtr ShellExecute(IntPtr hwnd,
                                                  string lpOperation,
                                                  string lpFile,
                                                  string lpParameters,
                                                  string lpDirectory,
                                                  ShowCommands nShowCmd);


        private static GridItem FindGridItem(string sLabel, GridItem gridItemToSearch)
        {
            // Searches through gridItemToSearch and all of its child nodes
            // Returns the first GridItem that has a Label equal to the sLabel parameter.
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

        private void DeviceConfigGrid_PropertySortChanged(object sender, EventArgs e)
        {
            if (DeviceConfigGrid.PropertySort == PropertySort.CategorizedAlphabetical)
                DeviceConfigGrid.PropertySort = PropertySort.Categorized;
            refreshDeviceConfigurationView(false);
        }

        private void DeviceConfigGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) { DeviceConfigGrid_UpdateNext(); }

        private void DeviceConfigGrid_UpdateNext()
        {
            if (mCurrentDeviceItem.SaveDirectory.Length > 0 && mCurrentDeviceItem.BaseFilename.Length > 0 &&
                mCurrentDeviceItem.DeviceDescription.Length > 0)
                wizMain.NextEnabled = true;
            else
                wizMain.NextEnabled = false;
        }

        private static void DeviceConfigGridPostGridFocus(Object o)
        {
            PropertyGrid grid = (PropertyGrid) o;
            grid.Focus();
            SendKeys.Send("{TAB}");
        }

        private void rbDeviceSelection_New_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void refreshDeviceConfigurationView(bool bInitial)
        {
            mDeviceConfigGridWrapper.Instance = mCurrentDeviceItem;
            DeviceConfigGrid.SelectedObject = mDeviceConfigGridWrapper.DynamicDescriptor;
            if (bInitial)
            {
                mCurrentDeviceItem.FillUserSettings(mSettings);
                GridItem gridToSel = FindGridItem("Save Directory", GetParentGridItem(DeviceConfigGrid.SelectedGridItem));
                if (gridToSel != null) DeviceConfigGrid.SelectedGridItem = gridToSel;

                mDeviceConfigGridWrapper.PropertySortType = DynPropertySortTypes.UsePropertySortAttributes;
                SynchronizationContext.Current.Post(DeviceConfigGridPostGridFocus, DeviceConfigGrid);
            }
            DeviceConfigGrid_UpdateNext();
            DeviceConfigGrid.Refresh();
        }

        private void refreshDeviceSelectionView()
        {
            DEIFlags flags = DEIFlags.DICFG_AllClasses;
            
            bool driverLessOnly = deviceSelection_DriverlessOnlyCheckBox.Checked;
            bool connnectedOnly = deviceSelection_ConnectedOnlyCheckBox.Checked;

            if (connnectedOnly)
                flags = flags | DEIFlags.DICFG_Present;
            
            if (driverLessOnly)
                flags |= DEIFlags.Driverless;

#if DEBUG
            flags |= DEIFlags.IncludeWindowsServices;
#endif
            cmdRemoveDevice.Enabled = false;
            gridDeviceSelection.Rows.Clear();

            if (deviceSelection_CreateNewRadio.Checked)
            {
                mCurrentDeviceItem = new DeviceItem();
                wizMain.NextEnabled = true;
                deviceSelection_RefreshButton.Enabled = false;
                return;
            }
            deviceSelection_RefreshButton.Enabled = true;

            DeviceEnumeratorInfo deInfo = new DeviceEnumeratorInfo(flags, Handle);
            mDeviceList = deInfo.DeviceList;
            DeviceSelectionHelper.EnumerateDevices(deInfo);

            foreach (DeviceItem deviceItem in mDeviceList)
            {
                int iRow =
                    gridDeviceSelection.Rows.Add(new object[]
                                                     {
                                                         "0x" + deviceItem.VendorID, "0x" + deviceItem.ProductID, deviceItem.DeviceDescription,
                                                         deviceItem.Manufacturer, deviceItem.mDriverless, deviceItem.mDeviceId
                                                     });
                gridDeviceSelection.Rows[iRow].Tag = deviceItem;
                deviceItem.ResetDirty();
            }

            wizMain.NextEnabled = (gridDeviceSelection.Rows.Count > 0);
            if (wizMain.NextEnabled && mCurrentDeviceItem == null)
                mCurrentDeviceItem = (DeviceItem) gridDeviceSelection.SelectedRows[0].Tag;
        }

        private void cmdRemoveDevice_Click(object sender, EventArgs e)
        {
            RemoveDeviceForm f = new RemoveDeviceForm(mCurrentDeviceItem, deviceSelection_ConnectedOnlyCheckBox.Checked);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                refreshDeviceSelectionView();
            }
        }

        private void buttonSaveProfile_Click(object sender, EventArgs e)
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
                    mCurrentDeviceItem.mLastDriverName = mCurrentDriverResource.DisplayName;
                    FileInfo fiDest = new FileInfo(d.FileName);

                    if (fiDest.Exists) fiDest.Delete();

                    FileStream fsDest = fiDest.Create();
                    DeviceItem.Save(mCurrentDeviceItem, fsDest);
                    fsDest.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed saving profile.");
                }
            }
        }

        private void cmdLoadProfile_Click(object sender, EventArgs e)
        {

        }

        private void loadProfile(string fileName) { loadProfile(fileName, false); }

        private void loadProfile(string fileName, bool quiet)
        {
            try
            {
                FileInfo fiDest = new FileInfo(fileName);

                FileStream fsDest = fiDest.Open(FileMode.Open, FileAccess.Read);
                DeviceItemProfile deviceItemProfile = DeviceItem.Load(fsDest);
                mCurrentDeviceItem = deviceItemProfile.DeviceItem;
                fsDest.Close();
                if (DriverResManager.Check())
                {
                    wizMain.NextTo(wizardPageConfigDevice);
                    return;
                }
                MessageBox.Show(this, "Driver resources were not found.");
            }
            catch (Exception ex)
            {
                if (!quiet)
                    MessageBox.Show(ex.ToString(), "Failed loading profile.");
            }
        }

        private void InfWizardForm_Load(object sender, EventArgs e)
        {
            if (!DriverResManager.LoadResources())
            {
                
            }
        }

        private void rbDeviceSelection_DriverlessDevices_CheckedChanged(object sender, EventArgs e) { refreshDeviceSelectionView(); }


        private void gridDeviceSelection_SelectionChanged(object sender, EventArgs e)
        {
            if (gridDeviceSelection.SelectedRows.Count>0)
            {
                mCurrentDeviceItem = (DeviceItem) gridDeviceSelection.CurrentRow.Tag;
                wizMain.NextEnabled = true;
                cmdRemoveDevice.Enabled = true;
                return;
            }
            wizMain.NextEnabled = false;
            cmdRemoveDevice.Enabled = false;
        }

        private void buttonInstallDriver_Click(object sender, EventArgs e)
        {
            DialogResult dr = DialogResult.OK;
            if (SetupApi.CMP_WaitNoPendingInstallEvents(0) != 0)
            {
                WaitForSetupForm fWait = new WaitForSetupForm();
                dr = fWait.ShowDialog(this);
            }
            if (dr == DialogResult.OK)
            {
                buttonInstallDriver.Enabled = false;
                wizardPageFinishedEnable(false);
                Thread installSetupThread = new Thread(InstallSetupPackageFn);
                installSetupThread.Start(null);
            }
        }

        private void InstallSetupPackageFn(object state)
        {
            Thread.Sleep(0);
            rtfFinishedSatus.LoggingEnabled = true;

            InfWizardStatus.Log(CategoryType.InstallSetupPackage, StatusType.Info, "installing driver, please wait..");

            bool success = Wdi.InstallSetupPackage(mCurrentDeviceItem.BuildInfHardwareID(), mCurrentInfWriter.GetDriverPathFilename("inf"));

            if (success)
                InfWizardStatus.Log(CategoryType.InstallSetupPackage, StatusType.Success, "driver installed successfully");
            else
                InfWizardStatus.Log(CategoryType.InstallSetupPackage, StatusType.Error, "driver install failed");

            rtfFinishedSatus.LoggingEnabled = false;
            Invoke(new GenericStateDelegate(InstallSetupPackageCompleteFn), new object[] { success });
        }

        private void InstallSetupPackageCompleteFn(object state)
        {
            mCurrentDeviceItem.mDriverless = false;
            bool success = (bool)state;
            if (success)
            {
                labelInstallDriver.Text = resInfWizard.INSTALLDRIVER_SUCCESS;
                pictureBoxInstallDriver.Image = resInfWizard.RightArrowImage;

            }
            else
            {
                labelInstallDriver.Text = resInfWizard.INSTALLDRIVER_FAILED;
                pictureBoxInstallDriver.Image = resInfWizard.StopImage;

            }
            wizardPageFinishedEnable(true);
        }

        private void comboBoxDriverResource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDriverResource.SelectedIndex >= 0)
            {
                toolTip1.SetToolTip(comboBoxDriverResource, ((DriverResource) comboBoxDriverResource.SelectedItem).Strings["Description"]);
            }
        }

        private void wizardPageFinishedEnable(bool enable)
        {
            wizMain.NextEnabled = enable;
            wizMain.BackEnabled = enable;
            wizMain.CancelEnabled = enable;
            groupBoxInstallDriver.Enabled = enable;
        }

        private void buttonJumpToLocateDriverResources_Click(object sender, EventArgs e)
        {
        }

        private void linkToLudnDriverResources_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://sites.google.com/site/libusbdotnet/driver_resources");
        }

        #region WIZARD PAGES SHOW/CLOSE

        private void wizardPageConfigDevice_CloseFromBack(object sender, PageEventArgs e)
        {
            mSettings.Last_CreateDriverDirectory = mCurrentDeviceItem.CreateDriverDirectory;
            mSettings.Last_SaveDirectory = mCurrentDeviceItem.SaveDirectory;
        }

        private void wizardPageConfigDevice_CloseFromNext(object sender, PageEventArgs e)
        {
            mSettings.Last_SaveDirectory = mCurrentDeviceItem.SaveDirectory;
            mSettings.Save();
        }

        private void wizardPageConfigDevice_ShowFromNext(object sender, EventArgs e)
        {
            comboBoxDriverResource.Items.Clear();
            foreach (DriverResource driverResource in DriverResManager.ResourceList)
                comboBoxDriverResource.Items.Add(driverResource);

            int iSelIndex = -1;
            if (!String.IsNullOrEmpty(mCurrentDeviceItem.mLastDriverName))
                iSelIndex = comboBoxDriverResource.FindStringExact(mCurrentDeviceItem.mLastDriverName);
            
            if (iSelIndex==-1 && !String.IsNullOrEmpty(mSettings.Last_DriverName))
                iSelIndex = comboBoxDriverResource.FindStringExact(mSettings.Last_DriverName);
            
            comboBoxDriverResource.SelectedIndex = iSelIndex==-1 ? 0 : iSelIndex;

            refreshDeviceConfigurationView(true);
        }

        private void wizardPageSelectDevice_ShowFromNext(object sender, EventArgs e) { refreshDeviceSelectionView(); }

        private void wizardPageFinished_CloseFromNext(object sender, PageEventArgs e)
        {
            // TODO: Move ShellExecute to a button on the final wizard page.
            ShellExecute(IntPtr.Zero, "open", mCurrentDeviceItem.SaveDirectory, null, null, ShowCommands.SW_SHOWNORMAL);
            e.CloseInFinsh = true;
        }

        private void wizardPageFinished_CloseFromBack(object sender, PageEventArgs e) { rtfFinishedSatus.LoggingEnabled = false; }

        private delegate void GenericStateDelegate(object state);

        private void wizardPageFinished_ShowFromNext(object sender, EventArgs e)
        {
            rtfFinishedSatus.Clear();
            wizardPageFinishedEnable(false);
            labelInstallDriver.Image = null;
            labelInstallDriver.Text = "please wait..";
            mCurrentDriverResource = comboBoxDriverResource.SelectedItem as DriverResource;
            mCurrentInfWriter = new InfWriter(mCurrentDeviceItem, mCurrentDriverResource);

            Thread writeSetupThread = new Thread(writeSetupPackageFn);
            writeSetupThread.Start(null);
        }

        private List<DeviceItem> mFoundLikeDevices;

        private void writeSetupPackageFn(object state)
        {
            mFoundLikeDevices = new List<DeviceItem>();

            rtfFinishedSatus.LoggingEnabled = true;
            mSetupPackageWritten = mCurrentInfWriter.Write();
            rtfFinishedSatus.LoggingEnabled = false;

            if (!mSetupPackageWritten) return;

            if (mSettings.UserDeviceManufacturers == null) mSettings.UserDeviceManufacturers = new StringCollection();
            if (!mSettings.UserDeviceManufacturers.Contains(mCurrentDeviceItem.Manufacturer)) mSettings.UserDeviceManufacturers.Add(mCurrentDeviceItem.Manufacturer);
            while (mSettings.UserDeviceManufacturers.Count > 5) mSettings.UserDeviceManufacturers.RemoveAt(0);

            mCurrentDeviceItem.SaveUserSettings(mSettings);
            mSettings.Last_DriverName = mCurrentDriverResource.DisplayName;
            mSettings.Save();

            // Check (by hardware ID) if connected? driverless?
            DeviceEnumeratorInfo deCheck = new DeviceEnumeratorInfo(DEIFlags.DICFG_AllClasses|DEIFlags.IncludeWindowsServices,IntPtr.Zero);
            DeviceSelectionHelper.EnumerateDevices(deCheck);


            string currentHardwareID = mCurrentDeviceItem.BuildInfHardwareID();
            foreach (DeviceItem deviceItem in deCheck.DeviceList)
            {
                if (currentHardwareID != deviceItem.BuildInfHardwareID()) continue;
                mFoundLikeDevices.Add(deviceItem);
            }
            Invoke(new GenericStateDelegate(writeSetupPackageCompletedFn), new object[] {null});
        }

        private void writeSetupPackageCompletedFn(object state)
        {
            if (mSetupPackageWritten)
            {
                rtfFinishedSatus.LoggingEnabled = true;
                foreach (DeviceItem deviceItem in mFoundLikeDevices)
                {
                    string connected = deviceItem.IsConnected ? "connected" : "unplugged";
                    string driverless = deviceItem.IsDriverless ? "without a driver" : "with a driver";
                    string description = deviceItem.DeviceDescription;
                    if (!deviceItem.mDriverless)
                    {
                        mCurrentDeviceItem.mDriverless = false;
                        if (deviceItem.IsConnected)
                        {
                            mCurrentDeviceItem.mDeviceId = deviceItem.mDeviceId;
                            mCurrentDeviceItem.mIsConnected = true;
                        }
                        if (!String.IsNullOrEmpty(deviceItem.mServiceName))
                        {
                            mCurrentDeviceItem.mServiceName = deviceItem.mServiceName;
                        }
                    }

                    Color customColor = deviceItem.mDriverless ? Color.DarkBlue : Color.DarkOrange;
                    if (deviceItem.mIsSkipServiceName) customColor = Color.DarkRed;
                    string serviceName = String.IsNullOrEmpty(deviceItem.mServiceName) ? "" : " (" + deviceItem.mServiceName + ")";

                    object[] customLog = new object[]
                                             {
                                                 "\n", FontStyle.Bold, customColor, "InstallCheck", Color.Black, " :\t", FontStyle.Regular, description, serviceName, "\n",
                                                 "\t\tfound ", connected, " device ", driverless, " (", deviceItem.mDeviceId.Substring(4), ")\n"
                                             };
                    InfWizardStatus.LogRaw(customLog);

                    //InfWizardStatus.Log(CategoryType.EnumerateDevices,
                    //                    deviceItem.IsConnected && !deviceItem.mDriverless ? StatusType.Warning : StatusType.Info,
                    //                    "found {0} {1} device {2} matching {3}",
                    //                    connected,
                    //                    driverless,
                    //                    description,
                    //                    currentHardwareID.Substring(4));
                }
                List<DeviceItem> driverlessList = mFoundLikeDevices.FindAll(DeviceItem.FindDriverlessPredicate);
                List<DeviceItem> skipServiceList = mFoundLikeDevices.FindAll(DeviceItem.FindSkipServicePredicate);
                
                if (driverlessList.Count == mFoundLikeDevices.Count)
                    mCurrentDeviceItem.mDriverless = true;
                if (mFoundLikeDevices.Count == 0)
                    mCurrentDeviceItem.mDriverless = true;

                if (skipServiceList.Count>0)
                {
                    pictureBoxInstallDriver.Image = resInfWizard.StopImage;
                    labelInstallDriver.Text = String.Format(resInfWizard.INSTALLDRIVER_WINDOWSSERVICE_TEXT,mCurrentDeviceItem.mServiceName);
                    buttonInstallDriver.Enabled = false;
                   
                }
                else if ((mCurrentDeviceItem.mDriverless))
                {
                    pictureBoxInstallDriver.Image = resInfWizard.InfoImage;
                    labelInstallDriver.Text = resInfWizard.INSTALLDRIVER_DRIVERLESS_TEXT;
                    buttonInstallDriver.Enabled = true;
                }
                else
                {
                    pictureBoxInstallDriver.Image = resInfWizard.YieldImage;
                    labelInstallDriver.Text = String.Format(resInfWizard.INSTALLDRIVER_UPDATE_TEXT, mCurrentDeviceItem.mServiceName);
                    buttonInstallDriver.Enabled = true;
                }
            }
            else
            {
                pictureBoxInstallDriver.Image = resInfWizard.StopImage;
                labelInstallDriver.Text = resInfWizard.INSTALLDRIVER_NOPACKAGE_TEXT;
                buttonInstallDriver.Enabled = false;
            }
            rtfFinishedSatus.LoggingEnabled = false;

            wizardPageFinishedEnable(true);
        }


        private void wizardPageWelcome_CloseFromNext(object sender, PageEventArgs e)
        {
            rtfWelcomeStatus.LoggingEnabled = false;

            if (mSkipToDriverResourceLocator)
            {
                mSkipToDriverResourceLocator = false;
                return;
            }
            if (DriverResManager.Check())
            {
                SynchronizationContext.Current.Post(moveToWizardPage, wizardPageSelectDevice);
            }
        }

        private void moveToWizardPage(object wizardPage) { wizMain.NextTo((WizardPage) wizardPage); }


        private void wizardPageSelectDevice_CloseFromBack(object sender, PageEventArgs e) { SynchronizationContext.Current.Post(moveToWizardPage, wizardPageWelcome); }

        private void wizardPageGetDrivers_ShowFromNext(object sender, EventArgs e)
        {
            wizMain.NextEnabled = DriverResManager.Check();
        }

        #endregion

        private void radioButtonDownloadDrivers_Click(object sender, EventArgs e)
        {
            groupBoxDownloadStatus.Enabled = true;
            downloadDriverResourceList();
        }

        private void buttonCancelDownload_Click(object sender, EventArgs e)
        {
            buttonCancelDownload.Enabled = false;
            driverResourceDownloader.Abort();
        }

        private void buttonDownloadDriverResources_Click(object sender, EventArgs e) { downloadDriverResources(); }

        private void dataGridViewDriverDownloadList_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewDriverDownloadList.SelectedRows.Count > 0)
                buttonDownloadDriverResources.Enabled = true;
            else
                buttonDownloadDriverResources.Enabled = false;
        }

        private void buttonSelectAllDriverResources_Click(object sender, EventArgs e) { selectDriverResources(true); }

        private void buttonSelectNoneDriverResources_Click(object sender, EventArgs e) { selectDriverResources(false); }

        private void wizardPageWelcome_ShowFromBack(object sender, EventArgs e)
        {
            rtfWelcomeStatus.LoggingEnabled = true;
        }

        private void dataGridViewDriverDownloadList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
           if (e.ColumnIndex==1)
           {
               e.Value = e.Value.ToString().Replace("\r\n", " ").Replace("\n", " ");
               e.FormattingApplied = true;
           }
        }

        private void deviceSelection_ConnectedOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            deviceSelection_CreateNewRadio.CheckedChanged -= deviceSelection_CreateNewRadio_CheckedChanged;
            deviceSelection_CreateNewRadio.Checked = false;
            deviceSelection_CreateNewRadio.CheckedChanged += deviceSelection_CreateNewRadio_CheckedChanged;
            
            refreshDeviceSelectionView();
        }

        private void deviceSelection_DriverlessOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            deviceSelection_CreateNewRadio.CheckedChanged -= deviceSelection_CreateNewRadio_CheckedChanged;
            deviceSelection_CreateNewRadio.Checked = false;
            deviceSelection_CreateNewRadio.CheckedChanged += deviceSelection_CreateNewRadio_CheckedChanged;

            refreshDeviceSelectionView();
        }

        private void deviceSelection_CreateNewRadio_CheckedChanged(object sender, EventArgs e)
        {
            refreshDeviceSelectionView();
        }

        private void deviceSelection_RefreshButton_Click(object sender, EventArgs e)
        {
            refreshDeviceSelectionView();
        }

        private void linkLoadProfile_Click(object sender, EventArgs e)
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

        private void linkJumpToDownloadDriverResources_Click(object sender, EventArgs e)
        {
            mSkipToDriverResourceLocator = true;
            wizMain.Next();
        }

    }
}