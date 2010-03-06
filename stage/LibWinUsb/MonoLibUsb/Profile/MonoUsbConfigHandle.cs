using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;
using MonoLibUsb.Descriptors;

namespace MonoLibUsb.Profile
{
    /// <summary>
    /// The <see cref="MonoUsbConfigHandle"/> class hold the internal pointer to a libusb <see cref="MonoUsbConfigDescriptor"/>.
    /// </summary>
    /// <remarks>
    /// <para>This is a <see cref="SafeHandle"/>. When this handle is no longer in-use <see cref="SafeHandle.ReleaseHandle"/> is called and the internal configuration pointer is automatically freed with <see cref="MonoLibUsbApi.libusb_free_config_descriptor"/>.</para>
    /// <para>To access configuration information using this handle see <see cref="MonoUsbConfigDescriptor(MonoUsbConfigHandle)"/>.</para>
    /// <para>
    /// To acquire a <see cref="MonoUsbConfigHandle"/> use:
    /// <list type="bullet">
    /// <item><see cref="MonoLibUsbApi.libusb_get_active_config_descriptor"/></item>
    /// <item><see cref="MonoLibUsbApi.libusb_get_config_descriptor"/></item>
    /// <item><see cref="MonoLibUsbApi.libusb_get_config_descriptor_by_value"/></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class MonoUsbConfigHandle:SafeContextHandle
    {
        private MonoUsbConfigHandle() : base(IntPtr.Zero,true) {}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle() 
        {
            if (!IsInvalid)
            {
                MonoLibUsbApi.libusb_free_config_descriptor(handle);
                SetHandleAsInvalid();
                return true;
            }
            return false;
        }
    }
}
