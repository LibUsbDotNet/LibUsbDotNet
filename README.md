# LibUsbDotNet
[![Build status](https://ci.appveyor.com/api/projects/status/5fe0th33i60h24bw?svg=true)](https://ci.appveyor.com/project/qmfrederik/libusbdotnet)
![NuGet version](https://img.shields.io/nuget/v/CoreCompat.LibUsbDotNet.svg)

> __NOTE__ This repository contains a fork of LibUsbDotNet; the original repository is located on [SourceForge](https://sourceforge.net/p/libusbdotnet/).
> This repository adds some minor bug fixes and support for .NET Core.

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
