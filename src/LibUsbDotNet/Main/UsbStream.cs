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
using LibUsbDotNet.LibUsb;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
#if !NETCOREAPP && !NETSTANDARD
using System.Windows.Forms;
#endif

namespace LibUsbDotNet.Main
{
    public class IOCancelledException : IOException
    {
        public IOCancelledException(string message) : base(message)
        {
        }
    }

    public class UsbStreamAsyncTransfer : IAsyncResult
    {
        internal readonly int mCount;
        internal readonly int mOffset;
        internal readonly object mState;
        private readonly int mTimeout;
        internal AsyncCallback mCallback;
        internal ManualResetEvent mCompleteEvent = new ManualResetEvent(false);
        internal GCHandle mGCBuffer;
        internal bool mIsComplete;
        private Error mResult;
        private int mTrasferredLength;
        internal UsbEndpointBase mUsbEndpoint;

        public UsbStreamAsyncTransfer(UsbEndpointBase usbEndpoint,
                                      byte[] buffer,
                                      int offset,
                                      int count,
                                      AsyncCallback callback,
                                      object state,
                                      int timeout)
        {
            this.mUsbEndpoint = usbEndpoint;
            this.mOffset = offset;
            this.mCount = count;
            this.mState = state;
            this.mTimeout = timeout;
            this.mCallback = callback;
            this.mGCBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        }

        public Error Result
        {
            get { return this.mResult; }
        }

        public int TransferredLength
        {
            get { return this.mTrasferredLength; }
        }

#region IAsyncResult Members

        public bool IsCompleted
        {
            get { return this.mIsComplete; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return this.mCompleteEvent; }
        }

        public object AsyncState
        {
            get { return this.mState; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

#endregion

        public Error SyncTransfer()
        {
            this.mResult = this.mUsbEndpoint.Transfer(this.mGCBuffer.AddrOfPinnedObject(), this.mOffset, this.mCount, this.mTimeout, out this.mTrasferredLength);
            this.mGCBuffer.Free();
            this.mIsComplete = true;
            if (this.mCallback != null)
            {
                this.mCallback(this as IAsyncResult);
            }

            this.mCompleteEvent.Set();
            return this.mResult;
        }
    }

    public class UsbStream : Stream
    {
        private readonly UsbEndpointBase mUsbEndpoint;
        private int mTimeout = UsbConstants.DEFAULT_TIMEOUT;
#if !NETCOREAPP && !NETSTANDARD
        private Thread mWaitThread;
#endif

        public UsbStream(UsbEndpointBase usbEndpoint)
        {
            this.mUsbEndpoint = usbEndpoint;
        }

#region NOT SUPPORTED

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

#endregion

#region Overridden Members

#if !NETCOREAPP && !NETSTANDARD
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            UsbStreamAsyncTransfer asyncTransfer = new UsbStreamAsyncTransfer(this.mUsbEndpoint, buffer, offset, count, callback, state, this.ReadTimeout);
            this.WaitThread.Start(asyncTransfer);
            return asyncTransfer;
        }

        private Thread WaitThread
        {
            get
            {
                if (ReferenceEquals(this.mWaitThread, null))
                {
                    this.mWaitThread = new Thread(AsyncTransferFn);
                }

                while (this.mWaitThread.IsAlive)
                {
                    Application.DoEvents();
                }

                return this.mWaitThread;
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            UsbStreamAsyncTransfer asyncTransfer = new UsbStreamAsyncTransfer(this.mUsbEndpoint, buffer, offset, count, callback, state, this.WriteTimeout);
            this.WaitThread.Start(asyncTransfer);
            return asyncTransfer;
        }
#endif

        public override bool CanRead
        {
            get { return (this.mUsbEndpoint.EpNum & 0x80) == 0x80; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanTimeout
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return (this.mUsbEndpoint.EpNum & 0x80) == 0; }
        }

#if !NETCOREAPP && !NETSTANDARD
        public override int EndRead(IAsyncResult asyncResult)
        {
            UsbStreamAsyncTransfer asyncTransfer = (UsbStreamAsyncTransfer)asyncResult;
            asyncTransfer.mCompleteEvent.WaitOne();

            if (asyncTransfer.Result == Error.Success)
            {
                return asyncTransfer.TransferredLength;
            }

            if (asyncTransfer.Result == Error.Timeout)
            {
                throw new TimeoutException(string.Format("{0}:Endpoint 0x{1:X2} IO timed out.", asyncTransfer.Result, this.mUsbEndpoint.EpNum));
            }

            if (asyncTransfer.Result == Error.Interrupted)
            {
                throw new IOCancelledException(string.Format("{0}:Endpoint 0x{1:X2} IO was cancelled.", asyncTransfer.Result, this.mUsbEndpoint.EpNum));
            }

            throw new IOException(string.Format("{0}:Failed reading from endpoint:{1}", asyncTransfer.Result, this.mUsbEndpoint.EpNum));
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            UsbStreamAsyncTransfer asyncTransfer = (UsbStreamAsyncTransfer)asyncResult;
            asyncTransfer.mCompleteEvent.WaitOne();

            if (asyncTransfer.Result == Error.Success && asyncTransfer.mCount == asyncTransfer.TransferredLength)
            {
                return;
            }

            if (asyncTransfer.Result == Error.Timeout)
            {
                throw new TimeoutException(string.Format("{0}:Endpoint 0x{1:X2} IO timed out.", asyncTransfer.Result, this.mUsbEndpoint.EpNum));
            }

            if (asyncTransfer.Result == Error.Interrupted)
            {
                throw new IOCancelledException(string.Format("{0}:Endpoint 0x{1:X2} IO was cancelled.", asyncTransfer.Result, this.mUsbEndpoint.EpNum));
            }

            if (asyncTransfer.mCount != asyncTransfer.TransferredLength)
            {
                throw new IOException(string.Format("{0}:Failed writing {1} byte(s) to endpoint 0x{2:X2}.",
                                                    asyncTransfer.Result,
                                                    asyncTransfer.mCount - asyncTransfer.TransferredLength,
                                                    this.mUsbEndpoint.EpNum));
            }

            throw new IOException(string.Format("{0}:Failed writing to endpoint 0x{1:X2}", asyncTransfer.Result, this.mUsbEndpoint.EpNum));
        }
#endif

        public override void Flush()
        {
            return;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new InvalidOperationException(string.Format("Cannot read from WriteEndpoint {0}.", (WriteEndpointID)this.mUsbEndpoint.EpNum));
            }

            int transferred;
            Error ec = this.mUsbEndpoint.Transfer(buffer, offset, count, this.ReadTimeout, out transferred);

            if (ec == Error.Success)
            {
                return transferred;
            }

            if (ec == Error.Timeout)
            {
                throw new TimeoutException(string.Format("{0}:Endpoint 0x{1:X2} IO timed out.", ec, this.mUsbEndpoint.EpNum));
            }

            if (ec == Error.Interrupted)
            {
                throw new IOCancelledException(string.Format("{0}:Endpoint 0x{1:X2} IO was cancelled.", ec, this.mUsbEndpoint.EpNum));
            }

            throw new IOException(string.Format("{0}:Failed reading from endpoint:{1}", ec, this.mUsbEndpoint.EpNum));
        }

        public override int ReadTimeout
        {
            get { return this.mTimeout; }
            set { this.mTimeout = value; }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this.CanWrite)
            {
                throw new InvalidOperationException(string.Format("Cannot write to ReadEndpoint {0}.", (ReadEndpointID)this.mUsbEndpoint.EpNum));
            }

            int transferred;
            Error ec = this.mUsbEndpoint.Transfer(buffer, offset, count, this.WriteTimeout, out transferred);

            if (ec == Error.Success && count == transferred)
            {
                return;
            }

            if (ec == Error.Timeout)
            {
                throw new TimeoutException(string.Format("{0}:Endpoint 0x{1:X2} IO timed out.", ec, this.mUsbEndpoint.EpNum));
            }

            if (ec == Error.Interrupted)
            {
                throw new IOCancelledException(string.Format("{0}:Endpoint 0x{1:X2} IO was cancelled.", ec, this.mUsbEndpoint.EpNum));
            }

            if (count != transferred)
            {
                throw new IOException(string.Format("{0}:Failed writing {1} byte(s) to endpoint 0x{2:X2}.",
                                                    ec,
                                                    count - transferred,
                                                    this.mUsbEndpoint.EpNum));
            }

            throw new IOException(string.Format("{0}:Failed writing to endpoint 0x{1:X2}", ec, this.mUsbEndpoint.EpNum));
        }

        public override int WriteTimeout
        {
            get { return this.mTimeout; }
            set { this.mTimeout = value; }
        }

#endregion

#region STATIC Members

        private static void AsyncTransferFn(object oContext)
        {
            UsbStreamAsyncTransfer context = oContext as UsbStreamAsyncTransfer;
            context.SyncTransfer();
        }

#endregion
    }
}
