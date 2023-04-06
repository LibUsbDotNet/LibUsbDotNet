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
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb;

/// <summary>
/// Handles submission and awaiting of asynchronous transfers.
/// </summary>
internal static class AsyncTransfer
{
    private static readonly object TransferLock = new object();
    private static int _transferIndex;

    private static readonly unsafe IntPtr TransferDelegatePtr =
        Marshal.GetFunctionPointerForDelegate(new TransferDelegate(Callback));
    private static readonly ConcurrentDictionary<int, TaskCompletionSource<(Error error, int transferLength)>>
        TransferDictionary = new ConcurrentDictionary<int, TaskCompletionSource<(Error error, int transferLength)>>();

    public static Task<(Error error, int transferLength)> TransferAsync(
        DeviceHandle device,
        byte endPoint,
        EndpointType endPointType,
        IntPtr buffer,
        int offset,
        int length,
        int timeout) =>
        TransferAsync(device, endPoint, endPointType, buffer, offset, length, timeout, 0);

    public static async Task<(Error error, int transferLength)> TransferAsync(
        DeviceHandle device,
        byte endPoint,
        EndpointType endPointType,
        IntPtr buffer,
        int offset,
        int length,
        int timeout,
        int isoPacketSize)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));

        var transferCompletion =
            new TaskCompletionSource<(Error error, int transferLength)>(TaskCreationOptions.RunContinuationsAsynchronously);
            
        int transferId;
        lock (TransferLock)
        {
            if (_transferIndex == int.MaxValue) // Potential edge case for long-running application?
                _transferIndex = 0;
            transferId = _transferIndex++;
        }

        if (!TransferDictionary.TryAdd(transferId, transferCompletion))
            throw new InvalidOperationException(
                $"{transferId} already exists in {nameof(TransferDictionary)}");

        FillAndSubmitTransfer(device, endPoint, endPointType, buffer, offset, length, timeout, isoPacketSize,
            transferId);

        return await transferCompletion.Task.ConfigureAwait(false);
    }

    private static unsafe void FillAndSubmitTransfer(
        DeviceHandle device,
        byte endPoint,
        EndpointType endPointType,
        IntPtr buffer,
        int offset,
        int length,
        int timeout,
        int isoPacketSize,
        int transferId)
    {
        // Determine the amount of iso-synchronous packets
        int numIsoPackets = 0;

        if (isoPacketSize > 0)
            numIsoPackets = length / isoPacketSize;

        var transfer = NativeMethods.AllocTransfer(numIsoPackets);

        // Fill common properties
        transfer->DevHandle = device.DangerousGetHandle();
        transfer->Endpoint = endPoint;
        transfer->Timeout = (uint)timeout;
        transfer->Type = (byte)endPointType;
        transfer->Buffer = (byte*)buffer + offset;
        transfer->Length = length;
        transfer->NumIsoPackets = numIsoPackets;
        transfer->Flags = (byte)TransferFlags.None;
        transfer->Callback = TransferDelegatePtr;
        transfer->UserData = new IntPtr(transferId);

        NativeMethods.SubmitTransfer(transfer).ThrowOnError();
    }

    private static unsafe void Callback(Transfer* transfer)
    {
        int transferId = transfer->UserData.ToInt32();
        if (TransferDictionary.TryRemove(transferId, out var transferCompletion))
            transferCompletion.TrySetResult((GetErrorFromTransferStatus(transfer->Status), transfer->ActualLength));
        else
            throw new InvalidOperationException(
                $"Can't find transfer id # {transferId} in {nameof(TransferDictionary)}");
        NativeMethods.FreeTransfer(transfer);
    }

    private static Error GetErrorFromTransferStatus(TransferStatus status)
    {
        Error ret;

        switch (status)
        {
            case TransferStatus.Completed:
                ret = Error.Success;
                break;

            case TransferStatus.TimedOut:
                ret = Error.Timeout;
                break;

            case TransferStatus.Stall:
                ret = Error.Pipe;
                break;

            case TransferStatus.Overflow:
                ret = Error.Overflow;
                break;

            case TransferStatus.NoDevice:
                ret = Error.NoDevice;
                break;

            case TransferStatus.Error:
            case TransferStatus.Cancelled:
                ret = Error.Io;
                break;

            default:
                ret = Error.Other;
                break;
        }

        return ret;
    }
}