using System;
using System.Collections.Generic;
using System.Text;

namespace LibUsb.Common
{
    public interface IUsbInterfaceDescriptor : IUsbDescriptor
    {
        /// <summary>
        /// Number of Interface
        /// </summary>
        byte InterfaceID { get; }

        /// <summary>
        /// Value used to select alternative setting
        /// </summary>
        byte AlternateID { get; }

        /// <summary>
        /// Number of Endpoints used for this interface
        /// </summary>
        byte EndpointCount { get; }

        /// <summary>
        /// Class Code (Assigned by USB Org)
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
        /// Index of String Descriptor Describing this interface
        /// </summary>
        byte StringIndex { get; }
    }
}
