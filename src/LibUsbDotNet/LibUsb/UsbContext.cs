// Copyright © 2006-2010 Travis Robinson. All rights reserved.
//
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace LibUsbDotNet.LibUsb
{
    /// <summary>
    /// An instance of the libusb API. You can use multiple <see cref="UsbContext"/> which are independent
    /// from each other.
    /// </summary>
    public class UsbContext : IUsbContext
    {
        /// <summary>
        /// The native context.
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// Tracks whether this context has been disposed of, or not.
        /// </summary>
        private bool disposed = false;

        private Thread eventHandlingThread;
        private bool shouldHandleEvents = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbContext"/> class.
        /// </summary>
        public UsbContext()
        {
            IntPtr contextHandle = IntPtr.Zero;
            NativeMethods.Init(ref contextHandle).ThrowOnError();
            this.context = Context.DangerousCreate(contextHandle);
        }

        ~UsbContext()
        {
            // Put cleanup code in Dispose(bool disposing).
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void SetDebugLevel(LogLevel level)
        {
            NativeMethods.SetDebug(this.context, (int)level);
        }

        /// <summary>
        /// Returns a list of USB devices currently attached to the system.
        /// </summary>
        /// <returns>
        /// A <see cref="UsbDeviceCollection"/> which contains the devices currently
        /// attached to the system.</returns>
        /// <remarks>
        /// <para>
        /// This is your entry point into finding a USB device to operate.
        /// </para>
        /// <para>
        /// You are expected to dispose all the devices once you are done with them. Disposing the <see cref="UsbDeviceCollection"/>
        /// will dispose all devices in that collection. You can <see cref="UsbDevice.Clone"/> a device to get a copy of the device
        /// which you can use after you've disposed the <see cref="UsbDeviceCollection"/>.
        /// </para>
        /// </remarks>
        public unsafe UsbDeviceCollection List()
        {
            IntPtr* list;
            var deviceCount = NativeMethods.GetDeviceList(this.context, &list);

            Collection<IUsbDevice> devices = new Collection<IUsbDevice>();

            for (int i = 0; i < deviceCount.ToInt32(); i++)
            {
                Device device = Device.DangerousCreate(list[i]);
                devices.Add(new UsbDevice(device));
            }

            NativeMethods.FreeDeviceList(list, unrefDevices: 0 /* Do not unreference the devices */);

            return new UsbDeviceCollection(devices);
        }

        /// <inheritdoc/>
        public IUsbDevice Find(UsbDeviceFinder finder)
        {
            if (finder == null)
            {
                throw new ArgumentNullException(nameof(finder));
            }

            return this.Find(finder.Check);
        }

        /// <inheritdoc/>
        public IUsbDevice Find(Func<IUsbDevice, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            using (var list = this.List())
            {
                foreach (var device in list)
                {
                    if (predicate(device))
                    {
                        return device.Clone();
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public UsbDeviceCollection FindAll(UsbDeviceFinder finder)
        {
            if (finder == null)
            {
                throw new ArgumentNullException(nameof(finder));
            }

            return this.FindAll(finder.Check);
        }

        /// <inheritdoc/>
        public UsbDeviceCollection FindAll(Func<IUsbDevice, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var matchingDevices = new List<IUsbDevice>();

            using (var list = this.List())
            {
                foreach (var device in list)
                {
                    if (predicate(device))
                    {
                        matchingDevices.Add(device.Clone());
                    }
                }
            }

            UsbDeviceCollection devices = new UsbDeviceCollection(matchingDevices);
            return devices;
        }

        public void StartHandlingEvents()
        {
            if (this.eventHandlingThread == null)
            {
                this.eventHandlingThread = new Thread(this.HandleEvents);
                this.shouldHandleEvents = true;
                this.eventHandlingThread.Start();
            }
        }

        public void StopHandlingEvents()
        {
            if (this.eventHandlingThread != null)
            {
                this.shouldHandleEvents = false;
                this.eventHandlingThread.Join();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                this.disposed = true;
            }
        }

        private void HandleEvents()
        {
            while (this.shouldHandleEvents)
            {
                int completed = this.shouldHandleEvents ? 0 : 1;
                NativeMethods.HandleEventsCompleted(this.context, ref completed).ThrowOnError();
            }
        }
    }
}
