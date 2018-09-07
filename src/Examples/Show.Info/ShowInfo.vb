Imports System
Imports LibUsbDotNet
Imports LibUsbDotNet.Info
Imports LibUsbDotNet.Main
Imports System.Collections.ObjectModel

Namespace Examples
	Friend Class ShowInfo
		Public Shared MyUsbDevice As UsbDevice

		Public Shared Sub Main(args As String())
			' Dump all devices and descriptor information to console output.
			Dim allDevices As UsbRegDeviceList = UsbDevice.AllDevices
			For Each usbRegistry As UsbRegistry In allDevices
				If usbRegistry.Open(MyUsbDevice) Then
					Console.WriteLine(MyUsbDevice.Info.ToString())
					For iConfig As Integer = 0 To MyUsbDevice.Configs.Count - 1
						Dim configInfo As UsbConfigInfo = MyUsbDevice.Configs(iConfig)
						Console.WriteLine(configInfo.ToString())

						Dim interfaceList As ReadOnlyCollection(Of UsbInterfaceInfo) = configInfo.InterfaceInfoList
						For iInterface As Integer = 0 To interfaceList.Count - 1
							Dim interfaceInfo As UsbInterfaceInfo = interfaceList(iInterface)
							Console.WriteLine(interfaceInfo.ToString())

							Dim endpointList As ReadOnlyCollection(Of UsbEndpointInfo) = interfaceInfo.EndpointInfoList
							For iEndpoint As Integer = 0 To endpointList.Count - 1
								Console.WriteLine(endpointList(iEndpoint).ToString())
							Next
						Next
					Next
				End If
			Next


			' Free usb resources.
			' This is necessary for libusb-1.0 and Linux compatibility.
			UsbDevice.[Exit]()

			' Wait for user input..
			Console.ReadKey()
		End Sub
	End Class
End Namespace
