using System;
using System.Collections.ObjectModel;
using System.Linq;
using LibUsbDotNet.LibUsb;

namespace LibUsbDotNet.Info;

/// <summary>
/// Value representing a combination of a <see cref="UsbDevice.BusNumber"/> and <see cref="UsbDevice.PortNumbers"/>
/// tree.
/// </summary>
public readonly struct LocationId
{
    public LocationId(byte busNumber, ReadOnlyCollection<byte> portNumbers)
    {
        BusNumber = busNumber;
        PortNumbers = portNumbers;
    }

    public byte BusNumber { get; }
    public ReadOnlyCollection<byte> PortNumbers { get; }

    public static readonly LocationId Zero = new(0, new ReadOnlyCollection<byte>(Array.Empty<byte>()));
    
    public static bool operator ==(LocationId locationId1, LocationId locationId2)
        => Equals(locationId1, locationId2);

    public static bool operator !=(LocationId locationId1, LocationId locationId2) 
        => !(locationId1 == locationId2);

    public override bool Equals(object obj)
    {
        if (obj is not LocationId locationId)
            return false;

        return BusNumber == locationId.BusNumber && PortNumbers.SequenceEqual(locationId.PortNumbers);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (BusNumber.GetHashCode() * 397) ^ (PortNumbers != null ? PortNumbers.GetHashCode() : 0);
        }
    }

    public override string ToString()
    {
        string result = BusNumber.ToString();
        
        for (int i = 0; i < PortNumbers.Count; i++)
        {
            if (i == 0)
                result += '-';
            result += PortNumbers[i].ToString();
            if (i != PortNumbers.Count - 1)
                result += '.';
        }

        return result;
    }
}