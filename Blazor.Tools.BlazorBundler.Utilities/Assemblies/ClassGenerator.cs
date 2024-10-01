/*====================================================================================================
    Class Name  : ClassGenerator
    Created By  : Solomio S. Sisante
    Created On  : August 31, 2024
    Purpose     : To create EmployeeVM DisposableAssembly class dynamically and save it to a .dll file.
  ====================================================================================================*/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OutputKind = Microsoft.CodeAnalysis.OutputKind;
using Assembly = System.Reflection.Assembly;
using Blazor.Tools.BlazorBundler.Extensions;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using ICSharpCode.Decompiler.CSharp.Syntax;
using SyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class ClassGenerator : IDisposable
    {

        private string? _assemblyName;
        private bool _disposed = false;
        private byte[] _assemblyBytes;
        private Assembly? _assembly;

        public Assembly? Assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        private CSharpCompilation? _compilation;

        public CSharpCompilation? Compilation
        {
            get { return _compilation; }
            set { _compilation = value; }
        }

        public byte[] AssemblyBytes
        {
            get { return _assemblyBytes; }
        }

        private OutputKind _outputKind;

        public OutputKind OutputKind
        {
            get { return _outputKind; }
            set { _outputKind = value; }
        }

        public ClassGenerator(string assemblyName, string version = "1.0.0.0", OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            _assemblyName = assemblyName;
            _outputKind = outputKind;

            var assemblyAttributes = new[]
            {
                $"[assembly: System.Reflection.AssemblyVersion(\"{version}\")]",
                $"[assembly: System.Reflection.AssemblyFileVersion(\"{version}\")]"
            };

            var assemblyInfoSyntax = string.Join(Environment.NewLine, assemblyAttributes);

            var compilationOptions = new CSharpCompilationOptions(_outputKind, moduleName: assemblyName);

            var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(assemblyInfoSyntax) };
            
            // Include assembly attributes in the compilation
            _compilation = CSharpCompilation.Create(
                assemblyName: assemblyName,
                options: compilationOptions,
                syntaxTrees: syntaxTrees);
        }

        public void AddReference(string assemblyPath)
        {
            AppLogger.WriteInfo($"Adding reference to {assemblyPath}" );

            var reference = MetadataReference.CreateFromFile(assemblyPath);
            _compilation = _compilation?.AddReferences(reference) ?? CSharpCompilation.Create(_assemblyName).AddReferences(reference);
        }

        public void AddReference(Stream assemblyStream)
        {
            var reference = MetadataReference.CreateFromStream(assemblyStream);
            _compilation = _compilation?.AddReferences(reference);
        }

        public void AddReference(byte[] assemblyBytes)
        {
            var reference = MetadataReference.CreateFromImage(assemblyBytes);
            _compilation = _compilation?.AddReferences(reference);
        }
         
        public Type? CreateType(string nameSpace, string className, params string[] sourceCodes)
        {
            Type? type = null;

            try
            {
                var syntaxTrees = sourceCodes.Select(code => CSharpSyntaxTree.ParseText(code)).ToList();

                _compilation = _compilation?.AddSyntaxTrees(syntaxTrees);

                using var ms = new MemoryStream();
                var result = _compilation?.Emit(ms);

                if (result != null && !result.Success)
                {
                    var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                    foreach (var diagnostic in failures)
                    {
                        AppLogger.WriteInfo($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }

                    throw new CompilationException(result.Diagnostics);
                }

                ms.Seek(0, SeekOrigin.Begin);
                _assemblyBytes = ms.ToArray();
                _assembly = Assembly.Load(_assemblyBytes);

                try
                {
                    var types = _assembly.GetTypes();
                    type = _assembly.GetType($"{nameSpace}.{className}");
                    AppLogger.WriteInfo($"Module name: {_assembly?.GetModules().FirstOrDefault()?.Name ?? "<Unknown Module>"}");
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var loaderExceptions = ex.LoaderExceptions;
                    foreach (var loaderException in loaderExceptions)
                    {
                        AppLogger.WriteInfo(loaderException?.Message!);
                    }
                }

            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
            }

            return type;
        }

        //public void SaveAssemblyToTempFolder(string dllPath)
        //{
        //    using (var fs = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
        //    {
        //        var result = _compilation?.Emit(fs);
        //        if (result == null || !result.Success)
        //        {
        //             Handle compilation errors
        //            throw new CompilationException(result?.Diagnostics!);
        //        }
        //    }
        //}

        public void SaveAssemblyToTempFolder(string fileName)
        {
            using var ms = new MemoryStream();
            var result = _compilation?.Emit(ms);

            if (result != null && !result.Success)
            {
                var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    AppLogger.WriteInfo($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            bool success = false;
            int retries = 3;
            while (!success && retries > 0)
            {
                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        ms.WriteTo(fs); // Write the MemoryStream contents directly to the FileStream
                        fs.Flush(true);  // Ensure all data is written and buffers are cleared
                    }

                    while (fileName.IsFileInUse()) // Ensure no processes are locking the file
                    {
                        Thread.Sleep(1000);
                    }

                    success = true;
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // HResult 0x20 (ERROR_SHARING_VIOLATION)
                {
                    AppLogger.WriteInfo(ex.Message);
                    while (fileName.IsFileInUse()) // Ensure no processes are locking the file
                    {
                        fileName.KillLockingProcesses();
                        Thread.Sleep(1000);
                    }

                    retries--;
                    Thread.Sleep(1000); // Wait for 1 second before retrying
                }
            }

            if (!success)
            {
                throw new IOException("Failed to write the file after multiple attempts.");
            }

            AppLogger.WriteInfo($"Successfully compiled and saved to {fileName}");
        }

        // Implement IDisposable to clean up resources
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected method for cleanup logic
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Perform any necessary cleanup here
                    _compilation = null;
                    _assemblyName = null;
                    _assembly = null;
                }

                _disposed = true;
            }
        }

        // Destructor to ensure resources are cleaned up
        ~ClassGenerator()
        {
            Dispose(false);
        }
    }
}