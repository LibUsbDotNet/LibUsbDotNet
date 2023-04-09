namespace LibUsbDotNet.LibUsb;

public static class NativeLibraryVersion
{
    static unsafe NativeLibraryVersion()
    {
        var version = NativeMethods.GetVersion();
        Major = version->Major;
        Minor = version->Minor;
        Micro = version->Micro;
        Nano = version->Nano;
    }
    
    /// <summary>
    ///  Library major version.
    /// </summary>
    public static ushort Major { get; }

    /// <summary>
    ///  Library minor version.
    /// </summary>
    public static ushort Minor { get; }

    /// <summary>
    ///  Library micro version.
    /// </summary>
    public static ushort Micro { get; }

    /// <summary>
    ///  Library nano version.
    /// </summary>
    public static ushort Nano { get; }

    /// <summary>
    /// libusb version with nano included
    /// </summary>
    public static string FullVersion 
        => $"{Major}.{Minor}.{Micro}.{Nano}";
    
    /// <summary>
    /// libusb version
    /// </summary>
    public static string Version 
        => $"{Major}.{Minor}.{Micro}";
}