using System;

namespace LibUsbDotNet.Win32.Hotplug
{
    /// <summary>
    /// Represents an event triggered by a Win32 hotplug device, containing information about the device's vendor and product identifiers.
    /// </summary>
    public sealed class Win32HotplugEventArgs : EventArgs
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
        /// Represents the device interface class GUID.
        /// </summary>
        public Guid DeviceInterfaceClass { get; set; }

        /// <summary>
        /// Represents the type of event that occurred in the Win32 hotplug handler.
        /// </summary>
        public Win32HotplugEventType Type { get; set; } = Win32HotplugEventType.Undefined;
    }
}
