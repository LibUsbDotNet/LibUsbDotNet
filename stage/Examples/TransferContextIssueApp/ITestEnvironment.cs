using LibUsbDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TransferContextIssueApp
{
    /// <summary>
    /// Describes a USB device environment to open, close, attach and detach the device.
    /// </summary>
    internal interface ITestEnvironment
    {
        /// <summary>
        /// Cancellation token to cancel the operation.
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// Represents the USB device.
        /// </summary>
        UsbDevice UsbDevice { get; }

        /// <summary>
        /// Attach the USB device.
        /// </summary>
        /// <returns>True, if USB device could be attached</returns>
        Task<bool> AttachUsbDeviceAsync();

        /// <summary>
        /// Detach the USB device.
        /// </summary>
        /// <returns>True, if USB device could be detached</returns>
        Task<bool> DetachUsbDeviceAsync();

        /// <summary>
        /// Open the USB device.
        /// </summary>
        /// <returns>USB device</returns>
        Task<UsbDevice> OpenUsbDeviceAsync();

        /// <summary>
        /// Close the USB device.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        void CloseUsbDevice(UsbDevice usbDevice);
    }
}
