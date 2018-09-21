using System;

namespace LibUsbDotNet.LibUsb
{
    public partial class UsbDevice : IDisposable, ICloneable
    {
        private bool disposed;

        public UsbDevice(Device device)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public UsbDevice Clone()
        {
            return new UsbDevice(NativeMethods.RefDevice(this.device));
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Close the libusb_device_handle if required.
            this.Close();

            // Close the libusb_device handle.
            this.device.Dispose();

            this.disposed = true;
        }

        public override string ToString()
        {
            if (this.IsOpen)
            {
                return this.Descriptor.ToString();
            }
            else
            {
                return base.ToString();
            }
        }

        protected void EnsureNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(UsbDevice));
            }
        }
    }
}
