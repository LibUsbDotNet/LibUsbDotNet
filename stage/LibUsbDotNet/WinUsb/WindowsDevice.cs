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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Internal.WinUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb.Internal;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.WinUsb {
    /// <summary> 
    /// Contains members specific to Microsofts WinUSB driver or libusbK driver.
    /// </summary> 
    /// <remarks>
    /// A <see cref="WindowsDevice"/> should be thought of as a part of, or an interface of a USB device.
    /// The <see cref="WindowsDevice"/> class does not have members for selecting configurations and
    /// intefaces.  This is done at a lower level by the winusb driver depending on which interface the
    /// <see cref="WindowsDevice"/> belongs to.
    /// </remarks> 
    public abstract class WindowsDevice : UsbDevice, IUsbInterface {

        protected readonly string mDevicePath;
        protected PowerPolicies mPowerPolicies;
        protected SafeFileHandle mSafeDevHandle;

        internal WindowsDevice(UsbApiBase usbApi,
                              SafeFileHandle usbHandle,
                              SafeHandle handle,
                              string devicePath)
            : base(usbApi, handle) {
            mDevicePath = devicePath;
            mSafeDevHandle = usbHandle;
            mPowerPolicies = new PowerPolicies(this);
        }

        /// <summary>
        /// Gets the power policies for this <see cref="WinUsbDevice"/>.
        /// </summary>
        public PowerPolicies PowerPolicy {
            get { return mPowerPolicies; }
        }

        /// <summary>
        /// Gets the device path used to open this <see cref="WinUsbDevice"/>.
        /// </summary>
        public override string DevicePath {
            get { return mDevicePath; }
        }

        #region IUsbInterface Members

        /// <summary>
        /// Returns the DriverMode this USB device is using.
        /// </summary>
        public override DriverModeType DriverMode {
            get { return DriverModeType.Windows; }
        }

        /// <summary>
        /// Closes the <see cref="UsbDevice"/> and disposes any <see cref="UsbDevice.ActiveEndpoints"/>.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Close() {
            if (IsOpen) {
                ActiveEndpoints.Clear();
                mUsbHandle.Dispose();

                if (mSafeDevHandle != null)
                    if (!mSafeDevHandle.IsClosed)
                        mSafeDevHandle.Dispose();
            }
            return true;
        }

        /// <summary>
        /// sets the alternate interface number for the previously claimed interface. <see cref="IUsbDevice.ClaimInterface"/>
        /// </summary>
        /// <param name="alternateID">The alternate interface number.</param>
        /// <returns>True on success.</returns>
        public abstract bool SetAltInterface(int alternateID);

        /// <summary>
        /// Gets the alternate interface number for the previously claimed interface. <see cref="IUsbDevice.ClaimInterface"/>
        /// </summary>
        /// <param name="alternateID">The alternate interface number.</param>
        /// <returns>True on success.</returns>
        public abstract bool GetAltInterface(out int alternateID);

        #endregion

        /// <summary>
        /// Gets endpoint policies for the specified endpoint id.
        /// </summary>
        /// <param name="epNum">The endpoint ID to retrieve <see cref="PipePolicies"/> for.</param>
        /// <returns>A <see cref="PipePolicies"/> class.</returns>
        public PipePolicies EndpointPolicies(ReadEndpointID epNum) { return new PipePolicies(mUsbHandle, (byte)epNum); }

        /// <summary>
        /// Gets endpoint policies for the specified endpoint id.
        /// </summary>
        /// <param name="epNum">The endpoint ID to retrieve <see cref="PipePolicies"/> for.</param>
        /// <returns>A <see cref="PipePolicies"/> class.</returns>
        public PipePolicies EndpointPolicies(WriteEndpointID epNum) { return new PipePolicies(mUsbHandle, (byte)epNum); }

        /// <summary>
        /// Gets an interface associated with this <see cref="WindowsDevice"/>.
        /// </summary>
        /// <param name="associatedInterfaceIndex">The index to retrieve. (0 = next interface, 1= interface after next, etc.).</param>
        /// <param name="usbDevice">A new <see cref="WindowsDevice"/> class for the specified AssociatedInterfaceIndex.</param>
        /// <returns>True on success.</returns>
        public abstract bool GetAssociatedInterface(byte associatedInterfaceIndex, out WindowsDevice usbDevice);

        /// <summary>
        /// Gets the device speed.
        /// </summary>
        /// <param name="deviceSpeed">The device speed.</param>
        /// <returns>True on success.</returns>
        public abstract bool QueryDeviceSpeed(out DeviceSpeedTypes deviceSpeed);

        /// <summary>
        /// Gets a <see cref="UsbInterfaceDescriptor"/> for the specified AlternateInterfaceNumber,
        /// </summary>
        /// <param name="alternateInterfaceNumber">The alternate interface index for the <see cref="UsbInterfaceDescriptor"/> to retrieve. </param>
        /// <param name="usbAltInterfaceDescriptor">The <see cref="UsbInterfaceDescriptor"/> for the specified AlternateInterfaceNumber.</param>
        /// <returns>True on success.</returns>
        public abstract bool QueryInterfaceSettings(byte alternateInterfaceNumber, ref UsbInterfaceDescriptor usbAltInterfaceDescriptor);

        internal abstract bool GetPowerPolicy(PowerPolicyType policyType, ref int valueLength, IntPtr pBuffer);

        internal abstract bool SetPowerPolicy(PowerPolicyType policyType, int valueLength, IntPtr pBuffer);

        /// <summary>
        /// Closes the device. <see cref="WindowsDevice.Close"/>.
        /// </summary>
        ~WindowsDevice() { Close(); }
    }
}