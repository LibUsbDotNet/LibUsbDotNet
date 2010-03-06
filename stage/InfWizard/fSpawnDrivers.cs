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
using System.Drawing;
using System.Windows.Forms;
using InfWizard.InfWriters;

namespace InfWizard
{
    public partial class fSpawnDrivers : Form
    {
        private readonly InfBaseWriter infWriter;
        private List<DriverFile> mDriverFileList=new List<DriverFile>();
        public fSpawnDrivers()
        {
            InitializeComponent();
        }

        public fSpawnDrivers(InfBaseWriter infWriter)
        {
            this.infWriter = infWriter;
            InitializeComponent();
        }

        private void cmdDriverSourcePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = txtDriverSourcePath.Text;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtDriverSourcePath.Text = dlg.SelectedPath;
            }
        }

        private void ValidateSpawnDriverPath(string driverPath)
        {
            AbortFindDriverWorker();
            if (findDriverWorker.IsBusy)
            {
                SetStatus(eStatus.SearchCancelling);
                return;
            }
            SetStatus(eStatus.Searching);

            infWriter.SpawnDriverPath = driverPath;
            if (infWriter.SpawnDriverPathIsValid)
            {
                findDriverWorker.RunWorkerAsync(null);
            }
            else
            {
                SetStatus(eStatus.SpawnSrcPathInvalid);
            }
        }

        private bool AbortFindDriverWorker() 
        {
            if (findDriverWorker.IsBusy && !findDriverWorker.CancellationPending)
            {
                findDriverWorker.CancelAsync();
            }
            else if (findDriverWorker.CancellationPending)
            {
                return false;
            }
            return true;
        }

        private void SetStatus(eStatus statusEnum)
        {
            string statusText = string.Empty;
            Color textColor = Color.FromKnownColor(KnownColor.ControlText);

            switch (statusEnum)
            {
                case eStatus.SpawnSrcPathInvalid:
                    statusText = "Invalid driver source path.";
                    textColor = Color.Red;
                    cmdContinue.Enabled = false;
                    tsProgress.Enabled = false;
                    tsProgress.Visible = false;
                    break;
                case eStatus.SpawnPathEmpty:
                    statusText = "Drivers not found in source path.";
                    textColor = Color.Red;
                    cmdContinue.Enabled = false;
                    tsProgress.Enabled = false;
                    tsProgress.Visible = false;
                    break;
                case eStatus.SpawnPathOk:
                    statusText = "Drivers found.  Done!";
                    cmdContinue.Enabled = true;
                    tsProgress.Enabled = false;
                    tsProgress.Visible = false;
                    break;
                case eStatus.Searching:
                    statusText = "Searching..";
                    cmdContinue.Enabled = false;
                    tsProgress.Visible = true;
                    tsProgress.Enabled = true;
                    break;
                case eStatus.SearchCancelling:
                    statusText = "Cancelling Search..";
                    textColor = Color.Red;
                    cmdContinue.Enabled = false;
                    tsProgress.Visible = true;
                    tsProgress.Enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("statusEnum");
            }

            tslStatus.Text = statusText;
            tslStatus.ForeColor = textColor;
        }

        private void fSpawnDrivers_Load(object sender, EventArgs e)
        {
            SetDisplayText();
        }

        private void SetDisplayText()
        {
            labelSPAWNDRIVER_INFO_TEXT.Text = infWriter.Tagger.TagString(labelSPAWNDRIVER_INFO_TEXT.Text);
            txtDriverSourcePath.Text = infWriter.SpawnDriverPath;
        }

        private void cmdContinue_Click(object sender, EventArgs e)
        {
            infWriter.SpawnDriverPath = txtDriverSourcePath.Text;
            infWriter.SaveDefaults();
        }

        private void txtDriverSourcePath_TextChanged(object sender, EventArgs e)
        {
            ValidateSpawnDriverPath(txtDriverSourcePath.Text);
        }

        #region Nested Types

        private enum eStatus
        {
            Searching,
            SpawnSrcPathInvalid,
            SpawnPathEmpty,
            SpawnPathOk,
            SearchCancelling
        }

        #endregion

        private void findDriverWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            infWriter.BackGroundWorker = findDriverWorker;
            mDriverFileList = infWriter.DriverFileList;
        }

        private void findDriverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            infWriter.BackGroundWorker = null;
            if (!e.Cancelled)
            {
                if (mDriverFileList.Count > 0)
                {
                    SetStatus(eStatus.SpawnPathOk);
                    txtDriverSourcePath.TextChanged -= txtDriverSourcePath_TextChanged;
                    txtDriverSourcePath.Text = infWriter.SpawnDriverPath;
                    txtDriverSourcePath.TextChanged += txtDriverSourcePath_TextChanged;
                }
                else
                {
                    SetStatus(eStatus.SpawnPathEmpty);
                }
            }
        }

        private void fSpawnDrivers_FormClosing(object sender, FormClosingEventArgs e)
        {
            AbortFindDriverWorker();
        }
    }
}