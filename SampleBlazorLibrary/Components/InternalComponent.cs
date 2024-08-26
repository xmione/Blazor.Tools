using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;

namespace SampleBlazorLibrary.Components
{
    internal class InternalComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "h1");
            builder.AddMarkupContent(1, $"Hello Blazor at {DateTime.Now.ToLongTimeString()} ");
            builder.CloseElement();
        }
    }
}
