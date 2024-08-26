using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class DateOnlyPicker : ComponentBase
    {
        [Parameter] public string ColumnName { get; set; } = default!;
        [Parameter] public string HeaderName { get; set; } = default!;
        [Parameter] public DateOnly? Value { get; set; }
        [Parameter] public bool IsEditMode { get; set; } = default!;
        [Parameter] public int RowID { get; set; } = default!;
        [Parameter] public EventCallback<DateOnly?> ValueChanged { get; set; }
        [Parameter] public string CssClass { get; set; }

        private string InternalDateValue
        {
            get => Value.HasValue ? Value.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd") : string.Empty;
            set
            {
                if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    var dateOnly = DateOnly.FromDateTime(dateTime);
                    if (Value != dateOnly)
                    {
                        Value = dateOnly;
                        ValueChanged.InvokeAsync(dateOnly);
                    }
                }
                else
                {
                    if (Value.HasValue)
                    {
                        Value = null;
                        ValueChanged.InvokeAsync(null);
                    }
                }
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            if (IsEditMode)
            {
                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "type", "date"); // Hardcode the type to "date"
                builder.AddAttribute(seq++, "value", InternalDateValue); // Hardcode a date value for testing

                if (!string.IsNullOrEmpty(CssClass))
                {
                    builder.AddAttribute(seq++, "class", CssClass);
                }

                // Using Binder to handle value change correctly
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.CreateBinder<string>(
                    this, __value => OnDateChanged(__value), InternalDateValue));
                builder.AddAttribute(5, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, HandleOnBlur));
                builder.CloseComponent();
            }
            else
            {
                builder.OpenElement(seq++, "label");
                builder.AddContent(seq++, Value.HasValue ? Value.Value.ToString("d") : string.Empty);
                builder.CloseElement();
            }
        }

        private void OnDateChanged(string newDate)
        {
            if (DateTime.TryParseExact(newDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                Value = DateOnly.FromDateTime(dateTime);
            }
        }


        private async Task HandleOnBlur(FocusEventArgs e)
        {
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
