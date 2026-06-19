// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

namespace LibUsbDotNet.Generator.Primitives
{
    public class SafeHandle : IPrimitive
    {
        public string Name { get; set; }
        public string NativeName { get; set; }
    }
}
