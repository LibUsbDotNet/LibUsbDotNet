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
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using TestDevice;

// ReSharper disable InconsistentNaming

namespace Benchmark
{
    public partial class fBenchmark : Form
    {
        private readonly BenchMarkParameters mBenchMarkParameters = new BenchMarkParameters();
        private byte bValidatePosEP1;
        private bool bWriteThreadEP1Enabled;
        private byte[] loopTestBytes;
        private bool mbInLoopTestError;
        private StopWatch mEndPointStopWatch = new StopWatch();
        private UsbEndpointReader mEP1Reader;
        private UsbEndpointWriter mEP1Writer;
        private int mLoopTestCompletedPackets;
        private int mLoopTestOffset;
        private byte mReadTestPID;
        private UsbRegDeviceList mRegInfo;

        private Thread mthWriteThreadEP1;
        private UsbDevice mUsbDevice;
        private UsbTestType mUsbTestType;

        private UsbEndpointInfo mReadEndpointInfo;
        private UsbEndpointInfo mWriteEndpointInfo;
        private UsbInterfaceInfo mInterfaceInfo;

        public fBenchmark() { InitializeComponent(); }


        private void cboTestType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!mUsbDevice.IsOpen) return;

            timerUpdateUI.Enabled = false;

            UsbTestType currentTestType = mUsbTestType;
            UsbTestType newTestType = (UsbTestType) cboTestType.SelectedItem;

            if (newTestType == currentTestType) return;

            cboTestType.Enabled = false;

            stopReadWrite();

            if (PIC18TestDevice.SetTestType(mUsbDevice, newTestType, false, mInterfaceInfo.Descriptor.InterfaceID))
            {
                SetStatus("Test Selected:" + newTestType, false);
            }
            else
            {
                SetStatus("Test Select Failed", true);
            }

            ResetBenchmark();
            mUsbTestType = newTestType;

            if (newTestType == UsbTestType.WriteToDevice)
            {
                mthWriteThreadEP1 = new Thread(WriteThreadEP1_NoRecv);
                bWriteThreadEP1Enabled = true;
                mthWriteThreadEP1.Start();
            }
            else if (newTestType == UsbTestType.Loop)
            {
                mthWriteThreadEP1 = new Thread(WriteThreadEP1);
                bWriteThreadEP1Enabled = true;
                mthWriteThreadEP1.Start();
            }

            if (newTestType == UsbTestType.None)
            {
                ResetBenchmark();
                panTest.Enabled = false;
            }
            else
            {
                timerUpdateUI.Stop();
                timerUpdateUI.Interval = mBenchMarkParameters.RefreshDisplayInterval;
                timerUpdateUI.Enabled = true;
                timerUpdateUI.Start();

                panTest.Enabled = true;
            }
            if (newTestType != UsbTestType.WriteToDevice)
                mEP1Reader.DataReceivedEnabled = true;

            cboTestType.Enabled = true;
        }

        private void closeTestDevice(object state)
        {
            if (!ReferenceEquals(mUsbDevice, null))
            {
                if (mUsbDevice.IsOpen)
                {
                    mEP1Reader.DataReceived -= OnDataReceived;
                    stopReadWrite();
                    mUsbDevice.ActiveEndpoints.Clear();
                    UsbDevice.UsbErrorEvent -= OnUsbError;

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
                }
                mUsbDevice = null;
                mEP1Reader = null;
                mEP1Writer = null;
            }
            cmdOpenClose.Text = "Open";
            panDevice.Enabled = false;
        }

        private void cmdGetTestType_Click(object sender, EventArgs e)
        {
            cmdGetTestType.Enabled = false;
            if (mUsbDevice != null && mUsbDevice.IsOpen)
            {
                //bool bRecvEnabled = mEP1.DataReceivedEnabled;
                //mEP1.DataReceivedEnabled = false;
                UsbTestType bTestType;
                if (getTestType(out bTestType, mInterfaceInfo.Descriptor.InterfaceID))
                {
                    SetStatus("Test Type:" + bTestType, false);
                }
                else
                    SetStatus("GetTestType Failed.", true);

                //mEP1.DataReceivedEnabled = bRecvEnabled;
            }
            cmdGetTestType.Enabled = true;
        }

        private void cmdOpenClose_Click(object sender, EventArgs e)
        {
            cmdOpenClose.Enabled = false;
            if (cmdOpenClose.Text == "Open" && cboDevice.SelectedIndex != -1)
            {
                // Open Device
                UsbRegistry winUsbRegistry = mRegInfo[cboDevice.SelectedIndex];

                if (openAsTestDevice(winUsbRegistry))
                {
                    benchParamsPropertyGrid.Enabled = false;
                    SetStatus("Device Opened", false);
                    cmdOpenClose.Text = "Close";
                    panDevice.Enabled = true;
                }
            }
            else
            {
                closeTestDevice(mUsbDevice);
                benchParamsPropertyGrid.Enabled = true;
                SetStatus("Device Closed", false);
            }
            cmdOpenClose.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!ReferenceEquals(null, mUsbDevice))
                    closeTestDevice(mUsbDevice);
                UsbDevice.Exit();
            }
            catch
            {
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            benchParamsPropertyGrid.SelectedObject = mBenchMarkParameters;
            cboTestType.Items.AddRange(new object[] {UsbTestType.None, UsbTestType.ReadFromDevice, UsbTestType.WriteToDevice, UsbTestType.Loop});
        }

        private void GetConfigValue_Click(object sender, EventArgs e)
        {
            byte bConfig;
            if (mUsbDevice.GetConfiguration(out bConfig))
            {
                ReadOnlyCollection<UsbConfigInfo> configProfiles = mUsbDevice.Configs;
                if (bConfig == 0 || bConfig > configProfiles.Count)
                {
                    tInfo.AppendText("[ERROR] Invalid configuration data received.");
                    return;
                }
                UsbConfigInfo currentConfig = configProfiles[bConfig - 1];
                SetStatus(string.Format("Config Value:{0} Size:{1}", bConfig, currentConfig.Descriptor.TotalLength), false);

                tInfo.AppendText(currentConfig.ToString());
            }
            else
                SetStatus("GetConfiguration Failed.", true);
        }

        private bool getTestType(out UsbTestType testType, byte interfaceID)
        {
            if (mUsbDevice.IsOpen)
            {
                if (PIC18TestDevice.GetTestType(mUsbDevice, out testType, interfaceID))
                    return true;

                testType = 0;
                return false;
            }

            throw new Exception("Device Not Opened");
        }

        private static void makeTestBytes(out byte[] bytes, int size)
        {
            Random r = new Random();
            bytes = new byte[size];
            r.NextBytes(bytes);
            for (int i = 0; i < size; i++)
            {
                if (bytes[i] == 0 || bytes[i] == 0x80)
                    bytes[i] = 0xff;
            }
            bytes[0] = 0;
        }

        private void OnDataReceived(object sender, EndpointDataEventArgs e)
        {
            if (mUsbTestType == UsbTestType.Loop)
            {
                OnDataReceived_LoopTest(sender, e);
            }
            else
            {
                for (int i = 0; i < e.Count; i++)
                {
                    if (mbInLoopTestError)
                    {
                        if (e.Buffer[i] == 0)
                        {
                            mReadTestPID = e.Buffer[i + 1];
                            mbInLoopTestError = false;
                        }
                        else
                            continue;
                    }

                    if (e.Buffer[i] == 0)
                    {
                        mLoopTestCompletedPackets++;

                        if (e.Buffer[i + 1] == mReadTestPID)
                        {
                            i++;
                            mReadTestPID++;
                            bValidatePosEP1 = 2;
                        }
                        else
                        {
                            mbInLoopTestError = true;
                            if (mLoopTestCompletedPackets > 1)
                            {
                                mEndPointStopWatch.PacketErrorCount++;
                                SetStatus(string.Format("Data validation mismatch at position:{0}/{1}.", i, loopTestBytes.Length), true);
                            }
                            break;
                        }
                    }
                    else if (e.Buffer[i] == bValidatePosEP1)
                    {
                        bValidatePosEP1++;
                    }
                }

                if (mLoopTestCompletedPackets < 2) return;

                if (!mEndPointStopWatch.IsStarted)
                {
                    mEndPointStopWatch.DiffWithNow();
                    return;
                }

                UpdateDataRate(e.Count);
            }
        }

        private void OnDataReceived_LoopTest(object sender, EndpointDataEventArgs e)
        {
            if (mBenchMarkParameters.Verify)
            {
                for (int i = 0; i < e.Count; i++)
                {
                    if (mbInLoopTestError)
                    {
                        if (e.Buffer[i] == 0)
                        {
                            mLoopTestOffset = 0;
                            mbInLoopTestError = false;
                        }
                        else
                            continue;
                    }
                    if (mLoopTestOffset >= loopTestBytes.Length)
                    {
                        mLoopTestOffset = 0;
                        mLoopTestCompletedPackets++;
                    }

                    if (e.Buffer[i] == loopTestBytes[mLoopTestOffset])
                        mLoopTestOffset++;
                    else
                    {
                        mbInLoopTestError = true;
                        if (mLoopTestCompletedPackets > 0)
                        {
                            mEndPointStopWatch.PacketErrorCount++;
                            SetStatus(string.Format("Data validation mismatch at position:{0}/{1}.", i, loopTestBytes.Length), true);
                        }
                        break;
                    }
                }
                if (mLoopTestCompletedPackets == 0) return;
            }

            if (!mEndPointStopWatch.IsStarted)
            {
                mEndPointStopWatch.DiffWithNow();
                return;
            }

            UpdateDataRate(e.Count);
        }


        private void OnUsbError(object sender, UsbError e) { SetStatus(e.ToString(), true); }

        private bool openAsTestDevice(UsbRegistry usbRegistry)
        {
            if (!ReferenceEquals(mUsbDevice, null))
                closeTestDevice(mUsbDevice);

            mUsbDevice = null;

            try
            {
                if (usbRegistry.Open(out mUsbDevice))
                {
                    UsbInterfaceInfo readInterfaceInfo;
                    UsbInterfaceInfo writeInterfaceInfo;

                    UsbDevice.UsbErrorEvent += OnUsbError;

                    if (!UsbEndpointBase.LookupEndpointInfo(mUsbDevice.Configs[0],
                                                       0x80,
                                                       out readInterfaceInfo,
                                                       out mReadEndpointInfo))
                    {
                        throw new Exception("failed locating read endpoint.");
                    }

                    mBenchMarkParameters.BufferSize -= (mBenchMarkParameters.BufferSize%(mReadEndpointInfo.Descriptor.MaxPacketSize));

                    if (!UsbEndpointBase.LookupEndpointInfo(mUsbDevice.Configs[0],
                                                       0x00,
                                                       out writeInterfaceInfo,
                                                       out mWriteEndpointInfo))
                    {
                        throw new Exception("failed locating write endpoint.");
                    }

                    if (((mWriteEndpointInfo.Descriptor.Attributes & 3)==(int) EndpointType.Isochronous) ||
                        ((mReadEndpointInfo.Descriptor.Attributes & 3)==(int) EndpointType.Isochronous))
                    {
                        throw new Exception("buenchmark GUI application does not support ISO endpoints. Use BenchmarkCon instead.");
                    }

                    mBenchMarkParameters.BufferSize -= (mBenchMarkParameters.BufferSize%(mWriteEndpointInfo.Descriptor.MaxPacketSize));

                    if (writeInterfaceInfo.Descriptor.InterfaceID != readInterfaceInfo.Descriptor.InterfaceID)
                        throw new Exception("read/write endpoints must be on the same interface.");

                    mEP1Reader = mUsbDevice.OpenEndpointReader(
                        (ReadEndpointID)mReadEndpointInfo.Descriptor.EndpointID,
                        mBenchMarkParameters.BufferSize,
                        (EndpointType)(mReadEndpointInfo.Descriptor.Attributes & 3));

                    mEP1Writer = mUsbDevice.OpenEndpointWriter(
                        (WriteEndpointID) mWriteEndpointInfo.Descriptor.EndpointID,
                        (EndpointType) (mWriteEndpointInfo.Descriptor.Attributes & 3));

                    mInterfaceInfo = writeInterfaceInfo;

                    mEP1Reader.ReadThreadPriority = mBenchMarkParameters.Priority;
                    mEP1Reader.DataReceived += OnDataReceived;

                    makeTestBytes(out loopTestBytes, mBenchMarkParameters.BufferSize);


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
                        wholeUsbDevice.ClaimInterface(mInterfaceInfo.Descriptor.InterfaceID);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                SetStatus(ex.Message, true);
            }

            if (!ReferenceEquals(mUsbDevice,null))
            {
                try
                {
                    closeTestDevice(mUsbDevice);
                }
                finally
                {
                    mUsbDevice = null;
                    mEP1Reader = null;
                    mEP1Writer = null;
                }
            }
            return false;
        }

        private void ResetBenchmark()
        {
            mReadTestPID = 0;
            mLoopTestCompletedPackets = 0;
            mLoopTestOffset = 0;
            mbInLoopTestError = false;
            mEndPointStopWatch.PacketErrorCount = 0;

            bValidatePosEP1 = 0;

            mEndPointStopWatch.Reset();
        }


        private void stopReadWrite()
        {
            if (mthWriteThreadEP1 != null && bWriteThreadEP1Enabled)
            {
                bWriteThreadEP1Enabled = false;
                mEP1Writer.Abort();
                DateTime dtStart = DateTime.Now;
                while (mthWriteThreadEP1.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                    if ((DateTime.Now - dtStart).TotalMilliseconds > 1000)
                    {
                        System.Diagnostics.Debug.Print("[CRITICAL ERROR] Benchmark write thread dead-locked! Terminating thread..");
                        mthWriteThreadEP1.Abort();
                        break;
                    }
                }

                mthWriteThreadEP1 = null;
            }

            mEP1Reader.DataReceivedEnabled = false;
        }

        private void WriteThreadEP1()
        {
            while (bWriteThreadEP1Enabled)
            {
                int bytesTransmitted;
                //if (mEP1Writer.Type == EndpointType.Isochronous)
                //    mEP1Writer.Reset();
                ErrorCode ec = mEP1Writer.Write(loopTestBytes, mBenchMarkParameters.Timeout, out bytesTransmitted);
                if (ec != ErrorCode.Success)
                {
                    bWriteThreadEP1Enabled = false;
                    break;
                }
                Thread.Sleep(0);
            }
        }

        private void WriteThreadEP1_NoRecv()
        {
            ResetBenchmark();

            byte[] dat = new byte[mBenchMarkParameters.BufferSize];
            for (uint i = 0; i < dat.Length; i++)
                dat[i] = (byte) (i & 0xff);
            while (bWriteThreadEP1Enabled)
            {
                int uiBytesTransmitted;

                //if (mEP1Writer.Type == EndpointType.Isochronous)
                //    mEP1Writer.Reset();
                if (mEP1Writer.Write(dat, mBenchMarkParameters.Timeout, out uiBytesTransmitted) == ErrorCode.None)
                {
                    UpdateDataRate(uiBytesTransmitted);
                }
                else
                {
                    bWriteThreadEP1Enabled = false;
                    break;
                } 
                Thread.Sleep(0);
            }
        }


        private void cboDevice_DropDown(object sender, EventArgs e)
        {
            cboDevice.Items.Clear();
            mRegInfo = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in mRegInfo)
            {
                string sCboText = "";
                object oHardwareID = usbRegistry[SPDRP.HardwareId.ToString()];
                if (oHardwareID != null && oHardwareID is string[])
                {
                    UsbSymbolicName symVidPid = UsbSymbolicName.Parse(((string[]) oHardwareID)[0]);
                    sCboText = string.Format("Vid:{0} Pid:{1} {2}", symVidPid.Vid.ToString("X4"), symVidPid.Pid.ToString("X4"), usbRegistry.FullName);
                    cboDevice.Items.Add(sCboText);
                }
            }
        }

        private void cmdGetDeviceInfo_Click(object sender, EventArgs e) { tInfo.AppendText(mUsbDevice.Info.ToString()); }

        #region Nested Types

        #endregion
    }
}