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
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.LudnMonoLibUsb;
using EC = LibUsbDotNet.Main.ErrorCode;

namespace Test_Bulk
{
    // ReSharper disable InconsistentNaming
    public partial class fTestBulk : Form
    {
        private UsbDevice mUsbDevice;
        private UsbEndpointReader mEpReader;
        private UsbEndpointWriter mEpWriter;
        private string mLogFileName = String.Empty;
        private FileStream mLogFileStream;
        private UsbRegDeviceList mRegDevices;

        public fTestBulk()
        {
            InitializeComponent();
            UsbDevice.UsbErrorEvent += UsbGlobals_UsbErrorEvent;
        }

        #region STATIC Members

        /// <summary>
        /// Converts bytes into a hexidecimal string
        /// </summary>
        /// <param name="data">Bytes to converted to a a hex string.</param>
        private static StringBuilder GetHexString(byte[] data, int offset, int length)
        {
            StringBuilder sb = new StringBuilder(length*3);
            for (int i = offset; i < (offset + length); i++)
            {
                sb.Append(data[i].ToString("X2") + " ");
            }
            return sb;
        }

        #endregion

        private void UsbGlobals_UsbErrorEvent(object sender, UsbError e) { Invoke(new UsbErrorEventDelegate(UsbGlobalErrorEvent), new object[] {sender, e}); }

        private void UsbGlobalErrorEvent(object sender, UsbError e) { tRecv.AppendText(e + "\r\n"); }

        private void chkRead_CheckedChanged(object sender, EventArgs e)
        {
            if (mEpReader != null)
            {
                chkRead.Enabled = false;
                if (chkRead.Checked)
                {
                    // If the autorea
                    mEpReader.DataReceivedEnabled = true;
                    cmdRead.Enabled = false;
                }
                else
                {
                    mEpReader.DataReceivedEnabled = false;
                    cmdRead.Enabled = true;
                }
                chkRead.Enabled = true;
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e) { tRecv.Text = ""; }

        private void closeDevice()
        {
            if (mUsbDevice != null)
            {
                if (mUsbDevice.IsOpen)
                {


                    if (mEpReader != null)
                    {
                        mEpReader.DataReceivedEnabled = false;
                        mEpReader.DataReceived -= mEp_DataReceived;
                        mEpReader.Dispose();
                        mEpReader = null;
                    }
                    if (mEpWriter != null)
                    {
                        mEpWriter.Abort();
                        mEpWriter.Dispose();
                        mEpWriter = null;
                    }

                    // If this is a "whole" usb device (libusb-win32, linux libusb)
                    // it will have an IUsbDevice interface. If not (WinUSB) the 
                    // variable will be null indicating this is an interface of a 
                    // device.
                    IUsbDevice wholeUsbDevice = mUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);
                    }

                    mUsbDevice.Close();
                    mUsbDevice = null;
                    chkLogToFile.Checked = false;
                }

            }
            panTransfer.Enabled = false;
        }


        private void cmdOpen_Click(object sender, EventArgs e)
        {
            cmdOpen.Enabled = false;
            if (cmdOpen.Text == "Open")
            {
                if (cboDevices.SelectedIndex >= 0)
                {
                    if (openDevice(cboDevices.SelectedIndex))
                    {
                        cmdOpen.Text = "Close";
                    }
                }
            }
            else
            {
                closeDevice();
                cmdOpen.Text = "Open";
            }
            cmdOpen.Enabled = true;
        }

        private void cmdRead_Click(object sender, EventArgs e)
        {
            cmdRead.Enabled = false;
            byte[] readBuffer = new byte[64];

            int uiTransmitted;
            ErrorCode eReturn;
            if ((eReturn = mEpReader.Read(readBuffer, 1000, out uiTransmitted)) == ErrorCode.None)
            {
                tsStatus.Text = uiTransmitted + " bytes read.";
                showBytes(readBuffer, uiTransmitted);
            }
            else
                tsStatus.Text = "No data to read! " + eReturn;

            cmdRead.Enabled = true;
        }

        private void cmdWrite_Click(object sender, EventArgs e)
        {
            cmdWrite.Enabled = false;
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(tWrite.Text);

            int uiTransmitted;
            if (mEpWriter.Write(bytesToWrite, 1000, out uiTransmitted) == ErrorCode.None)
            {
                tsStatus.Text = uiTransmitted + " bytes written.";
            }
            else
                tsStatus.Text = "Write failed!";

            cmdWrite.Enabled = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeDevice();
            Application.Exit();
        }

        private void fTestBulk_FormClosing(object sender, FormClosingEventArgs e)
        {
            closeDevice();
            UsbDevice.Exit();
            CloseLogFile();
        }

        private void getConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte bCfgValue;
            if (mUsbDevice.GetConfiguration(out bCfgValue))
            {
                tsStatus.Text = "Configuration Value:" + bCfgValue;
            }
            else
                tsStatus.Text = "Failed getting configuration value!";
        }

        private void mEp_DataReceived(object sender, EndpointDataEventArgs e) { Invoke(new OnDataReceivedDelegate(OnDataReceived), new object[] {sender, e}); }

        private void OnDataReceived(object sender, EndpointDataEventArgs e)
        {
            if (chkLogToFile.Checked && mLogFileStream != null)
            {
                if (ckShowAsHex.Checked)
                {
                    // get the bytes as a hex string
                    StringBuilder sb = GetHexString(e.Buffer, 0, e.Count);

                    // get the hexstring as bytes and write to log file
                    Byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
                    mLogFileStream.Write(data, 0, data.Length);
                }
                else
                    mLogFileStream.Write(e.Buffer, 0, e.Count);
            }
            else
            {
                showBytes(e.Buffer, e.Count);
            }
        }

        private bool openDevice(int index)
        {
            bool bRtn = false;

            closeDevice();
            chkRead.CheckedChanged -= chkRead_CheckedChanged;
            chkRead.Checked = false;
            cmdRead.Enabled = true;
            chkRead.CheckedChanged += chkRead_CheckedChanged;

            if (mRegDevices[index].Open(out mUsbDevice))
            {
                bRtn = true;

                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = mUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                if (bRtn)
                {
                    if (String.IsNullOrEmpty(comboBoxEndpoint.Text)) comboBoxEndpoint.SelectedIndex = 0;

                    byte epNum = byte.Parse(comboBoxEndpoint.Text);
                    mEpReader = mUsbDevice.OpenEndpointReader((ReadEndpointID)(epNum | 0x80));
                    mEpWriter = mUsbDevice.OpenEndpointWriter((WriteEndpointID)epNum);
                    mEpReader.DataReceived += mEp_DataReceived;
                    mEpReader.Flush();
                    panTransfer.Enabled = true;
                }
            }

            if (bRtn)
            {
                tsStatus.Text = "Device Opened.";
            }
            else
            {
                tsStatus.Text = "Device Failed to Opened!";
                if (!ReferenceEquals(mUsbDevice, null))
                {
                    if (mUsbDevice.IsOpen) mUsbDevice.Close();
                    mUsbDevice = null;
                }
            }

            return bRtn;
        }

        private void showBytes(byte[] readBuffer, int uiTransmitted)
        {
            if (ckShowAsHex.Checked)
            {
                // Convert the data to a hex string before displaying
                tRecv.AppendText(GetHexString(readBuffer, 0, uiTransmitted).ToString());
            }
            else
            {
                // Display the raw data
                tRecv.AppendText(Encoding.UTF8.GetString(readBuffer, 0, uiTransmitted));
            }
        }

        private void standardRequestsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (mUsbDevice != null && mUsbDevice.IsOpen)
            {
                standardRequestsToolStripMenuItem.Enabled = true;
            }
            else
            {
                standardRequestsToolStripMenuItem.Enabled = false;
            }
        }


        private void cboDevices_DropDown(object sender, EventArgs e)
        {
            // Get a new device list each time the device dropdown is opened
            cboDevices.Items.Clear();
            mRegDevices = UsbDevice.AllDevices;

            foreach (UsbRegistry regDevice in mRegDevices)
            {
                // add the Vid, Pid, and usb device description to the dropdown display.
                // NOTE: There are many more properties available to provide you with more device information.
                // See the LibUsbDotNet.Main.SPDRP enumeration.
                string sItem = String.Format("Vid:{0} Pid:{1} {2}",
                                             regDevice.Vid.ToString("X4"),
                                             regDevice.Pid.ToString("X4"),
                                             regDevice.FullName);
                cboDevices.Items.Add(sItem);
            }
            tsNumDevices.Text = cboDevices.Items.Count.ToString();
        }

        private void chkLogToFile_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLogToFile.Checked)
            {
                grpLogToFile.Enabled = true;
                mLogFileName = txtLogFile.Text;
                OpenLogFile();
            }
            else
            {
                mLogFileName = String.Empty;
                CloseLogFile();
                grpLogToFile.Enabled = false;
            }
        }

        private void cmdOpenLogFile_Click(object sender, EventArgs e)
        {
            if (sfdLogFile.ShowDialog(this) == DialogResult.OK)
            {
                mLogFileName = sfdLogFile.FileName;
                OpenLogFile();
            }
        }

        private void OpenLogFile()
        {
            try
            {
                CloseLogFile();
                mLogFileStream = File.Open(mLogFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                mLogFileStream.Seek(0, SeekOrigin.End);
                txtLogFile.Text = mLogFileName;
            }
            catch
            {
                txtLogFile.Text = String.Empty;
                CloseLogFile();
            }
        }

        private void CloseLogFile()
        {
            if (mLogFileStream != null)
            {
                mLogFileStream.Flush();
                mLogFileStream.Close();
                mLogFileStream = null;
            }
        }

        #region Nested Types

        private delegate void OnDataReceivedDelegate(object sender, EndpointDataEventArgs e);

        private delegate void UsbErrorEventDelegate(object sender, UsbError e);

        #endregion

    }
}