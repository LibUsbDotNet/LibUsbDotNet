using System;

namespace LibUsbDotNet.LibUsb;

public readonly struct UsbTransferToken : IEquatable<UsbTransferToken>
{
    internal int TransferId { get; }

    internal UsbTransferToken(int transferId)
    {
        TransferId = transferId;
    }

    public override bool Equals(object obj) => obj is UsbTransferToken other && Equals(other);

    public bool Equals(UsbTransferToken other) => this.TransferId == other.TransferId;

    public override int GetHashCode() => TransferId.GetHashCode();

    public static bool operator ==(UsbTransferToken a, UsbTransferToken b) => a.Equals(b);

    public static bool operator !=(UsbTransferToken a, UsbTransferToken b) => !a.Equals(b);
}
