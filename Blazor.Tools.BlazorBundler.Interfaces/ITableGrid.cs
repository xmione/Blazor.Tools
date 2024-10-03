using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITableGrid
    {
        Task InitializeVariablesAsync();
        Task RenderMainContentAsync(RenderTreeBuilder builder);
    }
}
