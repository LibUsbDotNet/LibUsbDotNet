// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace LibUsbDotNet.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetDirectory = "../../../../LibUsbDotNet/Generated";

            if (args.Length >= 1)
            {
                targetDirectory = args[0];
            }

            targetDirectory = Path.GetFullPath(targetDirectory);

            var vcpkgPath = Environment.GetEnvironmentVariable("VCPKG_ROOT");

            if (vcpkgPath == null)
            {
                Console.Error.WriteLine("Please set the VCPKG_ROOT environment variable to the folder where you've installed VCPKG.");
                return;
            }

            vcpkgPath = Path.Combine(vcpkgPath, "installed", "x64-windows", "include");
            Console.WriteLine($"Reading include files from {vcpkgPath}");

            var libusbHeader = Path.Combine(vcpkgPath, "libusb-1.0/libusb.h");

            DotNetHelpers.Register();

            Generator generator = new Generator();
            generator.InputFile = libusbHeader;
            generator.TargetDirectory = targetDirectory;
            generator.GenerateTypes();
            generator.WriteTypes();
        }
    }
}
