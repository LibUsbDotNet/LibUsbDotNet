// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

using Core.Clang;
using System;

namespace LibUsbDotNet.Generator
{
    internal static class CursorExtensions
    {
        public static bool IsInSystemHeader(this Cursor cursor)
        {
            try
            {
                // GetLocation() can return null for cursors libclang/ClangSharp
                // synthesizes without a backing source location; treat those as
                // not being in a system header.
                var location = cursor.GetLocation();
                return location != null && location.IsInSystemHeader();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

