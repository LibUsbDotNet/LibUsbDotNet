// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

namespace LibUsbDotNet.Generator.Primitives
{
    public class Struct : IPrimitive
    {
        public string Name { get; set; }
        public Collection<Field> Fields { get; } = new Collection<Field>();
    }
}
