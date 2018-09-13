// <copyright file="FunctionVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using LibUsbDotNet.Generator.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LibUsbDotNet.Generator
{
    internal sealed class FunctionVisitor
    {
        private readonly Generator generator;

        public FunctionVisitor(Generator generator)
        {
            this.generator = generator;
        }

        public ChildVisitResult Visit(Cursor cursor, Cursor parent)
        {
            // We only care about function declarations in the main file.
            if (!cursor.GetLocation().IsFromMainFile())
            {
                return ChildVisitResult.Continue;
            }

            // We can't P/Invoke inlined functions.
            if (cursor.IsInlinedFunction())
            {
                return ChildVisitResult.Continue;
            };

            CursorKind curKind = cursor.Kind;

            if (curKind == CursorKind.UnexposedDecl)
            {
                return ChildVisitResult.Recurse;
            }

            if (curKind == CursorKind.FunctionDecl)
            {
                var function = this.ExtractFunction(cursor);
                this.generator.AddMethod(cursor.GetDisplayName(), function);
                return ChildVisitResult.Continue;
            }

            return ChildVisitResult.Continue;
        }

        private Method ExtractFunction(Cursor cursor)
        {
            var functionType = cursor.GetTypeInfo();
            var nativeName = cursor.GetSpelling();
            var resultType = cursor.GetResultType();

            Method method = new Method();
            method.ReturnType = resultType.ToClrType();
            method.NativeName = nativeName;

            Dictionary<string, string> nameMappings = new Dictionary<string, string>();
            nameMappings.Add("libusb_strerror", "StrError");
            nameMappings.Add("libusb_setlocale", "SetLocale");

            if (nameMappings.ContainsKey(nativeName))
            {
                method.Name = nameMappings[nativeName];
            }
            else
            {
                method.Name = NameConversions.ToClrName(nativeName, NameConversion.Function);
            }

            int numArgTypes = functionType.GetNumArgTypes();

            for (uint i = 0; i < numArgTypes; ++i)
            {
                var argument = ArgumentGenerator.GenerateArgument(this.generator, cursor, i);
                method.Arguments.Add(argument);
            }

            this.GetComment(cursor, method);

            return method;
        }

        private void GetComment(Cursor cursor, Method method)
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
                return;
            }

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
                this.GetCommentInnerText(childComment, textBuilder);
                string text = textBuilder.ToString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (childCommentKind == CommentKind.Paragraph)
                {
                    method.Documentation += text;
                }
                else if (childCommentKind == CommentKind.ParamCommand)
                {
                    // Get the parameter name
                    var paramName = childComment.GetParamName();
                    var param = method.Arguments.Single(p => p.NativeName == paramName);
                    param.Documentation += text;
                }
                else if (childCommentKind == CommentKind.BlockCommand)
                {
                    var name = childComment.GetCommandName();

                    if (name == "note")
                    {
                        method.Remarks += text;
                    }
                    else if (name == "return")
                    {
                        method.ReturnValueDocumentation += text;
                    }
                }
            }
        }

        private void GetCommentInnerText(Comment comment, StringBuilder builder)
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
                    this.GetCommentInnerText(child, builder);
                }
            }
        }
    }
}