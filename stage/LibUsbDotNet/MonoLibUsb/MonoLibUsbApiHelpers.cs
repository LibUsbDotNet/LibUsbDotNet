using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Main;
using MonoLibUsb.Profile;
using MonoLibUsb.Transfer;

namespace MonoLibUsb
{
    public static partial class MonoUsbApi
    {
        internal const int LIBUSB_PACK = 0;

        #region Private Members

        private static readonly MonoUsbTransferDelegate DefaultAsyncDelegate = DefaultAsyncCB;
        private static void DefaultAsyncCB(MonoUsbTransfer transfer)
        {
            ManualResetEvent completeEvent = GCHandle.FromIntPtr(transfer.PtrUserData).Target as ManualResetEvent;
            completeEvent.Set();
        }

        #endregion

        #region API LIBRARY FUNCTIONS - Error Handling
        /// <summary>
        /// Get a string describing a <see cref="MonoUsbError"/>.
        /// </summary>
        /// <param name="errcode">The <see cref="MonoUsbError"/> code to retrieve a description for.</param>
        /// <returns>A string describing the <see cref="MonoUsbError"/> code.</returns>
        public static string StrError(MonoUsbError errcode)
        {
            switch (errcode)
            {
                case MonoUsbError.Success:
                    return "Success";
                case MonoUsbError.ErrorIO:
                    return "Input/output error";
                case MonoUsbError.ErrorInvalidParam:
                    return "Invalid parameter";
                case MonoUsbError.ErrorAccess:
                    return "Access denied (insufficient permissions)";
                case MonoUsbError.ErrorNoDevice:
                    return "No such device (it may have been disconnected)";
                case MonoUsbError.ErrorBusy:
                    return "Resource busy";
                case MonoUsbError.ErrorTimeout:
                    return "Operation timed out";
                case MonoUsbError.ErrorOverflow:
                    return "Overflow";
                case MonoUsbError.ErrorPipe:
                    return "Pipe error or endpoint halted";
                case MonoUsbError.ErrorInterrupted:
                    return "System call interrupted (perhaps due to signal)";
                case MonoUsbError.ErrorNoMem:
                    return "Insufficient memory";
                case MonoUsbError.ErrorIOCancelled:
                    return "Transfer was canceled";
                case MonoUsbError.ErrorNotSupported:
                    return "Operation not supported or unimplemented on this platform";
                default:
                    return "Unknown error:" + errcode;
            }
        }
        #endregion

        #region API LIBRARY FUNCTIONS - USB descriptors

        /// <summary>
        /// Retrieve a descriptor from the default control pipe. 
        /// </summary>
        /// <remarks>
        /// <para>This is a convenience function which formulates the appropriate control message to retrieve the descriptor.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="desc"/></note>
        /// </remarks>
        /// <param name="deviceHandle">Retrieve a descriptor from the default control pipe.</param>
        /// <param name="descType">The descriptor type, <see cref="DescriptorType"/></param>
        /// <param name="descIndex">The index of the descriptor to retrieve.</param>
        /// <param name="pData">Output buffer for descriptor.</param>
        /// <param name="length">Size of data buffer.</param>
        /// <returns>Number of bytes returned in data, or a <see cref="MonoUsbError"/> code on failure.</returns>
        public static int GetDescriptor(MonoUsbDeviceHandle deviceHandle, byte descType, byte descIndex, IntPtr pData, int length)
        {
            return ControlTransfer(deviceHandle,
                                           (byte)UsbEndpointDirection.EndpointIn,
                                           (byte)UsbStandardRequest.GetDescriptor,
                                           (short)((descType << 8) | descIndex),
                                           0,
                                           pData,
                                           (short)length,
                                           1000);
        }

        /// <summary>
        /// Retrieve a descriptor from the default control pipe. 
        /// </summary>
        /// <remarks>
        /// <para>This is a convenience function which formulates the appropriate control message to retrieve the descriptor.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="desc"/></note>
        /// </remarks>
        /// <param name="deviceHandle">Retrieve a descriptor from the default control pipe.</param>
        /// <param name="descType">The descriptor type, <see cref="DescriptorType"/></param>
        /// <param name="descIndex">The index of the descriptor to retrieve.</param>
        /// <param name="data">Output buffer for descriptor. This object is pinned using <see cref="PinnedHandle"/>.</param>
        /// <param name="length">Size of data buffer.</param>
        /// <returns>Number of bytes returned in data, or <see cref="MonoUsbError"/> code on failure.</returns>
        public static int GetDescriptor(MonoUsbDeviceHandle deviceHandle, byte descType, byte descIndex, object data, int length)
        {
            PinnedHandle p = new PinnedHandle(data);
            return GetDescriptor(deviceHandle, descType, descIndex, p.Handle, length);
        }

        #endregion

        #region API LIBRARY FUNCTIONS - Device handling and enumeration (part 2)

        /// <summary>
        /// Convenience function for finding a device with a particular idVendor/idProduct combination. 
        /// </summary>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <param name="vendorID">The idVendor value to search for.</param>
        /// <param name="productID">The idProduct value to search for.</param>
        /// <returns>Null if the device was not opened or not found, otherwise an opened device handle.</returns>
        public static MonoUsbDeviceHandle OpenDeviceWithVidPid([In]MonoUsbSessionHandle sessionHandle, short vendorID, short productID)
        {
            IntPtr pHandle = OpenDeviceWithVidPidInternal(sessionHandle, vendorID, productID);
            if (pHandle == IntPtr.Zero) return null;
            return new MonoUsbDeviceHandle(pHandle);
        }

        /// <summary>
        /// Get a <see cref="MonoUsbProfileHandle"/> for a <see cref="MonoUsbDeviceHandle"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function differs from the Libusb-1.0 C API in that when the new <see cref="MonoUsbProfileHandle"/> is returned, the device profile reference count 
        /// is incremented ensuring the profile will remain valid as long as it is in-use.
        /// </para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="dev"/></note>
        /// </remarks>
        /// <param name="devicehandle">A device handle.</param>
        /// <returns>The underlying profile handle.</returns>
        public static MonoUsbProfileHandle GetDevice(MonoUsbDeviceHandle devicehandle) { return new MonoUsbProfileHandle(GetDeviceInternal(devicehandle)); }

        #endregion

        #region API LIBRARY FUNCTIONS - Synchronous device I/O

        /// <summary>
        /// Perform a USB bulk transfer. 
        /// </summary>
        /// <remarks>
        /// <para>The direction of the transfer is inferred from the direction bits of the endpoint address.</para>
        /// <para>
        /// For bulk reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para>
        /// <para>
        /// You should also check the transferred parameter for bulk writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="data">
        /// <para>A suitably-sized data buffer for either input or output (depending on endpoint).</para>
        /// This value can be:
        /// <list type="bullet">
        /// <item>An <see cref="Array"/> of bytes or other <a href="http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx">blittable</a> types.</item>
        /// <item>An already allocated, pinned <see cref="GCHandle"/>. In this case <see cref="GCHandle.AddrOfPinnedObject"/> is used for the buffer address.</item>
        /// <item>An <see cref="IntPtr"/>.</item>
        /// </list>
        /// </param>
        /// <param name="length">For bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actualLength">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success (and populates <paramref name="actualLength"/>)</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the endpoint halted</item>
        /// <item><see cref="MonoUsbError.ErrorOverflow"/>if the device offered more data, see <a href="http://libusb.sourceforge.net/api-1.0/packetoverflow.html">Packets and overflows</a></item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        public static int BulkTransfer([In] MonoUsbDeviceHandle deviceHandle, byte endpoint, object data, int length, out int actualLength, int timeout)
        {
            PinnedHandle p = new PinnedHandle(data);
            int ret = BulkTransfer(deviceHandle, endpoint, p.Handle, length, out actualLength, timeout);
            p.Dispose();
            return ret;
        }

        /// <summary>
        /// Perform a USB interrupt transfer. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction of the transfer is inferred from the direction bits of the endpoint address.
        /// </para><para>
        /// For interrupt reads, the length field indicates the maximum length of data you are expecting to receive.
        /// If less data arrives than expected, this function will return that data, so be sure to check the 
        /// transferred output parameter.
        /// </para><para>
        /// You should also check the transferred parameter for interrupt writes. Not all of the data may have been 
        /// written. Also check transferred when dealing with a timeout error code. libusb may have to split 
        /// your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the 
        /// timeout may expire after the first few chunks have completed. libusb is careful not to lose any 
        /// data that may have been transferred; do not assume that timeout conditions indicate a complete lack 
        /// of I/O.
        /// </para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="endpoint">The address of a valid endpoint to communicate with.</param>
        /// <param name="data">
        /// <para>A suitably-sized data buffer for either input or output (depending on endpoint).</para>
        /// This value can be:
        /// <list type="bullet">
        /// <item>An <see cref="Array"/> of bytes or other <a href="http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx">blittable</a> types.</item>
        /// <item>An already allocated, pinned <see cref="GCHandle"/>. In this case <see cref="GCHandle.AddrOfPinnedObject"/> is used for the buffer address.</item>
        /// <item>An <see cref="IntPtr"/>.</item>
        /// </list>
        /// </param>
        /// <param name="length">For interrupt writes, the number of bytes from data to be sent. for interrupt reads, the maximum number of bytes to receive into the data buffer.</param>
        /// <param name="actualLength">Output location for the number of bytes actually transferred.</param>
        /// <param name="timeout">Timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>0 on success (and populates <paramref name="actualLength"/>)</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the endpoint halted</item>
        /// <item><see cref="MonoUsbError.ErrorOverflow"/>if the device offered more data, see <a href="http://libusb.sourceforge.net/api-1.0/packetoverflow.html">Packets and overflows</a></item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        public static int InterruptTransfer([In] MonoUsbDeviceHandle deviceHandle, byte endpoint, object data, int length, out int actualLength, int timeout)
        {
            PinnedHandle p = new PinnedHandle(data);
            int ret = InterruptTransfer(deviceHandle, endpoint, p.Handle, length, out actualLength, timeout);
            p.Dispose();
            return ret;
        }

        /// <summary>
        /// Perform a USB control transfer for multi-threaded applications using the <see cref="MonoUsbEventHandler"/> class.
        /// </summary>
        /// <remarks>
        /// <para>The direction of the transfer is inferred from the bmRequestType field of the setup packet.</para>
        /// <para>The wValue, wIndex and wLength fields values should be given in host-endian byte order.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="requestType">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="pData">A suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType).</param>
        /// <param name="dataLength">The length field for the setup packet. The data buffer should be at least this size.</param>
        /// <param name="timeout">timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>on success, the number of bytes actually transferred</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the control request was not supported by the device.</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        public static int ControlTransferAsync([In] MonoUsbDeviceHandle deviceHandle, byte requestType, byte request, short value, short index, IntPtr pData, short dataLength, int timeout)
        {
            MonoUsbControlSetupHandle setupHandle = new MonoUsbControlSetupHandle(requestType, request, value, index, pData, dataLength);
            MonoUsbTransfer transfer = new MonoUsbTransfer(0);
            ManualResetEvent completeEvent = new ManualResetEvent(false);
            GCHandle gcCompleteEvent = GCHandle.Alloc(completeEvent);

            transfer.FillControl(deviceHandle, setupHandle, DefaultAsyncDelegate, GCHandle.ToIntPtr(gcCompleteEvent), timeout);

            int r = (int)transfer.Submit();
            if (r < 0)
            {
                transfer.Free();
                gcCompleteEvent.Free();
                return r;
            }
            IntPtr pSessionHandle;
            MonoUsbSessionHandle sessionHandle = MonoUsbEventHandler.SessionHandle;
            if (sessionHandle == null)
                pSessionHandle = IntPtr.Zero;
            else
                pSessionHandle = sessionHandle.DangerousGetHandle();

            if (MonoUsbEventHandler.IsStopped)
            {
                while (!completeEvent.WaitOne(0))
                {
                    r = HandleEvents(pSessionHandle);
                    if (r < 0)
                    {
                        if (r == (int)MonoUsbError.ErrorInterrupted)
                            continue;
                        transfer.Cancel();
                        while (!completeEvent.WaitOne(0))
                            if (HandleEvents(pSessionHandle) < 0)
                                break;
                        transfer.Free();
                        gcCompleteEvent.Free();
                        return r;
                    }
                }
            }
            else
            {
                completeEvent.WaitOne(Timeout.Infinite);
            }

            if (transfer.Status == MonoUsbTansferStatus.TransferCompleted)
            {
                r = transfer.ActualLength;
                if (r > 0)
                {
                    byte[] ctrlDataBytes = setupHandle.ControlSetup.GetData(r);
                    Marshal.Copy(ctrlDataBytes, 0, pData, Math.Min(ctrlDataBytes.Length, dataLength));
                }

            }
            else
                r = (int)MonoLibUsbErrorFromTransferStatus(transfer.Status);

            transfer.Free();
            gcCompleteEvent.Free();
            return r;
        }

        /// <summary>
        /// Perform a USB control transfer.
        /// </summary>
        /// <remarks>
        /// <para>The direction of the transfer is inferred from the bmRequestType field of the setup packet.</para>
        /// <para>The wValue, wIndex and wLength fields values should be given in host-endian byte order.</para>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="syncio"/></note>
        /// </remarks>
        /// <param name="deviceHandle">A handle for the device to communicate with.</param>
        /// <param name="requestType">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="data">
        /// <para>A suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType).</para>
        /// This value can be:
        /// <list type="bullet">
        /// <item>An <see cref="Array"/> of bytes or other <a href="http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx">blittable</a> types.</item>
        /// <item>An already allocated, pinned <see cref="GCHandle"/>. In this case <see cref="GCHandle.AddrOfPinnedObject"/> is used for the buffer address.</item>
        /// <item>An <see cref="IntPtr"/>.</item>
        /// </list>
        /// </param>
        /// <param name="dataLength">The length field for the setup packet. The data buffer should be at least this size.</param>
        /// <param name="timeout">timeout (in milliseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>on success, the number of bytes actually transferred</item>
        /// <item><see cref="MonoUsbError.ErrorTimeout"/> if the transfer timed out</item>
        /// <item><see cref="MonoUsbError.ErrorPipe"/> if the control request was not supported by the device.</item>
        /// <item><see cref="MonoUsbError.ErrorNoDevice"/> if the device has been disconnected</item>
        /// <item>another <see cref="MonoUsbError"/> code on other failures</item>
        /// </list>
        /// </returns>
        public static int ControlTransfer([In] MonoUsbDeviceHandle deviceHandle, byte requestType, byte request, short value, short index, object data, short dataLength, int timeout)
        {
            PinnedHandle p = new PinnedHandle(data);
            int ret = ControlTransfer(deviceHandle, requestType, request, value, index, p.Handle, dataLength, timeout);
            p.Dispose();
            return ret;
        }

        #endregion

        #region API LIBRARY FUNCTIONS - Polling and timing

        /// <summary>
        /// Retrieve a list of file descriptors that should be polled by your main loop as libusb event sources. 
        /// </summary>
        /// <remarks>
        /// <note type="tip" title="Libusb-1.0 API:"><seelibusb10 group="poll"/></note>
        /// </remarks>
        /// <param name="sessionHandle">A valid <see cref="MonoUsbSessionHandle"/>.</param>
        /// <returns>A list of PollfdItem structures, or null on error.</returns>
        public static List<PollfdItem> GetPollfds(MonoUsbSessionHandle sessionHandle)
        {
            List<PollfdItem> rtnList = new List<PollfdItem>();
            IntPtr pList = GetPollfdsInternal(sessionHandle);
            if (pList == IntPtr.Zero) return null;

            IntPtr pNext = pList;
            IntPtr pPollfd;
            while ((((pNext != IntPtr.Zero))) && (pPollfd = Marshal.ReadIntPtr(pNext)) != IntPtr.Zero)
            {
                PollfdItem pollfdItem = new PollfdItem(pPollfd);
                rtnList.Add(pollfdItem);
                pNext = new IntPtr(pNext.ToInt64() + IntPtr.Size);
            }
            Marshal.FreeHGlobal(pList);

            return rtnList;
        }

        #endregion


        #region API LIBRARY - Windows Testing Only
#if WINDOWS_TESTING
        internal static internal_windows_device_priv GetWindowsPriv(MonoUsbProfileHandle profileHandle)
        {
            internal_windows_device_priv priv = new internal_windows_device_priv();
            IntPtr pPriv = new IntPtr(profileHandle.DangerousGetHandle().ToInt64() + Marshal.SizeOf(typeof(internal_libusb_device)));
            Marshal.PtrToStructure(pPriv, priv);
            return priv;
        }

        [StructLayout(LayoutKind.Sequential,Pack=0)]
        internal class internal_libusb_device
        {
            public readonly IntPtr mutexLock;
            public readonly int refCnt;                 /*QL - changed short to int*/

            public readonly IntPtr ctx;

            public readonly byte busNumber;
            public readonly byte deviceAddress;
            public readonly byte numConfigurations;

            public readonly internal_list_head list = new internal_list_head();
            public readonly uint sessionData;
            //public readonly IntPtr temp;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal class internal_list_head
        {
            public readonly IntPtr prev;
            public readonly IntPtr next;
        }

        internal struct internal_usb_interface
        {
            public readonly IntPtr path;                    // each interface needs a Windows device interface path,
            public readonly IntPtr apib;                    // an API backend (multiple drivers support),
            public readonly byte nb_endpoints;              // and a set of endpoint addresses (USB_MAXENDPOINTS)
            public readonly IntPtr endpoint;                /*QL - added */
            public readonly bool restricted_functionality;  /*QL - added */         
        }

[StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal class internal_windows_device_priv
        {
            public readonly IntPtr parent_dev;     // access to parent is required for usermode ops
            public readonly uint connection_index;  // also required for some usermode ops
            public readonly IntPtr path;            // path used by Windows to reference the USB node

            public readonly IntPtr apib;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly internal_usb_interface[] usb_interfaces=new internal_usb_interface[32];

            public readonly byte composite_api_flags;        // HID and composite devices require additional data
            public readonly IntPtr hid;
            public readonly byte active_config;

            public readonly IntPtr dev_descriptor;       /*QL - added*/
            public readonly IntPtr config_descriptor;    /*QL - added*/
            //USB_DEVICE_DESCRIPTOR dev_descriptor;
            //char **config_descriptor;  // list of pointers to the cached config descriptors
        }
#endif
        #endregion

        /// <summary>
        /// Converts a <see cref="MonoUsbTansferStatus"/> enum to a <see cref="MonoUsbError"/> enum.
        /// </summary>
        /// <param name="status">the <see cref="MonoUsbTansferStatus"/> to convert.</param>
        /// <returns>A <see cref="MonoUsbError"/> that represents <paramref name="status"/>.</returns>
        public static MonoUsbError MonoLibUsbErrorFromTransferStatus(MonoUsbTansferStatus status)
        {
            switch (status)
            {
                case MonoUsbTansferStatus.TransferCompleted:
                    return MonoUsbError.Success;
                case MonoUsbTansferStatus.TransferError:
                    return MonoUsbError.ErrorPipe;
                case MonoUsbTansferStatus.TransferTimedOut:
                    return MonoUsbError.ErrorTimeout;
                case MonoUsbTansferStatus.TransferCancelled:
                    return MonoUsbError.ErrorIOCancelled;
                case MonoUsbTansferStatus.TransferStall:
                    return MonoUsbError.ErrorPipe;
                case MonoUsbTansferStatus.TransferNoDevice:
                    return MonoUsbError.ErrorNoDevice;
                case MonoUsbTansferStatus.TransferOverflow:
                    return MonoUsbError.ErrorOverflow;
                default:
                    return MonoUsbError.ErrorOther;
            }
        }

        /// <summary>
        /// Calls <see cref="MonoUsbEventHandler.Init()"/> and <see cref="MonoUsbEventHandler.Start"/> if <see cref="MonoUsbEventHandler.IsStopped"/> = true.
        /// </summary>
        internal static void InitAndStart()
        {
            if (!MonoUsbEventHandler.IsStopped) return;
            MonoUsbEventHandler.Init();
            MonoUsbEventHandler.Start();
        }

        /// <summary>
        /// Calls <see cref="MonoUsbEventHandler.Stop"/> and <see cref="MonoUsbEventHandler.Exit"/>.
        /// </summary>
        internal static void StopAndExit()
        {
#if LIBUSBDOTNET
            if (LibUsbDotNet.LudnMonoLibUsb.MonoUsbDevice.mMonoUSBProfileList != null) LibUsbDotNet.LudnMonoLibUsb.MonoUsbDevice.mMonoUSBProfileList.Close();
            LibUsbDotNet.LudnMonoLibUsb.MonoUsbDevice.mMonoUSBProfileList = null;
#endif
            MonoUsbEventHandler.Stop(true);
            MonoUsbEventHandler.Exit();
        }

#if LIBUSBDOTNET
        internal static ErrorCode ErrorCodeFromLibUsbError(int ret, out string description)
        {
            description = string.Empty;
            if (ret == 0) return ErrorCode.Success;

            switch ((MonoUsbError)ret)
            {
                case MonoUsbError.Success:
                    description += "Success";
                    return ErrorCode.Success;
                case MonoUsbError.ErrorIO:
                    description += "Input/output error";
                    return ErrorCode.IoSyncFailed;
                case MonoUsbError.ErrorInvalidParam:
                    description += "Invalid parameter";
                    return ErrorCode.InvalidParam;
                case MonoUsbError.ErrorAccess:
                    description += "Access denied (insufficient permissions)";
                    return ErrorCode.AccessDenied;
                case MonoUsbError.ErrorNoDevice:
                    description += "No such device (it may have been disconnected)";
                    return ErrorCode.DeviceNotFound;
                case MonoUsbError.ErrorBusy:
                    description += "Resource busy";
                    return ErrorCode.ResourceBusy;
                case MonoUsbError.ErrorTimeout:
                    description += "Operation timed out";
                    return ErrorCode.IoTimedOut;
                case MonoUsbError.ErrorOverflow:
                    description += "Overflow";
                    return ErrorCode.Overflow;
                case MonoUsbError.ErrorPipe:
                    description += "Pipe error or endpoint halted";
                    return ErrorCode.PipeError;
                case MonoUsbError.ErrorInterrupted:
                    description += "System call interrupted (perhaps due to signal)";
                    return ErrorCode.Interrupted;
                case MonoUsbError.ErrorNoMem:
                    description += "Insufficient memory";
                    return ErrorCode.InsufficientMemory;
                case MonoUsbError.ErrorIOCancelled:
                    description += "Transfer was canceled";
                    return ErrorCode.IoCancelled;
                case MonoUsbError.ErrorNotSupported:
                    description += "Operation not supported or unimplemented on this platform";
                    return ErrorCode.NotSupported;
                default:
                    description += "Unknown error:" + ret;
                    return ErrorCode.UnknownError;
            }
        }
#endif
    }
}
