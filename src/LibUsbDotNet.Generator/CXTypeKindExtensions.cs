// <copyright file="TypeKindExtensions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using System;

namespace LibUsbDotNet.Generator
{
    internal static class TypeKindExtensions
    {

        public static Type ToClrType(this TypeInfo type)
        {
            var canonical = type.GetCanonicalType();

            switch (canonical.Kind)
            {
                case TypeKind.Bool:
                    return typeof(bool);

                case TypeKind.UChar:
                case TypeKind.Char_U:
                    return typeof(char);

                case TypeKind.SChar:
                case TypeKind.Char_S:
                    return typeof(sbyte);

                case TypeKind.UShort:
                    return typeof(ushort);

                case TypeKind.Short:
                    return typeof(short);

                case TypeKind.Float:
                    return typeof(float);

                case TypeKind.Double:
                    return typeof(double);

                case TypeKind.Int:
                case TypeKind.Enum:
                    return typeof(int);

                case TypeKind.UInt:
                    return typeof(uint);

                case TypeKind.Pointer:
                case TypeKind.IncompleteArray:
                    return typeof(IntPtr);

                case TypeKind.Long:
                    return typeof(int);

                case TypeKind.ULong:
                    return typeof(int);

                case TypeKind.LongLong:
                    return typeof(long);

                case TypeKind.ULongLong:
                    return typeof(ulong);

                case TypeKind.Void:
                    return typeof(void);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
