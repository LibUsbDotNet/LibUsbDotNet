// <copyright file="FunctionVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using LibUsbDotNet.Generator.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Core.Clang.Documentation.Doxygen;

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

            // Most methods return ints to indicate whether an operation completed successfully.
            // The int is value which is defined in the libusb_error enum. Thus, cast the result
            // to Error.
            // There are a couple of exceptions, for which we default to returning an int and
            // let the caller decide what to do.

            Collection<string> methodsThatReturnInt = new Collection<string>()
            {
                "libusb_has_capability", // => true/false
                "libusb_kernel_driver_active", // => true/false
                "libusb_control_transfer", // => > 0: # of bytes, < 0: error
                "libusb_try_lock_events", // => true/false
                "libusb_event_handling_ok", // => true/false
                "libusb_event_handler_active", // => true/false
                "libusb_wait_for_event", // => true/false
                "libusb_get_device_speed", // => libusb_speed
                "libusb_get_max_packet_size",
                "libusb_get_max_iso_packet_size"
            };

            // Methods that return ints should return Error. Exceptions:
            if (method.ReturnType == "int" && !methodsThatReturnInt.Contains(nativeName))
            {
                method.ReturnType = "Error";
            }

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

            var pinvokeMethods = new Collection<string>()
            {
                "libusb_exit",
                "libusb_unref_device",
                "libusb_close"
            };

            var functionKind = FunctionKind.Default;

            if (pinvokeMethods.Contains(nativeName))
            {
                functionKind = FunctionKind.Close;
            }

            int numArgTypes = functionType.GetNumArgTypes();

            for (uint i = 0; i < numArgTypes; ++i)
            {
                var argument = ArgumentGenerator.GenerateArgument(this.generator, functionKind, cursor, i);
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
            var fullComment = Comment.FromCursor(cursor);
            var fullCommentKind = fullComment.GetKind();
            var fullCommentChildren = fullComment.GetNumChildren();

            if (fullCommentKind != CommentKind.FullComment || fullCommentChildren < 1)
            {
                return;
            }

            for (uint i = 0; i < fullCommentChildren; i++)
            {
                var childComment = fullComment.GetChild(i);
                var childCommentKind = childComment.GetKind();

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
                else if (childCommentKind == CommentKind.ParamCommand && childComment is ParamCommandComment paramCommandComment)
                {
                    // Get the parameter name
                    var paramName = paramCommandComment.GetParamName();
                    var param = method.Arguments.Single(p => p.NativeName == paramName);
                    param.Documentation += text;
                }
                else if (childCommentKind == CommentKind.BlockCommand && childComment is BlockCommandComment blockCommandComment)
                {
                    var name = blockCommandComment.GetCommandName();

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
            var commentKind = comment.GetKind();

            if (commentKind == CommentKind.Text)
            {
                var text = comment.GetNormalizedText();
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

                for (uint i = 0; i < childCount; i++)
                {
                    var child = comment.GetChild(i);
                    this.GetCommentInnerText(child, builder);
                }
            }
        }
    }
}