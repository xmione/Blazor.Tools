/*====================================================================================================
    Class Name  : AssemblyDecompilerExtensions
    Created By  : Solomio S. Sisante
    Created On  : August 28, 2024
    Purpose     : To help in decompiling assemblies in assembly paths.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Entities;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class AssemblyDecompilerExtensions
    {
        public static string DecompileType(this string assemblyPath, string typeName)
        {
            var decompiledType = string.Empty;
            var decompiler = new AssemblyDecompiler(assemblyPath);

            decompiledType = decompiler.DecompileType(typeName);

            return decompiledType;
        }

        public static string DecompileMethod(this string assemblyPath, string typeName, string methodName)
        {
            var decompiledMethod = string.Empty;

            var decompiler = new AssemblyDecompiler(assemblyPath);

            decompiledMethod = decompiler.DecompileMethod(typeName, methodName);

            return decompiledMethod;
        }

        [Obsolete]
        public static string DecompileWholeModuleToClass(this string assemblyPath, string outputPath)
        {
            //string assemblyPath = "DynamicAssembly.dll";
            //string outputPath = "DecompiledCode.cs";

            var assemblyDecompiler = new AssemblyDecompiler(assemblyPath);
            var decompiledCode = assemblyDecompiler?.DecompileWholeAssembly() ?? string.Empty;

            // Write the decompiled code to a file
            File.WriteAllText(outputPath, decompiledCode);

            Console.WriteLine($"Decompiled code written to {outputPath}");

            return decompiledCode;
        }
    }

}
