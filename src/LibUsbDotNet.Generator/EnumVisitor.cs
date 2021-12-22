// <copyright file="EnumVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Core.Clang;
using Core.Clang.Documentation.Doxygen;
using System.Collections.Generic;
using Enum = LibUsbDotNet.Generator.Primitives.Enum;
using EnumValue = LibUsbDotNet.Generator.Primitives.EnumValue;

namespace LibUsbDotNet.Generator
{
    internal sealed class EnumVisitor
    {
        private readonly Generator generator;

        public EnumVisitor(Generator generator)
        {
            this.generator = generator;
        }

        public ChildVisitResult Visit(Cursor cursor, Cursor parent)
        {
            if (cursor.GetLocation()?.IsFromMainFile() != true)
            {
                return ChildVisitResult.Continue;
            }

            CursorKind curKind = cursor.Kind;

            if (curKind == CursorKind.EnumDecl)
            {
                var nativeName = cursor.GetSpelling();
                var enumComment = this.GetComment(cursor);

                // enumName can be empty because of typedef enum { .. } enumName;
                // so we have to find the sibling, and this is the only way I've found
                // to do with libclang, maybe there is a better way?
                if (string.IsNullOrEmpty(nativeName))
                {
                    var forwardDeclaringVisitor = new ForwardDeclarationVisitor(cursor, skipSystemHeaderCheck: true);
                    forwardDeclaringVisitor.VisitChildren(cursor.GetLexicalParent());
                    nativeName = forwardDeclaringVisitor.ForwardDeclarationCursor.GetSpelling();

                    if (string.IsNullOrEmpty(nativeName))
                    {
                        nativeName = "_";
                    }
                }

                var clrName = NameConversions.ToClrName(nativeName, NameConversion.Type);

                Enum enumDeclaration = new Enum()
                {
                    Name = clrName,
                    Description = enumComment,
                    BaseType = "byte"
                };

                // Most enums maps to bytes, with these exceptiosn
                if (nativeName == "libusb_error" || nativeName == "libusb_capability")
                {
                    enumDeclaration.BaseType = "int";
                }

                // Normally the enum values are prefixed with the enum name, e.g.
                // enum log_level => LOG_LEVEL_DEBUG = 1
                //
                // However, this is not always consistent, e.g.
                // enum libusb_capability => LIBUSB_CAP_HAS_CAPABILITY = 1
                //
                // Patch the prefix where required.
                var prefix = nativeName;

                var mappings = new Dictionary<string, string>();
                mappings.Add("libusb_capability", "libusb_cap");
                mappings.Add("libusb_class_code", "libusb_class");
                mappings.Add("libusb_descriptor_type", "libusb_dt");
                mappings.Add("libusb_endpoint_direction", "libusb_endpoint");
                mappings.Add("libusb_hotplug_flag", "libusb_hotplug");
                mappings.Add("libusb_request_recipient", "libusb_recipient");
                mappings.Add("libusb_standard_request", "libusb_request");
                mappings.Add("libusb_transfer_flags", "libusb_transfer");
                mappings.Add("libusb_transfer_status", "libusb_transfer");
                mappings.Add("libusb_bos_type", "libusb_bt");

                if (mappings.ContainsKey(prefix))
                {
                    prefix = mappings[prefix];
                }

                // visit all the enum values
                DelegatingCursorVisitor visitor = new DelegatingCursorVisitor(
                    (c, vistor) =>
                    {
                        var value = c.GetEnumConstantDeclValue();

                        var field =
                            new EnumValue()
                            {
                                Name = NameConversions.ToClrName(c.GetSpelling(), prefix, NameConversion.Enum),
                                Value = value > 0 ? $"0x{(int)c.GetEnumConstantDeclValue():X}" : $"{value}"
                            };

                        field.Description = this.GetComment(c);

                        enumDeclaration.Values.Add(field);
                        return ChildVisitResult.Continue;
                    });
                visitor.VisitChildren(cursor);

                // Add a missing 'None' value
                if (nativeName == "libusb_transfer_flags")
                {
                    enumDeclaration.Values.Add(new EnumValue()
                    {
                        Name = "None",
                        Value = "0x0"
                    });
                }

                // Add variants which were defined with #define
                if (nativeName == "libusb_hotplug_flag")
                {
                    enumDeclaration.Values.Add(new EnumValue()
                    {
                        Name = "NoFlags",
                        Value = "0x0",
                        Description = "Default value when not using any flags.",
                    });
                    enumDeclaration.Values.Add(new EnumValue()
                    {
                        Name = "MatchAny",
                        Value = "0xFF",
                        Description = "Wildcard matching for hotplug events.",
                    });
                }

                this.generator.AddType(nativeName, enumDeclaration);
            }

            return ChildVisitResult.Recurse;
        }

        private string GetComment(Cursor cursor)
        {
            return Comment.FromCursor(cursor).GetCommentInnerText();
        }
    }
}
