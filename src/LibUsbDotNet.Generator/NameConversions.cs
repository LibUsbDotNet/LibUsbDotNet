// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace LibUsbDotNet.Generator
{
    internal static class NameConversions
    {
        public static string ToClrName(string nativeName, NameConversion conversion)
        {
            return ToClrName(nativeName, string.Empty, conversion);
        }

        public static string ToClrName(string nativeName, string parentName, NameConversion conversion)
        {
            var patchedName = nativeName;

            if (!string.IsNullOrEmpty(parentName) && patchedName.StartsWith(parentName, StringComparison.OrdinalIgnoreCase))
            {
                patchedName = patchedName.Substring(parentName.Length);
            }

            List<string> parts = new List<string>(patchedName.Split('_', StringSplitOptions.RemoveEmptyEntries));

            StringBuilder nameBuilder = new StringBuilder();

            int i = 0;

            while (i < parts.Count)
            {
                // Remove the libusb prefix
                if (i == 0 && string.Equals(parts[i], "libusb", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    continue;
                }

                if (conversion == NameConversion.Parameter && i == 0)
                {
                    nameBuilder.Append(char.ToLowerInvariant(parts[i][0]) + parts[i].Substring(1).ToLowerInvariant());
                }
                else
                {
                    nameBuilder.Append(char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1).ToLowerInvariant());
                }

                i++;
            }

            return nameBuilder.ToString();
        }
    }
}
