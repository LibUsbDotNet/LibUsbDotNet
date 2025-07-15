using System;

namespace LibUsbDotNet.Win32.Hotplug
{
    /// <summary>
    /// Describes an interface for Win32 hotplug events, allowing applications to receive notifications when USB devices are added or removed.
    /// </summary>
    /// <remarks>
    /// Is only needed for Windows operating systems (WIN32) als long as no Hotplug support is provided by libusb!
    /// </remarks>
    public interface IWin32Hotplug : IDisposable
    {
        /// <summary>
        /// Represents an event that is raised when a device change occurs.
        /// </summary>
        public event EventHandler<Win32HotplugEventArgs> DeviceChangedEvent;
    }
}
