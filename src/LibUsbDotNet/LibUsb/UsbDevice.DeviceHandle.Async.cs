using System;
using System.Threading.Tasks;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.LibUsb;

public partial class UsbDevice
{
    /// <inheritdoc/>
    public async Task<int> ControlTransferAsync(UsbSetupPacket setupPacket, byte[] buffer, int offset, int length)
    {
        this.EnsureNotDisposed();
        this.EnsureOpen();

        byte[] data;
        if (buffer == null)
        {
            data = new byte[8];
        }
        else
        {
            data = new byte[buffer.Length + 8];
            if ((setupPacket.RequestType & (byte)UsbCtrlFlags.Direction_In) == 0)
            {
                Array.Copy(buffer, 0, data, 8, length);
            }
        }

        data[0] = setupPacket.RequestType;
        data[1] = setupPacket.Request;
        Array.Copy(BitConverter.GetBytes(setupPacket.Value), 0, data, 2, 2);
        Array.Copy(BitConverter.GetBytes(setupPacket.Index), 0, data, 4, 2);
        Array.Copy(BitConverter.GetBytes(setupPacket.Length), 0, data, 6, 2);

        (Error error, int dataTransferred) = await AsyncTransfer.TransferAsync(deviceHandle, 0, EndpointType.Control, data, 0, data.Length, ControlTransferTimeout).ConfigureAwait(false);

        error.ThrowOnError();

        if (buffer is not null && (setupPacket.RequestType & (byte)UsbCtrlFlags.Direction_In) != 0) {
            Array.Copy(data, 8, buffer, 0, dataTransferred);
        }

        return dataTransferred;
    }
        
    /// <inheritdoc/>
    public Task<int> ControlTransferAsync(UsbSetupPacket setupPacket) => 
        ControlTransferAsync(setupPacket, null, 0, 0);
}