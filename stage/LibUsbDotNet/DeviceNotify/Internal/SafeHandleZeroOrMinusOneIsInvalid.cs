#if NETSTANDARD1_5
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
