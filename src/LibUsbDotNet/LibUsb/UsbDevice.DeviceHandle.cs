using LibUsbDotNet.Main;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibUsbDotNet.LibUsb
{
    public partial class UsbDevice
    {
        private readonly byte[] emptyByteArray =
#if NET45
            new byte[]{};
#else
            Array.Empty<byte>();
#endif

        private DeviceHandle deviceHandle;

        public bool IsOpen
        {
            get
            {
                return this.deviceHandle != null;
            }
        }

        public int Configuration
        {
            get
            {
                this.EnsureNotDisposed();

                this.EnsureOpen();

                int config = 0;
                NativeMethods.GetConfiguration(this.deviceHandle, ref config).ThrowOnError();
                return config;
            }
        }

        public unsafe string GetStringDescriptor(byte descriptorIndex, bool failSilently = false)
        {
            if (failSilently && !this.IsOpen)
            {
                return null;
            }

            this.EnsureOpen();

            if (descriptorIndex == 0)
            {
                return null;
            }

            byte[] buffer = new byte[1024];

            fixed (byte* ptr = &buffer[0])
            {
                var length = (int)NativeMethods.GetStringDescriptorAscii(this.deviceHandle, descriptorIndex, ptr, buffer.Length);

                if (length < 0)
                {
                    if (failSilently)
                    {
                        return null;
                    }
                    else
                    {
                        ((Error)length).ThrowOnError();
                    }
                }

                return Encoding.ASCII.GetString(buffer, 0, length);
            }
        }

        /// <summary>
        /// Transmits control data over a default control endpoint.
        /// </summary>
        /// <param name="setupPacket">An 8-byte setup packet which contains parameters for the control request. 
        /// See section 9.3 USB Device Requests of the Universal Serial Bus Specification Revision 2.0 for more information. </param>
        /// <param name="buffer">Data to be sent/received from the device.</param>
        /// <param name="bufferLength">Length of the buffer param.</param>
        /// <returns>The number of bytes sent or received (depends on the direction of the control transfer).</returns>
        public unsafe int ControlTransfer(UsbSetupPacket setupPacket)
        {
            return ControlTransfer(setupPacket, emptyByteArray, 0, 0);
        }

        /// <summary>
        /// Transmits control data over a default control endpoint.
        /// </summary>
        /// <param name="setupPacket">An 8-byte setup packet which contains parameters for the control request. 
        /// See section 9.3 USB Device Requests of the Universal Serial Bus Specification Revision 2.0 for more information. </param>
        /// <param name="buffer">Data to be sent/received from the device.</param>
        /// <param name="bufferLength">Length of the buffer param.</param>
        /// <returns>The number of bytes sent or received (depends on the direction of the control transfer).</returns>
        public unsafe int ControlTransfer(UsbSetupPacket setupPacket, byte[] buffer, int offset, int length)
        {
            int result = 0;

            if (length > 0)
            {
                fixed (byte* data = &buffer[0])
                {
                    result = NativeMethods.ControlTransfer(
                        this.deviceHandle,
                        setupPacket.RequestType,
                        setupPacket.Request,
                        (ushort)setupPacket.Value,
                        (ushort)setupPacket.Index,
                        data,
                        (ushort)length,
                        UsbConstants.DEFAULT_TIMEOUT);
                }
            }
            else
            {
                result = NativeMethods.ControlTransfer(
                    this.deviceHandle,
                    setupPacket.RequestType,
                    setupPacket.Request,
                    (ushort)setupPacket.Value,
                    (ushort)setupPacket.Index,
                    null,
                    0,
                    UsbConstants.DEFAULT_TIMEOUT);
            }

            if (result >= 0)
            {
                return result;
            }
            else
            {
                throw new MonoUsbException((Error)result);
            }
        }

        public void Open()
        {
            if (this.IsOpen)
            {
                return;
            }

            IntPtr deviceHandle = IntPtr.Zero;
            NativeMethods.Open(this.device, ref deviceHandle).ThrowOnError();

            this.deviceHandle = DeviceHandle.DangerousCreate(deviceHandle);
        }

        public bool TryOpen()
        {
            if (this.IsOpen)
            {
                return true;
            }

            IntPtr deviceHandle = IntPtr.Zero;
            if (NativeMethods.Open(this.device, ref deviceHandle) == Error.Success)
            {
                this.deviceHandle = DeviceHandle.DangerousCreate(deviceHandle);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Close()
        {
            if (!this.IsOpen)
            {
                return;
            }

            this.deviceHandle.Dispose();
            this.deviceHandle = null;
        }

        protected void EnsureOpen()
        {
            if (!this.IsOpen)
            {
                throw new MonoUsbException("The device has not been opened. You need to call Open() first.");
            }
        }
    }
}
