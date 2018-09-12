// <copyright file="CursorExtensions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

namespace LibUsbDotNet.Generator
{
    using System;
    using System.CodeDom;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using Core.Clang;

    internal static class CursorExtensions
    {
        public static bool IsInSystemHeader(this Cursor cursor)
        {
            try
            {
                return cursor.GetLocation().IsInSystemHeader();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
