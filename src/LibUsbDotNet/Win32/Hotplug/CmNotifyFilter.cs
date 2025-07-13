using System.Runtime.InteropServices;

namespace LibUsbDotNet.Win32.Hotplug
{
	/// <summary>
	/// Device notification filter structure
	/// </summary>
	/// <remarks>
	/// See also documentation: <see href="https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/ns-cfgmgr32-cm_notify_filter" />
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct CmNotifyFilter
	{
		/// <summary>
		/// The size of the structure.
		/// </summary>
		public uint CbSize;

		/// <summary>
		/// Is used to specify the type of notification to be received (e.g PnP events for all device interface classes).
		/// </summary>
		public uint Flags;

		/// <summary>
		/// Type of the notification filter.
		/// </summary>
		public uint FilterType;

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		public uint Reserved;

		/// <summary>
		/// A union that contains information about the device to receive notifications for.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 400)]
		public byte[] Data = new byte[400];

		/// <summary>
		/// To satisfy the requirements of the structure.
		/// </summary>
		public CmNotifyFilter()
		{
		}
	}
}
