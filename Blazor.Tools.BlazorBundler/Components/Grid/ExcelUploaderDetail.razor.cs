using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class ExcelUploaderDetail : ComponentBase
    {
        [Parameter] public DataSet? ExcelDataSet { get; set; } = default!;
        [Parameter] public ExcelProcessor ExcelProcessor { get; set; } = default!;
        [Parameter] public string ModelsAssemblyName { get; set; } = default!; //"AccSol.EF.Interfaces"
        [Parameter] public string ModelsAssemblyPath { get; set; } = default!; // @"C:\repo\AccSol\AccSol.Interfaces\bin\Debug\net8.0\AccSol.Interfaces.dll"
        [Parameter] public string ServicesAssemblyName { get; set; } = default!; //"AccSol.Services"
        [Parameter] public string ServicesAssemblyPath { get; set; } = default!; // @"C:\repo\AccSol\AccSol.Services\bin\Debug\net8.0\AccSol.Services.dll"
        [Parameter] public bool LoadAssemblyFromDLLFile { get; set; } = false;
        [Parameter] public bool IsInterface { get; set; } = false;

        private DataTable? _selectedTable;
        private string? _selectedTableName;
        private bool _isReceived = false;
        private List<AssemblyTable>? _tableList = null;
        private SessionManager _sessionManager = SessionManager.Instance;
        private bool _isRetrieved = false;
        private IList<SessionItem>? _sessionItems;

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                await base.OnParametersSetAsync();
                await InitializeVariables();
                await RetrieveDataFromSessionTableAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0} \r\n StackTrace: {1}", ex.Message, ex.StackTrace);
            }

        }

        private async Task InitializeVariables()
        {
            Assembly? modelsAssembly = null;
            Assembly? servicesAssembly = null;

            if (LoadAssemblyFromDLLFile)
            {
                if (ModelsAssemblyPath == null)
                {
                    throw new ArgumentException("ModelsAssemblyPath is required.");
                }
                else
                {
                    modelsAssembly = ReflectionExtensions.LoadAssemblyFromDLLFile(ModelsAssemblyPath);
                }

                if (ServicesAssemblyPath == null)
                {
                    throw new ArgumentException("ServicesAssemblyPath is required.");
                }
                else
                {
                    servicesAssembly = ReflectionExtensions.LoadAssemblyFromDLLFile(ServicesAssemblyPath);
                }

            }
            else
            {
                if (ModelsAssemblyName == null)
                {
                    throw new ArgumentException("ModelsAssemblyName is required.");
                }
                else
                {
                    modelsAssembly = ReflectionExtensions.LoadAssemblyFromName(ModelsAssemblyName);
                }

                if (ServicesAssemblyName == null)
                {
                    throw new ArgumentException("ServicesAssemblyName is required.");
                }
                else
                {
                    servicesAssembly = ReflectionExtensions.LoadAssemblyFromName(ServicesAssemblyName);
                }
            }

            _sessionItems = new List<SessionItem>
            {
                new SessionItem()
                {
                    Key = "_excelDataSet", Value = ExcelDataSet, Type = typeof(DataSet), Serialize = true
                },
                new SessionItem()
                {
                    Key = "_selectedTable", Value=_selectedTable, Type = typeof(DataTable), Serialize = true
                },
                new SessionItem()
                {
                    Key = "_selectedTableName", Value=_selectedTableName, Type = typeof(string), Serialize = false
                }
            };

            if (_tableList == null)
            {
                _tableList = new List<AssemblyTable>();
            }

            var interfaceNames = modelsAssembly?.GetAssemblyInterfaceNames().ToList();
            var modelsAssemblyName = ModelsAssemblyName ?? modelsAssembly?.GetName().Name ?? string.Empty;
            var servicesAssemblyName = ServicesAssemblyName ?? servicesAssembly?.GetName().Name ?? string.Empty;
            if (interfaceNames != null)
            {
                for (int i = 0; i < interfaceNames.Count(); i++)
                {
                    var iFace = interfaceNames[i];
                    var assemblyTable = new AssemblyTable()
                    {
                        ID = i + 1,
                        AssemblyName = modelsAssemblyName,
                        AssemblyPath = ModelsAssemblyPath,
                        ServiceName = servicesAssemblyName,
                        ServicePath = ServicesAssemblyPath,
                        LoadAssemblyFromDLLFile = LoadAssemblyFromDLLFile,
                        IsInterface = IsInterface,
                        TableName = iFace.Item2.Substring(1, iFace.Item2.Length - 1),
                        TypeName = iFace.Item2
                    };

                    _tableList?.Add(assemblyTable);
                }
            }

            await Task.CompletedTask;
        }

        private async Task RetrieveDataFromSessionTableAsync()
        {
            try
            {
                if (!_isRetrieved && _sessionItems != null)
                {
                    _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
                    ExcelDataSet = (DataSet?)_sessionItems?.FirstOrDefault(s => s.Key.Equals("_excelDataSet"))?.Value;
                    _selectedTable = (DataTable?)_sessionItems?.FirstOrDefault(s => s.Key.Equals("_selectedTable"))?.Value;
                    _selectedTableName = _sessionItems?.FirstOrDefault(s => s.Key.Equals("_selectedTableName"))?.Value?.ToString();

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

        private async Task SelectTableAsync(DataTable table)
        {
            _selectedTable = table;
            _selectedTableName = table.TableName;

            await _sessionManager.SaveToSessionTableAsync("_selectedTable", _selectedTable, serialize: true);
            await _sessionManager.SaveToSessionTableAsync("_selectedTableName", _selectedTable);

            StateHasChanged();
        }

        private async Task UploadData()
        {
            if (ExcelDataSet != null)
            {
                if (ExcelProcessor != null)
                {
                    await ExcelProcessor.SaveDataToDatabaseAsync(ExcelDataSet);
                }

                //TODO: sol: Optionally, show a success message or handle post-upload actions
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int sequence = 0;

            // Check if ExcelDataSet is not null and has tables
            if (ExcelDataSet != null && ExcelDataSet.Tables.Count > 0)
            {
                // Outer div
                builder.OpenElement(sequence++, "div");

                // Nav tabs
                builder.OpenElement(sequence++, "ul");
                builder.AddAttribute(sequence++, "class", "nav nav-tabs");

                foreach (var table in ExcelDataSet.Tables.Cast<DataTable>())
                {
                    builder.OpenElement(sequence++, "li");
                    builder.AddAttribute(sequence++, "class", "nav-item");

                    builder.OpenElement(sequence++, "a");
                    builder.AddAttribute(sequence++, "class", $"cursor-pointer nav-link {(table == _selectedTable ? "active" : "")}");
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => SelectTableAsync(table)));
                    builder.AddContent(sequence++, table.TableName);
                    builder.CloseElement();

                    builder.CloseElement();
                }

                builder.CloseElement(); // Close "nav-tabs" ul

                // Tab content
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "tab-content");

                if (_selectedTable != null)
                {
                    builder.OpenElement(sequence++, "div");
                    builder.AddAttribute(sequence++, "class", "tab-pane fade show active");

                    builder.OpenElement(sequence++, "h4");
                    builder.AddContent(sequence++, _selectedTableName);
                    builder.CloseElement();

                    // DataTableGrid component
                    builder.OpenComponent<DataTableGrid>(sequence++);
                    builder.AddAttribute(sequence++, "Title", _selectedTableName);
                    builder.AddAttribute(sequence++, "SelectedTable", _selectedTable);
                    builder.AddAttribute(sequence++, "AllowCellSelection", true);
                    builder.AddAttribute(sequence++, "TableList", _tableList);
                    builder.CloseComponent();

                    builder.CloseElement(); // Close "tab-pane" div
                }
                else
                {
                    builder.OpenElement(sequence++, "p");
                    builder.AddContent(sequence++, "Select a table to view its data.");
                    builder.CloseElement();
                }

                builder.CloseElement(); // Close "tab-content" div

                // Icon button
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "mt-3");

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "ColumnName", "IconName.Upload");
                builder.AddAttribute(sequence++, "Class", "cursor-pointer");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, UploadData));
                builder.AddAttribute(sequence++, "title", "Upload to new tables");
                builder.CloseComponent();

                builder.CloseElement(); // Close icon button div

                builder.CloseElement(); // Close outer div
            }
            else
            {
                builder.OpenElement(sequence++, "p");
                builder.AddContent(sequence++, "No data available.");
                builder.CloseElement();
            }
        }

    }
}

