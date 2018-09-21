using LibUsbDotNet.Info;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibUsbDotNet.LibUsb
{
    public partial class UsbDevice
    {
        private readonly Device device;

        private UsbDeviceInfo descriptor;

        public unsafe UsbDeviceInfo Descriptor
        {
            get
            {
                this.EnsureNotDisposed();

                if (this.descriptor == null)
                {
                    DeviceDescriptor descriptor;
                    NativeMethods.GetDeviceDescriptor(this.device, &descriptor).ThrowOnError();
                    this.descriptor = UsbDeviceInfo.FromUsbDeviceDescriptor(this, descriptor);
                }

                return this.descriptor;
            }
        }

        public ReadOnlyCollection<UsbConfigInfo> Configs
        {
            get
            {
                return this.Descriptor.Configurations;
            }
        }

        public unsafe UsbConfigInfo ActiveConfigDescriptor
        {
            get
            {
                this.EnsureNotDisposed();

                ConfigDescriptor* list = null;
                UsbConfigInfo value = null;

                try
                {
                    NativeMethods.GetActiveConfigDescriptor(this.device, &list).ThrowOnError();
                    value = UsbConfigInfo.FromUsbConfigDescriptor(this, list[0]);
                    return value;
                }
                finally
                {
                    if (list != null)
                    {
                        NativeMethods.FreeConfigDescriptor(list);
                    }
                }
            }
        }

        public byte BusNumber
        {
            get
            {
                return NativeMethods.GetBusNumber(this.device);
            }
        }

        public byte PortNumber
        {
            get
            {
                return NativeMethods.GetPortNumber(this.device);
            }
        }

        public unsafe List<byte> PortNumbers
        {
            get
            {
                byte[] portNumbers = new byte[8];

                fixed (byte* ptr = portNumbers)
                {
                    NativeMethods.GetPortNumbers(this.device, ptr, portNumbers.Length).ThrowOnError();
                }

                return new List<byte>(portNumbers);
            }
        }

        public UsbDevice GetParent()
        {
            var parent = NativeMethods.GetParent(this.device);
            return new UsbDevice(parent);
        }

        public byte Address
        {
            get
            {
                return NativeMethods.GetDeviceAddress(this.device);
            }
        }

        public int Speed
        {
            get
            {
                return NativeMethods.GetDeviceSpeed(this.device);
            }
        }

        public int GetMaxPacketSize(byte endPoint)
        {
            return NativeMethods.GetMaxPacketSize(this.device, endPoint);
        }

        public int GetMaxIsoPacketSize(byte endPoint)
        {
            return NativeMethods.GetMaxIsoPacketSize(this.device, endPoint);
        }

        public unsafe UsbConfigInfo GetConfigDescriptor(byte configIndex)
        {
            this.EnsureNotDisposed();

            ConfigDescriptor* list = null;
            UsbConfigInfo value = null;

            try
            {
                NativeMethods.GetConfigDescriptor(this.device, configIndex, &list).ThrowOnError();
                value = UsbConfigInfo.FromUsbConfigDescriptor(this, list[0]);
                return value;
            }
            finally
            {
                if (list != null)
                {
                    NativeMethods.FreeConfigDescriptor(list);
                }
            }
        }
    }
}
