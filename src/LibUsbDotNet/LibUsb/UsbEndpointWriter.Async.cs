using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LibUsbDotNet.LibUsb;

public partial class UsbEndpointWriter
{
    /// <summary>
    /// Writes data asynchronously to the current <see cref="UsbEndpointWriter"/>.
    /// </summary>
    /// <param name="buffer">The buffer storing the data to write.</param>
    /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
    /// <returns>
    /// Tuple of (<see cref="Error"/> error, <see cref="int"/> transferLength). error is <see cref="Error.Success"/> on success.
    /// </returns>
    public virtual async Task<(Error error, int transferLength)> WriteAsync(byte[] buffer, int timeout) => 
        await this.WriteAsync(buffer, 0, buffer.Length, timeout).ConfigureAwait(false);

    /// <summary>
    /// Writes data asynchronously to the current <see cref="UsbEndpointWriter"/>.
    /// </summary>
    /// <param name="pBuffer">The buffer storing the data to write.</param>
    /// <param name="offset">The position in buffer to start writing the data from.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
    /// <returns>
    /// Tuple of (<see cref="Error"/> error, <see cref="int"/> transferLength). error is <see cref="Error.Success"/> on success.
    /// </returns>
    public virtual async Task<(Error error, int transferLength)> WriteAsync(IntPtr pBuffer, int offset, int count, int timeout) => 
        await this.TransferAsync(pBuffer, offset, count, timeout).ConfigureAwait(false);

    /// <summary>
    /// Writes data asynchronously to the current <see cref="UsbEndpointWriter"/>.
    /// </summary>
    /// <param name="buffer">The buffer storing the data to write.</param>
    /// <param name="offset">The position in buffer to start writing the data from.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
    /// <returns>
    /// Tuple of (<see cref="Error"/> error, <see cref="int"/> transferLength). error is <see cref="Error.Success"/> on success.
    /// </returns>
    public virtual async Task<(Error error, int transferLength)> WriteAsync(byte[] buffer, int offset, int count, int timeout) => 
        await this.TransferAsync(buffer, offset, count, timeout).ConfigureAwait(false);

    /// <summary>
    /// Writes data asynchronously to the current <see cref="UsbEndpointWriter"/>.
    /// </summary>
    /// <param name="buffer">The buffer storing the data to write.</param>
    /// <param name="offset">The position in buffer to start writing the data from.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
    /// <returns>
    /// Tuple of (<see cref="Error"/> error, <see cref="int"/> transferLength). error is <see cref="Error.Success"/> on success.
    /// </returns>
    public virtual async Task<(Error error, int transferLength)> WriteAsync(object buffer, int offset, int count, int timeout) => 
        await this.TransferAsync(buffer, offset, count, timeout).ConfigureAwait(false);

    /// <summary>
    /// Writes data asynchronously to the current <see cref="UsbEndpointWriter"/>.
    /// </summary>
    /// <param name="buffer">The buffer storing the data to write.</param>
    /// <param name="timeout">Maximum time to wait for the transfer to complete.  If the transfer times out, the IO operation will be cancelled.</param>
    /// <returns>
    /// Tuple of (<see cref="Error"/> error, <see cref="int"/> transferLength). error is <see cref="Error.Success"/> on success.
    /// </returns>
    public virtual async Task<(Error error, int transferLength)> WriteAsync(object buffer, int timeout) => 
        await this.WriteAsync(buffer, 0, Marshal.SizeOf(buffer), timeout).ConfigureAwait(false);
}