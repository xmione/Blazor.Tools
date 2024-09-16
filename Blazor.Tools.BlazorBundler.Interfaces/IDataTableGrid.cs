namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IDataTableGrid
    {
        Task CreateDynamicBundlerDLL();
        Task DefineConstructors(IDynamicClassBuilder vmClassBuilder, string modelVMTempDllPath);
        Task DefineMethods(IDynamicClassBuilder vmClassBuilder, Type tModelType, Type tiModelType);
        Task DefineTableColumns(string dllPath, string modelTypeName, string modelVMTypeName);
    }
}
