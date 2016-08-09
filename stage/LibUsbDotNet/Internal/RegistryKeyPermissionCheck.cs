#if NETSTANDARD1_5
namespace LibUsbDotNet.Internal
{
    internal enum RegistryKeyPermissionCheck
    {
        Default = 0,
        ReadSubTree = 1,
        ReadWriteSubTree = 2
    }
}
#endif
