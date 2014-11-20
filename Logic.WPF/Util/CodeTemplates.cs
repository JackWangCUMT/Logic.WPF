using Logic.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util
{
    public class CodeTemplates
    {
        public class Imports
        {
            [ImportMany(typeof(ITemplate))]
            public IList<ITemplate> Templates { get; set; }
        }

        public static IList<ITemplate> Import(string csharp)
        {
            var sw = Stopwatch.StartNew();

            var imports = new Imports() { Templates = new List<ITemplate>() };

            var assemblyPath = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
            var executingPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var references = new[] 
            {
                MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(executingPath, "Logic.Core.dll")),
                MetadataReference.CreateFromFile((Assembly.GetEntryAssembly().Location))
            };

            var syntaxTree = CSharpSyntaxTree.ParseText(csharp);
            var compilation = CSharpCompilation.Create(
                "Imports",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    assemblyIdentityComparer : DesktopAssemblyIdentityComparer.Default));

            using (var ms = new System.IO.MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    var assembly = Assembly.Load(ms.GetBuffer());

                    var builder = new RegistrationBuilder();
                    builder.ForTypesDerivedFrom<ITemplate>().Export<ITemplate>();

                    var catalog = new AggregateCatalog();
                    catalog.Catalogs.Add(new AssemblyCatalog(assembly, builder));

                    var container = new CompositionContainer(catalog);
                    container.ComposeParts(imports);

                    sw.Stop();
                    Log.LogInformation("Roslyn code import: " + sw.Elapsed.TotalMilliseconds + "ms");

                    return imports.Templates;
                }
                else
                {
                    Log.LogError("Failed to compile code using Roslyn.");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Log.LogError(diagnostic.Description);
                    }
                }
            }

            return null;
        }
    }
}
