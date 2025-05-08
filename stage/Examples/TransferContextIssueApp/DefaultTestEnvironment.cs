using LibUsbDotNet;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace TransferContextIssueApp
{
    /// <summary>
    /// Represents a default test environment for USB device communication.
    /// </summary>
    internal sealed class DefaultTestEnvironment : ITestEnvironment
    {
        /// <inheritdoc/>
        public CancellationToken Token { get; }

        /// <inheritdoc/>
        public UsbDevice UsbDevice { get; private set; }

        public DefaultTestEnvironment(LogLevel logLevel, CancellationToken token)
        {
        }

        /// <inheritdoc/>
        public Task<bool> AttachUsbDeviceAsync()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> DetachUsbDeviceAsync()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<UsbDevice> OpenUsbDeviceAsync()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void CloseUsbDevice(UsbDevice usbDevice)
        {
            throw new System.NotImplementedException();
        }
    }
}
