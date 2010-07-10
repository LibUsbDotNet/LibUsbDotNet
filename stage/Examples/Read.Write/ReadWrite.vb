Imports System
Imports System.Text
Imports System.Text.RegularExpressions
Imports LibUsbDotNet
Imports LibUsbDotNet.Main

Namespace Examples
	Friend Class ReadWrite
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

				' If this is a "whole" usb device (libusb-win32, linux libusb)
				' it will have an IUsbDevice interface. If not (WinUSB) the 
				' variable will be null indicating this is an interface of a 
				' device.
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

				' open write endpoint 1.
				Dim writer As UsbEndpointWriter = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01)

				' Remove the exepath/startup filename text from the begining of the CommandLine.
				Dim cmdLine As String = Regex.Replace(Environment.CommandLine, "^"".+?""^.*? |^.*? ", "", RegexOptions.Singleline)

				If Not [String].IsNullOrEmpty(cmdLine) Then
					Dim bytesWritten As Integer
					ec = writer.Write(Encoding.[Default].GetBytes(cmdLine), 2000, bytesWritten)
					If ec <> ErrorCode.None Then
						Throw New Exception(UsbDevice.LastErrorString)
					End If

					Dim readBuffer As Byte() = New Byte(1023) {}
					While ec = ErrorCode.None
						Dim bytesRead As Integer

						' If the device hasn't sent data in the last 100 milliseconds,
						' a timeout error (ec = IoTimedOut) will occur. 
						ec = reader.Read(readBuffer, 100, bytesRead)

						If bytesRead = 0 Then
							Throw New Exception("No more bytes!")
						End If

						' Write that output to the console.
						Console.Write(Encoding.[Default].GetString(readBuffer, 0, bytesRead))
					End While

					Console.WriteLine(vbCr & vbLf & "Done!" & vbCr & vbLf)
				Else
					Throw New Exception("Nothing to do.")
				End If
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
