Imports System
Imports System.Windows.Forms
Imports LibUsbDotNet.DeviceNotify

Namespace Device.Notification
	Friend Class DeviceNotification
		Public Shared UsbDeviceNotifier As IDeviceNotifier = DeviceNotifier.OpenDeviceNotifier()

		Private Shared Sub Main(args As String())
			' Hook the device notifier event
			AddHandler UsbDeviceNotifier.OnDeviceNotify, AddressOf OnDeviceNotifyEvent

			' Exit on and key pressed.
			Console.Clear()
			Console.WriteLine()
			Console.WriteLine("Waiting for system level device events..")
			Console.Write("[Press any key to exit]")

			While Not Console.KeyAvailable
				Application.DoEvents()
			End While

			UsbDeviceNotifier.Enabled = False
			' Disable the device notifier
			' Unhook the device notifier event
			RemoveHandler UsbDeviceNotifier.OnDeviceNotify, AddressOf OnDeviceNotifyEvent
		End Sub

		Private Shared Sub OnDeviceNotifyEvent(sender As Object, e As DeviceNotifyEventArgs)
			' A Device system-level event has occured

			Console.SetCursorPosition(0, Console.CursorTop)

			Console.WriteLine(e.ToString())
			' Dump the event info to output.
			Console.WriteLine()
			Console.Write("[Press any key to exit]")
		End Sub
	End Class
End Namespace
