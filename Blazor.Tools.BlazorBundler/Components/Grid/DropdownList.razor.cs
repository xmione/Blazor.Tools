using Blazor.Tools.BlazorBundler.Extensions;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

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

        private object? _selectedOptionID { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            if (IsEditMode)
            {
                builder.OpenElement(seq++, "select");
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleSelectedChange));

                foreach (TItem item in Items)
                { 
                    var optionID = item?.GetProperty(OptionIDFieldName)?.ToString();
                    var optionValue = item?.GetProperty(OptionValueFieldName)?.ToString();

                                builder.OpenElement(seq++, "option");
                                builder.AddAttribute(seq++, "value", optionID);
                                //if (optionValue == editValues)
                                //{
                                //    builder.AddAttribute(seq++, "selected", "selected");
                                //}
                                builder.AddContent(seq++, optionValue);
                                builder.CloseElement();
                }


                builder.CloseElement();
            }
            else 
            {
                builder.OpenElement(0, "label");
                builder.AddContent(1, Value);
                builder.CloseElement();
            }
        }

        private void HandleSelectedChange(ChangeEventArgs e)
        {
            _selectedOptionID = e.Value;
        }
    }
}
