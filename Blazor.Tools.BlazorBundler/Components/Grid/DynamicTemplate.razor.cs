using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DynamicTemplate : ComponentBase
    {
        [Parameter] public string ColumnName { get; set; } = default!;
        [Parameter] public object Value { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, Value);
            builder.CloseElement();
        }
    }
}
