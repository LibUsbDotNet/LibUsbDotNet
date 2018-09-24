using System;
using System.Runtime.InteropServices;
using LibUsbDotNet;
using MonoLibUsb.Transfer.Internal;

namespace MonoLibUsb.Transfer
{
    /// <summary>
    /// Wraps an iso packet structure
    /// </summary>
    public unsafe class MonoUsbIsoPacket
    {
        private IsoPacketDescriptor* mpMonoUsbIsoPacket = null;

        /// <summary>
        /// Creates a structure that wraps an iso packet.
        /// </summary>
        /// <param name="isoPacketPtr">The pointer to the iso packet to wrap.</param>
        public MonoUsbIsoPacket(IsoPacketDescriptor* isoPacketPtr)
        {
            mpMonoUsbIsoPacket = isoPacketPtr;
        }

        /// <summary>
        /// Returns the location in memory of this iso packet.
        /// </summary>
        public IntPtr PtrIsoPacket
        {
            get { return new IntPtr(mpMonoUsbIsoPacket); }
        }

        /// <summary>
        /// Amount of data that was actually transferred. 
        /// </summary>
        public int ActualLength
        {
            get { return (int)this.mpMonoUsbIsoPacket->ActualLength; }
            set { this.mpMonoUsbIsoPacket->ActualLength = (uint)value; }
        }

        /// <summary>
        /// Length of data to request in this packet. 
        /// </summary>
        public int Length
        {
            get { return (int)this.mpMonoUsbIsoPacket->Length; }
            set { this.mpMonoUsbIsoPacket->Length = (uint)value; }
        }

        /// <summary>
        /// Status code for this packet. 
        /// </summary>
        public TransferStatus Status
        {
            get { return this.mpMonoUsbIsoPacket->Status; }
            set { this.mpMonoUsbIsoPacket->Status = value; }
        }
    }
}