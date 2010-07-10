#Define IS_BENCHMARK_DEVICE

Imports System
Imports LibUsbDotNet
Imports LibUsbDotNet.Info
Imports LibUsbDotNet.Main

Namespace Examples
	Friend Class ReadIsochronous

		#Region "SET YOUR USB Vendor and Product ID!"

		Public Shared MyUsbFinder As New UsbDeviceFinder(1234, 1)

		#End Region

		''' <summary>Use the first read endpoint</summary>
		Public Shared ReadOnly TRANFER_ENDPOINT As Byte = UsbConstants.ENDPOINT_DIR_MASK

		''' <summary>Number of transfers to sumbit before waiting begins</summary>
		Public Shared ReadOnly TRANFER_MAX_OUTSTANDING_IO As Integer = 3

		''' <summary>Number of transfers before terminating the test</summary>
		Public Shared ReadOnly TRANSFER_COUNT As Integer = 30

		''' <summary>Size of each transfer</summary>
		Public Shared TRANFER_SIZE As Integer = 4096

		Private Shared mStartTime As DateTime = DateTime.MinValue
		Private Shared mTotalBytes As Double = 0
		Private Shared mTransferCount As Integer = 0
		Public Shared MyUsbDevice As UsbDevice

		Public Shared Sub Main(args As String())
			Dim ec As ErrorCode = ErrorCode.None

			Try
				' Find and open the usb device.
				Dim regList As UsbRegDeviceList = UsbDevice.AllDevices.FindAll(MyUsbFinder)
				If regList.Count = 0 Then
					Throw New Exception("Device Not Found.")
				End If

				Dim usbInterfaceInfo As UsbInterfaceInfo = Nothing
				Dim usbEndpointInfo As UsbEndpointInfo = Nothing

				' Look through all conected devices with this vid and pid until
				' one is found that has and and endpoint that matches TRANFER_ENDPOINT.
				'
				For Each regDevice As UsbRegistry In regList
					If regDevice.Open(MyUsbDevice) Then
						If MyUsbDevice.Configs.Count > 0 Then
							' if TRANFER_ENDPOINT is 0x80 or 0x00, LookupEndpointInfo will return the 
							' first read or write (respectively).
							If UsbEndpointBase.LookupEndpointInfo(MyUsbDevice.Configs(0), TRANFER_ENDPOINT, usbInterfaceInfo, usbEndpointInfo) Then
								Exit For
							End If

							MyUsbDevice.Close()
							MyUsbDevice = Nothing
						End If
					End If
				Next

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
					wholeUsbDevice.ClaimInterface(usbInterfaceInfo.Descriptor.InterfaceID)
				End If

				' open read endpoint.
				Dim reader As UsbEndpointReader = MyUsbDevice.OpenEndpointReader(DirectCast(usbEndpointInfo.Descriptor.EndpointID, ReadEndpointID), 0, DirectCast((usbEndpointInfo.Descriptor.Attributes And &H3), EndpointType))

				If ReferenceEquals(reader, Nothing) Then
					Throw New Exception("Failed locating read endpoint.")
				End If

				reader.Reset()

				' The benchmark device firmware works with this example but it must be put into PC read mode.
				#If IS_BENCHMARK_DEVICE Then
				Dim transferred As Integer
				Dim ctrlData As Byte() = New Byte(0) {}
				Dim setTestTypePacket As New UsbSetupPacket(CByte((UsbCtrlFlags.Direction_In Or UsbCtrlFlags.Recipient_Device Or UsbCtrlFlags.RequestType_Vendor)), &He, &H1, usbInterfaceInfo.Descriptor.InterfaceID, 1)
				MyUsbDevice.ControlTransfer(setTestTypePacket, ctrlData, 1, transferred)
				#End If
				TRANFER_SIZE -= (TRANFER_SIZE Mod usbEndpointInfo.Descriptor.MaxPacketSize)

				Dim transferQeue As New UsbTransferQueue(reader, TRANFER_MAX_OUTSTANDING_IO, TRANFER_SIZE, 5000, usbEndpointInfo.Descriptor.MaxPacketSize)

				Do
					Dim handle As UsbTransferQueue.Handle

					' Begin submitting transfers until TRANFER_MAX_OUTSTANDING_IO has benn reached.
					' then wait for the oldest outstanding transfer to complete.
					'
					ec = transferQeue.Transfer(handle)
					If ec <> ErrorCode.Success Then
						Throw New Exception("Failed getting async result")
					End If

					' Show some information on the completed transfer.
					showTransfer(handle, mTransferCount)
				Loop While System.Math.Max(System.Threading.Interlocked.Increment(mTransferCount),mTransferCount - 1) < TRANSFER_COUNT

				' Cancels any oustanding transfers and free's the transfer queue handles.
				' NOTE: A transfer queue can be reused after it's freed.
				transferQeue.Free()

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
				End If

				' Wait for user input..
				Console.ReadKey()

				' Free usb resources
				UsbDevice.[Exit]()
			End Try
		End Sub

		Private Shared Sub showTransfer(handle As UsbTransferQueue.Handle, transferIndex As Integer)
			If mStartTime = DateTime.MinValue Then
				mStartTime = DateTime.Now
				Console.WriteLine("Synchronizing..")
				Return
			End If

			mTotalBytes += handle.Transferred
			Dim bytesSec As Double = mTotalBytes / (DateTime.Now - mStartTime).TotalSeconds

			Console.WriteLine("#{0} complete. {1} bytes/sec ({2} bytes) Data[1]={3:X2}", transferIndex, Math.Round(bytesSec, 2), handle.Transferred, handle.Data(1))
		End Sub
	End Class
End Namespace
