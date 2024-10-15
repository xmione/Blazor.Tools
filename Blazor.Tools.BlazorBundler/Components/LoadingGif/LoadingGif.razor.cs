using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.LoadingGif
{
    public class LoadingGif : ComponentBase
    {
        [Inject]
        private LoadingService LS { get; set; } = default!;

        [Parameter]
        public bool IsLoading { get; set; } = default!;

        [Parameter]
        public string Message { get; set; } = "Loading... Please wait...";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            AppLogger.WriteInfo($"<OnAfterRenderAsync()> IsLoading: {IsLoading}, Message: {Message}");

            if (firstRender)
            {
                if (firstRender)
                {
                    if (IsLoading && LS != null)
                    {
                        await LS.ShowLoading(Message);
                    }
                    else if (LS != null)
                    {
                        await LS.HideLoading();
                    }
                    else
                    {
                        AppLogger.WriteInfo("Warning: LoadingService is not registered.");
                    }
                }
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (IsLoading)
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
