using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IDataTableGrid
    {
        Task InitializeVariablesAsync();
        Task RenderMainContentAsync(RenderTreeBuilder builder);
        Task CreateDynamicBundlerDLLAsync();
        Task DefineConstructorsAsync(IDynamicClassBuilder vmClassBuilder, string modelVMTempDllPath);
        Task DefineMethodsAsync(IDynamicClassBuilder vmClassBuilder, Type tModelType, Type tiModelType);
        Task DefineTableColumnsAsync(string dllPath, string modelTypeName, string modelVMTypeName);
    }
}
