// Copyright © 2006-2010 Travis Robinson. All rights reserved.
// Copyright © 2011-2023 LibUsbDotNet contributors. All rights reserved.
// 
// website: http://github.com/libusbdotnet/libusbdotnet
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
//

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LibUsbDotNet.Info;

/// <summary> Contains USB device descriptor information.
/// </summary>
public class UsbDeviceInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsbDeviceInfo"/> class.
    /// </summary>
    protected UsbDeviceInfo()
    {
    }

    private readonly Collection<UsbConfigInfo> configurations = new Collection<UsbConfigInfo>();

    public static UsbDeviceInfo FromUsbDeviceDescriptor(LibUsb.IUsbDevice device, DeviceDescriptor descriptor)
    {
        Debug.Assert(descriptor.DescriptorType == (int)DescriptorType.Device, "A config descriptor was expected");

        var value = new UsbDeviceInfo
        {
            Device = descriptor.Device,
            DeviceClass = descriptor.DeviceClass,
            DeviceProtocol = descriptor.DeviceProtocol,
            DeviceSubClass = descriptor.DeviceSubClass,
            ProductId = descriptor.IdProduct,
            VendorId = descriptor.IdVendor,
            Manufacturer = device.GetStringDescriptor(descriptor.Manufacturer, failSilently: true),
            MaxPacketSize0 = descriptor.MaxPacketSize0,
            NumConfigurations = descriptor.NumConfigurations,
            Product = device.GetStringDescriptor(descriptor.Product, failSilently: true),
            SerialNumber = device.GetStringDescriptor(descriptor.SerialNumber, failSilently: true),
            Usb = descriptor.USB
        };

        for (byte i = 0; i < descriptor.NumConfigurations; i++)
        {
            if (device.TryGetConfigDescriptor(i, out var configDescriptor))
            {
                value.configurations.Add(configDescriptor);
            }
        }
            
        return value;
    }

    public virtual ushort Device { get; protected set; }

    public virtual byte DeviceClass { get; protected set; }

    public virtual byte DeviceProtocol { get; protected set; }

    public virtual byte DeviceSubClass { get; protected set; }

    public virtual ushort ProductId { get; protected set; }

    public virtual ushort VendorId { get; protected set; }

    public virtual string Manufacturer { get; protected set; }

    public virtual byte MaxPacketSize0 { get; protected set; }

    public virtual byte NumConfigurations { get; protected set; }

    public virtual string Product { get; protected set; }

    public virtual string SerialNumber { get; protected set; } = string.Empty;

    public virtual ushort Usb { get; protected set; }

    public virtual ReadOnlyCollection<UsbConfigInfo> Configurations => new(this.configurations);

    public override string ToString() =>
        $"Device: 0x{Device:X4}\n" +
        $"DeviceClass: {DeviceClass}\n" +
        $"DeviceSubClass: 0x{DeviceSubClass:X2}\n" +
        $"VendorId: 0x{VendorId:X4}\n" +
        $"ProductId: 0x{ProductId:X4}\n" +
        $"Manufacturer: {Manufacturer}\n" +
        $"Product: {Product}\n" +
        $"SerialNumber: {SerialNumber}\n" +
        $"USB: 0x{Usb:X4}\n" +
        $"MaxPacketSize: {MaxPacketSize0}\n" +
        $"NumConfigurations: {NumConfigurations}";
}