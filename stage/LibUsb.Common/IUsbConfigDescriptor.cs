using System;
using System.Collections.Generic;
using System.Text;

namespace LibUsb.Common
{
    public interface IUsbConfigDescriptor : IUsbDescriptor
    {
        /// <summary>
        /// Total length in bytes of data returned
        /// </summary>
        short TotalLength { get; }

        /// <summary>
        /// Number of Interfaces
        /// </summary>
        byte InterfaceCount { get; }

        /// <summary>
        /// Value to use as an argument to select this Configuration
        /// </summary>
        byte ConfigID { get; }

        /// <summary>
        /// Index of String Descriptor describing this Configuration
        /// </summary>
        byte StringIndex { get; }

        /// <summary>
        /// D7 Reserved, set to 1. (USB 1.0 Bus Powered)
        /// D6 Self Powered
        /// D5 Remote Wakeup
        /// D4..0 Reserved, set to 0.
        /// </summary>
        byte Attributes { get; }

        /// <summary>
        /// Maximum Power Consumption in 2mA units 
        /// </summary>
        byte MaxPower { get; }
    }
}
