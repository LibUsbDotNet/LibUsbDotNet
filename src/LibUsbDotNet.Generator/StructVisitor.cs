// <copyright file="StructVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using LibUsbDotNet.Generator.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

                this.current = new Struct()
                {
                    Name = clrName,
                    Description = GetComment(cursor)
                };

                // This is a lazy attempt to handle forward declarations. A better way would be as described here:
                // https://joshpeterson.github.io/identifying-a-forward-declaration-with-libclang
                // but libclang doesn't expose clang_getNullCursor (yet)
                current = this.generator.AddType(nativeName, current) as Struct;

                // If the struct is in use as a 'handle', this AddType would have returned an Handle and the 'as Struct'
                // statement would have cast it to null. In that case, we don't create an explicit struct.
                if (current != null)
                {
                    var visitor = new DelegatingCursorVisitor(this.Visit);
                    visitor.VisitChildren(cursor);
                }

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
            var comment = GetComment(cursor);

            switch (canonical.Kind)
            {
                case TypeKind.ConstantArray:
                    var size = canonical.GetArraySize();

                    if (size == 0)
                    {
                        // This happens in e.g., libusb_bos_dev_capability_descriptor
                        // #if defined(__STDC_VERSION__) && (__STDC_VERSION__ >= 199901L)
                        // [] /* valid C99 code */
                        // #else
                        // [0] /* non-standard, but usually working code */
                        // #endif
                        Field emptyArrayField = new Field();
                        emptyArrayField.Name = cursorSpelling;
                        emptyArrayField.Type = "IntPtr";
                        emptyArrayField.Description = comment;
                        yield return emptyArrayField;
                    }
                    else if (canonical.GetArrayElementType().GetCanonicalType().Kind == TypeKind.UChar)
                    {
                        Field fixedLengthField = new Field();
                        fixedLengthField.Name = cursorSpelling;
                        fixedLengthField.Type = "string";
                        fixedLengthField.FixedLengthString = (int)size;
                        fixedLengthField.Description = comment;
                        yield return fixedLengthField;
                    }
                    else if (canonical.GetArrayElementType().GetCanonicalType().Kind == TypeKind.Pointer)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            Field fixedLengthField = new Field();
                            fixedLengthField.Name = $"{cursorSpelling}_{i}";
                            fixedLengthField.Type = "void*";
                            fixedLengthField.Description = comment;
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
                            fixedLengthField.Description = comment;
                            yield return fixedLengthField;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;

                case TypeKind.Pointer:
                    var targetSpelling = cursor.GetTypeInfo().GetPointeeType().GetSpelling();
                    var targetType = generator.Types.ContainsKey(targetSpelling) ? generator.Types[targetSpelling] : null;

                    if (targetType is SafeHandle)
                    {
                        // Adding a SafeHandle to a struct would generate CS0208, so let's
                        // use an IntPtr instead.
                        yield return
                            new Field()
                            {
                                Name = cursorSpelling,
                                Type = "IntPtr",
                                Description = comment,
                                Unsafe = true
                            };
                    }
                    else
                    {
                        yield return
                            new Field()
                            {
                                Name = cursorSpelling,
                                Type = cursor.GetTypeInfo().ToClrType(),
                                Description = comment,
                                Unsafe = true
                            };
                    }

                    break;

                case TypeKind.Enum:
                    var enumField = new Field();
                    enumField.Name = cursorSpelling;
                    enumField.Type = NameConversions.ToClrName(canonical.GetSpelling(), NameConversion.Type);
                    enumField.Description = comment;
                    yield return enumField;
                    break;

                case TypeKind.Record:
                    var recordField = new Field();
                    recordField.Name = cursorSpelling;
                    recordField.Type = NameConversions.ToClrName(canonical.GetSpelling(), NameConversion.Type);
                    recordField.Description = comment;
                    yield return recordField;
                    break;

                default:
                    var field = new Field();
                    field.Name = cursorSpelling;
                    field.Type = canonical.ToClrType();
                    field.Description = comment;
                    yield return field;
                    break;
            }
        }

        private static string GetComment(Cursor cursor)
        {
            // Standard hierarchy:
            // - Full Comment
            // - Paragraph Comment or ParamCommand comment
            // - Text Comment
            var fullComment = cursor.GetParsedComment();
            var fullCommentKind = fullComment.Kind;
            var fullCommentChildren = fullComment.GetNumChildren();

            if (fullCommentKind != CommentKind.FullComment || fullCommentChildren < 1)
            {
                return null;
            }

            StringBuilder comment = new StringBuilder();

            for (int i = 0; i < fullCommentChildren; i++)
            {
                var childComment = fullComment.GetChild(i);
                var childCommentKind = childComment.Kind;

                if (childCommentKind != CommentKind.Paragraph
                    && childCommentKind != CommentKind.ParamCommand
                    && childCommentKind != CommentKind.BlockCommand)
                {
                    continue;
                }

                StringBuilder textBuilder = new StringBuilder();
                GetCommentInnerText(childComment, textBuilder);
                string text = textBuilder.ToString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (childCommentKind == CommentKind.Paragraph)
                {
                    comment.Append(text);
                }
                else if (childCommentKind == CommentKind.BlockCommand)
                {
                    var name = childComment.GetCommandName();
                    throw new NotImplementedException();
                }
            }

            return comment.ToString();
        }

        private static void GetCommentInnerText(Comment comment, StringBuilder builder)
        {
            var commentKind = comment.Kind;

            if (commentKind == CommentKind.Text)
            {
                var text = comment.GetText();
                text = text.Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    builder.Append(" ");
                    builder.AppendLine(text);
                }
            }
            else
            {
                // Recurse
                var childCount = comment.GetNumChildren();

                for (int i = 0; i < childCount; i++)
                {
                    var child = comment.GetChild(i);
                    GetCommentInnerText(child, builder);
                }
            }
        }
    }
}
