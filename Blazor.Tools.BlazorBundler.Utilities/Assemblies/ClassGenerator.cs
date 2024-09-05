﻿/*====================================================================================================
    Class Name  : ClassGenerator
    Created By  : Solomio S. Sisante
    Created On  : August 31, 2024
    Purpose     : To create EmployeeVM Assembly class dynamically and save it to a .dll file.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using OutputKind = Microsoft.CodeAnalysis.OutputKind;
using Assembly = System.Reflection.Assembly;
using Blazor.Tools.BlazorBundler.Extensions;
using System.Reflection;
namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class ClassGenerator
    {
        private CSharpCompilation _compilation;

        public CSharpCompilation Compilation
        {
            get { return _compilation; }
            set { _compilation = value; }
        }

        private string _assemblyName;
        private OutputKind _outputKind;

        public OutputKind OutputKind
        {
            get { return _outputKind; }
            set { _outputKind = value; }
        }

        public ClassGenerator(string assemblyName)
        {
            _assemblyName = assemblyName;
            _outputKind = OutputKind.DynamicallyLinkedLibrary;
            _compilation = CSharpCompilation.Create(assemblyName,
                options: new CSharpCompilationOptions(_outputKind));

        }

        public void AddReference(string assemblyPath)
        {
            Console.WriteLine("Adding reference to {0}", assemblyPath);

            var reference = MetadataReference.CreateFromFile(assemblyPath);
            _compilation = _compilation?.AddReferences(reference) ?? CSharpCompilation.Create(_assemblyName).AddReferences(reference);
        }

        public void AddReference(Stream assemblyStream)
        {
            var reference = MetadataReference.CreateFromStream(assemblyStream);
            _compilation = _compilation.AddReferences(reference);
        }

        public void AddReference(byte[] assemblyBytes)
        {
            var reference = MetadataReference.CreateFromImage(assemblyBytes);
            _compilation = _compilation.AddReferences(reference);
        }

        public Type? CreateType(string classCode, string nameSpace, string className)
        {
            Type? type = null;
            var syntaxTree = CSharpSyntaxTree.ParseText(classCode);

            _compilation = _compilation.AddSyntaxTrees(syntaxTree);

            using var ms = new MemoryStream();
            var result = _compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            
            try
            {
                var types = assembly.GetTypes();
                type = assembly.GetType($"{nameSpace}.{className}");
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loaderExceptions = ex.LoaderExceptions;
                foreach (var loaderException in loaderExceptions)
                {
                    Console.WriteLine(loaderException.Message);
                }
            }

            return type;
        }

        public void SaveAssemblyToTempFolder(string fileName)
        {
            using var ms = new MemoryStream();
            var result = _compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
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
                    
                    int msLength = Convert.ToInt32(ms.Length);
                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        fs.Write(ms.ToArray(), 0, msLength);
                    }

                    success = true;
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // HResult 0x20 (ERROR_SHARING_VIOLATION)
                {
                    Console.WriteLine(ex.Message);
                    while (fileName.IsFileInUse())
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

            Console.WriteLine("Successfully compiled and saved to {0}", fileName);
        }
         
    }
}