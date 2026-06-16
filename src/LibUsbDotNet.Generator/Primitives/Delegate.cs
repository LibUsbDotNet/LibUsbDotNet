// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

using System.Collections.ObjectModel;

namespace LibUsbDotNet.Generator.Primitives
{
    public class Delegate : IPrimitive
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public Collection<Argument> Arguments { get; } = new Collection<Argument>();

        public override string ToString()
        {
            return this.Name;
        }
    }
}
