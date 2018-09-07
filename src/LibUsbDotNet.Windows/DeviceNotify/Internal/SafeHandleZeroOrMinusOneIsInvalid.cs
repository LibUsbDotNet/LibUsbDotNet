#if NETSTANDARD1_5 || NETSTANDARD1_6
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace LibUsbDotNet.DeviceNotify.Internal
{
    [SecurityCritical]
    internal abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
    {
        protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        {
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get 
            {
                return handle == IntPtr.Zero || handle == new IntPtr(-1); 
            }
        }
    }
}
#endif
