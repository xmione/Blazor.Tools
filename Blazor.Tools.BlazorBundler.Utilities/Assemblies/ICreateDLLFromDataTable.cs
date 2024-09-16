using System.Data;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public interface ICreateDLLFromDataTable
    {
        public string ContextAssemblyName { get; }
        public string DLLPath { get; }
        public void BuildAndSaveAssembly(DataTable dataTable);
    }
}
