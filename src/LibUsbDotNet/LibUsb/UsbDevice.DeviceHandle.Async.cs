using LibUsbDotNet.Main;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb;

public partial class UsbDevice
{
    public event EventHandler<UsbTransferEventArgs> TransferCompleted;

    private readonly ConcurrentDictionary<int, (nint Transfer, MemoryHandle MemoryHandle)> ActiveTransfers = new();
    private readonly object TransferCompletionLock = new();

    private readonly TransferDelegate TransferCompletionDelegate;
    private readonly IntPtr TransferDelegatePtr;

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

        (Error error, int dataTransferred) = await AsyncTransfer.TransferAsync(deviceHandle, 0, EndpointType.Control, data.AsMemory(), ControlTransferTimeout).ConfigureAwait(false);

        error.ThrowOnError();

        if (buffer is not null && (setupPacket.RequestType & (byte)UsbCtrlFlags.Direction_In) != 0)
        {
            Array.Copy(data, 8, buffer, 0, dataTransferred);
        }

        return dataTransferred;
    }

    /// <inheritdoc/>
    public Task<int> ControlTransferAsync(UsbSetupPacket setupPacket) =>
        ControlTransferAsync(setupPacket, null, 0, 0);

    public unsafe UsbTransferToken TransferAsync(byte endpoint, EndpointType endpointType, Memory<byte> buffer, int timeout, int isoPacketSize = 0)
    {
        var memoryHandle = buffer.Pin();

        var transfer = AsyncTransfer.Allocate(deviceHandle.DangerousGetHandle(), endpoint, (uint)timeout, endpointType,
                                                (byte*)memoryHandle.Pointer, buffer.Length, TransferDelegatePtr, out var transferId, isoPacketSize);

        if (!ActiveTransfers.TryAdd(transferId, ((nint)transfer, memoryHandle)))
            throw new InvalidOperationException($"Transfer id # {transferId} already exists in {nameof(ActiveTransfers)}.");

        var error = NativeMethods.SubmitTransfer(transfer);

        if (error != Error.Success)
        {
            memoryHandle.Dispose();
            error.ThrowOnError();
        }

        return new UsbTransferToken(transferId);
    }

    public unsafe Error CancelTransfer(UsbTransferToken token)
    {
        lock (TransferCompletionLock)
        {
            if (ActiveTransfers.TryGetValue(token.TransferId, out var transferData))
                return NativeMethods.CancelTransfer((Transfer*)transferData.Transfer);
            else
                return Error.NotFound;
        }
    }

    private unsafe void TransferCompletionCallback(Transfer* transfer)
    {
        lock (TransferCompletionLock)
        {
            int transferId = transfer->UserData.ToInt32();

            if (ActiveTransfers.TryRemove(transferId, out var transferData))
            {
                var args = new UsbTransferEventArgs(new UsbTransferToken(transferId), transfer->Status, transfer->ActualLength);
                transferData.MemoryHandle.Dispose();
                TransferCompleted?.Invoke(null, args);
            }
            else
            {
                throw new InvalidOperationException($"Can't find transfer id # {transferId} in {nameof(ActiveTransfers)}.");
            }
            NativeMethods.FreeTransfer(transfer);
        }
    }
}