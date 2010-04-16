Imports System
Imports LibUsbDotNet
Imports LibUsbDotNet.Info
Imports LibUsbDotNet.Main

Namespace Examples
	Friend Class ShowInfo
		Public Shared MyUsbDevice As UsbDevice

		#Region "SET YOUR USB Vendor and Product ID!"

		Public Shared MyUsbFinder As New UsbDeviceFinder(&H4d8, &H53)

		#End Region

		Public Shared Sub Main(args As String())
			' Check for a valid & connected usb device by vendor and product id.
			Console.WriteLine("Finding your device..")
			Dim myUsbRegistry As UsbRegistry = UsbDevice.AllDevices.Find(MyUsbFinder)

			If ReferenceEquals(myUsbRegistry, Nothing) Then
				' The device is not connected or cannot be accessed by libusbdotnet
				Console.WriteLine("Device not connected!")
				ShowLastUsbError()
				Return
			End If

			' Display the usb device description from the registry.
			' This is the description that was set by the install inf. 
			Console.WriteLine("Found device {0}", myUsbRegistry(SPDRP.DeviceDesc))

			' Display the usb devices DeviceInterfaceGuids.
			' This set by the install inf.

			Dim deviceInterfaceGuids As Guid() = myUsbRegistry.DeviceInterfaceGuids
			For Each deviceInterfaceGuid As Guid In deviceInterfaceGuids
				Console.WriteLine("Device Interface Guid: {0}", deviceInterfaceGuid)
			Next

			' Open this usb device.
			If Not myUsbRegistry.Open(MyUsbDevice) Then
				' If a UsbRegistry class is obtained this should never happen with libusb-win32.
				' It will happen with WinUsb only of the device is being used by another process.
				Console.WriteLine("Failed opening device!")
				ShowLastUsbError()
				Return
			End If

			' Get the REAL usb DeviceDescriptor information from the usb device.
			' Up until this point, we have just been querying the windows registry,
			' nothing had actually been sent or received from the usb device.
			Dim myDeviceInfo As UsbDeviceInfo = MyUsbDevice.Info

			' Dump the UsbDeviceDesciptor to console output.
			Console.WriteLine(myDeviceInfo.Descriptor.ToString())

			' Display REAL Manufacturer String (if one exists)
			If myDeviceInfo.Descriptor.ManufacturerStringIndex <> 0 Then
				Console.WriteLine("Manufacturer: {0}", myDeviceInfo.ManufacturerString)
			End If

			' Display REAL Product String (if one exists)
			If myDeviceInfo.Descriptor.ProductStringIndex <> 0 Then
				Console.WriteLine("Product: {0}", myDeviceInfo.ProductString)
			End If

			' Display REAL Serial Number String (if one exists)
			If myDeviceInfo.Descriptor.SerialStringIndex <> 0 Then
				Console.WriteLine("Serial Number: {0}", myDeviceInfo.SerialString)
			End If

			' Close the device.
			' When a UsbDevice class is closed, it is disposed and all resources 
			' are freed. It cannot be re-open; you must create a new instance.
			MyUsbDevice.Close()

			' Free usb resources
			UsbDevice.[Exit]()

			' Wait for user input..
			Console.ReadKey()
		End Sub

		Private Shared Sub ShowLastUsbError()
			Console.WriteLine("Error Number: {0}", UsbDevice.LastErrorNumber)
			Console.WriteLine(UsbDevice.LastErrorString)

			' Wait for user input..
			Console.ReadKey()
		End Sub
	End Class
End Namespace
