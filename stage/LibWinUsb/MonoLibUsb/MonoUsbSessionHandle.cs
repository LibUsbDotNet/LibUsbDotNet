using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace MonoLibUsb
{
    /// <summary>
    /// Class representing a Libusb-1.0 session. 
    /// </summary>
    /// <remarks>
    /// <para>The concept of individual Libusb-1.0 sessions allows for your program to use two libraries (or dynamically load two modules) which both independently use libusb. This will prevent interference between the individual libusb users - for example <see cref="MonoLibUsbApi.libusb_set_debug"></see> will not affect the other user of the library, and <see cref="MonoLibUsbApi.libusb_exit(IntPtr)"></see> will not destroy resources that the other user is still using.</para>
    /// <para>Sessions are created by <see cref="MonoLibUsbApi.libusb_init(ref IntPtr)"></see> and destroyed through <see cref="MonoLibUsbApi.libusb_exit(IntPtr)"></see>.</para>
    /// <para></para>
    /// </remarks>
    public class MonoUsbSessionHandle:SafeContextHandle
    {
        private static Object sessionLOCK = new object();
        private static MonoUsbError mLastReturnCode;
        private static String mLastReturnString=String.Empty;

        /// <summary>
        /// If the session handle is <see cref="SafeContextHandle.IsInvalid"/>, gets the <see cref="MonoUsbError"/> status code indicating the reason.
        /// </summary>
        public static MonoUsbError LastErrorCode
        {
            get
            {
                lock (sessionLOCK)
                {
                    return mLastReturnCode;
                }
            }
        }
        /// <summary>
        /// If the session handle is <see cref="SafeContextHandle.IsInvalid"/>, gets a descriptive string for the <see cref="LastErrorCode"/>.
        /// </summary>
        public static string LastErrorString
        {
            get
            {
                lock (sessionLOCK)
                {
                    return mLastReturnString;
                }
            }
        }

        /// <summary>
        /// Creates and initialize a <a href="http://libusb.sourceforge.net/api-1.0/index.html">Libusb-1.0</a> USB session handle.
        /// </summary>
        /// <remarks>
        /// <para>A <see cref="MonoUsbSessionHandle"/> instance must be created before calling any other <a href="http://libusb.sourceforge.net/api-1.0/index.html">Libusb-1.0 API</a> function.</para>
        /// </remarks>
        public MonoUsbSessionHandle() : base(IntPtr.Zero, true) 
        {
            lock (sessionLOCK)
            {
                IntPtr pNewSession = IntPtr.Zero;
                mLastReturnCode = (MonoUsbError)MonoLibUsbApi.libusb_init(ref pNewSession);
                if ((int)mLastReturnCode < 0)
                {
                    mLastReturnString=MonoLibUsbApi.libusb_strerror(mLastReturnCode);
                    SetHandleAsInvalid();
                }
                else
                    SetHandle(pNewSession);
              
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle() 
        { 
            if (!IsInvalid)
            {
                lock (sessionLOCK)
                {
                    MonoLibUsbApi.libusb_exit(handle);
                    SetHandleAsInvalid();
                }
            }
            return true;
        }
    }
}