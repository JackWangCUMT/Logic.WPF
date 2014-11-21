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
    public class CSharpCodeImporter
    {
        public static PortableExecutableReference[] GetReferences()
        {
            var assemblyPath = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
            var executingPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return new[]
            {
                MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(executingPath, "Logic.Core.dll")),
                MetadataReference.CreateFromFile((Assembly.GetEntryAssembly().Location))
            };
        }

        public static void Compose<T>(Assembly assembly, object part)
        {
            var builder = new RegistrationBuilder();
            builder.ForTypesDerivedFrom<T>().Export<T>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(assembly, builder));

            var container = new CompositionContainer(catalog);
            container.ComposeParts(part);
        }

        public static bool Import<T>(string csharp, object part)
        {
            var sw = Stopwatch.StartNew();

            var references = GetReferences();
            var syntaxTree = CSharpSyntaxTree.ParseText(csharp);
            var compilation = CSharpCompilation.Create(
                "Imports",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            using (var ms = new System.IO.MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    Assembly assembly = Assembly.Load(ms.GetBuffer());
                    if (assembly != null)
                    {
                        Compose<T>(assembly, part);

                        sw.Stop();
                        Log.LogInformation("Roslyn code import: " + sw.Elapsed.TotalMilliseconds + "ms");

                        return true;
                    }
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
            return false;
        }
    }
}
