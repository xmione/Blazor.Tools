using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class ExternalComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<InternalComponent>(0);
            builder.CloseComponent();
        }

        protected override bool ShouldRender()
        {
            Debug.WriteLine("ExternalComponent: ShouldRender");
            // Control the rendering logic here
            return true;
        }
    }
}
