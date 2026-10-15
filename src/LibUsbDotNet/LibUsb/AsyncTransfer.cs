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

using LibUsbDotNet.Main;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb;

/// <summary>
/// Handles submission and awaiting of asynchronous transfers.
/// </summary>
internal static class AsyncTransfer
{
    private class TransferCallbackCompletion : IDisposable
    {
        private MemoryHandle _memoryHandle;
        public TaskCompletionSource<(Error error, int transferLength)> TaskCompletionSource { get; }

        public TransferCallbackCompletion(
            TaskCompletionSource<(Error error, int transferLength)> taskCompletionSource, MemoryHandle memoryHandle)
        {
            TaskCompletionSource = taskCompletionSource;
            _memoryHandle = memoryHandle;
        }

        public void Dispose() => _memoryHandle.Dispose();
    }

    private static readonly object TransferLock = new object();
    private static int _transferIndex;

    private static readonly unsafe TransferDelegate TransferCallback = new TransferDelegate(Callback);
    private static readonly IntPtr TransferDelegatePtr =
        Marshal.GetFunctionPointerForDelegate(TransferCallback);
    private static readonly ConcurrentDictionary<int, TransferCallbackCompletion>
        TransferDictionary = new();

    public static unsafe Task<(Error error, int transferLength)> TransferAsync(
        DeviceHandle device,
        byte endPoint,
        EndpointType endPointType,
        Memory<byte> buffer,
        int timeout,
        int isoPacketSize = 0)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        var memoryHandle = buffer.Pin();
        var transferCompletion =
            new TaskCompletionSource<(Error error, int transferLength)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transferCallbackCompletion = new TransferCallbackCompletion(transferCompletion, memoryHandle);

        var transfer = Allocate(device.DangerousGetHandle(), endPoint, (uint)timeout, endPointType,
                                     (byte*)memoryHandle.Pointer, buffer.Length, TransferDelegatePtr, out var transferId, isoPacketSize);

        if (!TransferDictionary.TryAdd(transferId, transferCallbackCompletion))
            throw new InvalidOperationException(
                $"{transferId} already exists in {nameof(TransferDictionary)}");

        var error = NativeMethods.SubmitTransfer(transfer);

        if (error != Error.Success)
        {
            transferCallbackCompletion.Dispose();
            error.ThrowOnError();
        }

        return transferCompletion.Task;
    }

    /// <summary>
    /// Allocates a new transfer.
    /// </summary>
    /// <returns>A pointer to the new transfer.</returns>
    public static unsafe Transfer* Allocate(
        IntPtr devHandle,
        byte endpoint,
        uint timeout,
        EndpointType endpointType,
        byte* buffer,
        int bufferLen,
        IntPtr callback,
        out int transferId,
        int isoPacketSize = 0)
    {
        transferId = GetNextTransferId();

        // Determine the amount of iso-synchronous packets
        int numIsoPackets = 0;

        if (isoPacketSize > 0)
            numIsoPackets = bufferLen / isoPacketSize;

        var transfer = NativeMethods.AllocTransfer(numIsoPackets); // TODO: Check if transfer is null.

        // Fill common properties
        transfer->DevHandle = devHandle;
        transfer->Endpoint = endpoint;
        transfer->Timeout = timeout;
        transfer->Type = (byte)endpointType;
        transfer->Buffer = buffer;
        transfer->Length = bufferLen;
        transfer->NumIsoPackets = numIsoPackets;
        transfer->Flags = (byte)TransferFlags.None;
        transfer->Callback = callback;
        transfer->UserData = new IntPtr(transferId);

        return transfer;
    }

    private static int GetNextTransferId()
    {
        int transferId;
        lock (TransferLock)
        {
            if (_transferIndex == int.MaxValue) // Potential edge case for long-running application?
                _transferIndex = 0;
            transferId = _transferIndex++;
        }
        return transferId;
    }

    private static unsafe void Callback(Transfer* transfer)
    {
        int transferId = transfer->UserData.ToInt32();
        if (TransferDictionary.TryRemove(transferId, out var transferCompletion))
        {
            transferCompletion.TaskCompletionSource.TrySetResult((transfer->Status.ToError(), transfer->ActualLength));
            transferCompletion.Dispose();
        }
        else
        {
            throw new InvalidOperationException(
                $"Can't find transfer id # {transferId} in {nameof(TransferDictionary)}");
        }
        NativeMethods.FreeTransfer(transfer);
    }
}