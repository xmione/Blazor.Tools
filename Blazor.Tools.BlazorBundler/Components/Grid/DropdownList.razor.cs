using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class DropdownList : ComponentBase, IDropdownList
    {

        [Parameter] public IEnumerable<object> Items { get; set; } = Enumerable.Empty<object>();
        [Parameter] public string ColumnName { get; set; } = default!;
        [Parameter] public string HeaderName { get; set; } = default!;
        [Parameter] public object? Value { get; set; } = default!;
        [Parameter] public string OptionIDFieldName { get; set; } = default!;
        [Parameter] public string OptionValueFieldName { get; set; } = default!;
        [Parameter] public bool IsEditMode { get; set; } = default!;
        [Parameter] public int RowID { get; set; } = default!;
        [Parameter] public EventCallback<object> ValueChanged { get; set; }

        private bool _hasSelectedItem;
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;

            if (IsEditMode)
            {
                builder.OpenElement(seq++, "select");
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleSelectedChangeAsync));

                bool isFirstItemSelected = true;
                string? selectedValueString = Value?.ToString();

                foreach (var item in Items)
                {
                    var optionID = item?.GetProperty(OptionIDFieldName);
                    var optionValue = item?.GetProperty(OptionValueFieldName)?.ToString() ?? string.Empty;

                    var optionIDString = optionID?.ToString();
                    bool isSelected = IsSelectedValue(optionIDString, Value);

                    if (isSelected || (isFirstItemSelected && IsDefaultValue(Value)))
                    {
                        builder.OpenElement(seq++, "option");
                        builder.AddAttribute(seq++, "value", optionIDString);
                        builder.AddAttribute(seq++, "selected", "selected");
                        builder.AddContent(seq++, optionValue);
                        builder.CloseElement();
                        isFirstItemSelected = false;

                        Value = optionID;

                        _hasSelectedItem = true;
                    }
                    else
                    {
                        builder.OpenElement(seq++, "option");
                        builder.AddAttribute(seq++, "value", optionIDString);
                        builder.AddContent(seq++, optionValue);
                        builder.CloseElement();
                    }

                    
                }

                builder.CloseElement();

                
            }
            else
            {
                var selectedItem = Items.FirstOrDefault(item =>
                {
                    var optionID = item?.GetProperty(OptionIDFieldName)?.ToString();
                    return IsSelectedValue(optionID, Value);
                });

                var propVal = selectedItem?.GetProperty(OptionValueFieldName) ?? Value?.ToString();
                builder.OpenElement(seq++, "label");
                builder.AddContent(seq++, propVal);
                builder.CloseElement();
            }

        }

        private bool IsSelectedValue(string? optionID, object? value)
        {
            if (value == null)
            {
                return string.IsNullOrEmpty(optionID);
            }

            return value switch
            {
                int intValue => optionID == intValue.ToString(),
                string strValue => optionID == strValue,
                float floatValue => optionID == floatValue.ToString("G"),
                double doubleValue => optionID == doubleValue.ToString("G"),
                decimal decimalValue => optionID == decimalValue.ToString("G"),
                Guid guidValue => optionID == guidValue.ToString(),
                bool boolValue => optionID == boolValue.ToString(),
                DateTime dateTimeValue => optionID == dateTimeValue.ToString("O"),
                _ => optionID == value.ToString()
            };
        }

        private bool IsDefaultValue(object? value)
        {
            return value switch
            {
                int intValue => intValue == default,
                float floatValue => floatValue == default,
                double doubleValue => doubleValue == default,
                decimal decimalValue => decimalValue == default,
                Guid guidValue => guidValue == default,
                bool boolValue => boolValue == default,
                DateTime dateTimeValue => dateTimeValue == default,
                string strValue => string.IsNullOrEmpty(strValue),
                _ => value == null
            };
        }
        
        private async Task HandleSelectedChangeAsync(ChangeEventArgs e)
        {
            Value = e?.Value ?? Value;
            await ValueChanged.InvokeAsync(Value);
        }

        protected override async Task OnAfterRenderAsync(bool isFirstRender)
        {
            if (isFirstRender)
            {
                if (_hasSelectedItem)
                {
                    await ValueChanged.InvokeAsync(Value);
                }
            }

            
        }
    }
}
