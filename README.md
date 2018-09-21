# LibUsbDotNet
[![Build Status](https://dev.azure.com/libusbdotnet/libusbdotnet/_apis/build/status/LibUsbDotNet.LibUsbDotNet?branchName=master)](https://dev.azure.com/libusbdotnet/libusbdotnet/_build/latest?definitionId=1)
[![NuGet version](https://img.shields.io/nuget/v/LibUsbDotNet.svg)](https://www.nuget.org/packages/LibUsbDotNet/)
![LGPLv3 license](https://img.shields.io/github/license/LibUsbDotNet/LibUsbDotNet.svg)

[LibUsbDotNet](http://sourceforge.net/projects/libusbdotnet) is a .NET C# USB library for WinUsb, libusb-win32, and Linux libusb v1.x developers. 
All basic USB device functionality can be performed through common device classes allowing you to write OS and driver independent code.

* LibUsbDotNet versions 2.2.4 and above support the Libusb-1.0 driver.
* LibUsbDotNet 2.1.0 and above supports the genuine [libusb-win32](http://sourceforge.net/projects/libusb-win32) driver package. However, 
  access to basic device information via the windows registry is not available. See the [LegacyUsbRegistry](http://libusbdotnet.sourceforge.net/V2/html/9b8a7337-0d0c-c3e6-6f56-d47f1a3e5856.htm)
  class for more information.

## Features
* Full support for WinUSB. All WinUSB interfaces are treated as separate devices; each interface can be used by a different application.
* Extended kernel level support for libusb-win32.
* Supports Unix-like operating systems using Mono .NET and libusb-1.0.
* Common device classes allow for a single code base to support multiple drivers and platforms.
* Includes a Usb InfWizard utility (Windows only) for generating usb installation packages, removing devices, and installing drivers.
* Device discovery using any or all of the folowing criteria:
  * VendorID
  * ProductID
  * Revision Code
  * Serial Number
  * Device Interface GUID

Source package includes many small example applications.

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