
using System;
using System.Windows.Forms;
using LibUsbDotNet.DeviceNotify;

namespace Test_DeviceNotify
{
    public partial class fTestDeviceNotify : Form
    {
        private IDeviceNotifier devNotifier;

		delegate void AppendNotifyDelegate(string s);
		
        public fTestDeviceNotify()
        {
            InitializeComponent();
            devNotifier = DeviceNotifier.OpenDeviceNotifier();

            devNotifier.OnDeviceNotify += onDevNotify;
        }


        private void onDevNotify(object sender, DeviceNotifyEventArgs e)
        {
			Invoke(new AppendNotifyDelegate(AppendNotifyText),new object[] {e.ToString()});
        }

		private void AppendNotifyText(string s)
		{
           tNotify.AppendText(s);
		}
        private void tNotify_DoubleClick(object sender, EventArgs e)
        {
            tNotify.Clear();
        }
    }
}