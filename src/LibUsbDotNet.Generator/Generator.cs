using Core.Clang;
using Core.Clang.Diagnostics;
using LibUsbDotNet.Generator.Primitives;
using Nustache.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Delegate = LibUsbDotNet.Generator.Primitives.Delegate;
using Enum = LibUsbDotNet.Generator.Primitives.Enum;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree;
using SyntaxNodeExtensions = Microsoft.CodeAnalysis.SyntaxNodeExtensions;

namespace LibUsbDotNet.Generator
{
    public class Generator
    {
        public string InputFile
        {
            get;
            set;
        }

        public Collection<string> IncludeDirectories
        { get; } = new Collection<string>();

        public string TargetDirectory
        {
            get;
            set;
        }

        public Dictionary<string, IPrimitive> Types { get; } = new Dictionary<string, IPrimitive>();

        public Dictionary<string, Method> Methods { get; } = new Dictionary<string, Method>();

        public IPrimitive AddType(string nativeName, IPrimitive type)
        {
            if (this.Types.ContainsKey(nativeName))
            {
                return this.Types[nativeName];
            }
            else
            {
                this.Types.Add(nativeName, type);
                return type;
            }
        }

        public void AddMethod(string nativeName, Method method)
        {
            if (this.Methods.ContainsKey(nativeName))
            {
                return;
            }

            this.Methods.Add(nativeName, method);
        }

        public void GenerateTypes()
        {
            string[] arguments =
            {
                // Use the C++ backend
                "-x",
                "c++",

                // Parse the doxygen comments
                "-Wdocumentation",
            };

            arguments = arguments.Concat(this.IncludeDirectories.Select(x => "-I" + x)).ToArray();

            using (var createIndex = new Index(false, true))
            using (var translationUnit = createIndex.ParseTranslationUnit(this.InputFile, arguments))
            {
                StringWriter errorWriter = new StringWriter();
                var set = DiagnosticSet.FromTranslationUnit(translationUnit);
                var numDiagnostics = set.GetNumDiagnostics();

                bool hasError = false;
                bool hasWarning = false;

                for (uint i = 0; i < numDiagnostics; ++i)
                {
                    Diagnostic diagnostic = set.GetDiagnostic(i);
                    var severity = diagnostic.GetSeverity();

                    switch (severity)
                    {
                        case DiagnosticSeverity.Error:
                        case DiagnosticSeverity.Fatal:
                            hasError = true;
                            break;

                        case DiagnosticSeverity.Warning:
                            hasWarning = true;
                            break;
                    }

                    var location = diagnostic.GetLocation();
                    var fileName = location.SourceFile;
                    var line = location.Line;

                    var message = diagnostic.GetSpelling();
                    errorWriter.WriteLine($"{severity}: {fileName}:{line} {message}");
                }

                if (hasError)
                {
                    throw new Exception(errorWriter.ToString());
                }

                if (hasWarning)
                {
                    // Dump the warnings to the console output.
                    Console.WriteLine(errorWriter.ToString());
                }

                // Creates enums
                var enumVisitor = new EnumVisitor(this);
                var realEnumVisitor = new DelegatingCursorVisitor(enumVisitor.Visit);
                realEnumVisitor.VisitChildren(translationUnit.GetCursor());

                // Creates handles
                var typeDefVisitor = new TypeDefVisitor(this);
                var realTypeDefVisitor = new DelegatingCursorVisitor(typeDefVisitor.Visit);
                realTypeDefVisitor.VisitChildren(translationUnit.GetCursor());

                // Creates C# methods for C functions
                var functionVisitor = new FunctionVisitor(this);
                var realFunctionVisitor = new DelegatingCursorVisitor(functionVisitor.Visit);
                realFunctionVisitor.VisitChildren(translationUnit.GetCursor());

                // Creates C# strucs for C structs
                var structVisitor = new StructVisitor(this);
                var realStructVisitor = new DelegatingCursorVisitor(structVisitor.Visit);
                realStructVisitor.VisitChildren(translationUnit.GetCursor());
            }
        }

        public void WriteTypes()
        {
            // Generate the various types
            foreach (var type in this.Types)
            {
                if (type.Value is Enum)
                {
                    Write("Enum.cs.template", type.Value, $"{type.Value.Name}.cs");
                }

                if (type.Value is SafeHandle)
                {
                    Write("SafeHandle.cs.template", type.Value, $"{type.Value.Name}.cs");
                }

                if (type.Value is Delegate)
                {
                    Write("Delegate.cs.template", type.Value, $"{type.Value.Name}.cs");
                }

                if (type.Value is Struct)
                {
                    Write("Struct.cs.template", type.Value, $"{type.Value.Name}.cs");
                }
            }

            // Generate an NativeMethods file
            Write("NativeMethods.cs.template", this.Methods.Values, "NativeMethods.cs");
        }

        private void Write(string templateName, object input, string outputName)
        {
            var code = Render.FileToString(templateName, input);
            var path = Path.Combine(this.TargetDirectory, outputName);
            File.WriteAllText(path, code);
        }
    }
}
