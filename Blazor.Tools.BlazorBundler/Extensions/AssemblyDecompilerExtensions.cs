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
    }

}
