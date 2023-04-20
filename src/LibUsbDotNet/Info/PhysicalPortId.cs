using LibUsbDotNet.LibUsb;

namespace LibUsbDotNet.Info;

/// <summary>
/// An ID for a physical USB port.
/// </summary>
public sealed class PhysicalPortId
{
    public PhysicalPortId(LocationId usb2Id, LocationId usb3Id)
    {
        Usb2Id = usb2Id;
        Usb3Id = usb3Id;
    }
    
    public static PhysicalPortId FromDevice(UsbDevice device)
    {
        var usbId = device.LocationId;
        var speed = device.Speed;
        
        var usb2Id = speed > Speed.High ? new LocationId((byte)(usbId.BusNumber - 1), usbId.PortNumbers) : usbId;
        var usb3Id = speed > Speed.High ? usbId : new LocationId((byte)(usbId.BusNumber + 1), usbId.PortNumbers);

        return new PhysicalPortId(usb2Id, usb3Id);
    }

    /// <summary>
    /// The <see cref="LocationId"/> for USB 2.0 devices.
    /// </summary>
    public LocationId Usb2Id { get; }
    
    /// <summary>
    /// The <see cref="LocationId"/> for USB 3.0 devices.
    /// </summary>
    public LocationId Usb3Id { get; }
    
    public bool HasDevice(UsbDevice device) => device.LocationId == Usb2Id || device.LocationId == Usb3Id;

    public static bool operator ==(PhysicalPortId physicalPortId1, PhysicalPortId physicalPortId2)
        => Equals(physicalPortId1, physicalPortId2);

    public static bool operator !=(PhysicalPortId physicalPortId1, PhysicalPortId physicalPortId2) 
        => !(physicalPortId1 == physicalPortId2);
    
    public override bool Equals(object obj)
    {
        if (obj is not PhysicalPortId physicalPortId)
            return false;

        return Usb2Id == physicalPortId.Usb2Id && Usb3Id == physicalPortId.Usb3Id;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Usb2Id.GetHashCode() * 397) ^ Usb3Id.GetHashCode();
        }
    }

    public override string ToString() =>
        $"USB 2.0: {Usb2Id}\n" +
        $"USB 3.0: {Usb3Id}";
}