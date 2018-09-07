using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Main;
using MonoLibUsb;
using MonoLibUsb.Profile;
using MonoLibUsb.Transfer;

namespace MonoUsb.Async.ControlIO
{
    internal class AsyncControlIO
    {
        private static MonoUsbTransferDelegate controlTransferDelegate;
        private static MonoUsbSessionHandle sessionHandle;

        // Predicate functions for finding only devices with the specified VendorID & ProductID.
        private static bool MyVidPidPredicate(MonoUsbProfile profile)
        {
            if (profile.DeviceDescriptor.VendorID == 0x04d8 && profile.DeviceDescriptor.ProductID == 0x0053)
                return true;
            return false;
        }

        private static void Main(string[] args)
        {
            // Assign the control transfer delegate to the callback function. 
            controlTransferDelegate = ControlTransferCB;

            // Initialize the context.
            sessionHandle = new MonoUsbSessionHandle();
            if (sessionHandle.IsInvalid)
                throw new Exception(String.Format("Failed intializing libusb context.\n{0}:{1}",
                                                  MonoUsbSessionHandle.LastErrorCode,
                                                  MonoUsbSessionHandle.LastErrorString));

            MonoUsbProfileList profileList = new MonoUsbProfileList();
            MonoUsbDeviceHandle myDeviceHandle = null;

            try
            {
                // The list is initially empty.
                // Each time refresh is called the list contents are updated. 
                profileList.Refresh(sessionHandle);

                // Use the GetList() method to get a generic List of MonoUsbProfiles
                // Find the first profile that matches in MyVidPidPredicate.
                MonoUsbProfile myProfile = profileList.GetList().Find(MyVidPidPredicate);
                if (myProfile == null)
                {
                    Console.WriteLine("Device not connected.");
                    return;
                }

                // Open the device handle to perform I/O
                myDeviceHandle = myProfile.OpenDeviceHandle();
                if (myDeviceHandle.IsInvalid)
                    throw new Exception(String.Format("Failed opening device handle.\n{0}:{1}",
                                                      MonoUsbDeviceHandle.LastErrorCode,
                                                      MonoUsbDeviceHandle.LastErrorString));
                int ret;
                MonoUsbError e;

                // Set Configuration
                e = (MonoUsbError) (ret = MonoUsbApi.SetConfiguration(myDeviceHandle, 1));
                if (ret < 0) throw new Exception(String.Format("Failed SetConfiguration.\n{0}:{1}", e, MonoUsbApi.StrError(e)));

                // Claim Interface
                e = (MonoUsbError) (ret = MonoUsbApi.ClaimInterface(myDeviceHandle, 0));
                if (ret < 0) throw new Exception(String.Format("Failed ClaimInterface.\n{0}:{1}", e, MonoUsbApi.StrError(e)));

                // Create a vendor specific control setup, allocate 1 byte for return control data.
                byte requestType = (byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor);
                byte request = 0x0F;
                MonoUsbControlSetupHandle controlSetupHandle = new MonoUsbControlSetupHandle(requestType, request, 0, 0, 1);

                // Transfer the control setup packet
                ret = libusb_control_transfer(myDeviceHandle, controlSetupHandle, 1000);
                if (ret > 0)
                {
                    Console.WriteLine("\nSuccess!\n");
                    byte[] ctrlDataBytes = controlSetupHandle.ControlSetup.GetData(ret);
                    string ctrlDataString = Helper.HexString(ctrlDataBytes, String.Empty, "h ");
                    Console.WriteLine("Return Length: {0}", ret);
                    Console.WriteLine("DATA (hex)   : [ {0} ]\n", ctrlDataString.Trim());
                }
                MonoUsbApi.ReleaseInterface(myDeviceHandle, 0);
            }
            finally
            {
                profileList.Close();
                if (myDeviceHandle != null) myDeviceHandle.Close();
                sessionHandle.Close();
            }
        }

        private static void ControlTransferCB(MonoUsbTransfer transfer)
        {
            ManualResetEvent completeEvent = GCHandle.FromIntPtr(transfer.PtrUserData).Target as ManualResetEvent;
            completeEvent.Set();
        }

        private static int libusb_control_transfer(MonoUsbDeviceHandle deviceHandle, MonoUsbControlSetupHandle controlSetupHandle, int timeout)
        {
            MonoUsbTransfer transfer = MonoUsbTransfer.Alloc(0);
            ManualResetEvent completeEvent = new ManualResetEvent(false);
            GCHandle gcCompleteEvent = GCHandle.Alloc(completeEvent);

            transfer.FillControl(deviceHandle, controlSetupHandle, controlTransferDelegate, GCHandle.ToIntPtr(gcCompleteEvent), timeout);

            int r = (int) transfer.Submit();
            if (r < 0)
            {
                transfer.Free();
                gcCompleteEvent.Free();
                return r;
            }

            while (!completeEvent.WaitOne(0,false))
            {
                r = MonoUsbApi.HandleEvents(sessionHandle);
                if (r < 0)
                {
                    if (r == (int) MonoUsbError.ErrorInterrupted)
                        continue;
                    transfer.Cancel();
                    while (!completeEvent.WaitOne(0, false))
                        if (MonoUsbApi.HandleEvents(sessionHandle) < 0)
                            break;
                    transfer.Free();
                    gcCompleteEvent.Free();
                    return r;
                }
            }

            if (transfer.Status == MonoUsbTansferStatus.TransferCompleted)
                r = transfer.ActualLength;
            else
                r = (int) MonoUsbApi.MonoLibUsbErrorFromTransferStatus(transfer.Status);

            transfer.Free();
            gcCompleteEvent.Free();
            return r;
        }
    }
}