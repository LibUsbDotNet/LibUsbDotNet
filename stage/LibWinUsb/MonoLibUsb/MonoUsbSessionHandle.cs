using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace MonoLibUsb
{
    /// <summary>
    /// Class representing a Libusb-1.0 session session handle.
    /// Session handled are wrapped in a <see cref="System.Runtime.ConstrainedExecution.CriticalFinalizerObject"/>. 
    /// </summary>
    /// <remarks>
    /// <para>The concept of individual Libusb-1.0 sessions allows for your program to use two libraries 
    /// (or dynamically load two modules) which both independently use libusb. This will prevent interference between the 
    /// individual libusb users - for example <see cref="MonoLibUsbApi.SetDebug"/> will not affect the other 
    /// user of the library, and <see cref="MonoLibUsbApi.Exit"/> will not destroy resources that the 
    /// other user is still using.</para>
    /// <para>Sessions are created by <see cref="MonoLibUsbApi.Init"/> and destroyed through <see cref="MonoLibUsbApi.Exit"/>.</para>
    /// <para>A <see cref="MonoUsbSessionHandle"/> instance must be created before calling any other <a href="http://libusb.sourceforge.net/api-1.0/index.html">Libusb-1.0 API</a> function.</para>
    /// <para>Session handles are equivalent to a <a href="http://libusb.sourceforge.net/api-1.0/group__lib.html#ga4ec088aa7b79c4a9599e39bf36a72833">libusb_context</a>.</para>
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
                mLastReturnCode = (MonoUsbError)MonoLibUsbApi.Init(ref pNewSession);
                if ((int)mLastReturnCode < 0)
                {
                    mLastReturnString=MonoLibUsbApi.StrError(mLastReturnCode);
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
                    MonoLibUsbApi.Exit(handle);
                    SetHandleAsInvalid();
                }
            }
            return true;
        }
    }
}