using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using InfWizard.WizardClassHelpers;
using WinApiNet;

namespace InfWizard
{
    public partial class fRemoveDevice : Form
    {
        private readonly DeviceItem mDeviceItem;
        public static RemoveDeviceOptions DefaultRemoveOptions;
        public class RemoveDeviceOptions
        {
            private readonly DeviceItem mDeviceItem;

            public RemoveDeviceOptions(DeviceItem deviceItem) {
                mDeviceItem = deviceItem;
            }

            private bool mDeepClean;
            private bool mOnlyConnectedDevices;

            [DisplayName("Deep Clean INF Directory")]
            [Description("Scans the inf directory for all inf files using this usb devices VID and PID.")]
            [Category("Remove Options")]
            public bool DeepClean
            {
                get { return mDeepClean; }
                set { mDeepClean = value; }
            }

            [DisplayName("Connected Devices Only")]
            [Description("Only search for usb device that are currently connect to the host.")]
            [Category("Remove Options")]
            public bool OnlyConnectedDevices
            {
                get
                {
                    return mOnlyConnectedDevices;
                }
                set
                {
                    mOnlyConnectedDevices = value;
                }
            }

            [Category("USB Device Information")]
            [DisplayName("Description")]
            public string DeviceDescription
            {
                get
                {
                    return mDeviceItem.DeviceDescription;
                }
            }
            [Category("USB Device Information")]
            public string Manufacturer
            {
                get
                {
                    return mDeviceItem.Manufacturer;
                }
            }
            [Category("USB Device Information")]
            public string VendorID
            {
                get
                {
                    return mDeviceItem.VendorID;
                }
            }

            [Category("USB Device Information")]
            public string ProductID
            {
                get
                {
                    return mDeviceItem.ProductID;
                }
            }

            [Browsable(false)]
            public DeviceItem DeviceItem
            {
                get { return mDeviceItem; }
            }


        }
        public fRemoveDevice()
        {
            InitializeComponent();
        }

        public fRemoveDevice(DeviceItem deviceItem, bool onlyConnectedDevices)
            :this()
        {
            mDeviceItem = deviceItem;
            DefaultRemoveOptions = new RemoveDeviceOptions(mDeviceItem);
            DefaultRemoveOptions.OnlyConnectedDevices = onlyConnectedDevices;
            warningLabel.Text = Properties.Resources.REMOVE_DEVICE_WARNING;
            removeDeviceOptionGrid.SelectedObject = DefaultRemoveOptions;
        }

        private void Remove()
        {
            bool bSuccess;
            SetupApi.DICFG flags = SetupApi.DICFG.ALLCLASSES;
            Cursor = Cursors.WaitCursor;
            Enabled = false;

            if (DefaultRemoveOptions.OnlyConnectedDevices)
                flags = flags | SetupApi.DICFG.PRESENT;

            try
            {
                bSuccess = DeviceSelectionHelper.RemoveDevice(DefaultRemoveOptions, Handle, flags);
                if (!bSuccess) throw new Exception("Device Removal Failed!");

                if (DefaultRemoveOptions.DeepClean)
                {
                    if (DefaultRemoveOptions.DeepClean) 
                        bSuccess = DeepClean();

                    if (!bSuccess) throw new Exception("DeepClean Failed!");
                }

                MessageBox.Show(this, "Success!", "Device Removal");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                string message;
                SetupApi.GetLastWin32ErrorDetails(ex.Message, out message);
                MessageBox.Show(this, message, "Device Removal", MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            finally
            {
                Enabled = true;
                Cursor = Cursors.Default;
            }

        }

        private static bool DeepClean() 
        {
            try
            {
                // %USB\PicFW_Bootloader.DeviceDesc% =PicFW_Bootloader_Install, USB\VID_5472&PID_0002
                string sVidPidScan = String.Format("VID_{0}&PID_{1}", DefaultRemoveOptions.VendorID.Trim().ToUpper(), DefaultRemoveOptions.ProductID.Trim().ToUpper());
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
                            if (line.Contains(sVidPidScan))
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

                    if (MessageBox.Show(sb.ToString(), String.Format("Deep Clean: Remove {0} inf files?", foundInfFileList.Count), MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.OK)
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

        private void removeCommand_Click(object sender, EventArgs e)
        {
            Remove();
        }
    }
}