using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DetailDisplay : ComponentBase
    {
        // Cascading parameter to get ConfigParameter from TestComponent
        [CascadingParameter] public string ConfigParameter { get; set; } = "DefaultConfig";

        [Parameter] public int Value { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, $"Detail Display Value: {Value}");
            builder.AddContent(2, $" | Config from Parent: {ConfigParameter}");
            builder.CloseElement();
        }
    }
}
