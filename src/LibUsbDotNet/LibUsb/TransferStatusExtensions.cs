namespace LibUsbDotNet.LibUsb;

public static class TransferStatusExtensions
{
    extension(TransferStatus status)
    {
        public Error ToError() => status switch
        {
            TransferStatus.Completed => Error.Success,
            TransferStatus.TimedOut => Error.Timeout,
            TransferStatus.Stall => Error.Pipe,
            TransferStatus.Overflow => Error.Overflow,
            TransferStatus.NoDevice => Error.NoDevice,
            TransferStatus.Error or TransferStatus.Cancelled => Error.Io,
            _ => Error.Other,
        };
    }
}