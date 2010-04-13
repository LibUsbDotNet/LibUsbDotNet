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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace InfWizard
{
    public partial class RtfSatusControl : UserControl
    {
        private readonly object mRtfTextLock = new object();
        private bool mLoggingEnabled;

        public delegate bool StatusFilterDelegate(StatusArgs statusArgs);

        private StatusFilterDelegate mStatusFilterDelegate;

        public StatusFilterDelegate StatusFilter
        {
            get
            {
                lock(mRtfTextLock)
                    return mStatusFilterDelegate;
            }
            set
            {
                lock (mRtfTextLock)
                    mStatusFilterDelegate = value;
            }
        }
        public RtfSatusControl()
        {
            InitializeComponent();
            
        }

        public bool LoggingEnabled
        {
            get
            {
                return mLoggingEnabled;
            }
            set
            {
                if (mLoggingEnabled != value)
                {
                    if (value)
                    {
                        InfWizardStatus.StatusEvent += (StatusEvent);
                        InfWizardStatus.RawStatusEvent += (RawStatusEvent);
                    }
                    else
                    {
                        InfWizardStatus.StatusEvent -= (StatusEvent);
                        InfWizardStatus.RawStatusEvent -= (RawStatusEvent);
                    }
                    mLoggingEnabled = value;
                }
            }
        }
        private void RawStatusEvent(object sender, RawStatusArgs rawArgs)
        {
            if (IsDisposed)
            {
                LoggingEnabled = false;
                return;
            }

            if (!mLoggingEnabled) return;
            if (InvokeRequired)
            {
                Invoke(new RawStatusEventDelegate(RawStatusEvent), new object[] { sender, rawArgs });
                return;
            }
            lock (mRtfTextLock)
            {
                InfWizardStatus.WriteRtfRawStatus(rtfSatus, rawArgs.RawData);
            }
        }
        public override System.Drawing.Color BackColor
        {
            get
            {
                return rtfSatus.BackColor;
            }
            set
            {
                base.BackColor = value;
                rtfSatus.BackColor = value;
            }
        }
        [DefaultValue(BorderStyle.Fixed3D)]
        public new BorderStyle BorderStyle
        {
            get
            {
                return rtfSatus.BorderStyle;
            }
            set
            {
                base.BorderStyle = BorderStyle.None;
                rtfSatus.BorderStyle = value;
            }
        }
        private void StatusEvent(object sender, StatusArgs e)
        {
            if (IsDisposed)
            {
                LoggingEnabled = false;
                return;
            }

            if (!mLoggingEnabled) return;
            if (InvokeRequired)
            {
                Invoke(new StatusEventDelegate(StatusEvent), new object[] {sender, e});
                return;
            }
            lock (mRtfTextLock)
            {
                if (!ReferenceEquals(mStatusFilterDelegate,null))
                {
                    if (mStatusFilterDelegate(e))
                        InfWizardStatus.WriteRtfStatusArgsLine(rtfSatus, e);
                }
                else
                    InfWizardStatus.WriteRtfStatusArgsLine(rtfSatus, e);
            }
        }

        private void contextMenuSatus_Opening(object sender, CancelEventArgs e)
        {
            rtfSatus.Focus();
            int length = rtfSatus.TextLength;
            int selectionLength = rtfSatus.SelectionLength;

            if (length == selectionLength)
                selectAllToolStripMenuItem.Enabled = false;
            else
                selectAllToolStripMenuItem.Enabled = true;

            if (length == 0)
            {
                clearToolStripMenuItem.Enabled = false;
                saveToFileToolStripMenuItem.Enabled = false;
                copyToolStripMenuItem.Enabled = false;

            }
            else
            {
                clearToolStripMenuItem.Enabled = true;
                saveToFileToolStripMenuItem.Enabled = true;
                copyToolStripMenuItem.Enabled = true;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string rtfText;
            string text;
            lock (mRtfTextLock)
            {
                if (rtfSatus.SelectionLength == 0)
                {
                    rtfText = rtfSatus.Rtf;
                    text = rtfSatus.Text;                    
                }
                else
                {
                    rtfText = rtfSatus.SelectedRtf;
                    text = rtfSatus.SelectedText;                    
                }
            }
            Clipboard.SetText(rtfText, TextDataFormat.Rtf);
            Clipboard.SetText(text);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (mRtfTextLock)
            {
                rtfSatus.SelectionStart = 0;
                rtfSatus.SelectionLength = rtfSatus.TextLength;
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e) { rtfSatus.Clear(); }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileLog.ShowDialog(this) != DialogResult.OK) return;

            string text;
            lock (mRtfTextLock) text = rtfSatus.Text;
            try
            {
                File.WriteAllText(saveFileLog.FileName, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Failed saving log file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear() 
        {
            lock (mRtfTextLock)
            {
                rtfSatus.Clear();
            }
        }
    }
}