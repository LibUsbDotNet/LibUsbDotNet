using System.Runtime.InteropServices;

namespace LibUsbDotNet.Win32.Hotplug
{
    /// <summary>
    /// This is a device notification event data structure.
    /// </summary>
    /// <remarks>
    /// See also documentation: <see href="https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/ns-cfgmgr32-cm_notify_event_data" />
    /// </remarks>
    internal struct CmNotifyEventData
    {
        /// <summary>
        /// Filter type that was used to register for the notification.
        /// </summary>
        public uint FilterType;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public uint Reserved;

        /// <summary>
        /// A union that contains information about the notification event data. To determine which member of the union to examine, check the FilterType of the event data.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public byte[] Data = new byte[28];

        /// <summary>
        /// To satisfy the requirements of the structure.
        /// </summary>
        public CmNotifyEventData()
        {
        }
    }
}
