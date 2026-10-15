using LibUsbDotNet.Main;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb;

public partial class UsbDevice
{
    private readonly ConcurrentDictionary<int, TransferCallbackCompletion> ActiveTransfers = new();

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

    public unsafe Task<(Error error, int transferLength)> TransferAsync(byte endpoint, EndpointType endpointType, Memory<byte> buffer, int timeout, CancellationToken cancellationToken, int isoPacketSize = 0)
    {
        var memoryHandle = buffer.Pin();
        var transferCompletion = new TaskCompletionSource<(Error error, int transferLength)>();

        var transfer = AsyncTransfer.Allocate(deviceHandle.DangerousGetHandle(), endpoint, (uint)timeout, endpointType,
                                                (byte*)memoryHandle.Pointer, buffer.Length, TransferDelegatePtr, out var transferId, isoPacketSize);

        if (!ActiveTransfers.TryAdd(transferId, new TransferCallbackCompletion(transferCompletion, memoryHandle)))
            throw new InvalidOperationException($"Transfer id # {transferId} already exists in {nameof(ActiveTransfers)}.");

        var error = NativeMethods.SubmitTransfer(transfer);

        if (error != Error.Success)
        {
            memoryHandle.Dispose();
            error.ThrowOnError();
        }

        cancellationToken.Register(() => CancelTransfer(transfer));
        return transferCompletion.Task;
    }

    private static unsafe void CancelTransfer(Transfer* transfer)
    {
        var error = NativeMethods.CancelTransfer(transfer);
        error.ThrowOnError();
    }

    private unsafe void TransferCompletionCallback(Transfer* transfer)
    {
        int transferId = transfer->UserData.ToInt32();

        if (ActiveTransfers.TryRemove(transferId, out var transferCompletion))
        {
            var couldSetFinalState = transfer->Status == TransferStatus.Cancelled ?
                transferCompletion.TaskCompletionSource.TrySetCanceled() :
                transferCompletion.TaskCompletionSource.TrySetResult((transfer->Status.ToError(), transfer->ActualLength));

            if (!couldSetFinalState)
                throw new InvalidOperationException($"Unable to complete transfer with id # {transferId} and status {transfer->Status}.");

            transferCompletion.Dispose();
            NativeMethods.FreeTransfer(transfer);
        }
        else
        {
            throw new InvalidOperationException($"Can't find transfer id # {transferId} in {nameof(ActiveTransfers)}.");
        }
    }
}