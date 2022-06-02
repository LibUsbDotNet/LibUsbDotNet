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
using System.Runtime.InteropServices;
using System.Security;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Internal.WinUsb;
using LibUsbDotNet.Main;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.WinUsb.Internal {
#if !NETSTANDARD1_5 && !NETSTANDARD1_6
    [SuppressUnmanagedCodeSecurity]
#endif
    internal class LibusbKAPI : UsbApiBase {
        public const string LIBUSBK_DLL = "libusbK.dll";
        public const string LIBUSBK_PRE = "UsbK_";

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "AbortPipe", SetLastError = true)]
        private static extern bool UsbK_AbortPipe([In] SafeHandle InterfaceHandle, byte PipeID);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "ControlTransfer", SetLastError = true)]
        private static extern bool UsbK_ControlTransfer([In] SafeHandle InterfaceHandle,
                                                          [In] UsbSetupPacket SetupPacket,
                                                          IntPtr Buffer,
                                                          int BufferLength,
                                                          out int LengthTransferred,
                                                          IntPtr pOVERLAPPED);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "FlushPipe", SetLastError = true)]
        private static extern bool UsbK_FlushPipe([In] SafeHandle InterfaceHandle, byte PipeID);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "Free", SetLastError = true)]
        internal static extern bool UsbK_Free([In] IntPtr InterfaceHandle);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "GetAssociatedInterface", SetLastError = true)]
        internal static extern bool UsbK_GetAssociatedInterface([In] SafeHandle InterfaceHandle,
                                                                  byte AssociatedInterfaceIndex,
                                                                  ref IntPtr AssociatedInterfaceHandle);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "GetCurrentAlternateSetting", SetLastError = true)]
        internal static extern bool UsbK_GetCurrentAlternateSetting([In] SafeHandle InterfaceHandle, out byte SettingNumber);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "SetCurrentAlternateSetting", SetLastError = true)]
        internal static extern bool UsbK_SetCurrentAlternateSetting([In] SafeHandle InterfaceHandle, byte SettingNumber);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "GetDescriptor", SetLastError = true)]
        private static extern bool UsbK_GetDescriptor([In] SafeHandle InterfaceHandle,
                                                        byte DescriptorType,
                                                        byte Index,
                                                        ushort LanguageID,
                                                        IntPtr Buffer,
                                                        int BufferLength,
                                                        out int LengthTransferred);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "GetOverlappedResult", SetLastError = true)]
        private static extern bool UsbK_GetOverlappedResult([In] SafeHandle InterfaceHandle,
                                                              IntPtr pOVERLAPPED,
                                                              out int lpNumberOfBytesTransferred,
                                                              bool Wait);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "GetPipePolicy", SetLastError = true)]
        internal static extern bool UsbK_GetPipePolicy([In] SafeHandle InterfaceHandle,
                                                         byte PipeID,
                                                         PipePolicyType policyType,
                                                         ref int ValueLength,
                                                         IntPtr Value);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "GetPowerPolicy", SetLastError = true)]
        internal static extern bool UsbK_GetPowerPolicy([In] SafeHandle InterfaceHandle,
                                                          PowerPolicyType policyType,
                                                          ref int ValueLength,
                                                          IntPtr Value);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "Initialize", SetLastError = true)]
        internal static extern bool UsbK_Initialize([In] SafeHandle DeviceHandle, [Out, In] ref SafeLibusbKInterfaceHandle InterfaceHandle);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "QueryDeviceInformation", SetLastError = true)]
        internal static extern bool UsbK_QueryDeviceInformation([In] SafeHandle InterfaceHandle,
                                                                  DeviceInformationTypes InformationType,
                                                                  ref int BufferLength,
                                                                  [MarshalAs(UnmanagedType.AsAny), In, Out] object Buffer);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "QueryInterfaceSettings", SetLastError = true)]
        internal static extern bool UsbK_QueryInterfaceSettings([In] SafeHandle InterfaceHandle,
                                                                  byte AlternateInterfaceNumber,
                                                                  [MarshalAs(UnmanagedType.LPStruct), In, Out] UsbInterfaceDescriptor
                                                                      UsbAltInterfaceDescriptor);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "QueryPipe", SetLastError = true)]
        internal static extern bool UsbK_QueryPipe([In] SafeHandle InterfaceHandle,
                                                     byte AlternateInterfaceNumber,
                                                     byte PipeIndex,
                                                     [MarshalAs(UnmanagedType.LPStruct), In, Out] PipeInformation PipeInformation);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "ReadPipe", SetLastError = true)]
        private static extern bool UsbK_ReadPipe([In] SafeHandle InterfaceHandle,
                                                   byte PipeID,
                                                   Byte[] Buffer,
                                                   int BufferLength,
                                                   out int LengthTransferred,
                                                   IntPtr pOVERLAPPED);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "ReadPipe", SetLastError = true)]
        private static extern bool UsbK_ReadPipe([In] SafeHandle InterfaceHandle,
                                                   byte PipeID,
                                                   IntPtr pBuffer,
                                                   int BufferLength,
                                                   out int LengthTransferred,
                                                   IntPtr pOVERLAPPED);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "ResetPipe", SetLastError = true)]
        private static extern bool UsbK_ResetPipe([In] SafeHandle InterfaceHandle, byte PipeID);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "SetPipePolicy", SetLastError = true)]
        internal static extern bool UsbK_SetPipePolicy([In] SafeHandle InterfaceHandle,
                                                         byte PipeID,
                                                         PipePolicyType policyType,
                                                         int ValueLength,
                                                         IntPtr Value);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "SetPowerPolicy", SetLastError = true)]
        internal static extern bool UsbK_SetPowerPolicy([In] SafeHandle InterfaceHandle, PowerPolicyType policyType, int ValueLength, IntPtr Value);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "WritePipe", SetLastError = true)]
        private static extern bool UsbK_WritePipe([In] SafeHandle InterfaceHandle,
                                                    byte PipeID,
                                                    Byte[] Buffer,
                                                    int BufferLength,
                                                    out int LengthTransferred,
                                                    IntPtr pOVERLAPPED);

        [DllImport(LIBUSBK_DLL, EntryPoint = LIBUSBK_PRE + "WritePipe", SetLastError = true)]
        private static extern bool UsbK_WritePipe([In] SafeHandle InterfaceHandle,
                                                    byte PipeID,
                                                    IntPtr pBuffer,
                                                    int BufferLength,
                                                    out int LengthTransferred,
                                                    IntPtr pOVERLAPPED);


        public override bool AbortPipe(SafeHandle InterfaceHandle, byte PipeID) { return UsbK_AbortPipe(InterfaceHandle, PipeID); }

        public override bool ControlTransfer(SafeHandle InterfaceHandle,
                                             UsbSetupPacket SetupPacket,
                                             IntPtr Buffer,
                                             int BufferLength,
                                             out int LengthTransferred) { return UsbK_ControlTransfer(InterfaceHandle, SetupPacket, Buffer, BufferLength, out LengthTransferred, IntPtr.Zero); }

        public override bool FlushPipe(SafeHandle InterfaceHandle, byte PipeID) { return UsbK_FlushPipe(InterfaceHandle, PipeID); }

        public override bool GetDescriptor(SafeHandle InterfaceHandle,
                                           byte DescriptorType,
                                           byte Index,
                                           ushort LanguageID,
                                           IntPtr Buffer,
                                           int BufferLength,
                                           out int LengthTransferred) { return UsbK_GetDescriptor(InterfaceHandle, DescriptorType, Index, LanguageID, Buffer, BufferLength, out LengthTransferred); }

        public override bool GetOverlappedResult(SafeHandle InterfaceHandle, IntPtr pOVERLAPPED, out int numberOfBytesTransferred, bool Wait) {
            if (!InterfaceHandle.IsClosed) {
                return UsbK_GetOverlappedResult(InterfaceHandle, pOVERLAPPED, out numberOfBytesTransferred, Wait);
            } else {
                numberOfBytesTransferred = 0;
                return true;
            }
        }
        //public override bool ReadPipe(UsbEndpointBase endPointBase,
        //                              byte[] Buffer,
        //                              int BufferLength,
        //                              out int LengthTransferred,
        //                              int isoPacketSize,
        //                              IntPtr pOVERLAPPED) { return UsbK_ReadPipe(endPointBase.Device.Handle, endPointBase.EpNum, Buffer, BufferLength, out LengthTransferred, pOVERLAPPED); }

        public override bool ReadPipe(UsbEndpointBase endPointBase,
                                      IntPtr pBuffer,
                                      int BufferLength,
                                      out int LengthTransferred,
                                      int isoPacketSize,
                                      IntPtr pOVERLAPPED) { return UsbK_ReadPipe(endPointBase.Device.Handle, endPointBase.EpNum, pBuffer, BufferLength, out LengthTransferred, pOVERLAPPED); }

        public override bool ResetPipe(SafeHandle InterfaceHandle, byte PipeID) { return UsbK_ResetPipe(InterfaceHandle, PipeID); }

        //public override bool WritePipe(UsbEndpointBase endPointBase,
        //                               byte[] Buffer,
        //                               int BufferLength,
        //                               out int LengthTransferred,
        //                               int isoPacketSize,
        //                               IntPtr pOVERLAPPED) { return UsbK_WritePipe(endPointBase.Device.Handle, endPointBase.EpNum, Buffer, BufferLength, out LengthTransferred, pOVERLAPPED); }

        public override bool WritePipe(UsbEndpointBase endPointBase,
                                       IntPtr pBuffer,
                                       int BufferLength,
                                       out int LengthTransferred,
                                       int isoPacketSize,
                                       IntPtr pOVERLAPPED) { return UsbK_WritePipe(endPointBase.Device.Handle, endPointBase.EpNum, pBuffer, BufferLength, out LengthTransferred, pOVERLAPPED); }

        internal static bool OpenDevice(out SafeFileHandle sfhDevice, string DevicePath) {
            sfhDevice =
                Kernel32.CreateFile(DevicePath,
                                    NativeFileAccess.FILE_GENERIC_WRITE | NativeFileAccess.FILE_GENERIC_READ,
                                    NativeFileShare.FILE_SHARE_WRITE | NativeFileShare.FILE_SHARE_READ,
                                    IntPtr.Zero,
                                    NativeFileMode.OPEN_EXISTING,
                                    NativeFileFlag.FILE_ATTRIBUTE_NORMAL | NativeFileFlag.FILE_FLAG_OVERLAPPED,
                                    IntPtr.Zero);

            return (!sfhDevice.IsInvalid && !sfhDevice.IsClosed);
        }
    }
}
