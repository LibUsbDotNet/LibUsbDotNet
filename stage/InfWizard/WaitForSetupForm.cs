using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinApiNet;

namespace InfWizard
{
    public partial class WaitForSetupForm : Form
    {
        public WaitForSetupForm()
        {
            InitializeComponent();
        }

        private void timerCheckPendingInstalls_Tick(object sender, EventArgs e)
        {
            if (SetupApi.CMP_WaitNoPendingInstallEvents(0)==0)
            {
                timerCheckPendingInstalls.Enabled = false;
                SynchronizationContext.Current.Post(postCloseForm, DialogResult.OK);
            }
        }

        private void postCloseForm(object state)
        {
            if (state is DialogResult)
                DialogResult = (DialogResult)state;

            Close();
        }
    }
}
