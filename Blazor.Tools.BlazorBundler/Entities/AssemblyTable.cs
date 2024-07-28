using Blazor.Tools.BlazorBundler.Extensions;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class AssemblyTable
    {
        public int ID { get; set; }
        public string AssemblyName { get; set; } = default!;
        public string AssemblyPath { get; set; } = default!;
        public bool LoadAssemblyFromDLLFile { get; set; } = default!;
        public string TypeName { get; set; } = default!;
        public string TableName { get; set; } = default!;
        public bool IsInterface { get; set; }

        public IEnumerable<string> GetPropertyNames(bool loadAssemblyFromDLLFile = false) 
        {
            Assembly? assembly = null;
            var typeName = string.Join(AssemblyName, ".", TypeName);
            if (loadAssemblyFromDLLFile)
            {
                assembly = ReflectionExtensions.LoadAssemblyFromDLLFile(AssemblyPath);
            }
            else 
            {
                assembly = ReflectionExtensions.LoadAssemblyFromName(AssemblyName);
            }
            
            var properties = ReflectionExtensions.GetProperties(assembly, typeName, IsInterface);

            return properties;
        }
    }
}
