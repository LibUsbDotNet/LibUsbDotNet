// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

namespace LibUsbDotNet.Generator.Primitives
{
    public class Field : IPrimitive
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int? FixedLengthString { get; set; }
        public bool Unsafe { get; set; }
    }
}
