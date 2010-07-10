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

    public class UsbTestDeviceException : System.Exception
    {
        public UsbTestDeviceException(string description)
            : base(description) { }
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
            new UsbSetupPacket((byte)UsbEndpointDirection.EndpointIn | (byte)UsbRequestRecipient.RecipDevice | (byte)UsbRequestType.TypeVendor,
                               (byte) PICFW_COMMANDS.GET_TEST,
                               0,
                               0,
                               1);

        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.GET_TEST"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdReadEEDATA =
            new UsbSetupPacket((byte)UsbEndpointDirection.EndpointIn | (byte)UsbRequestRecipient.RecipDevice | (byte)UsbRequestType.TypeVendor,
                               (byte) PICFW_COMMANDS.GET_EEDATA,
                               0,
                               0,
                               1);

        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.SET_TEST"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdSetTestType =
            new UsbSetupPacket((byte)UsbEndpointDirection.EndpointIn | (byte)UsbRequestRecipient.RecipDevice | (byte)UsbRequestType.TypeVendor,
                               (byte) PICFW_COMMANDS.SET_TEST,
                               0,
                               0,
                               1);


        /// <summary>
        /// Gets the <see cref="UsbSetupPacket"/> that represents the custom vendor request <see cref="PICFW_COMMANDS.SET_EEDATA"/>.
        /// </summary>
        internal static readonly UsbSetupPacket UsbCmdWriteEEDATA =
            new UsbSetupPacket((byte)UsbEndpointDirection.EndpointIn | (byte)UsbRequestRecipient.RecipDevice | (byte)UsbRequestType.TypeVendor,
                               (byte) PICFW_COMMANDS.SET_EEDATA,
                               0,
                               0,
                               1);



        public static bool GetTestType(UsbDevice usbTestDevice, out UsbTestType usbTestType, byte interfaceID)
        {
            if (ReferenceEquals(usbTestDevice, null))
                throw new UsbTestDeviceException("UsbTestDevice must be set before invoking this member!");

            int lengthTransferred;
            byte[] buf = new byte[1];

            UsbSetupPacket cmd = UsbCmdGetTestType;
            cmd.Index = interfaceID;

            bool bSuccess = usbTestDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);

            if (bSuccess && lengthTransferred == 1)
            {
                usbTestType = (UsbTestType) buf[0];
                return true;
            }
            usbTestType = UsbTestType.Invalid;
            return false;
        }


        public static bool SetTestType(UsbDevice usbTestDevice, UsbTestType usbTestType, bool bCheck, byte interfaceID)
        {
            if (ReferenceEquals(usbTestDevice, null))
                throw new UsbTestDeviceException("UsbTestDevice must be set before invoking this member!");

            if (bCheck)
            {
                UsbTestType bCurrentTestType;
                if (GetTestType(usbTestDevice, out bCurrentTestType, interfaceID))
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
            cmd.Index = interfaceID;

            bool bSuccess = usbTestDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);

            return bSuccess && lengthTransferred == 1;
        }


        public static bool ReadEEDATA(UsbDevice usbTestDevice, byte address, out byte value)
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


        public static bool WriteEEDATA(UsbDevice usbTestDevice, byte address, byte value)
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