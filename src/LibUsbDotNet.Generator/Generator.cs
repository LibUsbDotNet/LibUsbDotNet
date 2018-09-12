using Core.Clang;
using Core.Clang.Diagnostics;
using LibUsbDotNet.Generator.Primitives;
using Nustache.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        public void AddType(string nativeName, IPrimitive type)
        {
            if (Types.ContainsKey(nativeName))
            {
                return;
            }

            this.Types.Add(nativeName, type);
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
                var cursor = translationUnit.GetCursor();
                realEnumVisitor.VisitChildren(cursor);
            }
        }

        public void WriteTypes()
        {
            foreach (var type in this.Types)
            {
                if (type.Value is Enum)
                {
                    var code = Render.FileToString("Enum.cs.template", type.Value);
                    var path = Path.Combine(this.TargetDirectory, $"{type.Value.Name}.cs");

                    var tree = CSharpSyntaxTree.ParseText(code);
                    var root = SyntaxNodeExtensions.NormalizeWhitespace(tree.GetRoot());
                    code = root.ToFullString();
                    File.WriteAllText(path, code);
                }
            }
        }
    }
}
