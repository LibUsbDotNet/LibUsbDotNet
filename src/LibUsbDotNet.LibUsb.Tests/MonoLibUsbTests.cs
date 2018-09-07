using System;
using Xunit;

namespace MonoLibUsb.Tests
{
    public class MonoLibUsbTests : IDisposable
    {
        MonoUsbSessionHandle usbSession = null;

        public MonoLibUsbTests()
        {
            usbSession = new MonoUsbSessionHandle();
        }

        public void Dispose()
        {
            usbSession.Close();
        }

        [Fact]
        public void InitAndExit()
        {
            System.IntPtr usbSessionPointer = System.IntPtr.Zero;
            var lastReturnCode = (MonoLibUsb.MonoUsbError)MonoLibUsb.MonoUsbApi.Init(ref usbSessionPointer);

            Assert.Equal(MonoUsbError.Success, lastReturnCode);

            MonoLibUsb.MonoUsbApi.Exit(usbSessionPointer);
        }

        [Fact]
        public void SetDebug()
        {
            MonoLibUsb.MonoUsbApi.SetDebug(usbSession, 3);
        }

        [Fact]
        public void GetVersion()
        {
            MonoLibUsb.Descriptors.MonoUsbVersion version = new Descriptors.MonoUsbVersion();
            var versionPtr = MonoLibUsb.MonoUsbApi.GetVersion();
            System.Runtime.InteropServices.Marshal.PtrToStructure(versionPtr, version);

            Assert.Equal(1, version.Major);
        }

        [Fact]
        public void HasCapability()
        {
            int result = MonoLibUsb.MonoUsbApi.HasCapability(MonoUsbCapability.LIBUSB_CAP_HAS_CAPABILITY);

            Assert.Equal(1, result);
        }
    }
}
