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
using System;
using System.Threading;

namespace LibUsbDotNet.Main
{
    /// <summary>
    /// Base class for async transfer context.
    /// </summary>
    public abstract class UsbTransfer : IDisposable,IAsyncResult
    {
        private readonly UsbEndpointBase mEndpointBase;

        private IntPtr mBuffer;
        private int mCurrentOffset;
        private int mCurrentRemaining;
        private int mCurrentTransmitted;

        private int mFailRetries;
        /// <summary></summary>
        protected int mOriginalCount;
        /// <summary></summary>
        protected int mOriginalOffset;
        private PinnedHandle mPinnedHandle;

        /// <summary></summary>
        protected int mTimeout;

        /// <summary></summary>
        protected bool mHasWaitBeenCalled = true;

        /// <summary></summary>
        protected ManualResetEvent mTransferCancelEvent = new ManualResetEvent(false);
        /// <summary></summary>
        protected internal ManualResetEvent mTransferCompleteEvent = new ManualResetEvent(true);

        /// <summary></summary>
        protected UsbTransfer(UsbEndpointBase endpointBase) { mEndpointBase = endpointBase; }

        /// <summary>
        /// Returns the <see cref="UsbEndpointReader"/> or <see cref="UsbEndpointWriter"/> this transfer context is associated with.
        /// </summary>
        public UsbEndpointBase EndpointBase
        {
            get { return mEndpointBase; }
        }

        /// <summary>
        /// Number of bytes that will be requested for the next transfer.
        /// </summary>
        protected int RequestCount
        {
            get { return (mCurrentRemaining > UsbEndpointBase.MaxReadWrite ? UsbEndpointBase.MaxReadWrite : mCurrentRemaining); }
        }

        /// <summary></summary>
        protected int FailRetries
        {
            get { return mFailRetries; }
        }

        /// <summary></summary>
        protected IntPtr NextBufPtr
        {
            get { return new IntPtr(mBuffer.ToInt64() + mCurrentOffset); }
        }

        ///<summary>
        /// True if the transfer has been cacelled with <see cref="Cancel"/>.
        ///</summary>
        public bool IsCancelled
        {
            get { return mTransferCancelEvent.WaitOne(0, UsbConstants.EXIT_CONTEXT); }
        }

        /// <summary>
        /// Gets the <see cref="WaitHandle"/> for the cancel event.
        /// </summary>
        public WaitHandle CancelWaitHandle
        {
            get { return mTransferCancelEvent; }
        }



        #region IDisposable Members

        /// <summary>
        /// Cancels any pending transfer and frees resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsCancelled) Cancel();

            int dummy;
            if (!mHasWaitBeenCalled) Wait(out dummy);
        }

        #endregion

        /// <summary>
        /// Cancels a pending transfer that was previously submitted with <see cref="Submit"/>.
        /// </summary>
        /// <returns></returns>
        public virtual ErrorCode Cancel()
        {
            mTransferCancelEvent.Set();
            return ErrorCode.Success;
        }

        /// <summary>
        /// Submits the transfer.
        /// </summary>
        /// <remarks>
        /// This functions submits the USB transfer and return immediately.
        /// </remarks>
        /// <returns>
        /// <see cref="ErrorCode.Success"/> if the submit succeeds, 
        /// otherwise one of the other <see cref="ErrorCode"/> codes.
        /// </returns>
        public abstract ErrorCode Submit();

        /// <summary>
        /// Wait for the transfer to complete, timeout, or get cancelled.
        /// </summary>
        /// <param name="transferredCount">The number of bytes transferred on <see cref="ErrorCode.Success"/>.</param>
        /// <returns><see cref="ErrorCode.Success"/> if the transfer completes successfully, otherwise one of the other <see cref="ErrorCode"/> codes.</returns>
        public abstract ErrorCode Wait(out int transferredCount);

        /// <summary>
        /// Fills the transfer with the data to <see cref="Submit"/>.
        /// </summary>
        /// <param name="buffer">The buffer; See <see cref="PinnedHandle"/> for more details.</param>
        /// <param name="offset">The offset on the buffer where the transfer should read/write.</param>
        /// <param name="count">The number of bytes to transfer.</param>
        /// <param name="timeout">Time (milliseconds) to wait before the transfer times out.</param>
        public virtual void Fill(object buffer, int offset, int count, int timeout)
        {
            if (mPinnedHandle != null) mPinnedHandle.Dispose();
            mPinnedHandle = new PinnedHandle(buffer);
            Fill(mPinnedHandle.Handle, offset, count, timeout);
        }
        /// <summary>
        /// Fills the transfer with the data to <see cref="Submit"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset on the buffer where the transfer should read/write.</param>
        /// <param name="count">The number of bytes to transfer.</param>
        /// <param name="timeout">Time (milliseconds) to wait before the transfer times out.</param>
        public virtual void Fill(IntPtr buffer, int offset, int count, int timeout)
        {
            mBuffer = buffer;

            mOriginalOffset = offset;
            mOriginalCount = count;
            mTimeout = timeout;
            Reset();
        }

        internal static ErrorCode SyncTransfer(UsbTransfer transferContext,
                                               IntPtr buffer,
                                               int offset,
                                               int length,
                                               int timeout,
                                               out int transferLength)
        {
            if (ReferenceEquals(transferContext, null)) throw new NullReferenceException("Invalid transfer context.");
            if (offset < 0) throw new ArgumentException("must be >=0", "offset");

            lock (transferContext)
            {
                transferLength = 0;

                int transferred;
                ErrorCode ec;

                transferContext.Fill(buffer, offset, length, timeout);

                while (true)
                {
                    ec = transferContext.Submit();
                    if (ec == ErrorCode.IoEndpointGlobalCancelRedo) continue;
                    if (ec != ErrorCode.Success) return ec;

                    ec = transferContext.Wait(out transferred);
                    if (ec == ErrorCode.IoEndpointGlobalCancelRedo) continue;
                    if (ec != ErrorCode.Success) return ec;

                    transferLength += transferred;

                    if ((ec != ErrorCode.None || transferred != UsbEndpointBase.MaxReadWrite) ||
                        !transferContext.IncrementTransfer(transferred))
                        break;
                }

                return ec;
            }
        }

        /// <summary>
        /// Increments the internal counters to the next transfer batch (for transfers greater than <see cref="UsbEndpointBase.MaxReadWrite"/>)
        /// </summary>
        /// <param name="amount">This will usually be the total transferred on the previous batch.</param>
        /// <returns>True if the buffer still has data available and internal counters were successfully incremented.</returns>
        public bool IncrementTransfer(int amount)
        {
            mCurrentTransmitted += amount;
            mCurrentOffset += amount;
            mCurrentRemaining -= amount;

            if (mCurrentRemaining <= 0) return false;

            return true;
        }

        /// <summary></summary>
        protected void IncFailRetries() { mFailRetries++; }

        /// <summary>
        /// Resets the transfer to its orignal state.
        /// </summary>
        /// <remarks>
        /// Prepares a <see cref="UsbTransfer"/> to be resubmitted.
        /// </remarks>
        public void Reset()
        {
            mCurrentOffset = mOriginalOffset;
            mCurrentRemaining = mOriginalCount;
            mCurrentTransmitted = 0;
            mFailRetries = 0;

            mTransferCancelEvent.Reset();
        }

        /// <summary>
        /// Gets an indication whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>
        /// true if the operation is complete; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsCompleted
        {
            get { return mTransferCompleteEvent.WaitOne(0, UsbConstants.EXIT_CONTEXT); }
        }


        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public WaitHandle AsyncWaitHandle
        {
            get { return mTransferCompleteEvent; }
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A user-defined object that qualifies or contains information about an asynchronous operation.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object AsyncState
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets an indication of whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>
        /// true if the asynchronous operation completed synchronously; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool CompletedSynchronously
        {
            get { return false; }
        }
    }
}