/*====================================================================================================
    Class Name  : AssemblyDecompiler
    Created By  : Solomio S. Sisante
    Created On  : August 28, 2024
    Purpose     : To manage decompiling of DLL classes.
  ====================================================================================================*/
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class AssemblyDecompiler
    {
        private readonly string _assemblyPath;

        public AssemblyDecompiler(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }

        public string DecompileType(string typeName)
        {
            using var stream = new FileStream(_assemblyPath, FileMode.Open, FileAccess.Read);
            var module = new PEFile("Assembly", stream);

            // Use a no-op assembly resolver
            var assemblyResolver = new AssemblyResolver();
            var typeSystem = new DecompilerTypeSystem(module, assemblyResolver);

            var decompiler = new CSharpDecompiler(typeSystem, new DecompilerSettings());

            // Find the type to decompile
            var type = typeSystem.MainModule.TypeDefinitions
                .FirstOrDefault(t => t.FullName == typeName);

            if (type == null)
                throw new ArgumentException($"Type {typeName} not found in assembly.");

            var decompiledCode = decompiler.DecompileTypeAsString(type.FullTypeName);
            return decompiledCode;
        }
    }

    // Built-in implementation for AssemblyResolver
    public class AssemblyResolver : IAssemblyResolver
    {
        public PEFile? Resolve(IAssemblyReference reference)
        {
            // You can provide logic to resolve referenced assemblies here if needed
            return null; // Default behavior: no assembly resolution
        }

        public PEFile? ResolveModule(PEFile mainModule, string moduleName)
        {
            // You can provide logic to resolve modules here if needed
            return null; // Default behavior: no module resolution
        }

        public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
        {
            return Task.FromResult(Resolve(reference));
        }

        public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
        {
            return Task.FromResult(ResolveModule(mainModule, moduleName));
        }
    }
}
