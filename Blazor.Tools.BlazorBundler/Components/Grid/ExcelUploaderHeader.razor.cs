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
        [Inject] private LoadingGifStateService LSS { get; set; } = default!;

        private SessionManager _sessionManager = SessionManager.Instance;
        private Dictionary<string, SessionItem>? _sessionItems;
        private bool _isRetrieved;

        protected override async Task OnParametersSetAsync()
        {
            try 
            {
                LSS.StartLoading("excel-uploader-header", "Initializing the Excel Uploader Header, please wait...");

                await base.OnParametersSetAsync();
                await InitializeVariables();
                await RetrieveDataFromSessionTableAsync();
                
            } 
            catch (Exception ex) 
            {
                AppLogger.HandleError(ex);
            }
            finally
            {
                // Mark the component as no longer loading
                LSS.EndLoading("excel-uploader-header");
            }

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

        private async Task HandleFileUpload(InputFileChangeEventArgs e)
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
            int seq = 0;

            // InputFile element
            builder.OpenComponent<InputFile>(seq++);
            builder.AddAttribute(seq++, "type", "file");
            builder.AddAttribute(seq++, "id", "fileInput");
            builder.AddAttribute(seq++, "accept", ".xlsx");
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<InputFileChangeEventArgs>(this, HandleFileUpload));
            builder.CloseComponent();
        }
         
    }
}
