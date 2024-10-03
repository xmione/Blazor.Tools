using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITableGridInternals
    {
        Task InitializeVariablesAsync();
        Task RenderMainContentAsync(RenderTreeBuilder builder);
    }
}
