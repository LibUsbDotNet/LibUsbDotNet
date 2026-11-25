using System;

namespace LibUsbDotNet.LibUsb;

public class UsbTransferEventArgs(UsbTransferToken transferToken, TransferStatus transferStatus, int transferLength) : EventArgs
{
    public UsbTransferToken TransferToken { get; } = transferToken;

    public TransferStatus TransferStatus { get; } = transferStatus;

    public int TransferLength { get; } = transferLength;
}
