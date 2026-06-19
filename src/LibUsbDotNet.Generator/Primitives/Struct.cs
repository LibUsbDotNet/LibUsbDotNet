// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

using System.Collections.ObjectModel;
using System.Linq;

namespace LibUsbDotNet.Generator.Primitives
{
    public class Struct : IPrimitive
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Collection<Field> Fields { get; } = new Collection<Field>();
        public string ExtraKeywords
        {
            get
            {
                if (Fields.Any(f => f.FixedLengthString != null || f.Unsafe))
                {
                    return "unsafe ";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
