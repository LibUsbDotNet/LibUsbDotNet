using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LibUsbDotNet.LibUsb
{
    /// <summary>
    /// Provides helper methods for resolving the native libraries.
    /// </summary>
    internal static class NativeLibraryResolver
    {
        static NativeLibraryResolver()
        {
#if !NETSTANDARD2_0
            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, DllImportResolver);
#endif
        }

        /// <summary>
        /// Ensures the library resolver is registered. This is a dummy method used to trigger the static constructor.
        /// </summary>
        public static void EnsureRegistered()
        {
            // Dummy method to trigger the static constructor.
        }

#if !NETSTANDARD2_0
        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != NativeMethods.LibUsbNativeLibrary)
            {
                return IntPtr.Zero;
            }

            IntPtr lib;
            string nativeLibraryName;

            // Library names for the various platforms:
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                nativeLibraryName = "libusb-1.0.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                nativeLibraryName = "libusb-1.0.so.0";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                nativeLibraryName = "libusb-1.0.0.dylib";
            }
            else
            {
                return IntPtr.Zero;
            }

            // First, attempt to load the native library from the NuGet packages
            var nativeSearchDirectories = AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") as string;
            var delimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";

            if (nativeSearchDirectories != null)
            {
                foreach (var directory in nativeSearchDirectories.Split(delimiter))
                {
                    var path = Path.Combine(directory, nativeLibraryName);
                    if (NativeLibrary.TryLoad(path, out lib))
                    {
                        return lib;
                    }
                }
            }

            // Next, try to load any OS-provided version of the library
            if (NativeLibrary.TryLoad(nativeLibraryName, out lib))
            {
                return lib;
            }

            return IntPtr.Zero;
        }
#endif
    }
}
