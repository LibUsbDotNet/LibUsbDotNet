using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Diagnostics;

namespace MonoLibUsb
{
    internal static class MonoUsbErrorMessage
    {
        internal static UsbError Error(ErrorCode errorCode, int ret, string description, object sender)
        {
            string win32Error = String.Empty;

            if (errorCode == ErrorCode.MonoApiError)
            {
                win32Error = ((Error)ret) + ":" + MonoUsbApi.StrError((Error)ret);
            }

            UsbError err = new UsbError(errorCode, ret, win32Error, description, sender);
            return err;
        }
    }
}
