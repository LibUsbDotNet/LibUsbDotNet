// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

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
