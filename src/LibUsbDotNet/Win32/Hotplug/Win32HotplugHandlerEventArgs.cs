using System;

namespace LibUsbDotNet.Win32.Hotplug
{
	/// <summary>
	/// Represents an event triggered by a Win32 hotplug device, containing information about the device's vendor and product identifiers.
	/// </summary>
	public sealed class Win32HotplugHandlerEventArgs : EventArgs
	{
		/// <summary>
		/// Represents the vendor ID.
		/// </summary>
		public uint VendorId { get; set; }

		/// <summary>
		/// Represents the product ID.
		/// </summary>
		public uint ProductId { get; set; }

		/// <summary>
		/// Represnts the device interface class GUID.
		/// </summary>
		public Guid DeviceInterfaceClass { get; set; }

		/// <summary>
		/// Represents the type of event that occurred in the Win32 hotplug handler.
		/// </summary>
		public Win32HotplugHandlerEventType Type { get; set; } = Win32HotplugHandlerEventType.Undefined;
	}
}
