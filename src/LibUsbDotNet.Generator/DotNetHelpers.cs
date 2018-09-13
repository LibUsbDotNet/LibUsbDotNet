// <copyright file="NameConversions.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Nustache.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibUsbDotNet.Generator
{
    public static class DotNetHelpers
    {
        public static void Register()
        {
            Helpers.Register(nameof(ToXmlDoc), ToXmlDoc);
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
    }
}
