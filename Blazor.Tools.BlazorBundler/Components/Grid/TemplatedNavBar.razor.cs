using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TemplatedNavBar<TItem> : ComponentBase
    {
        [Parameter]
        public RenderFragment? StartContent { get; set; }

        [Parameter, EditorRequired]
        public RenderFragment<TItem> ItemTemplate { get; set; } = default!;

        [Parameter, EditorRequired]
        public IReadOnlyList<TItem> Items { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;

            // Render the <nav> element
            builder.OpenElement(sequence++, "nav");
            builder.AddAttribute(sequence++, "class", "navbar navbar-expand navbar-light bg-light");

            // Render the <div> element with class="container justify-content-start"
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "container justify-content-start");

            // Render StartContent if provided
            if (StartContent != null)
            {
                builder.AddContent(sequence++, StartContent);
            }

            // Render the <div> element with class="navbar-nav"
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "navbar-nav");

            // Render each item using the ItemTemplate
            foreach (var item in Items)
            {
                builder.AddContent(sequence++, ItemTemplate(item));
            }

            // Close the <div> elements
            builder.CloseElement(); // Closing div class="navbar-nav"
            builder.CloseElement(); // Closing div class="container justify-content-start"
            builder.CloseElement(); // Closing nav
        }
    }
}
