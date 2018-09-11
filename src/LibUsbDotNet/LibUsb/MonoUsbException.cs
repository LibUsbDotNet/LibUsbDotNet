using LibUsbDotNet.Main;
using MonoLibUsb;
using System;
#if !NETSTANDARD1_6
using System.Runtime.Serialization;
#endif

namespace LibUsbDotNet.LibUsb
{
#if !NETSTANDARD1_6
    [Serializable]
#endif
    public class MonoUsbException : Exception
    {
        public MonoUsbException()
        {
        }

        public MonoUsbException(Error errorCode)
            : this(GetErrorMessage(errorCode))
        {
            this.ErrorCode = errorCode;
            this.HResult = (int)errorCode;
        }

        public MonoUsbException(string message)
            : base(message)
        {
        }

        public MonoUsbException(string message, Exception inner)
            : base(message, inner)
        {
        }

#if !NETSTANDARD1_6
        protected MonoUsbException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        public Error ErrorCode
        {
            get;
            private set;
        }

        private static string GetErrorMessage(Error errorCode)
        {
            if (MonoUsbApi.ErrorCodeFromLibUsbError((int)errorCode, out string errorMessage) == Main.ErrorCode.Success)
            {
                return errorMessage;
            }
            else
            {
                return $"An unknown error with code {(int)errorCode} has occurred.";
            }
        }
    }
}
