using System;
using System.Collections.Generic;
using System.Text;

namespace LibUsb.Common
{
    public interface IUsbEndpointDescriptor : IUsbDescriptor
    {
        /// <summary>
        /// Endpoint Address
        /// Bits 0..3b Endpoint Number.
        /// Bits 4..6b Reserved. Set to Zero
        /// Bits 7 Direction 0 = Out, 1 = In (Ignored for Control Endpoints)
        /// </summary>
        byte EndpointID { get; }

        /// <summary>
        /// Bits 0..1 Transfer Type 
        /// 00 = Control
        /// 01 = Isochronous
        /// 10 = Bulk
        /// 11 = Interrupt
        /// 
        /// Bits 2..7 are reserved. If Isochronous endpoint, 
        /// Bits 3..2 = Synchronisation Type (Iso Mode) 
        /// 00 = No Synchonisation
        /// 01 = Asynchronous
        /// 10 = Adaptive
        /// 11 = Synchronous
        /// 
        /// Bits 5..4 = Usage Type (Iso Mode) 
        /// 00 = Data Endpoint
        /// 01 = Feedback Endpoint
        /// 10 = Explicit Feedback Data Endpoint
        /// 11 = Reserved
        /// </summary>
        byte Attributes { get; }

        /// <summary>
        /// Maximum Packet Size this endpoint is capable of sending or receiving
        /// </summary>
        short MaxPacketSize { get; }

        /// <summary>
        /// Interval for polling endpoint data transfers. Value in frame counts. Ignored for Bulk and Control Endpoints. Isochronous must equal 1 and field may range from 1 to 255 for interrupt endpoints.
        /// </summary>
        byte Interval { get; }

        /// <summary>
        /// Audio endpoint specific.
        /// </summary>
        byte Refresh { get;  }

        /// <summary>
        /// Audio endpoint specific.
        /// </summary>
        byte SynchAddress { get; }
    }
}
