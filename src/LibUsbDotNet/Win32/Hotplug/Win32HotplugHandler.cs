using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LibUsbDotNet.Win32.Hotplug
{
	public sealed class Win32HotplugHandler : IDisposable
	{
		/// <summary>
		/// The GUID_DEVINTERFACE_USB_DEVICE device interface class is defined for USB devices that are attached to a USB hub.
		/// </summary>
		/// <remarks>
		/// See also documentation: <see href="https://learn.microsoft.com/en-us/windows-hardware/drivers/install/device-interface-classes" />
		/// </remarks>
		private const string GUID_DEVINTERFACE_USB_DEVICE = "A5DCBF10-6530-11D2-901F-00C04FB951ED";

		/// <summary>
		/// Register to receive notifications for PnP events for all device interface classes.
		/// </summary>
		private const uint CM_NOTIFY_FILTER_TYPE_DEVICEINTERFACE = 0;

		/// <summary>
		/// Calback delegate to handle device notifications.
		/// </summary>
		/// <remarks>
		/// See also documentation: <see href="https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/nf-cfgmgr32-cm_register_notification" />
		/// </remarks>
		private delegate long DeviceNotificationCallback(IntPtr notify, IntPtr context, CmNotifyAction action, IntPtr eventData, uint eventDataSize);

		/// <summary>
		/// Provides methods for registering device change notifications.
		/// </summary>
		private static class NativeMethods
		{
			/// <summary>
			/// The CM_Register_Notification function registers an application callback routine to be called when a PnP event of the specified type occurs.
			/// </summary>
			/// <remarks>
			/// See also documentation: <see href="https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/nf-cfgmgr32-cm_register_notification" />
			/// </remarks>
			[DllImport("cfgmgr32", EntryPoint = "CM_Register_Notification", CallingConvention = CallingConvention.StdCall)]
			public static extern uint CmRegisterNotification(ref CmNotifyFilter filter, IntPtr context, DeviceNotificationCallback callback, out CmNotifyHandle notify);
		}

		/// <summary>
		/// Unique identifier for the USB device interface class.
		/// </summary>
		private readonly Guid _usbDevIface = new(GUID_DEVINTERFACE_USB_DEVICE);

		/// <summary>
		/// Notification handle for device change notifications.
		/// </summary>
		private readonly CmNotifyHandle _notifyHandle;

		/// <summary>
		/// Represents an event that is raised when a device change occurs.
		/// </summary>
		public event EventHandler<Win32HotplugHandlerEventArgs> DeviceChangedEvent;

		/// <summary>
		/// Initializes a new instance of the <see cref="Win32HotplugHandler"/> class.
		/// </summary>
		/// <exception cref="InvalidOperationException"/>
		public Win32HotplugHandler()
		{
			var filter = new CmNotifyFilter
			{
				CbSize = (uint)Marshal.SizeOf<CmNotifyFilter>(),
				FilterType = CM_NOTIFY_FILTER_TYPE_DEVICEINTERFACE
			};
			Array.Copy(_usbDevIface.ToByteArray(), filter.Data, 16);

			var result = NativeMethods.CmRegisterNotification(ref filter, IntPtr.Zero, DeviceNotifyCallback, out var hNotify);
			if (result != 0)
			{
				throw new InvalidOperationException($"Failed to register device notification. Error code: {result}");
			}

			_notifyHandle = hNotify;
		}

		private long DeviceNotifyCallback(IntPtr notify, IntPtr context, CmNotifyAction action, IntPtr eventData, uint eventDataSize)
		{
			var data = Marshal.PtrToStructure<CmNotifyEventData>(eventData);
			if (data.FilterType != CM_NOTIFY_FILTER_TYPE_DEVICEINTERFACE)
			{
				return 0; // We are only interested in device interfaces
			}

			var eventArgs = ConvertToEventArgs(action, eventData);
			DeviceChangedEvent?.Invoke(this, eventArgs ?? new Win32HotplugHandlerEventArgs
			{
				VendorId = 0,
				ProductId = 0,
				DeviceInterfaceClass = Guid.Empty,
			});

			return 0;
		}

		/// <summary>
		/// Convert the given <see cref="CmNotifyAction"/> to a <see cref="Win32HotplugHandlerEventType"/>.
		/// </summary>
		private Win32HotplugHandlerEventType ConvertToEventType(CmNotifyAction action)
		{
			return action switch
			{
				CmNotifyAction.DeviceInterfaceArrival => Win32HotplugHandlerEventType.Arrival,
				CmNotifyAction.DeviceInterfaceRemoval => Win32HotplugHandlerEventType.Removal,
				_ => Win32HotplugHandlerEventType.Undefined,
			};
		}

		/// <summary>
		/// Convert the given event data to a <see cref="Win32HotplugHandlerEventArgs"/>.
		/// </summary>
		private Win32HotplugHandlerEventArgs ConvertToEventArgs(CmNotifyAction action, IntPtr eventData)
		{
			IntPtr symbolicLinkPtr = IntPtr.Add(eventData, 32);
			var device = Marshal.PtrToStringUni(symbolicLinkPtr) ?? string.Empty;

			// Example input: USB#VID_046D&PID_0A9C#...#{GUID}
			var match = Regex.Match(device, @"VID_([0-9A-Fa-f]{4})&PID_([0-9A-Fa-f]{4}).*?#\{([0-9A-Fa-f\-]{36})\}", RegexOptions.IgnoreCase);
			if (!match.Success)
			{
				return null;
			}

			return new Win32HotplugHandlerEventArgs
			{
				VendorId = uint.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber),
				ProductId = uint.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.HexNumber),
				DeviceInterfaceClass = Guid.Parse(match.Groups[3].Value),
				Type = ConvertToEventType(action)
			};
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_notifyHandle.Close();
		}
	}
}
