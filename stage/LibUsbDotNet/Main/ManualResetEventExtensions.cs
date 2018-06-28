#if !NETSTANDARD
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace LibUsbDotNet.Main
{
    internal static class ManualResetEventExtensions
    {
        public static SafeWaitHandle GetSafeWaitHandle(this ManualResetEvent mre)
        {
            return mre.SafeWaitHandle;
        }
    }
}
#endif
