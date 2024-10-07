using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.SessionManagement;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class ExcelUploader : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "Upload Excel File";
        [Parameter] public string ModelsAssemblyName { get; set; } = default!;
        [Parameter] public string ViewModelsAssemblyName { get; set; } = default!;
        [Parameter] public HostAssemblies HostAssemblies { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;

        private bool _isUploaded = false;
        private ExcelProcessor? _excelProcessor;
        private bool _isRetrieved = false;
        private string? _connectionString;
        private SessionManager _sessionManager = SessionManager.Instance;

        private Dictionary<string, SessionItem>? _sessionItems;

        protected override async Task OnParametersSetAsync()
        {
            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();
            await base.OnParametersSetAsync();
        }

        private async Task InitializeVariables()
        {
            _connectionString = Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _excelProcessor = new ExcelProcessor(_connectionString);
            _sessionItems = new Dictionary<string, SessionItem>
            {
                ["_excelDataSet"] =
                new SessionItem()
                {
                    Key = "_excelDataSet", Value = null, Type = typeof(DataSet), Serialize = true
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
                    _isUploaded = _sessionItems!["_excelDataSet"] != null;
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

        private async Task HandleFileUpload(IBrowserFile file)
        {
            if (file != null)
            {
                var customFile = new BBBrowserFile(file);
                string tempFilePath = string.Empty;
                _isUploaded = false;

                try
                {
                    tempFilePath = Path.Combine(Path.GetTempPath(), customFile.Name);

                    await using (var stream = customFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024)) // Allow up to 10 MB
                    {
                        await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }

                    if (_excelProcessor != null)
                    {
                        var excelDataSet = await _excelProcessor.ReadExcelDataAsync(tempFilePath);
                        _sessionItems!["_excelDataSet"] = excelDataSet;
                        await _sessionManager.SaveToSessionTableAsync("_excelDataSet", excelDataSet, serialize: true);
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.HandleError(ex);
                }
                finally
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                    _isUploaded = true;
                }
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int sequence = 0;

            // Outer div with class "col-md-12"
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col-md-12");

            // Inner div with class "card-header"
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "card-header");

            // Header with Title
            builder.OpenElement(sequence++, "h3");
            builder.AddContent(sequence++, Title);
            builder.CloseElement();

            // ExcelUploaderHeader component
            builder.OpenComponent<ExcelUploaderHeader>(sequence++);
            builder.AddAttribute(sequence++, "OnFileUpload", EventCallback.Factory.Create<IBrowserFile>(this, HandleFileUpload));
            builder.CloseComponent();

            builder.CloseElement(); // Close "card-header" div

            // Inner div with class "card-body"
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "card-body");

            // Conditional rendering of ExcelUploaderDetail component
            if (_isUploaded)
            {
                builder.OpenComponent<ExcelUploaderDetail>(sequence++);
                builder.AddAttribute(sequence++, "ExcelDataSet", (DataSet)_sessionItems!["_excelDataSet"]!);
                builder.AddAttribute(sequence++, "ExcelProcessor", _excelProcessor);
                builder.AddAttribute(sequence++, "ModelsAssemblyName", ModelsAssemblyName);
                builder.AddAttribute(sequence++, "ViewModelsAssemblyName", ViewModelsAssemblyName);
                builder.AddAttribute(sequence++, "HostAssemblies", HostAssemblies);
                builder.CloseComponent();
            }

            builder.CloseElement(); // Close "card-body" div

            builder.CloseElement(); // Close "col-md-12" div
        }

    }
}

