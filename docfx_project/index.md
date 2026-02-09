# LibUsbDotNet
![LGPLv3 license](https://img.shields.io/github/license/LibUsbDotNet/LibUsbDotNet.svg)

[LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet) is a .NET C# USB library for WinUsb, libusb-win32, and Linux libusb v1.x developers.
All basic USB device functionality can be performed through common device classes allowing you to write OS and driver independent code.

* This is the main v3 branch of LibUsbDotNet, you may be looking for the [legacy v2 branch](https://github.com/LibUsbDotNet/LibUsbDotNet/tree/v2).

## Features
* Full support for WinUSB. All WinUSB interfaces are treated as separate devices; each interface can be used by a different application.
* Extended kernel level support for libusb-win32.
* Common device classes allow for a single code base to support multiple drivers and platforms.
* Device discovery using any or all of the following criteria:
    * VendorID
    * ProductID
    * Revision Code
    * Serial Number (devices must opened)
    * Device Interface GUID

Examples can be found [here](https://github.com/LibUsbDotNet/LibUsbDotNet/tree/master/src/Examples).
