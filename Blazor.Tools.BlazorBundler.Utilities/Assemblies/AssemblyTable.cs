using Blazor.Tools.BlazorBundler.Extensions;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class AssemblyTable
    {
        public int ID { get; set; }
        public string AssemblyName { get; set; } = default!;
        public string AssemblyPath { get; set; } = default!;
        public string ServiceName { get; set; } = default!;
        public string ServicePath { get; set; } = default!;
        public bool LoadAssemblyFromDLLFile { get; set; } = default!;
        public string TypeName { get; set; } = default!;
        public string TableName { get; set; } = default!;
        public bool IsInterface { get; set; }

        public IEnumerable<string> GetPropertyNames()
        {
            Assembly? assembly = null;
            var typeName = string.Join(".", AssemblyName, TypeName);

            if (LoadAssemblyFromDLLFile)
            {
                assembly = AssemblyPath.LoadAssemblyFromDLLFile();
            }
            else
            {
                assembly = AssemblyName.LoadAssemblyFromName();
            }

            var properties = assembly.GetProperties(typeName, IsInterface);

            return properties;
        }
    }
}
