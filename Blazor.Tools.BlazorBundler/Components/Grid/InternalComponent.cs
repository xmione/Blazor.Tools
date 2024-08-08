using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    internal class InternalComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "h1");
            builder.AddMarkupContent(1, $"Hello Blazor at {DateTime.Now.ToLongTimeString()} ");
            builder.CloseElement();
        }

        protected override bool ShouldRender()
        {
            Debug.WriteLine("InternalComponent: ShouldRender");
            // Control the rendering logic here
            return true;
        }
    }
}
