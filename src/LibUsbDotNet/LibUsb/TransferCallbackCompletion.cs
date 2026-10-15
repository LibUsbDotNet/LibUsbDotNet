using System;
using System.Buffers;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb;

internal class TransferCallbackCompletion(
    TaskCompletionSource<(Error error, int transferLength)> taskCompletionSource, MemoryHandle memoryHandle) : IDisposable
{
    public TaskCompletionSource<(Error error, int transferLength)> TaskCompletionSource { get; } = taskCompletionSource;

    public void Dispose() => memoryHandle.Dispose();
}
