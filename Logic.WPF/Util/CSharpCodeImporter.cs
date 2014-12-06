using Logic.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
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

        public static IEnumerable<T> Compose<T>(Assembly assembly)
        {
            var builder = new ConventionBuilder();
            builder.ForTypesDerivedFrom<T>().Export<T>();

            var configuration = new ContainerConfiguration()
                .WithAssembly(assembly)
                .WithDefaultConventions(builder);

            using (var container = configuration.CreateContainer())
            {
                return container.GetExports<T>();
            }
        }

        public static IEnumerable<T> Import<T>(string csharp, ILog log)
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
                        var exports = Compose<T>(assembly);

                        sw.Stop();
                        log.LogInformation("Roslyn code import: " + sw.Elapsed.TotalMilliseconds + "ms");

                        return exports;
                    }
                }
                else
                {
                    log.LogError("Failed to compile code using Roslyn.");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        log.LogError(diagnostic.Description);
                    }
                }
            }
            return null;
        }
    }
}
