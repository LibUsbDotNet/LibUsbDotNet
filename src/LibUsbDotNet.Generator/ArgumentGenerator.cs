// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using LibUsbDotNet.Generator.Primitives;

namespace LibUsbDotNet.Generator
{
    public static class ArgumentGenerator
    {
        public static Argument GenerateArgument(this Generator generator, FunctionKind functionKind, Cursor functionCursor, uint argumentIndex)
        {
            var functionType = functionCursor.GetTypeInfo();
            var paramCursor = functionCursor.GetArgument(argumentIndex);
            var type = functionType.GetArgType(argumentIndex);

            var nativeName = paramCursor.GetSpelling();
            if (string.IsNullOrEmpty(nativeName))
            {
                nativeName = "param" + argumentIndex;
            }

            var name = NameConversions.ToClrName(nativeName, NameConversion.Parameter);

            Argument argument = new Argument();
            argument.Name = name;
            argument.NativeName = nativeName;
            argument.Type = type.ToClrType(functionKind);
            return argument;
        }
    }
}
