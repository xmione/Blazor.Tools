using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace SampleBlazorLibrary.Components
{
    public class ExternalComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<InternalComponent>(0);
            builder.CloseComponent();
        }
    }
}
