// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

namespace LibUsbDotNet.Generator.Primitives
{
    public class Argument : IPrimitive
    {
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string Type { get; set; }
        public string Documentation { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
