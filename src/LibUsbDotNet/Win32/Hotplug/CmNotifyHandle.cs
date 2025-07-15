using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace LibUsbDotNet.Win32.Hotplug
{
	/// <summary>
	/// Represent a CM notification handle for device change notifications for WIN32.
	/// </summary>
	/// <remarks>
	/// Pointer to receive the HCMNOTIFICATION handle that corresponds to the registration call.
	/// See also documentation: <see href="https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/nf-cfgmgr32-cm_register_notification" />
	/// </remarks>
	internal sealed class CmNotifyHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private const string DllCfgMgr32 = "cfgmgr32.dll";
		private const string CmUnregisterNotificationEntryPoint = "CM_Unregister_Notification";
		private const int CrSuccess = 0;

		[DllImport(DllCfgMgr32, EntryPoint = CmUnregisterNotificationEntryPoint, CallingConvention = CallingConvention.StdCall)]
		private static extern uint CmUnregisterNotification(IntPtr notify);

		/// <summary>
		/// Initializes a new instance of the <see cref="CmNotifyHandle"/> class.
		/// </summary>
		public CmNotifyHandle() : base(true)
		{
		}

		/// <inheritdoc/>
		protected override bool ReleaseHandle()
		{
			var result = CmUnregisterNotification(handle);
			return result == CrSuccess;
		}
	}
}
