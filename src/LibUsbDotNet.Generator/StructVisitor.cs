// <copyright file="StructVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using LibUsbDotNet.Generator.Primitives;
using System;
using System.Collections.Generic;

namespace LibUsbDotNet.Generator
{
    internal class StructVisitor
    {
        private readonly Generator generator;
        private int fieldPosition;
        private Struct current;

        public StructVisitor(Generator generator)
        {
            this.generator = generator;
        }

        public ChildVisitResult Visit(Cursor cursor, Cursor parent)
        {
            if (!cursor.GetLocation().IsFromMainFile())
            {
                return ChildVisitResult.Continue;
            }

            if (parent.Kind == CursorKind.UnionDecl)
            {
                return ChildVisitResult.Continue;
            }

            CursorKind curKind = cursor.Kind;
            if (curKind == CursorKind.StructDecl)
            {
                this.fieldPosition = 0;
                var nativeName = cursor.GetSpelling();

                // struct names can be empty, and so we visit its sibling to find the name
                if (string.IsNullOrEmpty(nativeName))
                {
                    var forwardDeclaringVisitor = new ForwardDeclarationVisitor(cursor, skipSystemHeaderCheck: true);
                    forwardDeclaringVisitor.VisitChildren(cursor.GetSemanticParent());
                    nativeName = forwardDeclaringVisitor.ForwardDeclarationCursor.GetSpelling();

                    if (string.IsNullOrEmpty(nativeName))
                    {
                        nativeName = "_";
                    }
                }

                var clrName = NameConversions.ToClrName(nativeName, NameConversion.Type);

                this.current = new Struct();
                this.generator.AddType(nativeName, current);
                this.current.Name = clrName;

                var visitor = new DelegatingCursorVisitor(this.Visit);
                visitor.VisitChildren(cursor);

                return ChildVisitResult.Continue;
            }

            if (curKind == CursorKind.FieldDecl)
            {
                var fieldName = cursor.GetSpelling();
                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = "field" + this.fieldPosition; // what if they have fields called field*? :)
                }
                else
                {
                    fieldName = NameConversions.ToClrName(fieldName, NameConversion.Field);
                }

                this.fieldPosition++;

                foreach (var member in GetFields(cursor, fieldName, this.generator))
                {
                    this.current.Fields.Add(member);
                }

                return ChildVisitResult.Continue;
            }

            return ChildVisitResult.Recurse;
        }

        public static IEnumerable<Field> GetFields(Cursor cursor, string cursorSpelling, Generator generator)
        {
            var canonical = cursor.GetTypeInfo().GetCanonicalType();

            switch (canonical.Kind)
            {
                case TypeKind.ConstantArray:
                    var size = canonical.GetArraySize();

                    if (canonical.GetArrayElementType().GetCanonicalType().Kind == TypeKind.UChar)
                    {
                        Field fixedLengthField = new Field();
                        fixedLengthField.Name = cursorSpelling;
                        fixedLengthField.Type = "string";
                        fixedLengthField.FixedLengthString = (int)size;
                        yield return fixedLengthField;
                    }
                    else if (canonical.GetArrayElementType().GetCanonicalType().Kind == TypeKind.Pointer)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            Field fixedLengthField = new Field();
                            fixedLengthField.Name = $"{cursorSpelling}_{i}";
                            fixedLengthField.Type = "void*";
                            yield return fixedLengthField;
                        }
                    }
                    else if (canonical.GetArrayElementType().GetCanonicalType().Kind == TypeKind.Record)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            Field fixedLengthField = new Field();
                            fixedLengthField.Name = $"{cursorSpelling}_{i}";
                            fixedLengthField.Type = "void*";
                            yield return fixedLengthField;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;

                case TypeKind.Pointer:
                    var pointeeType = canonical.GetPointeeType().GetCanonicalType();

                    var intPtrMember = new Field();
                    intPtrMember.Name = cursorSpelling;
                    intPtrMember.Type = "IntPtr";
                    yield return intPtrMember;

                    /*
                    if (pointeeType.Kind == TypeKind.Char_S)
                    {
                        CodeMemberProperty stringMember = new CodeMemberProperty();
                        stringMember.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                        stringMember.Name = cursorSpelling + "String";
                        stringMember.Type = new CodeTypeReference(typeof(string));

                        stringMember.HasGet = true;
                        stringMember.GetStatements.Add(
                            new CodeMethodReturnStatement(
                                new CodeMethodInvokeExpression(
                                    new CodeMethodReferenceExpression(
                                        new CodeTypeReferenceExpression("Utf8Marshal"),
                                        "PtrToStringUtf8"),
                                    new CodeFieldReferenceExpression(
                                        new CodeThisReferenceExpression(),
                                        intPtrMember.Name))));

                        yield return stringMember;
                    }*/

                    break;

                case TypeKind.Enum:
                    var enumField = new Field();
                    enumField.Name = cursorSpelling;
                    enumField.Type = NameConversions.ToClrName(canonical.GetSpelling(), NameConversion.Type);
                    yield return enumField;
                    break;

                case TypeKind.Record:
                    var recordField = new Field();
                    recordField.Name = cursorSpelling;
                    recordField.Type = NameConversions.ToClrName(canonical.GetSpelling(), NameConversion.Type);
                    yield return recordField;
                    break;

                default:
                    var field = new Field();
                    field.Name = cursorSpelling;
                    field.Type = canonical.ToClrType();
                    yield return field;
                    break;
            }
        }
    }
}
