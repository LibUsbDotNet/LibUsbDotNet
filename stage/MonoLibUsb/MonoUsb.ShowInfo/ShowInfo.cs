using System;
using MonoLibUsb.Profile;
using Usb = MonoLibUsb.MonoLibUsbApi;

namespace MonoLibUsb.ShowInfo
{
    internal class ShowInfo
    {
        public static void Main(string[] args)
        {
            int ret;
            MonoUsbSessionHandle sessionHandle;
            MonoUsbProfileList profileList = null;

            // Initialize the context.
            sessionHandle=new MonoUsbSessionHandle();
            if (sessionHandle.IsInvalid) throw new Exception("Failed to initialize context.");
            try
            {
                MonoLibUsbApi.libusb_set_debug(sessionHandle, 0);
                // Create a MonoUsbProfileList instance.
                profileList = new MonoUsbProfileList();

                // The list is initially empty.
                // Each time refresh is called the list contents are updated. 
                ret = profileList.Refresh(sessionHandle);
                if (ret < 0) throw new Exception("Failed to retrieve device list.");
                Console.WriteLine("{0} device(s) found.", ret);

                foreach (MonoUsbProfile profile in profileList)
                    Console.WriteLine(profile.DeviceDescriptor);
            }
            finally
            {
                // If the below lines are removed, the profileList will still get freed and the
                // session will still get closed.
                if (profileList != null)
                    profileList.Free();

                sessionHandle.Close();
            }
        }
    }
}