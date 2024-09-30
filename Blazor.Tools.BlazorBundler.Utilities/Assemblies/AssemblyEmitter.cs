using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Blazor.Tools.BlazorBundler.Entities;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class AssemblyEmitter
    {
        private byte[] _combinedAssemblyBytes;
        private Assembly _combinedAssembly;
        private ITestVM<IBase, ITestMEP> _dynamicInstance;
        public Assembly CombinedAssembly
        {
            get { return _combinedAssembly; }
        }

        public byte[] EmitAssemblyToMemory(string assemblyName, params string[] sourceCodes)
        {
            var syntaxTrees = sourceCodes.Select(code => CSharpSyntaxTree.ParseText(code)).ToList();
            var assemblyFolder = Path.GetDirectoryName(typeof(Task).Assembly.Location)!;
            var systemRuntime = Path.Combine(assemblyFolder, "System.Runtime.dll");
            var systemThreadingTasks = Path.Combine(assemblyFolder, "System.Threading.Tasks.dll");

            AppLogger.WriteInfo(systemThreadingTasks);
            var references = new List<MetadataReference>
            {

                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(systemRuntime),
                MetadataReference.CreateFromFile(systemThreadingTasks),
                MetadataReference.CreateFromFile(typeof(ITestVM<,>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    AppLogger.WriteInfo($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException("Compilation failed");
            }

            _combinedAssemblyBytes = ms.ToArray();
            return _combinedAssemblyBytes;
        }

        public void LoadAssembly()
        {
            // Load the combined assembly from memory
            _combinedAssembly = Assembly.Load(_combinedAssemblyBytes);
        }
    }
}
