namespace Blazor.Tools.BlazorBundler.Entities
{
    public class HostAssemblies
    {
        public string ModelsAssemblyName { get; set; } = default!;
        public string ModelsAssemblyPath { get; set; } = default!;
        public string ServicesAssemblyName { get; set; } = default!;
        public string ServicesAssemblyPath { get; set; } = default!;
        public bool LoadAssemblyFromDLLFile { get; set; }
        public bool IsInterface { get; set; }
    }
}
