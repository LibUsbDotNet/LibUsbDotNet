using LibUsbDotNet.LudnMonoLibUsb.Internal;
using LibUsbDotNet.Main;
using System;

namespace LibUsbDotNet.LibUsb
{
    public class UsbEndpointReader : LibUsbDotNet.UsbEndpointReader
    {
        private readonly UsbDevice usbDevice;

        internal UsbEndpointReader(UsbDevice usbDevice, int readBufferSize, byte alternateInterfaceID, ReadEndpointID readEndpointID, EndpointType endpointType)
            : base(usbDevice, readBufferSize, alternateInterfaceID, readEndpointID, endpointType)
        {
            this.usbDevice = usbDevice;
        }

        /// <summary>
        /// Calling this methods is that same as calling <see cref="UsbEndpointReader.ReadFlush"/>
        /// </summary>
        /// <returns>True an success.</returns>
        public override bool Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            return ReadFlush() == Error.Success;
        }

        /// <summary>
        /// Cancels pending transfers and clears the halt condition on an enpoint.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Reset()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            Abort();
            NativeMethods.ClearHalt(usbDevice.DeviceHandle, EpNum).ThrowOnError();
            return true;
        }

        protected override UsbTransfer CreateTransferContext()
        {
            return new MonoUsbTransferContext(this);
        }
    }
}
