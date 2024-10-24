using Blazor.Tools.BlazorBundler.Components.LoadingGif;
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.SessionManagement;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class ExcelUploaderHeader : ComponentBase
    {
        [Parameter] public EventCallback<IBrowserFile> OnFileUpload { get; set; }
        [Inject] private LoadingGifStateService LGSS { get; set; } = default!;

        private SessionManager _sessionManager = SessionManager.Instance;
        private Dictionary<string, SessionItem>? _sessionItems;
        private bool _isRetrieved;

        protected override void OnInitialized()
        {
            // Subscribe to changes in the loading state
            LGSS.Subscribe(OnLoadingStateChanged);
        }

        // Implement IDisposable to clean up subscriptions
        public void Dispose()
        {
            // Unsubscribe to avoid memory leaks
            LGSS.Unsubscribe(OnLoadingStateChanged);
        }

        private void OnLoadingStateChanged()
        {
            // Handle loading state changes, if necessary
            StateHasChanged(); // Trigger a re-render if needed
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                await LGSS.RunTaskAsync("excel-uploader-header", "Initializing the Excel Uploader Header, please wait...", InitializeAsync);

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

        }

        private async Task InitializeAsync()
        {
            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();
        }

        private async Task InitializeVariables()
        {
            _sessionItems = new Dictionary<string, SessionItem>
            {
                ["_excelFile"] =
                new SessionItem()
                {
                    Key = "_excelFile", Value = null, Type = typeof(BBBrowserFile), Serialize = true
                }
            };

            await Task.CompletedTask;
        }

        private async Task RetrieveDataFromSessionTableAsync()
        {
            try
            {
                if (!_isRetrieved && _sessionItems != null)
                {
                    _sessionItems = await _sessionManager.RetrieveSessionItemsAsync(_sessionItems);

                    _isRetrieved = true;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }

            await Task.CompletedTask;
        }

        private async Task HandleFileUploadAsync(InputFileChangeEventArgs e)
        {
            try
            {
                await LGSS.RunTaskAsync("excel-uploader-header-handle-file-upload-async", "Running HandleFileUploadAsync, please wait...", FileUploadAsync, e);

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }
        
        private async Task FileUploadAsync(InputFileChangeEventArgs e)
        {
            try
            {
                var browserFile = e.File;
                var excelFile = new BBBrowserFile(browserFile)
                {
                    Name = browserFile.Name,
                    LastModified = browserFile.LastModified,
                    Size = browserFile.Size,
                    ContentType = browserFile.ContentType
                };

                _sessionItems!["_excelFile"] = excelFile;

                await _sessionManager.ClearSessionAsync();
                await _sessionManager.SaveToSessionTableAsync("_excelFile", excelFile, serialize: true);

                await OnFileUpload.InvokeAsync(excelFile);
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            try
            {
                Task.Run(async () =>
                {
                    await LGSS.RunTaskAsync("excel-uploader-header-build-render-tree", "Rendering Excel Uploader Header, please wait...", RenderMainContentAsync, builder);
                });

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }
         
        public async Task RenderMainContentAsync(RenderTreeBuilder builder)
        {
            int seq = 0;

            // InputFile element
            builder.OpenComponent<InputFile>(seq++);
            builder.AddAttribute(seq++, "type", "file");
            builder.AddAttribute(seq++, "id", "fileInput");
            builder.AddAttribute(seq++, "accept", ".xlsx");
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<InputFileChangeEventArgs>(this, HandleFileUploadAsync));
            builder.CloseComponent();

            await Task.CompletedTask;
        }
         
    }
}
