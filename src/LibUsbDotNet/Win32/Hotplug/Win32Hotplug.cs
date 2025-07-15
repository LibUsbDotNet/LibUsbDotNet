using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LibUsbDotNet.Win32.Hotplug
{
    /// <summary>
    /// Represents a Win32 implementation for hotplug events, allowing applications to receive notifications when USB devices are added or removed.
    /// </summary>
    /// <remarks>
    /// Is only needed for Windows operating systems als long as no Hotplug support is provided by libusb.
    /// </remarks>
    public sealed class Win32Hotplug : IWin32Hotplug
    {
        /// <summary>
        /// The GUID_DEVINTERFACE_USB_DEVICE device interface class is defined for USB devices that are attached to a USB hub.
        /// </summary>
        /// <remarks>
        /// See also documentation: <see href="https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-device?source=recommendations" />
        /// </remarks>
        private const string GuidDeviceInterfaceUsbDevice = "A5DCBF10-6530-11D2-901F-00C04FB951ED";

        /// <summary>
        /// Register to receive notifications for PnP events for all device interface classes.
        /// </summary>
        private const uint CmNotifyFilterTypeDeviceInterface = 0;

        /// <summary>
        /// Success code for the CM_Register_Notification callback function.
        /// </summary>
        private const uint CrSuccess = 0x00000000;

        /// <summary>
        /// Error code for the CM_Register_Notification callback function when it fails.
        /// </summary>
        private const uint CrFailure = 0x00000013;

        /// <summary>
        /// Offset in byte(s) to read VID, PID and GUID from the event data structure.
        /// </summary>
        private const int UsbDeviceInterfaceOffset = 32;

        /// <summary>
        /// Callback to handle device notifications.
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
        private readonly Guid _usbDeviceInterface = new(GuidDeviceInterfaceUsbDevice);

        /// <summary>
        /// Notification handle for device change notifications.
        /// </summary>
        private readonly CmNotifyHandle _notifyHandle;

        /// <summary>
        /// Represents an event that is raised when a device change occurs.
        /// </summary>
        public event EventHandler<Win32HotplugEventArgs> DeviceChangedEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Win32Hotplug"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public Win32Hotplug()
        {
            var filter = new CmNotifyFilter
            {
                CbSize = (uint)Marshal.SizeOf<CmNotifyFilter>(),
                FilterType = CmNotifyFilterTypeDeviceInterface
            };
            Array.Copy(_usbDeviceInterface.ToByteArray(), filter.Data, 16);

            var result = NativeMethods.CmRegisterNotification(ref filter, IntPtr.Zero, DeviceNotifyCallback, out var hNotify);
            if (result != 0)
            {
                throw new InvalidOperationException($"Failed to register device notification. Error code: {result}");
            }

            _notifyHandle = hNotify;
        }

        /// <summary>
        /// Describes the callback function that is called when a device change notification occurs.
        /// </summary>
        private long DeviceNotifyCallback(IntPtr notify, IntPtr context, CmNotifyAction action, IntPtr eventData, uint eventDataSize)
        {
            try
            {
                var data = Marshal.PtrToStructure<CmNotifyEventData>(eventData);
                if (data.FilterType != CmNotifyFilterTypeDeviceInterface)
                {
                    return CrSuccess; // We are only interested in USB device interfaces.
                }

                var eventArgs = ConvertToEventArgs(action, eventData);
                DeviceChangedEvent?.Invoke(this, eventArgs ?? new Win32HotplugEventArgs
                {
                    VendorId = 0,
                    ProductId = 0,
                    DeviceInterfaceClass = Guid.Empty,
                });
            }
            catch (Exception)
            {
                return CrFailure;
            }
            return CrSuccess;
        }

        /// <summary>
        /// Convert the given <see cref="CmNotifyAction"/> to a <see cref="Win32HotplugEventType"/>.
        /// </summary>
        private static Win32HotplugEventType ConvertToEventType(CmNotifyAction action)
        {
            return action switch
            {
                CmNotifyAction.DeviceInterfaceArrival => Win32HotplugEventType.Arrival,
                CmNotifyAction.DeviceInterfaceRemoval => Win32HotplugEventType.Removal,
                _ => Win32HotplugEventType.Undefined,
            };
        }

        /// <summary>
        /// Convert the given event data to a <see cref="Win32HotplugEventArgs"/>.
        /// </summary>
        private static Win32HotplugEventArgs ConvertToEventArgs(CmNotifyAction action, IntPtr eventData)
        {
            var symbolicLinkPtr = IntPtr.Add(eventData, UsbDeviceInterfaceOffset);
            var device = Marshal.PtrToStringUni(symbolicLinkPtr) ?? string.Empty;

            // Example input: USB#VID_046D&PID_0A9C#...#{GUID}
            var match = Regex.Match(device, @"VID_([0-9A-Fa-f]{4})&PID_([0-9A-Fa-f]{4}).*?#\{([0-9A-Fa-f\-]{36})\}", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            return new Win32HotplugEventArgs
            {
                VendorId = Convert.ToUInt32(match.Groups[1].Value, 16),
                ProductId = Convert.ToUInt32(match.Groups[2].Value, 16),
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
