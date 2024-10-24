using Blazor.Tools.BlazorBundler.Components.LoadingGif;
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.SessionManagement;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
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
        [Parameter] public string ModelsAssemblyName { get; set; } = default!; 
        [Parameter] public string ViewModelsAssemblyName { get; set; } = default!; 
        [Parameter] public HostAssemblies HostAssemblies { get; set; } = default!;
        [Inject] private LoadingGifStateService LGSS { get; set; } = default!;

        private List<AssemblyTable>? _tableList = null;
        private SessionManager _sessionManager = SessionManager.Instance;
        private bool _isRetrieved = false;
        private Dictionary<string, SessionItem>? _sessionItems;

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
                await LGSS.RunTaskAsync("excel-uploader-detail", "Initializing the Excel Uploader Detail, please wait...", InitializeAsync);
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
            Assembly? modelsAssembly = null;
            Assembly? servicesAssembly = null;

            if (HostAssemblies.LoadAssemblyFromDLLFile)
            {
                if (HostAssemblies.ModelsAssemblyPath == null)
                {
                    throw new ArgumentException("ModelsAssemblyPath is required.");
                }
                else
                {
                    modelsAssembly = HostAssemblies.ModelsAssemblyPath.LoadAssemblyFromDLLFile();
                }

                if (HostAssemblies.ServicesAssemblyPath == null)
                {
                    throw new ArgumentException("ServicesAssemblyPath is required.");
                }
                else
                {
                    servicesAssembly = HostAssemblies.ServicesAssemblyPath.LoadAssemblyFromDLLFile();
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
                    modelsAssembly = ModelsAssemblyName.LoadAssemblyFromName();
                }

                if (HostAssemblies.ServicesAssemblyName == null)
                {
                    throw new ArgumentException("ServicesAssemblyName is required.");
                }
                else
                {
                    servicesAssembly = HostAssemblies.ServicesAssemblyName.LoadAssemblyFromName();
                }
            }

            _sessionItems = new Dictionary<string, SessionItem>
            {
                ["_excelDataSet"] = 
                new SessionItem()
                {
                    Key = "_excelDataSet", Value = ExcelDataSet, Type = typeof(DataSet), Serialize = true
                },
                ["_selectedTable"] =
                new SessionItem()
                {
                    Key = "_selectedTable", Value = null, Type = typeof(DataTable), Serialize = true
                },
                ["_selectedTableName"] =
                new SessionItem()
                {
                    Key = "_selectedTableName", Value = string.Empty, Type = typeof(string), Serialize = false
                }
            };

            if (_tableList == null)
            {
                _tableList = new List<AssemblyTable>();
            }

            var interfaceNames = modelsAssembly?.GetAssemblyInterfaceNames().ToList();
            var modelsAssemblyName = HostAssemblies.ModelsAssemblyName ?? modelsAssembly?.GetName().Name ?? string.Empty;
            var servicesAssemblyName = HostAssemblies.ServicesAssemblyName ?? servicesAssembly?.GetName().Name ?? string.Empty;
            if (interfaceNames != null)
            {
                for (int i = 0; i < interfaceNames.Count(); i++)
                {
                    var iFace = interfaceNames[i];
                    var assemblyTable = new AssemblyTable()
                    {
                        ID = i + 1,
                        AssemblyName = modelsAssemblyName,
                        AssemblyPath = HostAssemblies.ModelsAssemblyPath,
                        ServiceName = servicesAssemblyName,
                        ServicePath = HostAssemblies.ServicesAssemblyPath,
                        LoadAssemblyFromDLLFile = HostAssemblies.LoadAssemblyFromDLLFile,
                        IsInterface = HostAssemblies.IsInterface,
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
                    _sessionItems = await _sessionManager.RetrieveSessionItemsAsync(_sessionItems);

                    DataTable selectedTable = _sessionItems["_selectedTable"]!;
                    if (selectedTable != null)
                    {
                        selectedTable.TableName = _sessionItems["_selectedTableName"];
                    }

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

        private async Task SelectTableAsync(DataTable table)
        {
            _sessionItems!["_selectedTable"] = table;
            _sessionItems!["_selectedTableName"] = table.TableName;

            await _sessionManager.SaveToSessionTableAsync("_selectedTable", table, serialize: true);
            await _sessionManager.SaveToSessionTableAsync("_selectedTableName", table.TableName);

            StateHasChanged();
        }

        private async Task UploadData()
        {
            if (_sessionItems!["_excelDataSet"] != null)
            {
                if (ExcelProcessor != null)
                {
                    await ExcelProcessor.SaveDataToDatabaseAsync(_sessionItems["_excelDataSet"]!);
                }

                //TODO: sol: Optionally, show a success message or handle post-upload actions
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            try
            {
                Task.Run(async () =>
                {
                    await LGSS.RunTaskAsync("excel-uploader-detail-build-render-tree", "Rendering Excel Uploader Detail, please wait...", RenderMainContentAsync, builder);
                });

            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }


        public async Task RenderMainContentAsync(RenderTreeBuilder builder)
        {
            int sequence = 0;

            // Check if ExcelDataSet is not null and has tables
            if (_sessionItems != null && _sessionItems["_excelDataSet"] != null )
            {
                var dataSet = ((DataSet)_sessionItems["_excelDataSet"]!);
                if (dataSet != null)
                {
                    var tables = dataSet.Tables;
                    var tableCount = tables.Count;
                    if (tableCount > 0)
                    {
                        DataTable selectedTable = _sessionItems["_selectedTable"]!;
                        // Outer div
                        builder.OpenElement(sequence++, "div");

                        // Nav tabs
                        builder.OpenElement(sequence++, "ul");
                        builder.AddAttribute(sequence++, "class", "nav nav-tabs");

                        foreach (DataTable table in tables)
                        {
                            builder.OpenElement(sequence++, "li");
                            builder.AddAttribute(sequence++, "class", "nav-item");

                            builder.OpenElement(sequence++, "a");
                            builder.AddAttribute(sequence++, "class", $"cursor-pointer nav-link {(table == selectedTable ? "active" : "")}");
                            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => SelectTableAsync(table)));
                            builder.AddContent(sequence++, table.TableName);
                            builder.CloseElement();

                            builder.CloseElement();
                        }

                        builder.CloseElement(); // Close "nav-tabs" ul

                        // Tab content
                        builder.OpenElement(sequence++, "div");
                        builder.AddAttribute(sequence++, "class", "tab-content");

                        if (selectedTable != null)
                        {
                            builder.OpenElement(sequence++, "div");
                            builder.AddAttribute(sequence++, "class", "tab-pane fade show active");

                            builder.OpenElement(sequence++, "h4");
                            builder.AddContent(sequence++, selectedTable.TableName);
                            builder.CloseElement();

                            // DataTableGrid component
                            builder.OpenComponent<DataTableGrid>(sequence++);
                            builder.AddAttribute(sequence++, "Title", ""); // Too many Titles already so you need to blank this.
                                                                           //builder.AddAttribute(sequence++, "Title", _selectedTableName);
                            builder.AddAttribute(sequence++, "SelectedTable", selectedTable);
                            builder.AddAttribute(sequence++, "ModelsAssemblyName", ModelsAssemblyName);
                            builder.AddAttribute(sequence++, "ViewModelsAssemblyName", ViewModelsAssemblyName);
                            builder.AddAttribute(sequence++, "AllowCellRangeSelection", true);
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
                        builder.AddAttribute(sequence++, "ColumnName", IconName.Upload);
                        builder.AddAttribute(sequence++, "Class", "cursor-pointer");
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, UploadData));
                        builder.AddAttribute(sequence++, "title", "Upload to new tables");
                        builder.CloseComponent();

                        builder.CloseElement(); // Close icon button div

                        builder.CloseElement(); // Close outer div
                    }
                }
                
            }
            else
            {
                builder.OpenElement(sequence++, "p");
                builder.AddContent(sequence++, "No data available.");
                builder.CloseElement();
            }

            await Task.CompletedTask;
        }
         

    }
}

