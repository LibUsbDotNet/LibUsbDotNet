// Copyright © 2006-2009 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
// 
using System;
using System.Runtime.InteropServices;

using LibUsbDotNet.Main;
using MonoLibUsb.Transfer.Internal;

namespace MonoLibUsb.Transfer
{
    /// <summary>
    /// USB transfer wrapper structure.
    /// </summary>
    /// <remarks>
    /// The user populates this structure and then submits it in order to request a transfer. 
    /// After the transfer has completed, the library populates the transfer with the results 
    /// and passes it back to the user.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MonoUsbTransfer 
    {
        private static readonly int OfsActualLength = Marshal.OffsetOf(typeof (libusb_transfer), "actual_length").ToInt32();
        private static readonly int OfsEndpoint = Marshal.OffsetOf(typeof (libusb_transfer), "endpoint").ToInt32();
        private static readonly int OfsFlags = Marshal.OffsetOf(typeof (libusb_transfer), "flags").ToInt32();
        private static readonly int OfsLength = Marshal.OffsetOf(typeof (libusb_transfer), "length").ToInt32();
        private static readonly int OfsPtrBuffer = Marshal.OffsetOf(typeof (libusb_transfer), "pBuffer").ToInt32();
        private static readonly int OfsPtrCallbackFn = Marshal.OffsetOf(typeof (libusb_transfer), "pCallbackFn").ToInt32();
        private static readonly int OfsPtrDeviceHandle = Marshal.OffsetOf(typeof (libusb_transfer), "deviceHandle").ToInt32();
        private static readonly int OfsPtrUserData = Marshal.OffsetOf(typeof (libusb_transfer), "pUserData").ToInt32();
        private static readonly int OfsStatus = Marshal.OffsetOf(typeof (libusb_transfer), "status").ToInt32();
        private static readonly int OfsTimeout = Marshal.OffsetOf(typeof (libusb_transfer), "timeout").ToInt32();
        private static readonly int OfsType = Marshal.OffsetOf(typeof (libusb_transfer), "type").ToInt32();
        private static readonly int OfsNumIsoPackets = Marshal.OffsetOf(typeof (libusb_transfer), "num_iso_packets").ToInt32();
        private static readonly int OfsIsoPackets = Marshal.OffsetOf(typeof (libusb_transfer), "iso_packets").ToInt32();

        private IntPtr handle;
        /// <summary>
        /// Allocate a libusb transfer with a specified number of isochronous packet descriptors 
        /// </summary>
        /// <remarks>
        /// <para>The transfer is pre-initialized for you. When the new transfer is no longer needed, it should be freed with <see cref="Free"/>.</para>
        /// <para>Transfers intended for non-isochronous endpoints (e.g. control, bulk, interrupt) should specify an iso_packets count of zero.</para>
        /// <para>For transfers intended for isochronous endpoints, specify an appropriate number of packet descriptors to be allocated as part of the transfer. The returned transfer is not specially initialized for isochronous I/O; you are still required to set the <see cref="MonoUsbTransfer.NumIsoPackets"/> and <see cref="MonoUsbTransfer.Type"/> fields accordingly.</para>
        /// <para>It is safe to allocate a transfer with some isochronous packets and then use it on a non-isochronous endpoint. If you do this, ensure that at time of submission, <see cref="MonoUsbTransfer.NumIsoPackets"/> is 0 and that type is set appropriately.</para>
        /// </remarks>
        /// <param name="numIsoPackets">number of isochronous packet descriptors to allocate.</param>
        public MonoUsbTransfer(int numIsoPackets)
        {
            handle = MonoLibUsbApi.libusb_alloc_transfer(numIsoPackets);
        }

        /// <summary>
        /// Creates a new wrapper for transfers allocated by <see cref="MonoLibUsbApi.libusb_alloc_transfer"/>,
        /// </summary>
        /// <param name="pTransfer">The pointer to the transfer that was previously allocated with<see cref="MonoLibUsbApi.libusb_alloc_transfer"/>. </param>
        internal MonoUsbTransfer(IntPtr pTransfer)
        {
            handle = pTransfer;
        }

        /// <summary>
        /// Gets the buffer data pointer.
        /// </summary>
        public IntPtr PtrBuffer
        {
            get { return Marshal.ReadIntPtr(handle, OfsPtrBuffer); }
            set { Marshal.WriteIntPtr(handle, OfsPtrBuffer, value); }
        }

        /// <summary>
        /// User context data to pass to the callback function.
        /// </summary>
        public IntPtr PtrUserData
        {
            get { return Marshal.ReadIntPtr(handle, OfsPtrUserData); }
            set { Marshal.WriteIntPtr(handle, OfsPtrUserData, value); }
        }

        /// <summary>
        /// Callback function pointer. The callback function mast de declared as a <see cref="MonoUsbTransferDelegate"/>.
        /// </summary>
        public IntPtr PtrCallbackFn
        {
            get { return Marshal.ReadIntPtr(handle, OfsPtrCallbackFn); }
            set { Marshal.WriteIntPtr(handle, OfsPtrCallbackFn, value); }
        }

        /// <summary>
        /// Actual length of data that was transferred. 
        /// </summary>
        public int ActualLength
        {
            get { return Marshal.ReadInt32(handle, OfsActualLength); }
            set { Marshal.WriteInt32(handle, OfsActualLength, value); }
        }

        /// <summary>
        /// Length of the data buffer.
        /// </summary>
        public int Length
        {
            get { return Marshal.ReadInt32(handle, OfsLength); }
            set { Marshal.WriteInt32(handle, OfsLength, value); }
        }

        /// <summary>
        /// The status of the transfer.
        /// </summary>
        public MonoUsbTansferStatus Status
        {
            get { return (MonoUsbTansferStatus)Marshal.ReadInt32(handle, OfsStatus); }
            set { Marshal.WriteInt32(handle, OfsStatus, (int)value); }
        }

        /// <summary>
        /// Timeout for this transfer in millseconds.
        /// </summary>
        public int Timeout
        {
            get { return Marshal.ReadInt32(handle, OfsTimeout); }
            set { Marshal.WriteInt32(handle, OfsTimeout, value); }
        }

        /// <summary>
        /// Type of the endpoint. 
        /// </summary>
        public EndpointType Type
        {
            get { return (EndpointType)Marshal.ReadByte(handle, OfsType); }
            set { Marshal.WriteByte(handle, OfsType, (byte)value); }
        }

        /// <summary>
        /// Enpoint address.
        /// </summary>
        public byte Endpoint
        {
            get { return Marshal.ReadByte(handle, OfsEndpoint); }
            set { Marshal.WriteByte(handle, OfsEndpoint, value); }
        }

        /// <summary>
        /// A bitwise OR combination of <see cref="MonoUsbTransferFlags"/>.
        /// </summary>
        public MonoUsbTransferFlags Flags
        {
            get { return (MonoUsbTransferFlags)Marshal.ReadByte(handle, OfsFlags); }
            set { Marshal.WriteByte(handle, OfsFlags, (byte)value); }
        }

        /// <summary>
        /// Raw device handle pointer.
        /// </summary>
        public IntPtr PtrDeviceHandle
        {
            get { return Marshal.ReadIntPtr(handle, OfsPtrDeviceHandle); }
            set { Marshal.WriteIntPtr(handle, OfsPtrDeviceHandle, value); }
        }

        /// <summary>
        /// Number of isochronous packets. 
        /// </summary>
        public int NumIsoPackets
        {
            get { return Marshal.ReadInt32(handle, OfsNumIsoPackets); }
            set { Marshal.WriteInt32(handle, OfsNumIsoPackets, value); }
        }

        /// <summary>
        /// Frees the transfer with <see cref="MonoLibUsbApi.libusb_free_transfer"/>.
        /// </summary>
        ///<remarks>
        /// After freeing, <see cref="IsInvalid"/> will return <c>true</c>.
        /// Calling <see cref="Free"/> on a transfer that has already been freed will have no effect.
        /// </remarks>
        public void Free()
        {
            if (handle!=IntPtr.Zero)
            {
                MonoLibUsbApi.libusb_free_transfer(handle);
                handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Gets a unqiue name for this transfer that can be used as a system-wide identifier.
        /// </summary>
        /// <returns>A unqiue name for this transfer that can be used as a system-wide identifier.</returns>
        public String UniqueName()
        {
            String guidString = String.Format("_-EP[{0}]EP-_", handle);
            return guidString;
        }

        /// <summary>
        /// Gets a <see cref="MonoUsbIsoPacket"/> that represents the specified iso packet descriptor. 
        /// </summary>
        /// <param name="packetNumber">The iso packet descriptor to return.</param>
        /// <returns></returns>
        public MonoUsbIsoPacket IsoPacket(int packetNumber)
        {
            if (packetNumber > NumIsoPackets) throw new ArgumentOutOfRangeException("packetNumber");
            IntPtr pIsoPacket =
                new IntPtr(handle.ToInt64() + OfsIsoPackets + (packetNumber * Marshal.SizeOf(typeof(libusb_iso_packet_descriptor))));

            return new MonoUsbIsoPacket(pIsoPacket);
        }

        /// <summary>
        /// True if the transfer is allocated.
        /// </summary>
        public bool IsInvalid
        {
            get
            {
                return (handle == IntPtr.Zero);
            }
        }
        /// <summary>
        /// Cancels this transfer with <see cref="MonoLibUsbApi.libusb_cancel_transfer"/>.
        /// </summary>
        /// <returns><see cref="MonoUsbError.LIBUSB_SUCCESS"/> if the cancel succeeds, 
        /// otherwise one of the other <see cref="MonoUsbError"/> codes.</returns>
        public MonoUsbError Cancel()
        {
            if (IsInvalid) return MonoUsbError.LIBUSB_ERROR_NO_MEM;

            return (MonoUsbError) MonoLibUsbApi.libusb_cancel_transfer(this);
        }

        /// <summary>
        /// Helper function to populate the required <see cref="MonoUsbTransfer"/> properties for control, bulk, interrupt, and isochronous transfers.
        /// </summary>
        /// <remarks>
        /// Isochronous transfers are not supported on windows.
        /// </remarks>
        /// <param name="devHandle">Handle of the device that will handle the transfer.</param>
        /// <param name="endpoint">Address of the endpoint where this transfer will be sent</param>
        /// <param name="buffer">Data buffer.</param>
        /// <param name="length">Length of data buffer.</param>
        /// <param name="callback">Callback function to be invoked on transfer completion.</param>
        /// <param name="user_data">User data to pass to callback function.</param>
        /// <param name="timeout">Timeout for the transfer in milliseconds.</param>
        /// <param name="endpointType">The type of endpoint this transfer will be submitted on.</param>
        public void Fill(MonoUsbDeviceHandle devHandle,
                         byte endpoint,
                         IntPtr buffer,
                         int length,
                         Delegate callback,
                         IntPtr user_data,
                         int timeout,
                         EndpointType endpointType)
        {
            PtrDeviceHandle = devHandle.DangerousGetHandle();
            Endpoint = endpoint;
            PtrBuffer = buffer;
            Length = length;
            PtrCallbackFn = Marshal.GetFunctionPointerForDelegate(callback);
            PtrUserData = user_data;
            Timeout = timeout;
            Type = endpointType;
            Flags = MonoUsbTransferFlags.NONE;
        }

        /// <summary>
        /// Convenience function to locate the position of an isochronous packet within the buffer of an isochronous transfer. 
        /// </summary>
        /// <remarks>
        /// <para>This is a thorough function which loops through all preceding packets, accumulating their lengths to find the position of the specified packet. Typically you will assign equal lengths to each packet in the transfer, and hence the above method is sub-optimal. You may wish to use <see cref="GetIsoPacketBufferSimple"/> instead.</para>
        /// </remarks>
        /// <param name="packet">The packet to return the address of.</param>
        /// <returns>the base address of the packet buffer inside the transfer buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">This exception is thrown if the packet requested is >= <see cref="NumIsoPackets"/>.</exception>
        public IntPtr GetIsoPacketBuffer(int packet)
        {
            if (packet >= NumIsoPackets) throw new ArgumentOutOfRangeException("packet", "GetIsoPacketBuffer: packet must be < NumIsoPackets");
            long offset = PtrBuffer.ToInt64();

            for (int i = 0; i < packet; i++)
                offset += IsoPacket(i).Length;
            
            return new IntPtr(offset);
        }

        /// <summary>
        /// Convenience function to locate the position of an isochronous packet within the buffer of an isochronous transfer, for transfers where each packet is of identical size.
        /// </summary>
        /// <remarks>
        /// <para>This function relies on the assumption that every packet within the transfer is of identical size to the first packet. Calculating the location of the packet buffer is then just a simple calculation: buffer + (packet_size * packet)</para>
        /// <para>Do not use this function on transfers other than those that have identical packet lengths for each packet.</para>
        /// </remarks>
        /// <param name="packet">The packet to return the address of.</param>
        /// <returns>the base address of the packet buffer inside the transfer buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">This exception is thrown if the packet requested is >= <see cref="NumIsoPackets"/>.</exception>
        public IntPtr GetIsoPacketBufferSimple(int packet)
        {
            if (packet >= NumIsoPackets) throw new ArgumentOutOfRangeException("packet", "GetIsoPacketBufferSimple: packet must be < NumIsoPackets");

            return new IntPtr((PtrBuffer.ToInt64() + (IsoPacket(0).Length * packet)));

        }

        /// <summary>
        /// Convenience function to set the length of all packets in an isochronous transfer, based on the num_iso_packets field in the transfer structure. 
        /// </summary>
        /// <param name="length">The length to set in each isochronous packet descriptor.</param>
        public void SetIsoPacketLengths(int length)
        {
            int packetCount = NumIsoPackets;
            for (int i = 0; i < packetCount; i++)
                IsoPacket(i).Length = length;

        }
        /// <summary>
        /// Submits the transfer using <see cref="MonoLibUsbApi.libusb_submit_transfer"/>.
        /// </summary>
        /// <remarks>
        /// This functions submits the USB transfer and return immediately.
        /// </remarks>
        /// <returns>
        /// <see cref="MonoUsbError.LIBUSB_SUCCESS"/> if the submit succeeds, 
        /// otherwise one of the other <see cref="MonoUsbError"/> codes.
        /// </returns>
        public MonoUsbError Submit()
        {
            if (IsInvalid) return MonoUsbError.LIBUSB_ERROR_NO_MEM;
            return (MonoUsbError)MonoLibUsbApi.libusb_submit_transfer(this);
        }

        /// <summary>
        /// Allocate a libusb transfer with a specified number of isochronous packet descriptors 
        /// </summary>
        /// <remarks>This function will fire off the USB transfer and then return immediately.
        /// <para>The returned transfer is pre-initialized for you. When the new transfer is no longer needed, it should be freed with <see cref="Free"/>.</para>
        /// <para>Transfers intended for non-isochronous endpoints (e.g. control, bulk, interrupt) should specify an iso_packets count of zero.</para>
        /// <para>For transfers intended for isochronous endpoints, specify an appropriate number of packet descriptors to be allocated as part of the transfer. The returned transfer is not specially initialized for isochronous I/O; you are still required to set the <see cref="MonoUsbTransfer.NumIsoPackets"/> and <see cref="MonoUsbTransfer.Type"/> fields accordingly.</para>
        /// <para>It is safe to allocate a transfer with some isochronous packets and then use it on a non-isochronous endpoint. If you do this, ensure that at time of submission, <see cref="MonoUsbTransfer.NumIsoPackets"/> is 0 and that type is set appropriately.</para>
        /// </remarks>
        /// <param name="numIsoPackets">number of isochronous packet descriptors to allocate.</param>
        /// <returns>A newly allocated <see cref="MonoUsbTransfer"/>.</returns>
        /// <exception cref="OutOfMemoryException">If the transfer was not allocated.</exception>
        public static MonoUsbTransfer Alloc(int numIsoPackets)
        {
            IntPtr p = MonoLibUsbApi.libusb_alloc_transfer(numIsoPackets);
            if (p == IntPtr.Zero) throw new OutOfMemoryException("libusb_alloc_transfer");
            return new MonoUsbTransfer(p);
        }
    }
}