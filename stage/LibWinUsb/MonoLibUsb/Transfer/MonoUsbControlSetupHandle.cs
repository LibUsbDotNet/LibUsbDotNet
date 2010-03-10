using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace MonoLibUsb.Transfer
{
    /// <summary>
    /// Allocates memory and fills an asynchronous control setup packet. 
    /// </summary>
    /// <remarks>
    /// <note type="tip">This type is used for asynchronous control transfers only.</note>
    /// </remarks>
    public class MonoUsbControlSetupHandle:SafeContextHandle
    {
        /// <summary>
        /// Allocates memory and sets up a control setup packet.
        /// </summary>
        /// <remarks>
        /// This constructor is used when <paramref name="requestType"/> has the <see cref="UsbEndpointDirection.EndpointIn"/> flag and this request will contain extra data (more than just the setup packet). 
        /// </remarks>
        /// <param name="requestType">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="data">The control data buffer to copy into the setup packet.</param>
        public MonoUsbControlSetupHandle(byte requestType, byte request, short value, short index, byte[] data)
            : base(IntPtr.Zero, true)
                {
                    if (data==null) data=new byte[0];

                    ushort wlength = (ushort) data.Length;
                    int packetSize = MonoUsbControlSetup.SETUP_PACKET_SIZE + wlength;
                    IntPtr pConfigMem = Marshal.AllocHGlobal(packetSize);
                    if (pConfigMem == IntPtr.Zero) throw new OutOfMemoryException(String.Format("Marshal.AllocHGlobal failed allocating {0} bytes", packetSize));
                    SetHandle(pConfigMem);

                    MonoUsbControlSetup w = new MonoUsbControlSetup(pConfigMem);

                    w.RequestType = requestType;
                    w.Request = request;
                    w.Value = value;
                    w.Index = index;
                    w.Length = (short)wlength;

                    if (data.Length > 0)
                        w.SetData(data, 0, data.Length);
                }

        /// <summary>
        /// Allocates memory and sets up a control setup packet.
        /// </summary>
        /// <remarks>
        /// <para>This constructor is used when:
        /// <list type="bullet">
        /// <item><paramref name="requestType"/> has the <see cref="UsbEndpointDirection.EndpointIn"/> flag and this request will not contain extra data (just the setup packet).</item>
        /// <item><paramref name="requestType"/> does not have the <see cref="UsbEndpointDirection.EndpointIn"/> flag.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="requestType">The request type field for the setup packet.</param>
        /// <param name="request">The request field for the setup packet.</param>
        /// <param name="value">The value field for the setup packet</param>
        /// <param name="index">The index field for the setup packet.</param>
        /// <param name="length">The length to allocate for the data portion of the setup packet.</param>
        public MonoUsbControlSetupHandle(byte requestType, byte request, short value, short index, short length)
            :base(IntPtr.Zero,true)
        {
            ushort wlength = (ushort) length;
            int packetSize = MonoUsbControlSetup.SETUP_PACKET_SIZE + wlength;
            IntPtr pConfigMem = Marshal.AllocHGlobal(packetSize);
            if (pConfigMem == IntPtr.Zero) throw new OutOfMemoryException(String.Format("Marshal.AllocHGlobal failed allocating {0} bytes", packetSize));
            SetHandle(pConfigMem);

            MonoUsbControlSetup w=new MonoUsbControlSetup(pConfigMem);

            w.RequestType = requestType;
            w.Request = request;
            w.Value = value;
            w.Index = index;
            w.Length = (short) wlength;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        { 
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
                SetHandleAsInvalid();
                Debug.Print(GetType().Name + " : ReleaseHandle");
            }
            return true;
        }
    }
}
