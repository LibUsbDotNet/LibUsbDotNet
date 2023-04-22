# LibUsbDotNet
![LGPLv3 license](https://img.shields.io/github/license/LibUsbDotNet/LibUsbDotNet.svg)

[LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet) is a .NET C# USB library for WinUsb, libusb-win32, and Linux libusb v1.x developers.
All basic USB device functionality can be performed through common device classes allowing you to write OS and driver independent code.

* This is the pre-release v3 branch of LibUsbDotNet, you may be looking for the [stable v2 branch](https://github.com/LibUsbDotNet/LibUsbDotNet/tree/v2).

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

Examples can be found [here](https://github.com/LibUsbDotNet/LibUsbDotNet/tree/master/src/Examples). Please note that the Read.Isochronous and Read.Write.Async examples are broken, as those features are not yet supported in the v3 branch.

### Features not yet in v3
* Async (including isochronous) transfers are not yet implemented. Please test [this PR](https://github.com/LibUsbDotNet/LibUsbDotNet/pull/193) if you are interested in this feature.
* Device connect/disconnect event notification.

### Linux users

If you have installed `libusb-1.0` and you still have an error about loading library, it may be needed to make a symlink to allow runtime load the library.

First, find the location of the library. For example : `sudo find / -name "libusb-1.0*.so*"` can give you :
```
/lib/x86_64-linux-gnu/libusb-1.0.so.0.1.0
/lib/x86_64-linux-gnu/libusb-1.0.so.0
```
Then go to the directory, and make the symlink. it should match the library name, with extension (.so) without version :
```
cd /lib/x86_64-linux-gnu
sudo ln -s libusb-1.0.so.0 libusb-1.0.so
```