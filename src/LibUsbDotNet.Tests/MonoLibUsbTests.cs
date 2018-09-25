using LibUsbDotNet;
using System;
using Xunit;

namespace MonoLibUsb.Tests
{
    public class MonoLibUsbTests
    {
        [Fact]
        public void InitAndExit()
        {
            IntPtr usbSessionPointer = IntPtr.Zero;
            var lastReturnCode = (Error)NativeMethods.Init(ref usbSessionPointer);
            using (var usbSession = Context.DangerousCreate(usbSessionPointer))
            {
                Assert.Equal(Error.Success, lastReturnCode);
            }
        }

        [Fact]
        public void SetDebug()
        {
            IntPtr usbSessionPointer = IntPtr.Zero;
            var lastReturnCode = (Error)NativeMethods.Init(ref usbSessionPointer);
            using (var usbSession = Context.DangerousCreate(usbSessionPointer))
            {
                NativeMethods.SetDebug(usbSession, 3);
            }
        }

        [Fact]
        public unsafe void GetVersion()
        {
            var version = NativeMethods.GetVersion();
            Assert.Equal(1, version->Major);
        }

        [Fact]
        public void HasCapability()
        {
            int result = NativeMethods.HasCapability((uint)Capability.HasCapability);

            Assert.Equal(1, result);
        }
    }
}
