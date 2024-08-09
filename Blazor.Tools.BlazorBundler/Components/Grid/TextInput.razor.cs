using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class TextInput : ComponentBase
    {
        [Parameter] public string RowID { get; set; } = default!;
        [Parameter] public string ColumnName { get; set; } = default!;
        [Parameter] public string HeaderName { get; set; } = default!;
        [Parameter] public object Value { get; set; } = default!;
        [Parameter] public bool IsEditMode { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (IsEditMode)
            {
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", "text");
                builder.AddAttribute(2, "value", Value);
                builder.CloseElement();
            }
            else 
            {
                builder.OpenElement(0, "label");
                builder.AddContent(1, Value);
                builder.CloseElement();
            }
            
        }
    }
}
