Imports System
Imports System.Collections.Generic
Imports LibUsbDotNet.Main
Imports MonoLibUsb
Imports MonoLibUsb.Descriptors
Imports MonoLibUsb.Profile
Imports Usb = MonoLibUsb.MonoUsbApi

Namespace MonoUsb.ShowConfig
	Friend Class ShowConfig
		Private Shared sessionHandle As MonoUsbSessionHandle

		' Predicate functions for finding only devices with the specified VendorID & ProductID.
		Private Shared Function MyVidPidPredicate(profile As MonoUsbProfile) As Boolean
			If profile.DeviceDescriptor.VendorID = &H4d8 AndAlso profile.DeviceDescriptor.ProductID = &H53 Then
				Return True
			End If
			Return False
		End Function

		Public Shared Sub Main(args As String())
			' Initialize the context.
			sessionHandle = New MonoUsbSessionHandle()
			If sessionHandle.IsInvalid Then
				Throw New Exception([String].Format("Failed intialized libusb context." & vbLf & "{0}:{1}", MonoUsbSessionHandle.LastErrorCode, MonoUsbSessionHandle.LastErrorString))
			End If

			Dim profileList As New MonoUsbProfileList()

			' The list is initially empty.
			' Each time refresh is called the list contents are updated. 
			Dim ret As Integer = profileList.Refresh(sessionHandle)
			If ret < 0 Then
				Throw New Exception("Failed to retrieve device list.")
			End If
			Console.WriteLine("{0} device(s) found.", ret)

			' Use the GetList() method to get a generic List of MonoUsbProfiles
			' Find all profiles that match in the MyVidPidPredicate.
			Dim myVidPidList As List(Of MonoUsbProfile) = profileList.GetList().FindAll(AddressOf MyVidPidPredicate)

			' myVidPidList reresents a list of connected USB devices that matched
			' in MyVidPidPredicate.
			For Each profile As MonoUsbProfile In myVidPidList
				' Write the VendorID and ProductID to console output.
				Console.WriteLine("[Device] Vid:{0:X4} Pid:{1:X4}", profile.DeviceDescriptor.VendorID, profile.DeviceDescriptor.ProductID)

				' Loop through all of the devices configurations.
				For i As Byte = 0 To profile.DeviceDescriptor.ConfigurationCount - 1
					' Get a handle to the configuration.
					Dim configHandle As MonoUsbConfigHandle
					If MonoUsbApi.GetConfigDescriptor(profile.ProfileHandle, i, configHandle) < 0 Then
						Continue For
					End If
					If configHandle.IsInvalid Then
						Continue For
					End If

					' Create a MonoUsbConfigDescriptor instance for this config handle.
					Dim configDescriptor As New MonoUsbConfigDescriptor(configHandle)

					' Write the bConfigurationValue to console output.
					Console.WriteLine("  [Config] bConfigurationValue:{0}", configDescriptor.bConfigurationValue)

					' Interate through the InterfaceList
					For Each usbInterface As MonoUsbInterface In configDescriptor.InterfaceList
						' Interate through the AltInterfaceList
						For Each usbAltInterface As MonoUsbAltInterfaceDescriptor In usbInterface.AltInterfaceList
							' Write the bInterfaceNumber and bAlternateSetting to console output.
							Console.WriteLine("    [Interface] bInterfaceNumber:{0} bAlternateSetting:{1}", usbAltInterface.bInterfaceNumber, usbAltInterface.bAlternateSetting)

							' Interate through the EndpointList
							For Each endpoint As MonoUsbEndpointDescriptor In usbAltInterface.EndpointList
								' Write the bEndpointAddress, EndpointType, and wMaxPacketSize to console output.
								Console.WriteLine("      [Endpoint] bEndpointAddress:{0:X2} EndpointType:{1} wMaxPacketSize:{2}", endpoint.bEndpointAddress, CType((endpoint.bmAttributes And &H3), EndpointType), endpoint.wMaxPacketSize)
							Next
						Next
					Next
					' Not neccessary, but good programming practice.
					configHandle.Close()
				Next
			Next
			' Not neccessary, but good programming practice.
			profileList.Close()
			' Not neccessary, but good programming practice.
			sessionHandle.Close()
		End Sub
	End Class
End Namespace
