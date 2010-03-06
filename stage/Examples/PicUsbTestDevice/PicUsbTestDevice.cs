using System.Diagnostics;
using LibUsbDotNet;
using LibUsbDotNet.Main;

// ReSharper disable InconsistentNaming

namespace TestDevice
{
    public enum UsbTestType : byte
    {
        None,
        ReadFromDevice,
        WriteToDevice,
        Loop,
        Invalid = 0xff
    }

    public class UsbTestDeviceException : UsbException
    {
        public UsbTestDeviceException(string description)
            : base(typeof (PIC18TestDevice), description) { }
    }

    public static class PIC18TestDevice
    {
        public const ushort MCP_VID = 0x04D8;
        public const ushort MCP_WINUSB_PID = 0x0053;
        public const ushort MCP_LIBUSB_PID = 0x0204;

        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.GET_TEST"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdGetTestType =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType) PICFW_COMMANDS.GET_TEST,
                               0,
                               0,
                               1);

        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.GET_TEST"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdReadEEDATA =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType) PICFW_COMMANDS.GET_EEDATA,
                               0,
                               0,
                               1);

        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.SET_TEST"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdSetTestType =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType) PICFW_COMMANDS.SET_TEST,
                               0,
                               0,
                               1);


        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.SET_EEDATA"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdWriteEEDATA =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType) PICFW_COMMANDS.SET_EEDATA,
                               0,
                               0,
                               1);

        private static UsbDevice usbTestDevice;

        public static UsbDevice UsbTestDevice
        {
            get { return usbTestDevice; }
            set { usbTestDevice = value; }
        }

        public static bool GetTestType(UsbDevice usbDevice, out UsbTestType usbTestType)
        {
            UsbTestDevice = usbDevice;
            return GetTestType(out usbTestType);
        }

        public static bool GetTestType(out UsbTestType usbTestType)
        {
            if (ReferenceEquals(usbTestDevice, null))
                throw new UsbTestDeviceException("UsbTestDevice must be set before invoking this member!");

            int lengthTransferred;
            byte[] buf = new byte[1];

            UsbSetupPacket cmd = UsbCmdGetTestType;
            bool bSuccess = usbTestDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);

            if (bSuccess && lengthTransferred == 1)
            {
                usbTestType = (UsbTestType) buf[0];
                return true;
            }
            usbTestType = UsbTestType.Invalid;
            return false;
        }

        public static bool SetTestType(UsbDevice usbDevice, UsbTestType usbTestType)
        {
            UsbTestDevice = usbDevice;
            return SetTestType(usbTestType);
        }

        public static bool SetTestType(UsbTestType usbTestType) { return SetTestType(usbTestType, true); }

        public static bool SetTestType(UsbTestType usbTestType, bool bCheck)
        {
            if (ReferenceEquals(usbTestDevice, null))
                throw new UsbTestDeviceException("UsbTestDevice must be set before invoking this member!");

            if (bCheck)
            {
                UsbTestType bCurrentTestType;
                if (GetTestType(out bCurrentTestType))
                {
                    if (bCurrentTestType == usbTestType) return true;
                }
                else
                    return false;
            }
            int lengthTransferred;
            byte[] buf = new byte[1];

            UsbSetupPacket cmd = UsbCmdSetTestType;
            cmd.Value = (short) usbTestType;

            bool bSuccess = usbTestDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);

            return bSuccess && lengthTransferred == 1;
        }

        public static bool ReadEEDATA(byte address, out byte value) { return ReadEEDATA(UsbTestDevice, address, out value); }

        public static bool ReadEEDATA(UsbDevice usbDevice, byte address, out byte value)
        {
            if (ReferenceEquals(usbTestDevice, null))
                throw new UsbTestDeviceException("UsbTestDevice must be set before invoking this member!");

            int lengthTransferred;
            byte[] buf = new byte[1];

            UsbSetupPacket cmd = UsbCmdReadEEDATA;
            cmd.Value = address;
            bool bSuccess = usbTestDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);

            if (bSuccess && lengthTransferred == 1)
            {
                value = buf[0];
                return true;
            }

            value = 0;
            return false;
        }

        public static bool WriteEEDATA(byte address, byte value) { return WriteEEDATA(UsbTestDevice, address, value); }

        public static bool WriteEEDATA(UsbDevice usbDevice, byte address, byte value)
        {
            if (ReferenceEquals(usbTestDevice, null))
                throw new UsbTestDeviceException("UsbTestDevice must be set before invoking this member!");

            int lengthTransferred;
            byte[] buf = new byte[1];

            UsbSetupPacket cmd = UsbCmdReadEEDATA;
            cmd.Value = address;
            cmd.Index = value;
            bool bSuccess = usbTestDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);

            return bSuccess && lengthTransferred == 1;
        }

        public static bool SetTestType(UsbDevice usbDevice, UsbTestType usbTestType, bool check)
        {
            UsbTestDevice = usbDevice;
            return SetTestType(usbTestType, check);
        }

        #region Nested Types

        /// <summary>
        /// Custom vendor request implemented in the test firmware. 
        /// </summary>
        internal enum PICFW_COMMANDS
        {
            SET_TEST = 0x0E,
            GET_TEST = 0x0F,
            SET_EEDATA = 0x10,
            GET_EEDATA = 0x11,
        } ;

        #endregion
    }
}