Imports System
Imports System.Text
Imports LibUsbDotNet
Imports LibUsbDotNet.Main

Namespace Examples
	Friend Class ReadPolling
		Public Shared MyUsbDevice As UsbDevice

		#Region "SET YOUR USB Vendor and Product ID!"

		Public Shared MyUsbFinder As New UsbDeviceFinder(1234, 1)

		#End Region

		Public Shared Sub Main(args As String())
			Dim ec As ErrorCode = ErrorCode.None

			Try
				' Find and open the usb device.
				MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder)

				' If the device is open and ready
				If MyUsbDevice Is Nothing Then
					Throw New Exception("Device Not Found.")
				End If

				' If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
				' it exposes an IUsbDevice interface. If not (WinUSB) the 
				' 'wholeUsbDevice' variable will be null indicating this is 
				' an interface of a device; it does not require or support 
				' configuration and interface selection.
				Dim wholeUsbDevice As IUsbDevice = TryCast(MyUsbDevice, IUsbDevice)
				If Not ReferenceEquals(wholeUsbDevice, Nothing) Then
					' This is a "whole" USB device. Before it can be used, 
					' the desired configuration and interface must be selected.

					' Select config #1
					wholeUsbDevice.SetConfiguration(1)

					' Claim interface #0.
					wholeUsbDevice.ClaimInterface(0)
				End If

				' open read endpoint 1.
				Dim reader As UsbEndpointReader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01)


				Dim readBuffer As Byte() = New Byte(1023) {}
				While ec = ErrorCode.None
					Dim bytesRead As Integer

					' If the device hasn't sent data in the last 5 seconds,
					' a timeout error (ec = IoTimedOut) will occur. 
					ec = reader.Read(readBuffer, 5000, bytesRead)

					If bytesRead = 0 Then
						Throw New Exception(String.Format("{0}:No more bytes!", ec))
					End If
					Console.WriteLine("{0} bytes read", bytesRead)

					' Write that output to the console.
					Console.Write(Encoding.[Default].GetString(readBuffer, 0, bytesRead))
				End While

				Console.WriteLine(vbCr & vbLf & "Done!" & vbCr & vbLf)
			Catch ex As Exception
				Console.WriteLine()
				Console.WriteLine((If(ec <> ErrorCode.None, ec & ":", [String].Empty)) & ex.Message)
			Finally
				If MyUsbDevice IsNot Nothing Then
					If MyUsbDevice.IsOpen Then
						' If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
						' it exposes an IUsbDevice interface. If not (WinUSB) the 
						' 'wholeUsbDevice' variable will be null indicating this is 
						' an interface of a device; it does not require or support 
						' configuration and interface selection.
						Dim wholeUsbDevice As IUsbDevice = TryCast(MyUsbDevice, IUsbDevice)
						If Not ReferenceEquals(wholeUsbDevice, Nothing) Then
							' Release interface #0.
							wholeUsbDevice.ReleaseInterface(0)
						End If

						MyUsbDevice.Close()
					End If
					MyUsbDevice = Nothing

					' Free usb resources

					UsbDevice.[Exit]()
				End If

				' Wait for user input..
				Console.ReadKey()
			End Try
		End Sub
	End Class
End Namespace
