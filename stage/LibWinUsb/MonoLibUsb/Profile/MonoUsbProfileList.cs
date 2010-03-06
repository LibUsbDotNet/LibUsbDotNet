// Copyright © 2006-2009 Travis Robinson. All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace MonoLibUsb.Profile
{
    /// <summary>
    /// Manages the device list.  This class is thread safe.
    /// </summary>
    public class MonoUsbProfileList:IEnumerable<MonoUsbProfile>
    {
        private List<MonoUsbProfile> mList = new List<MonoUsbProfile>();
        private object LockProfileList = new object();

        /// <summary>
        /// Gets a copy of the internal list of <see cref="MonoUsbProfile"/> classes 
        /// </summary>
        public List<MonoUsbProfile> List
        {
            get
            {
                lock (LockProfileList)
                {
                    return new List<MonoUsbProfile>(mList);
                }
            }
        }

        private static bool FindDiscoveredFn(MonoUsbProfile check) { return check.mDiscovered; }
        private static bool FindUnDiscoveredFn(MonoUsbProfile check) { return !check.mDiscovered; }

        private void FireAddRemove(MonoUsbProfile monoUSBProfile, AddRemoveType addRemoveType)
        {
            EventHandler<AddRemoveEventArgs> temp = AddRemoveEvent;
            if (!ReferenceEquals(temp, null))
            {
                temp(this, new AddRemoveEventArgs(monoUSBProfile, addRemoveType));
            }
        }

        private void SetDiscovered(bool discovered)
        {
            foreach (MonoUsbProfile deviceProfile in mList)
            {
                deviceProfile.mDiscovered = discovered;
            }
        }

        private void syncWith(MonoUsbProfileList newList)
        {
            SetDiscovered(false);
            newList.SetDiscovered(true);
            List<MonoUsbProfile> syncedList = new List<MonoUsbProfile>();

            int iNewProfiles = newList.mList.Count;
            for (int iNewProfile = 0; iNewProfile < iNewProfiles; iNewProfile++)
            {
                MonoUsbProfile newProfile = newList.mList[iNewProfile];
                int iFoundOldIndex;
                if ((iFoundOldIndex = mList.IndexOf(newProfile)) == -1)
                {
                    //Console.WriteLine("DeviceDiscovery: Added: {0}", newProfile.ProfileHandle.DangerousGetHandle());
                    newProfile.mDiscovered = true;
                    syncedList.Add(newProfile);
                    FireAddRemove(newProfile, AddRemoveType.Added);
                }
                else
                {
                    if (mList[iFoundOldIndex].ProfileHandle.DangerousGetHandle().ToInt64() == newProfile.ProfileHandle.DangerousGetHandle().ToInt64())
                    {
                        //MonoLibUsbApi.libusb_ref_device(mList[iFoundOldIndex].ProfileHandle.DangerousGetHandle());
                    }
                    //Console.WriteLine("DeviceDiscovery: Unchanged: Orig:{0} New:{1}", mList[iFoundOldIndex].ProfileHandle.DangerousGetHandle(), newProfile.ProfileHandle.DangerousGetHandle());
                    mList[iFoundOldIndex].mDiscovered = true;
                    newProfile.mDiscovered = false;
                    syncedList.Add(mList[iFoundOldIndex]);
                   
                }
            }

            newList.mList.RemoveAll(FindDiscoveredFn);
            newList.Free();

            foreach (MonoUsbProfile deviceProfile in mList)
            {
                if (!deviceProfile.mDiscovered)
                {
                    // Close Unplugged device profiles.
                    //Console.WriteLine("DeviceDiscovery: Removed: {0}", deviceProfile.ProfileHandle.DangerousGetHandle());
                    FireAddRemove(deviceProfile, AddRemoveType.Removed);
                    deviceProfile.Close();
                }
            }

            // Remove Unplugged device profiles.
            mList.RemoveAll(FindUnDiscoveredFn);
            mList = syncedList;
        }

        /// <summary>
        /// Refreshes the <see cref="MonoUsbProfile"/> <see cref="List"/>.  If an event handler was added to <see cref="AddRemoveEvent"/> changes in the device profile list will reported.
        /// </summary>
        /// <remarks>
        /// <para>This is your entry point into finding a USB device to operate.</para>
        /// <para>This return value of this function indicates the number of devices in the resultant <see cref="List"/>.</para>
        /// <para>The <see cref="MonoUsbProfileList"/> has a crude form of built-in device notification that works on all platforms. By adding an event handler to the <see cref="AddRemoveEvent"/> changes in the device profile list are reported when <see cref="Refresh"/> is called.</para>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>The number of devices in the outputted list, or <see cref="MonoUsbError.LIBUSB_ERROR_NO_MEM"></see> on memory allocation failure.</returns>
        /// <example>
        /// <code source="..\MonoLibUsb\MonoUsb.ShowInfo\ShowInfo.cs" lang="cs"/>
        /// </example>
        public int Refresh(MonoUsbSessionHandle sessionHandle)
        {
            lock (LockProfileList)
            {
                MonoUsbProfileList newList = new MonoUsbProfileList();
                MonoUsbProfileListHandle monoUSBProfileListHandle;

                int ret = MonoLibUsbApi.libusb_get_device_list(sessionHandle, out monoUSBProfileListHandle);
                if (ret < 0 || monoUSBProfileListHandle.IsInvalid)
                {
#if LIBUSBDOTNET
                    UsbError.Error(ErrorCode.MonoApiError, ret, "Refresh:libusb_get_device_list Failed", this);
#else
                    System.Diagnostics.Debug.Print("libusb_get_device_list failed:{0} {1}",
                                                   (MonoUsbError) ret,
                                                   MonoLibUsbApi.libusb_strerror((MonoUsbError) ret));
#endif
                    return ret;
                }
                int stopCount = ret;
                foreach (MonoUsbProfileHandle deviceProfileHandle in monoUSBProfileListHandle)
                {
                    newList.mList.Add(new MonoUsbProfile(deviceProfileHandle));
                    stopCount--;
                    if (stopCount <= 0) break;
                }
                syncWith(newList);
                monoUSBProfileListHandle.Close();

                return ret;
            }
        }

        /// <summary>
        /// Frees all unreferenced profiles contained in the list.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="MonoUsbProfileHandle"/>s that are in-use are never closed until all reference(s) have gone 
        /// out-of-scope or specifically been closed with the <see cref="SafeHandle.Close"/> method.
        /// </para>
        /// </remarks>
        public void Free()
        {
            lock (LockProfileList)
            {
                foreach (MonoUsbProfile profile in mList)
                    profile.Close();

                mList.Clear();
            }
        }

        /// <summary>
        /// Usb device arrival/removal notifaction handler. 
        /// This event only reports when the <see cref="Refresh"/> method is called.
        /// </summary>
        /// <remarks>
        /// <see cref="AddRemoveEvent"/> could be used for a crude form for receiving usb 
        /// device arrival/removal notification.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Startup code
        /// MonoUsbProfileList profileList = new MonoUsbProfileList();
        /// profileList.AddRemoveEvent += OnDeviceAddRemove;
        /// 
        /// // Device AddRemove event template
        /// private void OnDeviceAddRemove(object sender, AddRemoveEventArgs addRemoveArgs)
        /// {
        /// // This method will only report when Refresh() is called.
        /// }
        /// 
        /// // Refresh profile list.
        /// // Any devices added or removed since the last call to Refresh() will be returned
        /// // in the OnDeviceAddRemove method.
        /// profileList.Refresh();
        /// </code>
        /// </example>
        public event EventHandler<AddRemoveEventArgs> AddRemoveEvent;

        /// <summary>
        /// Gets the <see cref="List"/> property enumerator. Same as <see cref="List"/>.GetEnumerator()
        /// </summary>
        /// <returns>A <see cref="MonoUsbProfile"/> <see cref="IEnumerator"/>.</returns>
        public IEnumerator<MonoUsbProfile> GetEnumerator() { return mList.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific <see cref="MonoUsbProfile"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(MonoUsbProfile item) { return mList.Contains(item); }


        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return mList.Count; }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        public int IndexOf(MonoUsbProfile item)
        {
            return (mList.IndexOf(item));
        }

        /// <summary>
        /// Gets the <see cref="MonoUsbProfile"/> at the specified index.
        /// </summary>
        /// <returns>
        /// The <see cref="MonoUsbProfile"/> at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the <see cref="MonoUsbProfile"/> to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        public MonoUsbProfile this[int index]
        {
            get { return mList[index]; }
        }
    }
}