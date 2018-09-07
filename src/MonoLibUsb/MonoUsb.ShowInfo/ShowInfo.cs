using System;
using MonoLibUsb.Profile;
using Usb = MonoLibUsb.MonoUsbApi;

namespace MonoLibUsb.ShowInfo
{
    internal class ShowInfo
    {
        // The first time the Session property is used it creates a new session
        // handle instance in '__sessionHandle' and returns it. Subsequent 
        // request simply return '__sessionHandle'.
        private static MonoUsbSessionHandle __sessionHandle;
        public static MonoUsbSessionHandle Session
        {
            get
            {
                if (ReferenceEquals(__sessionHandle, null))
                    __sessionHandle = new MonoUsbSessionHandle();
                return __sessionHandle;
            }
        }
        public static void Main(string[] args)
        {
            int ret;
            MonoUsbProfileList profileList = null;

            // Initialize the context.
            if (Session.IsInvalid) 
                throw new Exception("Failed to initialize context.");

            MonoUsbApi.SetDebug(Session, 0);
            // Create a MonoUsbProfileList instance.
            profileList = new MonoUsbProfileList();

            // The list is initially empty.
            // Each time refresh is called the list contents are updated. 
            ret = profileList.Refresh(Session);
            if (ret < 0) throw new Exception("Failed to retrieve device list.");
            Console.WriteLine("{0} device(s) found.", ret);

            // Iterate through the profile list; write the device descriptor to
            // console output.
            foreach (MonoUsbProfile profile in profileList)
                Console.WriteLine(profile.DeviceDescriptor);

            // Since profile list, profiles, and sessions use safe handles the
            // code below is not required but it is considered good programming
            // to explicitly free and close these handle when they are no longer
            // in-use.
            profileList.Close();
            Session.Close();
        }
    }
}