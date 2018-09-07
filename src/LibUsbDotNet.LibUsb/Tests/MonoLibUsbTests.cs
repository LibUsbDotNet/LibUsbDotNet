using NUnit.Framework;

namespace MonoLibUsb.Tests
{
    [TestFixture]
    class MonoLibUsbTests
    {
        MonoUsbSessionHandle usbSession = null;

        [SetUp]
        public void CreateAnUsbSession()
        {
            usbSession = new MonoUsbSessionHandle();
        }

        [TearDown]
        public void Exit()
        {
            usbSession.Close();
        }

        [Test]
        public void InitAndExit()
        {
            System.IntPtr usbSessionPointer = System.IntPtr.Zero;
            var lastReturnCode = (MonoLibUsb.MonoUsbError)MonoLibUsb.MonoUsbApi.Init(ref usbSessionPointer);

            Assert.AreEqual(lastReturnCode, MonoUsbError.Success);

            MonoLibUsb.MonoUsbApi.Exit(usbSessionPointer);
        }

        [Test]
        public void SetDebug()
        {
            MonoLibUsb.MonoUsbApi.SetDebug(usbSession, 3);
        }

        [Test]
        public void GetVersion()
        {
            MonoLibUsb.Descriptors.MonoUsbVersion version = new Descriptors.MonoUsbVersion();
            var versionPtr = MonoLibUsb.MonoUsbApi.GetVersion();
            System.Runtime.InteropServices.Marshal.PtrToStructure(versionPtr, version);

            Assert.AreEqual(version.Major, 1);
            Assert.AreEqual(version.Minor, 0);
        }

        [Test]
        public void HasCapability()
        {
            int result = MonoLibUsb.MonoUsbApi.HasCapability(MonoUsbCapability.LIBUSB_CAP_HAS_CAPABILITY);

            Assert.AreEqual(result, 1);
        }
    }
}
