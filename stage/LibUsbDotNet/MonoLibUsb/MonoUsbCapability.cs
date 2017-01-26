namespace MonoLibUsb
{
    public enum MonoUsbCapability
    {
        /// <summary>
        /// The libusb_has_capability() API is available.
        /// </summary>
        LIBUSB_CAP_HAS_CAPABILITY = 0x0000,
        /// <summary>
        /// Hotplug support is available on this platform.
        /// </summary>
	    LIBUSB_CAP_HAS_HOTPLUG = 0x0001,
        /// <summary>
        /// The library can access HID devices without requiring user intervention. 
        /// Note that before being able to actually access an HID device, you may still have to call additional libusb functions such as libusb_detach_kernel_driver(). 
        /// </summary>
        LIBUSB_CAP_HAS_HID_ACCESS = 0x0100,
        /// <summary>
        /// The library supports detaching of the default USB driver, using libusb_detach_kernel_driver(), if one is set by the OS kernel
        /// </summary>
        LIBUSB_CAP_SUPPORTS_DETACH_KERNEL_DRIVER = 0x0101
    }    
}
