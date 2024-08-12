using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class TextInput : ComponentBase
    {
        [Parameter] public string ColumnName { get; set; } = default!;
        [Parameter] public string HeaderName { get; set; } = default!;
        [Parameter] public object Value { get; set; } = default!;
        [Parameter] public bool IsEditMode { get; set; } = default!;
        [Parameter] public int RowID { get; set; } = default!;
        [Parameter] public EventCallback<object> ValueChanged { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (IsEditMode)
            {
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", "text");
                builder.AddAttribute(1, "class", "form-control");
                builder.AddAttribute(2, "value", Value);
                builder.AddAttribute(4, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleTextValueChanged));
                builder.AddAttribute(5, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, HandleOnBlur));
                builder.CloseElement();
            }
            else 
            {
                builder.OpenElement(0, "label");
                builder.AddContent(1, Value);
                builder.CloseElement();
            }
            
        }

        private void HandleTextValueChanged(ChangeEventArgs e)
        {
            Value = e?.Value?.ToString() ?? Value;
        }

        private async Task HandleOnBlur(FocusEventArgs e)
        {
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
