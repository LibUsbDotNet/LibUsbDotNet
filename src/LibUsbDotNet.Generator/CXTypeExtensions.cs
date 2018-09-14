// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using System;

namespace LibUsbDotNet.Generator
{
    public static class CXTypeExtensions
    {
        public static Primitives.Delegate ToDelegate(this TypeInfo type, string nativeName, Cursor cursor, Generator generator)
        {
            if (type.Kind != TypeKind.FunctionProto && type.Kind != TypeKind.Unexposed)
            {
                throw new InvalidOperationException();
            }

            var clrName = NameConversions.ToClrName(nativeName, NameConversion.Type);

            var delegateType = new Primitives.Delegate()
            {
                Name = clrName,
                ReturnType = type.GetResultType().ToClrType()
            };

            var cursorVisitor = new DelegatingCursorVisitor(
                delegate (Cursor c, Cursor parent1)
                {
                    if (c.Kind == CursorKind.ParmDecl)
                    {
                        delegateType.Arguments.Add(new Primitives.Argument()
                        {
                            Name = NameConversions.ToClrName(c.GetDisplayName(), NameConversion.Parameter),
                            NativeName = c.GetDisplayName(),
                            Type = c.GetTypeInfo().ToClrType()
                        });
                    }

                    return ChildVisitResult.Continue;
                });
            cursorVisitor.VisitChildren(cursor);

            return delegateType;
        }
    }
}
