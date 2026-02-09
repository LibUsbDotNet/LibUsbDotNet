# LibUsbDotNet
[![Build Status](https://dev.azure.com/libusbdotnet/libusbdotnet/_apis/build/status/LibUsbDotNet.LibUsbDotNet?branchName=master)](https://dev.azure.com/libusbdotnet/libusbdotnet/_build/latest?definitionId=1)
[![NuGet version](https://img.shields.io/nuget/v/LibUsbDotNet.svg)](https://www.nuget.org/packages/LibUsbDotNet/)
![LGPLv3 license](https://img.shields.io/github/license/LibUsbDotNet/LibUsbDotNet.svg)

[LibUsbDotNet](http://sourceforge.net/projects/libusbdotnet) is a .NET C# USB library for WinUsb, libusb-win32, and Linux libusb v1.x developers. 
All basic USB device functionality can be performed through common device classes allowing you to write OS and driver independent code.

* LibUsbDotNet versions 2.2.4 and above support the Libusb-1.0 driver.
* LibUsbDotNet 2.1.0 and above supports the genuine [libusb-win32](https://github.com/mcuee/libusb-win32/releases) driver package. However, 
  access to basic device information via the windows registry is not available.

## Features
* Full support for WinUSB. All WinUSB interfaces are treated as separate devices; each interface can be used by a different application.
* Extended kernel level support for libusb-win32.
* Supports Unix-like operating systems using Microsoft dotnet, Mono and libusb-1.0.
* Common device classes allow for a single code base to support multiple drivers and platforms.
* Device discovery using any or all of the folowing criteria:
  * VendorID
  * ProductID
  * Revision Code
  * Serial Number
  * Device Interface GUID

Source package includes many small example applications.
