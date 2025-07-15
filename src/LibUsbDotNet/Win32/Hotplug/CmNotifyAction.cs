namespace LibUsbDotNet.Win32.Hotplug
{
	/// <summary>
	/// This enumeration identifies Plug and Play device event types.
	/// </summary>
	/// <remarks>
	/// See also documentation: <see href="https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/ne-cfgmgr32-cm_notify_action" />
	/// </remarks>
	internal enum CmNotifyAction : uint
	{
		DeviceInterfaceArrival = 0,
		DeviceInterfaceRemoval,

		DeviceQueryRemove,
		DeviceQueryRemoveFailed,
		DeviceRemovePending,
		DeviceRemoveComplete,
		DeviceCustomEvent,

		DeviceInstanceEnumerated,
		DeviceInstanceStarted,
		DeviceInstanceRemoved,

		Max,
	}
}
