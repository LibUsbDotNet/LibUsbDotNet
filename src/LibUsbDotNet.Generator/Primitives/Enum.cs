// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

using System.Collections.ObjectModel;

namespace LibUsbDotNet.Generator.Primitives
{
    public class Enum : IPrimitive
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseType { get; set; }

        public Collection<EnumValue> Values { get; } = new Collection<EnumValue>();

        public override string ToString()
        {
            return this.Name;
        }
    }
}
