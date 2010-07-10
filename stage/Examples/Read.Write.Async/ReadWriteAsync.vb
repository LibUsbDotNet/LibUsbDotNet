Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports LibUsbDotNet
Imports LibUsbDotNet.Internal
Imports LibUsbDotNet.Main
Imports LibUsbDotNet.LudnMonoLibUsb

Namespace Examples
	Friend Class ReadWriteAsync
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

				' the write test data.
				Dim testWriteString As String = "ABCDEFGH"

				Dim ecWrite As ErrorCode
				Dim ecRead As ErrorCode
				Dim transferredOut As Integer
				Dim transferredIn As Integer
				Dim usbWriteTransfer As UsbTransfer
				Dim usbReadTransfer As UsbTransfer
				Dim bytesToSend As Byte() = Encoding.[Default].GetBytes(testWriteString)
				Dim readBuffer As Byte() = New Byte(1023) {}
				Dim testCount As Integer = 0
				Do
					' Create and submit transfer
					ecRead = reader.SubmitAsyncTransfer(readBuffer, 0, readBuffer.Length, 100, usbReadTransfer)
					If ecRead <> ErrorCode.None Then
						Throw New Exception("Submit Async Read Failed.")
					End If

					ecWrite = writer.SubmitAsyncTransfer(bytesToSend, 0, bytesToSend.Length, 100, usbWriteTransfer)
					If ecWrite <> ErrorCode.None Then
						usbReadTransfer.Dispose()
						Throw New Exception("Submit Async Write Failed.")
					End If

					WaitHandle.WaitAll(New WaitHandle() {usbWriteTransfer.AsyncWaitHandle, usbReadTransfer.AsyncWaitHandle}, 200, False)
					If Not usbWriteTransfer.IsCompleted Then
						usbWriteTransfer.Cancel()
					End If
					If Not usbReadTransfer.IsCompleted Then
						usbReadTransfer.Cancel()
					End If

					ecWrite = usbWriteTransfer.Wait(transferredOut)
					ecRead = usbReadTransfer.Wait(transferredIn)

					usbWriteTransfer.Dispose()
					usbReadTransfer.Dispose()

					Console.WriteLine("Read  :{0} ErrorCode:{1}", transferredIn, ecRead)
					Console.WriteLine("Write :{0} ErrorCode:{1}", transferredOut, ecWrite)
					Console.WriteLine("Data  :" & Encoding.[Default].GetString(readBuffer, 0, transferredIn))
					testCount += 1
				Loop While testCount < 5
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
