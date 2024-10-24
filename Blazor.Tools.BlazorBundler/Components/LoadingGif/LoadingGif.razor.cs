/*====================================================================================================
    Class Name  : LoadingGif.razor.cs
    Created By  : Solomio S. Sisante
    Created On  : October 15, 2024
    Purpose     : To provide a ComponentBase class for loading gif.
  ====================================================================================================*/
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.LoadingGif
{
    public class LoadingGif : ComponentBase
    {
        [Inject]
        private LoadingGifService LS { get; set; } = default!;

        [Inject]
        private LoadingGifStateService LSS { get; set; } = default!;

        [Parameter]
        public string Message { get; set; } = "Loading... Please wait...";

        protected override async Task OnParametersSetAsync()
        {
            if (LSS.IsLoading)
            {
                await LS.ShowLoading(Message);
            }
            else
            {
                await LS.HideLoading();
            }
        }

        // BuildRenderTree method to inject HTML and style elements directly into the component
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            // Style block with your loading overlay styles
            builder.OpenElement(0, "style");
            builder.AddContent(1, @"
            .loading-overlay {
                display: none; /* Hidden by default */
                position: fixed;
                z-index: 1000;
                top: 0;
                left: 0;
                height: 100%;
                width: 100%;
                background: rgba(0, 0, 0, 0.5);
                align-items: center;
                justify-content: center;
            }
        ");
            builder.CloseElement();

            // HTML structure for the loading overlay and spinner
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "id", "loading-overlay");
            builder.AddAttribute(4, "class", "loading-overlay");

            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "spinner-border text-primary");
            builder.AddAttribute(7, "role", "status");
            builder.CloseElement(); // Close spinner div

            builder.OpenElement(8, "div");
            builder.AddAttribute(9, "class", "row");
            builder.OpenElement(10, "span");
            builder.AddAttribute(11, "id", "loading-overlay-message");
            builder.CloseElement(); // Close span
            builder.CloseElement(); // Close row div

            builder.CloseElement(); // Close loading-overlay div
        }
    }
}
