using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.LibUsb;

namespace Examples;

internal static class ReadWriteAsync
{
    public static IUsbDevice MyUsbDevice;

    #region SET YOUR USB Vendor and Product ID!

    public static UsbDeviceFinder MyUsbFinder = new()
    {
        Vid = 0x1234,
        Pid = 0x0000
    };

    #endregion

    private static readonly Stopwatch TransferTimer = new Stopwatch();

    public enum TransferType
    {
        Read,
        Write
    }
        
    public class TransferHandle
    {
        public TransferType Type { get; }
        public Error Error { get; }
        public int TransferLength { get; }
        public double CompletionTime { get; }
        public int Id { get; }
        public string Data { get; }

        public TransferHandle(TransferType type, Error error, int transferLength, double completionTime, int id, string data)
        {
            Type = type;
            Error = error;
            TransferLength = transferLength;
            CompletionTime = completionTime;
            Id = id;
            Data = data;
        }
    }
        
    private static async Task<TransferHandle> ReadTransfer(int id, UsbEndpointReader reader, byte[] readBuffer)
    {
        var result = await reader.ReadAsync(readBuffer, 0, readBuffer.Length, 100);
        return new TransferHandle(TransferType.Read, result.error, result.transferLength, TransferTimer.Elapsed.TotalMilliseconds, id, Encoding.Default.GetString(readBuffer, 0, result.transferLength));
    }
        
    private static async Task<TransferHandle> WriteTransfer(int id, UsbEndpointWriter writer, byte[] bytesToSend)
    {
        var result = await writer.WriteAsync(bytesToSend, 0, bytesToSend.Length, 100);
        return new TransferHandle(TransferType.Write, result.error, result.transferLength, TransferTimer.Elapsed.TotalMilliseconds, id, Encoding.Default.GetString(bytesToSend, 0, result.transferLength));
    }
        
    public static async Task Main(string[] args)
    {
        Error ec = Error.Success;
            
        using (UsbContext context = new UsbContext())
        {
            try
            {
                // Find and open the usb device.
                MyUsbDevice = context.Find(MyUsbFinder);

                if (MyUsbDevice is null)
                {
                    Console.WriteLine("Can't find device.");
                    return;
                }
                    
                MyUsbDevice.Open();

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                var reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                // open write endpoint 1.
                var writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                    
                byte[] readBuffer = new byte[1024];
                List<Task<TransferHandle>> transfers = new();
                TransferTimer.Start();
                for (int testCount = 0; testCount < 10; testCount++)
                {
                    // Create and submit transfer
                    var usbReadTransfer = ReadTransfer(testCount, reader, readBuffer);
                    var usbWriteTransfer = WriteTransfer(testCount, writer, Encoding.Default.GetBytes(TransferTimer.Elapsed.TotalMilliseconds.ToString()));
                    transfers.Add(usbReadTransfer);
                    transfers.Add(usbWriteTransfer);

                    // TODO: Cancellation not supported yet
                    // if (!usbWriteTransfer.IsCompleted) usbWriteTransfer.Cancel();
                    // if (!usbReadTransfer.IsCompleted) usbReadTransfer.Cancel();
                }

                var completedTransfers = await Task.WhenAll(transfers);

                foreach (var completedTransfer in completedTransfers.OrderBy(transfer => transfer.CompletionTime))
                {
                    Console.WriteLine($"{completedTransfer.Type} transfer #{completedTransfer.Id} completed @ {completedTransfer.CompletionTime} ms with Error-{completedTransfer.Error} Length-{completedTransfer.TransferLength} Data-{completedTransfer.Data}");
                }
                    
                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != Error.Success ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;
                }

                // Wait for user input..
                Console.ReadKey();
            }
        }
    }
}