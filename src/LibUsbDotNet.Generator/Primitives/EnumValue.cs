// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

namespace LibUsbDotNet.Generator.Primitives
{
    public class EnumValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
