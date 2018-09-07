Imports System
Imports MonoLibUsb.Profile
Imports Usb = MonoLibUsb.MonoUsbApi

Namespace MonoLibUsb.ShowInfo
	Friend Class ShowInfo
		' The first time the Session property is used it creates a new session
		' handle instance in '__sessionHandle' and returns it. Subsequent 
		' request simply return '__sessionHandle'.
		Private Shared __sessionHandle As MonoUsbSessionHandle
		Public Shared ReadOnly Property Session() As MonoUsbSessionHandle
			Get
				If ReferenceEquals(__sessionHandle, Nothing) Then
					__sessionHandle = New MonoUsbSessionHandle()
				End If
				Return __sessionHandle
			End Get
		End Property
		Public Shared Sub Main(args As String())
			Dim ret As Integer
			Dim profileList As MonoUsbProfileList = Nothing

			' Initialize the context.
			If Session.IsInvalid Then
				Throw New Exception("Failed to initialize context.")
			End If

			MonoUsbApi.SetDebug(Session, 0)
			' Create a MonoUsbProfileList instance.
			profileList = New MonoUsbProfileList()

			' The list is initially empty.
			' Each time refresh is called the list contents are updated. 
			ret = profileList.Refresh(Session)
			If ret < 0 Then
				Throw New Exception("Failed to retrieve device list.")
			End If
			Console.WriteLine("{0} device(s) found.", ret)

			' Iterate through the profile list; write the device descriptor to
			' console output.
			For Each profile As MonoUsbProfile In profileList
				Console.WriteLine(profile.DeviceDescriptor)
			Next

			' Since profile list, profiles, and sessions use safe handles the
			' code below is not required but it is considered good programming
			' to explicitly free and close these handle when they are no longer
			' in-use.
			profileList.Close()
			Session.Close()
		End Sub
	End Class
End Namespace
