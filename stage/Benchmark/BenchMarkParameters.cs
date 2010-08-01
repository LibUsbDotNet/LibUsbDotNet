using System;
using System.ComponentModel;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace Benchmark
{
    public class BenchMarkParameters
    {
        public enum BenchmarInterfaces
        {
            Interface_00,
            Interface_01,
        }
        private int mTimeout = 5000;
        private int mBufferSize = UsbEndpointReader.DefReadBufferSize;
        private BenchmarInterfaces mMI = BenchmarInterfaces.Interface_00;
        private ThreadPriority mPriority = ThreadPriority.Normal;
        private bool mVerify = true;
        private int mScreenRefeshMs = 100;

        [Category("General")]
        [Description("How often (in milliseconds) to refresh the screen when a benchmark test is running.")]
        public int RefreshDisplayInterval
        {
            get { return mScreenRefeshMs; }
            set { mScreenRefeshMs = value; }
        }


        [Category("Interface")]
        [Description("The benchmark interface #")]
        public BenchmarInterfaces MI
        {
            get { return mMI; }
            set { mMI = value; }
        }

        [Category("Buffers")]
        [Description("Must be an interval of the endpoint(s) maximum packet size.")]
        public int BufferSize
        {
            get { return mBufferSize; }
            set { mBufferSize = value; }
        }

        [Category("Timeouts")]
        public int Timeout
        {
            get { return mTimeout; }
            set { mTimeout = value; }
        }

        [Category("General")]
        [Description("The priority level for the read/write threads.")]
        public ThreadPriority Priority
        {
            get { return mPriority; }
            set { mPriority = value; }
        }

        [Category("General")]
        [Description("If true, received data is verified against the sent data. This option is only used for a LOOP test.")] 
        public bool Verify
        {
            get {
                return mVerify;
            }
            set {
                mVerify = value;
            }
        }
    }
}