// SPDX-FileCopyrightText: 2006-2010 Travis Robinson
// SPDX-FileCopyrightText: 2011-2023 LibUsbDotNet contributors
// SPDX-License-Identifier: LGPL-2.0-or-later

using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System;

namespace LibUsbDotNet.LibUsb;

/// <summary>
/// Provides extension methods for the <see cref="IUsbDevice"/> interface.
/// </summary>
public static class UsbDeviceExtensions
{
    /// <summary>
    /// Opens an endpoint for reading
    /// </summary>
    /// <param name="usbDevice">The device to open the endpoint on.</param>
    /// <param name="endpointInfo">Endpoint information for read operations.</param>
    /// <returns>A <see cref="UsbEndpointReader"/> class ready for reading. If the specified endpoint is already been opened, the original <see cref="UsbEndpointReader"/> class is returned.</returns>
    public static UsbEndpointReader OpenEndpointReader(this IUsbDevice usbDevice, UsbEndpointInfo endpointInfo)
    {
        if (endpointInfo.EndpointDirection != EndpointDirection.In)
            throw new ArgumentException($"Endpoint {endpointInfo.EndpointAddress} is not a read endpoint.");
        return usbDevice.OpenEndpointReader((ReadEndpointID)endpointInfo.EndpointAddress, UsbEndpointReader.DefReadBufferSize, endpointInfo.EndpointType);
    }

    /// <summary>
    /// Opens an endpoint for writing
    /// </summary>
    /// <param name="usbDevice">The device to open the endpoint on.</param>
    /// <param name="endpointInfo">Endpoint information for write operations.</param>
    /// <returns>A <see cref="UsbEndpointWriter"/> class ready for writing. If the specified endpoint is already been opened, the original <see cref="UsbEndpointWriter"/> class is returned.</returns>
    public static UsbEndpointWriter OpenEndpointWriter(this IUsbDevice usbDevice, UsbEndpointInfo endpointInfo)
    {
        if (endpointInfo.EndpointDirection != EndpointDirection.Out)
            throw new ArgumentException($"Endpoint {endpointInfo.EndpointAddress} is not a write endpoint.");
        return usbDevice.OpenEndpointWriter((WriteEndpointID)endpointInfo.EndpointAddress, endpointInfo.EndpointType);
    }
}
