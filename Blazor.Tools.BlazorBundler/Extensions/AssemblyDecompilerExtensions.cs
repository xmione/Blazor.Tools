/*====================================================================================================
    Class Name  : AssemblyDecompilerExtensions
    Created By  : Solomio S. Sisante
    Created On  : August 28, 2024
    Purpose     : To help in decompiling assemblies in assembly paths.
  ====================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using Blazor.Tools.BlazorBundler.Entities;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class AssemblyDecompilerExtensions
    {
        public static string DecompileType(this string assemblyPath, string typeName)
        {
            using var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read);
            var module = new PEFile("Assembly", stream);

            var assemblyResolver = new AssemblyResolver();
            var typeSystem = new DecompilerTypeSystem(module, assemblyResolver);
            var decompiler = new CSharpDecompiler(typeSystem, new DecompilerSettings());

            var type = typeSystem.MainModule.TypeDefinitions
                .FirstOrDefault(t => t.FullName == typeName);

            if (type == null)
                throw new ArgumentException($"Type {typeName} not found in assembly.");

            return decompiler.DecompileTypeAsString(type.FullTypeName);
        }
    }

}
