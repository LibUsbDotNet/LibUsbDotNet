namespace LibUsbDotNet.Win32.Hotplug
{
	/// <summary>
	/// Represents the type of event that occurred in the Win32 hotplug handler.
	/// </summary>
	public enum Win32HotplugHandlerEventType : uint
	{
		/// <summary>
		/// USB device was added to the system.
		/// </summary>
		Arrival = 0,

		/// <summary>
		/// USB device was removed from the system.
		/// </summary>
		Removal,

		/// <summary>
		/// Event could not be processed in the WIN32 hotplug handler.
		/// </summary>
		Undefined,
	}
}
