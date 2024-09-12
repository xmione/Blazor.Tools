using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.SessionManagement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class ExcelUploaderHeader : ComponentBase
    {
        [Parameter] public EventCallback<IBrowserFile> OnFileUpload { get; set; }

        private SessionManager _sessionManager = SessionManager.Instance;
        private IList<SessionItem>? _sessionItems;
        private BBBrowserFile? _excelFile;
        private bool _isRetrieved;

        protected override async Task OnParametersSetAsync()
        {
            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();
            await base.OnParametersSetAsync();
        }

        private async Task InitializeVariables()
        {
            _sessionItems = new List<SessionItem>
            {
                new SessionItem()
                {
                    Key = "_excelFile", Value = _excelFile, Type = typeof(BBBrowserFile), Serialize = true
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
                    _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
                    _excelFile = (BBBrowserFile?)_sessionItems?.FirstOrDefault(s => s.Key.Equals("_excelFile"))?.Value;

                    _isRetrieved = true;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            await Task.CompletedTask;
        }

        private async Task HandleFileUpload(InputFileChangeEventArgs e)
        {
            try
            {
                var browserFile = e.File;
                _excelFile = new BBBrowserFile(browserFile)
                {
                    Name = browserFile.Name,
                    LastModified = browserFile.LastModified,
                    Size = browserFile.Size,
                    ContentType = browserFile.ContentType
                };

                await _sessionManager.ClearSessionAsync();
                await _sessionManager.SaveToSessionTableAsync("_excelFile", _excelFile, serialize: true);

                await OnFileUpload.InvokeAsync(_excelFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
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
