using Blazor.Tools.BlazorBundler.Extensions;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class AssemblyTable
    {
        public int ID { get; set; }
        public string AssemblyName { get; set; } = default!;
        public string TypeName { get; set; } = default!;
        public string TableName { get; set; } = default!;
        public bool IsInterface { get; set; }

        public IEnumerable<string> GetPropertyNames() 
        {
            var typeName = string.Join(AssemblyName, ".", TypeName);
            var properties = ReflectionExtensions.GetProperties(AssemblyName, typeName, IsInterface);

            return properties;
        }
    }
}
