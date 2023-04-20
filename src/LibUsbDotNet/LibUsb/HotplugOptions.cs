namespace LibUsbDotNet.LibUsb;

public class HotplugOptions
{
    internal int Handle;
    public int VendorId { get; set; } = (int)HotplugOptionFlag.LibusbHotplugMatchAny;
    public int ProductId { get; set; } = (int)HotplugOptionFlag.LibusbHotplugMatchAny;
    public int DeviceClass { get; set; } = (int)HotplugOptionFlag.LibusbHotplugMatchAny;
    public HotplugEvent HotplugEventFlags { get; set; } = HotplugEvent.DeviceArrived;
}

public enum HotplugOptionFlag
{
    LibusbHotplugMatchAny = -1
}