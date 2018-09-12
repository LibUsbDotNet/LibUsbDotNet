// <copyright file="EnumVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

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
