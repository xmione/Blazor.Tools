using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public class TestComponent<TItem> : ComponentBase
    {
        [Parameter] public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();
        [Parameter] public RenderFragment? StartContent { get; set; }
        [Parameter] public RenderFragment? TableHeader { get; set; }
        [Parameter] public RenderFragment<TItem> RowTemplate { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;

            // Render StartContent if provided
            if (StartContent != null)
            {
                builder.AddContent(seq++, StartContent);
            }

            // Render the table with headers and rows
            builder.OpenElement(seq++, "table");
            builder.AddAttribute(seq++, "class", "table");

            // Render TableHeader if provided
            if (TableHeader != null)
            {
                builder.OpenElement(seq++, "thead");
                builder.OpenElement(seq++, "tr");
                builder.AddContent(seq++, TableHeader);
                builder.CloseElement(); // tr
                builder.CloseElement(); // thead
            }

            builder.OpenElement(seq++, "tbody");

            // Render items using RowTemplate
            if (Items != null && RowTemplate != null)
            {
                foreach (var item in Items)
                {
                    builder.OpenElement(seq++, "tr");
                    builder.AddContent(seq++, RowTemplate(item));
                    builder.CloseElement(); // tr
                }
            }

            builder.CloseElement(); // tbody
            builder.CloseElement(); // table
        }
    }
}
