using System;

namespace LibUsb.Common
{
    public interface IUsbDeviceDescriptor : IUsbDescriptor
    {
        /// <summary>
        /// USB Specification Number which device complies too.
        /// </summary>
        short BcdUsb { get; }

        /// <summary>
        /// Class Code (Assigned by USB Org)
        /// If equal to Zero, each interface specifies it’s own class code; If equal to 0xFF, the class code is vendor specified; Otherwise field is valid Class Code.
        /// </summary>
        ClassCodeType Class { get; }

        /// <summary>
        /// Subclass Code (Assigned by USB Org)
        /// </summary>
        byte SubClass { get; }

        /// <summary>
        /// Protocol Code (Assigned by USB Org)
        /// </summary>
        byte Protocol { get; }

        /// <summary>
        /// Maximum Packet Size for Zero Endpoint. Valid Sizes are 8, 16, 32, 64
        /// </summary>
        byte MaxPacketSize0 { get; }

        /// <summary>
        /// Vendor ID (Assigned by USB Org)
        /// </summary>
        short VendorID { get; }

        /// <summary>
        /// Product ID (Assigned by Manufacturer)
        /// </summary>
        short ProductID { get; }

        /// <summary>
        /// Device Release Number
        /// </summary>
        short BcdDevice { get; }

        /// <summary>
        /// Index of Manufacturer String Descriptor
        /// </summary>
        byte ManufacturerStringIndex { get; }

        /// <summary>
        /// Index of Product String Descriptor
        /// </summary>
        byte ProductStringIndex { get; }

        /// <summary>
        /// Index of Serial Number String Descriptor
        /// </summary>
        byte SerialStringIndex { get; }

        /// <summary>
        /// Number of Possible Configurations
        /// </summary>
        byte ConfigurationCount { get; }
    }
}
