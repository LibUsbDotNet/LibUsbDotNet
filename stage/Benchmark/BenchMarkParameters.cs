using System;
using System.ComponentModel;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Benchmark
{
    public class BenchMarkParameters
    {
        private int mLoopWriteTimeout = 5000;
        private int mReadBufferSize = UsbEndpointReader.DefReadBufferSize;
        private ReadEndpointID mReadEndpoint = ReadEndpointID.Ep01;
        private ThreadPriority mReadThreadPriority = ThreadPriority.Normal;
        private int mWriteBufferSize = UsbEndpointReader.DefReadBufferSize;
        private WriteEndpointID mWriteEndpoint = WriteEndpointID.Ep01;
        private int mWriteTimeout = 1000;
        private bool mVerifyLoopData = true;
        private int mScreenRefeshMs = 100;

        [Category("General")]
        [Description("How often (in milliseconds) to refresh the screen when a benchmark test is running.")]
        public int RefreshDisplayInterval
        {
            get { return mScreenRefeshMs; }
            set { mScreenRefeshMs = value; }
        }


        [Category("Endpoints")]
        public WriteEndpointID WriteEndpoint
        {
            get { return mWriteEndpoint; }
            set { mWriteEndpoint = value; }
        }

        [Category("Endpoints")]
        public ReadEndpointID ReadEndpoint
        {
            get { return mReadEndpoint; }
            set { mReadEndpoint = value; }
        }

        [Category("Buffers")]
        public int ReadBufferSize
        {
            get { return mReadBufferSize; }
            set { mReadBufferSize = value; }
        }

        [Category("Buffers")]
        public int WriteBufferSize
        {
            get { return mWriteBufferSize; }
            set { mWriteBufferSize = value; }
        }

        [Category("Timeouts")]
        public int LoopWriteTimeout
        {
            get { return mLoopWriteTimeout; }
            set { mLoopWriteTimeout = value; }
        }

        [Category("Timeouts")]
        public int WriteTimeout
        {
            get { return mWriteTimeout; }
            set { mWriteTimeout = value; }
        }

        [Category("Endpoints")]
        public ThreadPriority ReadThreadPriority
        {
            get { return mReadThreadPriority; }
            set { mReadThreadPriority = value; }
        }

        [Category("General")]
        public bool VerifyLoopData
        {
            get {
                return mVerifyLoopData;
            }
            set {
                mVerifyLoopData = value;
            }
        }
    }
}