// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

namespace LibUsbDotNet.Generator
{
    using Core.Clang;
    using System;

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
