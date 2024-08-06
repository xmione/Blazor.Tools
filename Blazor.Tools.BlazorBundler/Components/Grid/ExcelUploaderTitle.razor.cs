using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class ExcelUploaderTitle : ComponentBase
    {
        [Parameter]
        public string Title { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int sequence = 0;

            // InputFile element
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "accsol-title");
            builder.AddContent(sequence++, Title );
            builder.CloseElement();
        }
    }
}

