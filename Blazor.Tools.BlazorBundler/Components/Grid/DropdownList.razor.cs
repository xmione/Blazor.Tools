using Blazor.Tools.BlazorBundler.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class DropdownList<TItem> : ComponentBase
    {

        [Parameter] public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();
        [Parameter] public string RowID { get; set; } = default!;
        [Parameter] public string ColumnName { get; set; } = default!;
        [Parameter] public string HeaderName { get; set; } = default!;
        [Parameter] public object Value { get; set; } = default!;
        [Parameter] public string OptionIDFieldName { get; set; } = default!;
        [Parameter] public string OptionValueFieldName { get; set; } = default!;
        [Parameter] public bool IsEditMode { get; set; } = default!;
        [Parameter] public EventCallback<object> ValueChanged { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            if (IsEditMode)
            {
                builder.OpenElement(seq++, "select");
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleSelectedChangeAsync));

                foreach (TItem item in Items)
                { 
                    var optionID = item?.GetProperty(OptionIDFieldName)?.ToString();
                    var optionValue = item?.GetProperty(OptionValueFieldName)?.ToString();

                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", optionID);
                    if (optionID == Value.ToString())
                    {
                        builder.AddAttribute(seq++, "selected", "selected");
                    }

                    builder.AddContent(seq++, optionValue);
                                builder.CloseElement();
                }


                builder.CloseElement();
            }
            else 
            {

                var selectedItem = Items.FirstOrDefault(item => isFound(item));

                if (selectedItem == null)
                {
                    builder.OpenElement(0, "label");
                    builder.AddContent(1, Value);
                    builder.CloseElement();
                }
                else 
                {
                    builder.OpenElement(0, "label");
                    builder.AddContent(1, selectedItem.GetProperty(OptionValueFieldName));
                    builder.CloseElement();
                }
                
            }
        }

        private bool isFound(TItem? item)
        {
            bool isFound = false;
            var propertyValue = item?.GetProperty(OptionIDFieldName);
            if (propertyValue != null)
            {
                if (propertyValue.ToString() == Value.ToString())
                {
                    isFound = true;
                }
            }

            return isFound;
        }
        private async Task HandleSelectedChangeAsync(ChangeEventArgs e)
        {
            Value = e?.Value ?? Value;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
