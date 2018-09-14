// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using LibUsbDotNet.Generator.Primitives;
using Nustache.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace LibUsbDotNet.Generator
{
    public static class DotNetHelpers
    {
        public static void Register()
        {
            Helpers.Register(nameof(ToXmlDoc), ToXmlDoc);
            Helpers.Register(nameof(ArgumentSeparator), ArgumentSeparator);
        }

        static void ToXmlDoc(RenderContext context, IList<object> arguments, IDictionary<string, object> options, RenderBlock fn, RenderBlock inverse)
        {
            if (arguments != null && arguments.Count > 0 && arguments[0] != null && arguments[0] is string)
            {
                int indent = 8;

                if (arguments.Count > 1 && int.TryParse((string)arguments[1], out int parsedValue))
                {
                    indent = parsedValue;
                }

                string indentation = new string(' ', indent);
                bool isFirst = true;

                using (StringReader reader = new StringReader(arguments[0] as string))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!isFirst)
                        {
                            context.Write(Environment.NewLine);
                            context.Write(indentation);
                        }
                        else
                        {
                            isFirst = false;
                        }

                        context.Write("/// ");
                        context.Write(line);
                    }
                }
            }
        }

        static void ArgumentSeparator(RenderContext context, IList<object> arguments, IDictionary<string, object> options, RenderBlock fn, RenderBlock inverse)
        {
            if (arguments != null && arguments.Count > 1 && arguments[0] is Collection<Argument> && arguments[1] is Argument)
            {
                context.Write(ArgumentSeparator(arguments[0] as Collection<Argument>, arguments[1] as Argument));
            }
        }

        static string ArgumentSeparator(Collection<Argument> arguments, Argument argument)
        {
            if (arguments.IndexOf(argument) < arguments.Count - 1)
            {
                return ", ";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
