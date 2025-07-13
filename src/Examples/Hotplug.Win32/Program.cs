using LibUsbDotNet.LibUsb;
using System;

using var context = new UsbContext()
{
	HotplugOptions = new HotplugOptions
	{
		VendorId = 0x046D, // Logitech
		ProductId = 0x0A9, // Logitech G502 HERO
	}
};

context.RegisterHotPlug(); // TODO check if Hotplug is supported?!

Console.ReadKey();

context.UnregisterHotPlug();
