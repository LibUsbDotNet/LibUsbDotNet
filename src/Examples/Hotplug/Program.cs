using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace Examples;

internal static class Hotplug
{

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
        using var context = new UsbContext();
        
        // Set finder for device.
        var port = new PhysicalPortId(new LocationId(5, new ReadOnlyCollection<byte>(new byte[] { 4 })), new LocationId(6, new ReadOnlyCollection<byte>(new byte[] { 4 })));
        var finder = new UsbDeviceFinder
        {
            PhyiscalPortId = port
        };
        
        // context.SetDebugLevel(LogLevel.Info);
        using var deviceManager = new DeviceManager(context);
        deviceManager.Start();

        using var device = await deviceManager.WaitForDevice(finder, TimeSpan.FromSeconds(2));

        Console.WriteLine("Got a stable connection.");
        
        device.Open();
        device.SetConfiguration(1);
        device.ClaimInterface(0);
        
        // open read endpoint 1.
        var reader = device.OpenEndpointReader(ReadEndpointID.Ep01);

        // open write endpoint 1.
        var writer = device.OpenEndpointWriter(WriteEndpointID.Ep01);
            
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
        }

        var completedTransfers = await Task.WhenAll(transfers);

        foreach (var completedTransfer in completedTransfers.OrderBy(transfer => transfer.CompletionTime))
        {
            Console.WriteLine($"{completedTransfer.Type} transfer #{completedTransfer.Id} completed @ {completedTransfer.CompletionTime} ms with Error-{completedTransfer.Error} Length-{completedTransfer.TransferLength} Data-{completedTransfer.Data}");
        }
        
        Console.WriteLine("Press any key to exit...");
        
        Console.ReadKey();
    }
}