using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.JSInterop;

namespace Blazor.Tools.BlazorBundler.Components.LoadingGif
{
    // The LoadingService class for JS interop to show/hide the loading overlay
    public class LoadingGifService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string LoadingOverlayId = "loading-overlay";
        private const string LoadingMessageId = "loading-overlay-message";

        public LoadingGifService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Show the loading overlay and set the message
        public async Task ShowLoading(string message = "Loading... Please wait...")
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{LoadingMessageId}').innerText = '{message}';");
                await _jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{LoadingOverlayId}').style.display = 'flex';");
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }

        // Hide the loading overlay
        public async Task HideLoading()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{LoadingOverlayId}').style.display = 'none';");
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }
    }
}

