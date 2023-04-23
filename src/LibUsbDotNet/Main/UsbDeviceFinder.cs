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

using LibUsbDotNet.LibUsb;
using System;
using System.Runtime.Serialization;
using LibUsbDotNet.Info;

namespace LibUsbDotNet.Main;

/// <summary>
/// Finds and identifies usb devices. Used for easily locating
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>
/// Pass instances of this class into the
/// <see cref="UsbContext.Find(UsbDeviceFinder)"/> or
/// <see cref="UsbContext.FindAll(UsbDeviceFinder)"/>
/// functions of a  <see cref="UsbContext"/>
/// instance to find connected usb devices without opening devices or interrogating the bus.
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code source="../../Examples/Show.Info/ShowInfo.cs" lang="cs"/>
/// </example>
public class UsbDeviceFinder : ISerializable
{
    public UsbDeviceFinder() {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UsbDeviceFinder"/> class using a serialization stream to fill the <see cref="UsbDeviceFinder"/> class.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected UsbDeviceFinder(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException("info");
        }

        this.Vid = (int)info.GetValue("Vid", typeof(int));
        this.Pid = (int)info.GetValue("Pid", typeof(int));
        this.Revision = (int)info.GetValue("Revision", typeof(int));
        this.SerialNumber = (string)info.GetValue("SerialNumber", typeof(string));
        this.DeviceInterfaceGuid = (Guid)info.GetValue("DeviceInterfaceGuid", typeof(Guid));
    }

    /// <summary>
    /// Gets the device interface guid string to find, or <see cref="string.Empty"/> to ignore.
    /// </summary>
    public Guid DeviceInterfaceGuid
    {
        get;
        init;
    } = Guid.Empty;

    /// <summary>
    /// Gets the device interface guid string to find, or <see cref="string.Empty"/> to ignore.
    /// </summary>
    public LocationId LocationId
    {
        get;
        init;
    } = LocationId.Zero;

    /// <summary>
    /// Gets the device interface guid string to find, or <see cref="string.Empty"/> to ignore.
    /// </summary>
    public PhysicalPortId PhyiscalPortId
    {
        get;
        init;
    } = null;

    /// <summary>
    /// Gets the serial number of the device to find.
    /// </summary>
    /// <remarks>
    /// Set to null to ignore.
    /// </remarks>
    public string SerialNumber
    {
        get;
        init;
    } = null;

    /// <summary>
    /// Gets the revision number of the device to find.
    /// </summary>
    /// <remarks>
    /// Set to <see cref="int.MaxValue"/> to ignore.
    /// </remarks>
    public int Revision
    {
        get;
        init;
    } = int.MaxValue;

    /// <summary>
    /// Gets the product id of the device to find.
    /// </summary>
    /// <remarks>
    /// Set to <see cref="int.MaxValue"/> to ignore.
    /// </remarks>
    public int Pid
    {
        get;
        init;
    } = int.MaxValue;

    /// <summary>
    /// Gets the vendor id of the device to find.
    /// </summary>
    /// <remarks>
    /// Set to <see cref="int.MaxValue"/> to ignore.
    /// </remarks>
    public int Vid
    {
        get;
        init;
    } = int.MaxValue;

    /// <inheritdoc/>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException("info");
        }

        info.AddValue("Vid", this.Vid);
        info.AddValue("Pid", this.Pid);
        info.AddValue("Revision", this.Revision);
        info.AddValue("SerialNumber", this.SerialNumber);
        info.AddValue("DeviceInterfaceGuid", this.DeviceInterfaceGuid);
    }

    /// <summary>
    /// Dynamic predicate find function. Pass this function into any method that has a <see cref="Predicate{UsbDevice}"/> parameter.
    /// </summary>
    /// <remarks>
    /// Override this member when inheriting the <see cref="UsbDeviceFinder"/> class to change/alter the matching behavior.
    /// </remarks>
    /// <param name="usbDevice">The UsbDevice to check.</param>
    /// <returns>True if the <see cref="UsbDevice"/> instance matches the <see cref="UsbDeviceFinder"/> properties.</returns>
    public virtual bool Check(IUsbDevice usbDevice)
    {
        try
        {
            if (this.Vid != int.MaxValue &&
                this.Vid != usbDevice.Info.VendorId)
            {
                return false;
            }

            if (this.Pid != int.MaxValue &&
                this.Pid != usbDevice.Info.ProductId)
            {
                return false;
            }

            if (this.Revision != int.MaxValue && 
                this.Revision != usbDevice.Info.Usb)
            {
                return false;
            }

            if (this.SerialNumber != null &&
                this.SerialNumber != usbDevice.Info.SerialNumber)
            {
                return false;
            }

            if (LocationId != Info.LocationId.Zero &&
                LocationId != usbDevice.LocationId)
            {
                return false;
            }
            
            if (PhyiscalPortId != null &&
                PhyiscalPortId.Usb2Id != usbDevice.LocationId &&
                 PhyiscalPortId.Usb3Id != usbDevice.LocationId)
            {
                return false;
            }

            return true;
        }
        catch (LibUsb.UsbException ex) when (ex.ErrorCode == Error.NotFound)
        {
            // The device has probably disconnected while we were inspecting it. Continue.
            return false;
        }
    }
    
    public virtual bool Check(CachedDeviceInfo info)
    {
        try
        {
            if (this.Vid != int.MaxValue &&
                this.Vid != info.Descriptor.VendorId)
            {
                return false;
            }

            if (this.Pid != int.MaxValue &&
                this.Pid != info.Descriptor.ProductId)
            {
                return false;
            }

            if (this.Revision != int.MaxValue && 
                this.Revision != info.Descriptor.Usb)
            {
                return false;
            }

            if (this.SerialNumber != null &&
                this.SerialNumber != info.Descriptor.SerialNumber)
            {
                return false;
            }

            if (LocationId != Info.LocationId.Zero &&
                LocationId != info.PortInfo)
            {
                return false;
            }
            
            if (PhyiscalPortId != null &&
                PhyiscalPortId.Usb2Id != info.PortInfo &&
                PhyiscalPortId.Usb3Id != info.PortInfo)
            {
                return false;
            }

            return true;
        }
        
        catch (LibUsb.UsbException ex) when (ex.ErrorCode == Error.NotFound)
        {
            // The device has probably disconnected while we were inspecting it. Continue.
            return false;
        }
    }

    protected bool Equals(UsbDeviceFinder other)
    {
        return Nullable.Equals(DeviceInterfaceGuid, other.DeviceInterfaceGuid) && SerialNumber == other.SerialNumber && Revision == other.Revision && Pid == other.Pid && Vid == other.Vid;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = DeviceInterfaceGuid.GetHashCode();
            hashCode = (hashCode * 397) ^ (SerialNumber != null ? SerialNumber.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Revision.GetHashCode();
            hashCode = (hashCode * 397) ^ Pid.GetHashCode();
            hashCode = (hashCode * 397) ^ Vid.GetHashCode();
            return hashCode;
        }
    }
}