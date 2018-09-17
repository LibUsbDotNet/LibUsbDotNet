// <copyright file="TypeDefVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.CodeDom;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Core.Clang;

namespace LibUsbDotNet.Generator
{

    internal class TypeDefVisitor
    {
        private readonly Generator generator;

        public TypeDefVisitor(Generator generator)
        {
            this.generator = generator;
        }

        public ChildVisitResult Visit(Cursor cursor, Cursor parent)
        {
            if (!cursor.GetLocation().IsFromMainFile())
            {
                return ChildVisitResult.Continue;
            }

            CursorKind curKind = cursor.Kind;
            if (curKind == CursorKind.TypedefDecl)
            {
                var nativeName = cursor.GetSpelling();
                var clrName = NameConversions.ToClrName(nativeName, NameConversion.Type);

                TypeInfo type = cursor.GetTypedefDeclUnderlyingType().GetCanonicalType();

                // we handle enums and records in struct and enum visitors with forward declarations also
                switch (type.Kind)
                {
                    case TypeKind.Record:
                        this.generator.AddType(
                            nativeName,
                            new Primitives.SafeHandle()
                            {
                                NativeName = nativeName,
                                Name = NameConversions.ToClrName(nativeName, NameConversion.Type)
                            });
                        return ChildVisitResult.Continue;

                    case TypeKind.Pointer:
                        var pointee = type.GetPointeeType();

                        if (pointee.Kind == TypeKind.FunctionProto)
                        {
                            var functionType = cursor.GetTypedefDeclUnderlyingType();
                            var pt = functionType.GetPointeeType();

                            var delegateType = pt.ToDelegate(nativeName, cursor, this.generator);
                            this.generator.AddType(nativeName, delegateType);

                            return ChildVisitResult.Continue;
                        }
                        else
                        {
                            return ChildVisitResult.Continue;
                        }

                    default:
                        return ChildVisitResult.Continue;
                }
            }

            return ChildVisitResult.Recurse;
        }
    }
}
