Imports System
Imports System.Text
Imports System.Text.RegularExpressions
Imports LibUsbDotNet
Imports LibUsbDotNet.Main

Namespace Examples
	Friend Class ReadWriteEventDriven
		Public Shared LastDataEventDate As DateTime = DateTime.Now
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

				' open write endpoint 1.
				Dim writer As UsbEndpointWriter = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01)

				' Remove the exepath/startup filename text from the begining of the CommandLine.
				Dim cmdLine As String = Regex.Replace(Environment.CommandLine, "^"".+?""^.*? |^.*? ", "", RegexOptions.Singleline)

				If Not [String].IsNullOrEmpty(cmdLine) Then
					reader.DataReceived += (AddressOf OnRxEndPointData)
					reader.DataReceivedEnabled = True

					Dim bytesWritten As Integer
					ec = writer.Write(Encoding.[Default].GetBytes(cmdLine), 2000, bytesWritten)
					If ec <> ErrorCode.None Then
						Throw New Exception(UsbDevice.LastErrorString)
					End If

					LastDataEventDate = DateTime.Now
					While (DateTime.Now - LastDataEventDate).TotalMilliseconds < 100
					End While

					' Always disable and unhook event when done.
					reader.DataReceivedEnabled = False
					reader.DataReceived -= (AddressOf OnRxEndPointData)

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
				End If
				MyUsbDevice = Nothing

				' Free usb resources
				UsbDevice.[Exit]()

				' Wait for user input..
				Console.ReadKey()
			End Try
		End Sub

		Private Shared Sub OnRxEndPointData(sender As Object, e As EndpointDataEventArgs)
			LastDataEventDate = DateTime.Now
			Console.Write(Encoding.[Default].GetString(e.Buffer, 0, e.Count))
		End Sub
	End Class
End Namespace
