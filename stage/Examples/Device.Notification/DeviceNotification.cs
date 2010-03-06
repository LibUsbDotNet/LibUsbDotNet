using System;
using System.Windows.Forms;
using LibUsbDotNet.DeviceNotify;

namespace Device.Notification
{
    internal class DeviceNotification
    {
        public static IDeviceNotifier UsbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

        private static void Main(string[] args)
        {
            // Hook the device notifier event
            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;

            // Exit on and key pressed.
            Console.Clear();            
            Console.WriteLine();
            Console.WriteLine("Waiting for system level device events..");
            Console.Write("[Press any key to exit]");

            while (!Console.KeyAvailable)
                Application.DoEvents();

            UsbDeviceNotifier.Enabled = false;  // Disable the device notifier

            // Unhook the device notifier event
            UsbDeviceNotifier.OnDeviceNotify -= OnDeviceNotifyEvent;
        }

        private static void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            // A Device system-level event has occured

            Console.SetCursorPosition(0,Console.CursorTop);
            
            Console.WriteLine(e.ToString()); // Dump the event info to output.
            
            Console.WriteLine();
            Console.Write("[Press any key to exit]");
        }
    }
}