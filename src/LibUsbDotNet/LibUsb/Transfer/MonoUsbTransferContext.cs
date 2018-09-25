// Copyright © 2006-2010 Travis Robinson. All rights reserved.
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
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using MonoLibUsb.Transfer;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LibUsbDotNet.LudnMonoLibUsb.Internal
{
    internal class MonoUsbTransferContext : UsbTransfer, IDisposable
    {
        private bool mOwnsTransfer;

        private static unsafe readonly TransferDelegate mMonoUsbTransferCallbackDelegate = TransferCallback;
        private static readonly IntPtr mMonoUsbTransferCallbackPtr = Marshal.GetFunctionPointerForDelegate(mMonoUsbTransferCallbackDelegate);
        
        private GCHandle mCompleteEventHandle;
        private MonoUsbTransfer mTransfer;

        public MonoUsbTransferContext(UsbEndpointBase endpointBase)
            : base(endpointBase)
        {
        }


        #region IDisposable Members

        /// <summary>Explicitly closes and frees the handle.</summary>
        public new virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool mbDisposed2;
        /// <summary>
        /// Cancels any pending transfer and frees resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!mbDisposed2)
            {
                mbDisposed2 = true;
                freeTransfer();
                if (disposing)
                {
                    // Dispose managed resources.
                }
                base.Dispose();
            }
        }


        #endregion

        ~MonoUsbTransferContext() { Dispose(false); }


        private void allocTransfer(UsbEndpointBase endpointBase, bool ownsTransfer, int isoPacketSize, int count)
        {
            int numIsoPackets = 0;

            // Patch for using libusb-1.0 on windows with libusbwK.sys
            EndpointType endpointType = endpointBase.Type;

            bool isLinux =
#if NET45
                false;
#else
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif

            if (isLinux)
            {

                if (isoPacketSize > 0)
                    numIsoPackets = count/isoPacketSize;
            }
            else
            {
                if (endpointType == EndpointType.Isochronous)
                    endpointType = EndpointType.Bulk;
            }
            ///////////////////////////////////////////////////////////////
            
            freeTransfer();
            mTransfer = MonoUsbTransfer.Alloc(numIsoPackets);
            mOwnsTransfer = ownsTransfer;
            mTransfer.Type = endpointType;
            mTransfer.Endpoint = endpointBase.EpNum;
            mTransfer.NumIsoPackets = numIsoPackets;

            if (!mCompleteEventHandle.IsAllocated)
                mCompleteEventHandle = GCHandle.Alloc(mTransferCompleteEvent);
            mTransfer.PtrUserData = GCHandle.ToIntPtr(mCompleteEventHandle);
            

            if (numIsoPackets > 0)
                mTransfer.SetIsoPacketLengths(isoPacketSize);
        }

        private void freeTransfer()
        {
            if (mTransfer.IsInvalid || mOwnsTransfer == false) return;
            mTransferCancelEvent.Set();
            mTransferCompleteEvent.WaitOne(200);
            mTransfer.Free();

            if (mCompleteEventHandle.IsAllocated)
                mCompleteEventHandle.Free();

           
        }

        /// <summary>
        /// Fills the transfer with the data to <see cref="UsbTransfer.Submit"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset on the buffer where the transfer should read/write.</param>
        /// <param name="count">The number of bytes to transfer.</param>
        /// <param name="timeout">Time (milliseconds) to wait before the transfer times out.</param>
        public override void Fill(IntPtr buffer, int offset, int count, int timeout)
        {
            allocTransfer(EndpointBase, true, 0, count);

            base.Fill(buffer, offset, count, timeout);

            mTransfer.Timeout =  (uint)timeout;
            mTransfer.PtrDeviceHandle = EndpointBase.Device.DeviceHandle.DangerousGetHandle();

            mTransfer.PtrCallbackFn = mMonoUsbTransferCallbackPtr;

            
            mTransfer.ActualLength = 0;
            mTransfer.Status = 0;
            mTransfer.Flags = TransferFlags.None;
        }

        /// <summary>
        /// Fills the transfer with the data to <see cref="UsbTransfer.Submit"/> an isochronous transfer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset on the buffer where the transfer should read/write.</param>
        /// <param name="count">The number of bytes to transfer.</param>
        /// <param name="timeout">Time (milliseconds) to wait before the transfer times out.</param>
        /// <param name="isoPacketSize">Size of each isochronous packet.</param>
        public override void Fill(IntPtr buffer, int offset, int count, int timeout, int isoPacketSize)
        {
            allocTransfer(EndpointBase, true, isoPacketSize, count);

            base.Fill(buffer, offset, count, timeout, isoPacketSize);

            mTransfer.Timeout = (uint)timeout;
            mTransfer.PtrDeviceHandle = EndpointBase.Device.DeviceHandle.DangerousGetHandle();

            mTransfer.PtrCallbackFn = mMonoUsbTransferCallbackPtr;

            mTransfer.ActualLength = 0;
            mTransfer.Status = 0;
            mTransfer.Flags = TransferFlags.None;
        }

        /// <summary>
        /// Submits the transfer.
        /// </summary>
        /// <remarks>
        /// This functions submits the USB transfer and return immediately.
        /// </remarks>
        /// <returns>
        /// <see cref="Error.Success"/> if the submit succeeds, 
        /// otherwise one of the other <see cref="Error"/> codes.
        /// </returns>
        public override Error Submit()
        {
            if (mTransferCancelEvent.WaitOne(0)) return Error.Io;

            if (!mTransferCompleteEvent.WaitOne(0)) return Error.Busy;

            mTransfer.PtrBuffer = NextBufPtr;
            mTransfer.Length = RequestCount;

            mTransferCompleteEvent.Reset();

            int ret = (int)mTransfer.Submit();
            if (ret < 0)
            {
                mTransferCompleteEvent.Set();
                return (Error)ret;
            }

            return Error.Success;
        }

        /// <summary>
        /// Wait for the transfer to complete, timeout, or get cancelled.
        /// </summary>
        /// <param name="transferredCount">The number of bytes transferred on <see cref="ErrorCode.Success"/>.</param>
        /// <param name="cancel">Not used for libusb-1.0. Transfers are always cancelled on timeout or error.</param>
        /// <returns><see cref="ErrorCode.Success"/> if the transfer completes successfully, otherwise one of the other <see cref="ErrorCode"/> codes.</returns>
        public override Error Wait(out int transferredCount, bool cancel)
        {
            transferredCount = 0;
            int ret = 0;

            int iWait = WaitHandle.WaitAny(new WaitHandle[] {mTransferCompleteEvent, mTransferCancelEvent},
                                           Timeout.Infinite);
            switch (iWait)
            {
                case 0: // TransferCompleteEvent

                    if (mTransfer.Status == TransferStatus.Completed)
                    {
                        transferredCount = mTransfer.ActualLength;
                        return Error.Success;
                    }

                    return MonoUsbErrorExtensions.ToError(mTransfer.Status);
                case 1: // TransferCancelEvent
                    ret = (int)mTransfer.Cancel();
                    bool bTransferComplete = mTransferCompleteEvent.WaitOne(100);
                    mTransferCompleteEvent.Set();

                    if (ret != 0 || !bTransferComplete)
                    {
                        var ec = ret == 0 ? Error.Io : Error.Other;
                        // MonoUsbErrorMessage.Error(ec, ret, String.Format("Wait:Unable to cancel transfer or the transfer did not return after it was cancelled. Cancelled:{0} TransferCompleted:{1}", (Error)ret, bTransferComplete), EndpointBase);
                        return ec;
                    }
                    return Error.Io;
                default: // Critical failure timeout
                    mTransfer.Cancel();
                    mTransferCompleteEvent.Set();
                    // MonoUsbErrorMessage.Error(ec, ret, String.Format("Wait:Critical timeout failure! The transfer callback function was not called within the allotted time."), EndpointBase);
                    return Error.Io;
            }
        }

        private static unsafe void TransferCallback(Transfer* pTransfer)
        {
            ManualResetEvent completeEvent = GCHandle.FromIntPtr(pTransfer->UserData).Target as ManualResetEvent;
            completeEvent.Set();
        }
    }
}