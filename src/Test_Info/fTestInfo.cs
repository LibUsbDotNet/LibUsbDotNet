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
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using LibUsbDotNet.LudnMonoLibUsb;

namespace Test_Info
{
    public partial class fTestInfo : Form
    {
        private UsbRegDeviceList mDevList;
        private UsbDevice mUsbDevice;
        private UsbRegistry mUsbRegistry;

        public fTestInfo() { InitializeComponent(); }

        #region STATIC Members
        private static StringBuilder getDescriptorReport(UsbRegistry usbRegistry)
        {
            UsbDevice usbDevice;
            StringBuilder sbReport = new StringBuilder();

            if (!usbRegistry.Open(out usbDevice)) return sbReport;

            sbReport.AppendLine(string.Format("{0} OSVersion:{1} LibUsbDotNet Version:{2} DriverMode:{3}", usbRegistry.FullName, UsbDevice.OSVersion, LibUsbDotNetVersion, usbDevice.DriverMode));
            sbReport.AppendLine(usbDevice.Info.ToString("",UsbDescriptor.ToStringParamValueSeperator,UsbDescriptor.ToStringFieldSeperator));
            foreach (UsbConfigInfo cfgInfo in usbDevice.Configs)
            {
                sbReport.AppendLine(string.Format("CONFIG #{1}\r\n{0}", cfgInfo.ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator), cfgInfo.Descriptor.ConfigID));
                foreach (UsbInterfaceInfo interfaceInfo in cfgInfo.InterfaceInfoList)
                {
                    sbReport.AppendLine(string.Format("INTERFACE ({1},{2})\r\n{0}", interfaceInfo.ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator), interfaceInfo.Descriptor.InterfaceID, interfaceInfo.Descriptor.AlternateID));

                    foreach (UsbEndpointInfo endpointInfo in interfaceInfo.EndpointInfoList)
                    {
                        sbReport.AppendLine(string.Format("ENDPOINT 0x{1:X2}\r\n{0}", endpointInfo.ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator), endpointInfo.Descriptor.EndpointID));
                    }
                }
            }
            usbDevice.Close();

            return sbReport;
        }

        #endregion

        public static string LibUsbDotNetVersion
        {
            get
            {
                Assembly assembly = Assembly.GetAssembly(typeof (UsbDevice));
                string[] assemblyKvp = assembly.FullName.Split(',');
                foreach (string s in assemblyKvp)
                {
                    string[] sKeyPair = s.Split('=');
                    if (sKeyPair[0].ToLower().Trim() == "version")
                    {
                        return sKeyPair[1].Trim();
                    }
                }
                return null;
            }
        }


        private void addDevice(UsbRegistry deviceReg, string display)
        {
            if (!deviceReg.Open(out mUsbDevice)) return;
            mUsbRegistry = deviceReg;

            TreeNode tvDevice = tvInfo.Nodes.Add(display);
            string[] sDeviceAdd = mUsbDevice.Info.ToString().Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in sDeviceAdd)
                tvDevice.Nodes.Add(s);


            foreach (UsbConfigInfo cfgInfo in mUsbDevice.Configs)
            {
                TreeNode tvConfig = tvDevice.Nodes.Add("Config " + cfgInfo.Descriptor.ConfigID);
                string[] sCfgAdd = cfgInfo.ToString().Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in sCfgAdd)
                    tvConfig.Nodes.Add(s);

                TreeNode tvInterfaces = tvConfig; //.Nodes.Add("Interfaces");
                foreach (UsbInterfaceInfo interfaceInfo in cfgInfo.InterfaceInfoList)
                {
                    TreeNode tvInterface =
                        tvInterfaces.Nodes.Add("Interface [" + interfaceInfo.Descriptor.InterfaceID + "," + interfaceInfo.Descriptor.AlternateID + "]");
                    string[] sInterfaceAdd = interfaceInfo.ToString().Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in sInterfaceAdd)
                        tvInterface.Nodes.Add(s);

                    TreeNode tvEndpoints = tvInterface; //.Nodes.Add("Endpoints");
                    foreach (UsbEndpointInfo endpointInfo in interfaceInfo.EndpointInfoList)
                    {
                        TreeNode tvEndpoint = tvEndpoints.Nodes.Add("Endpoint 0x" + (endpointInfo.Descriptor.EndpointID).ToString("X2"));
                        string[] sEndpointAdd = endpointInfo.ToString().Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in sEndpointAdd)
                            tvEndpoint.Nodes.Add(s);
                    }
                }
            }
            mUsbDevice.Close();
        }

        private void cboDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDevices.SelectedIndex >= 0)
            {
                tvInfo.Nodes.Clear();
                addDevice(mDevList[cboDevices.SelectedIndex], cboDevices.Text);
                copyAsPlainTextToolStripMenuItem.Enabled = true;

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void refreshDeviceList()
        {
            cboDevices.SelectedIndexChanged -= cboDevices_SelectedIndexChanged;
            mDevList = UsbDevice.AllDevices;
            tsNumDevices.Text = mDevList.Count.ToString();
            cboDevices.Sorted = false;
            cboDevices.Items.Clear();
            foreach (UsbRegistry device in mDevList)
            {
                string sAdd = string.Format("Vid:0x{0:X4} Pid:0x{1:X4} (rev:{2}) - {3}",
                                            device.Vid,
                                            device.Pid,
                                            (ushort) device.Rev,
                                            device[SPDRP.DeviceDesc]);

                cboDevices.Items.Add(sAdd);
            }
            cboDevices.SelectedIndexChanged += cboDevices_SelectedIndexChanged;

            if (mDevList.Count == 0)
            {
                tsNumDevices.ForeColor = Color.Red;
                tvInfo.Nodes.Clear();
                tvInfo.Nodes.Add("No USB devices found.");
                tvInfo.Nodes.Add("A device must be installed which uses the LibUsb-Win32 driver.");
                tvInfo.Nodes.Add("Or");
                tvInfo.Nodes.Add("The LibUsb-Win32 kernel service must be enabled.");
            }
            else
            {
                tsNumDevices.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
            }
        }

        private void tsRefresh_Click(object sender, EventArgs e) { refreshDeviceList(); }


        private void tabDevice_Selected(object sender, TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected)
            {
                if (e.TabPage == tabRegistry)
                {
                    if (mUsbRegistry != null)
                    {
                        tRegProps.Clear();
                        UsbSymbolicName symName = UsbSymbolicName.Parse(mUsbRegistry.SymbolicName);

                        if (symName.SerialNumber != string.Empty)
                        {
                            tRegProps.AppendText(string.Format("SerialNumber:{0}\r\n", symName.SerialNumber));
                        }
                        if (symName.ClassGuid != Guid.Empty)
                        {
                            tRegProps.AppendText(string.Format("Class Guid:{0}\r\n", symName.ClassGuid));
                        }
                        foreach (KeyValuePair<string, object> current in mUsbRegistry.DeviceProperties)
                        {
                            string key = current.Key;
                            object oValue = current.Value;

                            if (oValue is string[])
                            {
                                tRegProps.AppendText(key + ":\r\n");
                                key = new string(' ', key.Length);
                                string[] saValue = oValue as string[];
                                foreach (string s in saValue)
                                {
                                    tRegProps.AppendText(String.Format("{0} {1}\r\n", key, s));
                                }
                                continue;
                            }

                            tRegProps.AppendText(String.Format("{0}:{1}\r\n", key, oValue));
                        }
                    }
                }
                else if (e.TabPage == tabVersionInfo)
                {
                    rtfVersionInfo.Clear();
                    rtfVersionInfo.AppendText("Operating System:" + UsbDevice.OSVersion + "\n");
                    rtfVersionInfo.AppendText("LibUsbDotNet Version:" + LibUsbDotNetVersion + "\n");

                    rtfVersionInfo.AppendText("LibUsb Kernel Type:" + UsbDevice.KernelType + "\n");
                    rtfVersionInfo.AppendText("LibUsb Kernel Version:" + UsbDevice.KernelVersion + "\n");
                }
            }
        }

        private void OnUsbError(object sender, UsbError usbError) { Console.WriteLine(usbError.ToString()); }

        private void fTestInfo_Load(object sender, EventArgs e)
        {
            refreshDeviceList();
            UsbDevice.UsbErrorEvent += OnUsbError;
        }

        private void copyAsPlainTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cboDevices.SelectedIndex >= 0)
            {
                StringBuilder sb = getDescriptorReport(mDevList[cboDevices.SelectedIndex]);
                Clipboard.SetText(sb.ToString());

                MessageBox.Show("Descriptor report copied to clipboard.", "Descriptor Report Saved", MessageBoxButtons.OK);
            }
        }

        private void fTestInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            UsbDevice.Exit();
        }
    }
}