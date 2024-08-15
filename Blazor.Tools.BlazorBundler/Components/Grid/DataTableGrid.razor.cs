using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DataTableGrid : ComponentBase, IDataTableGrid
    {
        [Parameter] public string Title { get; set; } = default!;
        [Parameter] public DataTable SelectedTable { get; set; } = default!;
        [Parameter] public bool AllowCellSelection { get; set; } = false;
        [Parameter] public List<AssemblyTable>? TableList { get; set; } = default!;
        [Parameter] public List<string> HiddenColumnNames { get; set; } = default!;
        [Parameter] public Dictionary<string, string> HeaderNames { get; set; } = default!;

        [Inject] public ISessionTableService _sessionTableService { get; set; } = default!;

        private bool _showSetTargetTableModal = false;
        private DataRow[]? _selectedData = default!;
        private string _selectedFieldValue = string.Empty;
        private SessionManager _sessionManager = SessionManager.Instance;
        private bool _isRetrieved = false;
        private TableGrid _tableGrid = default!;

        private List<TargetTable>? _targetTables;
        private string _tableSourceName = string.Empty;

        private IList<SessionItem>? _sessionItems;

        //        protected override async Task OnParametersSetAsync()
        //        {
        //            await InitializeVariables();
        //            await RetrieveDataFromSessionTableAsync();
        //            await base.OnParametersSetAsync();
        //        }

        //        private async Task InitializeVariables()
        //        {
        //            _sessionManager = SessionManager.GetInstance(_sessionTableService);
        //            _tableSourceName = SelectedTable?.TableName + "DS";
        //            _sessionItems = new List<SessionItem>
        //            {
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_selectedData", Value = _selectedData, Type = typeof(DataRow[]), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_targetTables", Value = _targetTables, Type = typeof(List<TargetTable>), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_tableSourceName", Value = _tableSourceName, Type = typeof(string), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_showSetTargetTableModal", Value = _showSetTargetTableModal, Type = typeof(bool), Serialize = true
        //                }
        //            };

        //            await Task.CompletedTask;
        //        }

        //        private async Task RetrieveDataFromSessionTableAsync()
        //        {
        //            try
        //            {
        //                if (!_isRetrieved && _sessionItems != null)
        //                {
        //                    _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
        //                    _selectedData = (DataRow[]?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedData"))?.Value;
        //                    _targetTables = (List<TargetTable>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_targetTables"))?.Value;
        //                    _showSetTargetTableModal = (bool)(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_showSetTargetTableModal"))?.Value ?? false);

        //                    _isRetrieved = true;
        //                    StateHasChanged();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error: {0}", ex.Message);
        //            }

        //            await Task.CompletedTask;
        //        }

        //        private void CloseSetTargetTableModal()
        //        {
        //            _showSetTargetTableModal = false;
        //            StateHasChanged();

        //            _sessionManager.SaveToSessionTableAsync($"{Title}_showSetTargetTableModal", _showSetTargetTableModal, serialize: true).Wait();
        //        }

        //        private async Task SaveToTargetTableAsync(List<TargetTable>? targetTables)
        //        {
        //            CloseSetTargetTableModal();

        //            _targetTables = targetTables;
        //            await _sessionManager.SaveToSessionTableAsync($"{Title}_targetTables", _targetTables, serialize: true);

        //            StateHasChanged();

        //            await Task.CompletedTask;
        //        }

        //        private async Task HandleSelectedDataComb(DataRow[]? selectedData)
        //        {
        //            _selectedData = selectedData;

        //            if (_selectedData != null)
        //            {
        //                await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedData", _selectedData, serialize: true);
        //                await _tableGrid.HandleSelectedDataComb(_selectedData);
        //            }

        //            StateHasChanged();

        //            await Task.CompletedTask;
        //        }

        //        private async Task HandleSetTargetTableColumnList(List<TargetTableColumn> targetTableColumnList)
        //        {
        //            await Task.CompletedTask;
        //        }

        //        private async Task HandleFieldValueChangedAsync(string newValue)
        //        {
        //            _selectedFieldValue = newValue;
        //            await Task.CompletedTask;
        //        }

        //        private async Task ShowSetTargetTableModalAsync()
        //        {
        //            _showSetTargetTableModal = true;
        //            _selectedData = await _tableGrid.ShowSetTargetTableModalAsync();
        //            if (_selectedData == null)
        //            {
        //                _showSetTargetTableModal = false;
        //            }

        //            //await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedData", _selectedData, serialize: true);
        //            //await _sessionManager.SaveToSessionTableAsync($"{Title}_showSetTargetTableModal", _showSetTargetTableModal, serialize: true);
        //            StateHasChanged();
        //        }

        //        private async Task UploadData()
        //        {
        //            if (_targetTables != null)
        //            {
        //                await _sessionTableService.UploadTableListAsync(_targetTables);

        //                //TODO: sol: Optionally, show a success message or handle post-upload actions
        //            }

        //            await Task.CompletedTask;
        //        }

        //        protected override void BuildRenderTree(RenderTreeBuilder builder)
        //        {
        //            var sequence = 0;

        //            // TableGrid component
        //            builder.OpenComponent<TableGrid>(sequence++);
        //            builder.AddAttribute(sequence++, "Title", Title);
        //            builder.AddAttribute(sequence++, "AllowCellSelection", AllowCellSelection);
        //            builder.AddAttribute(sequence++, "HiddenColumnNames", HiddenColumnNames);
        //            builder.AddAttribute(sequence++, "HeaderNames", HeaderNames);
        //            builder.AddAttribute(sequence++, "DataSource", SelectedTable);

        //            // TableSource
        //            builder.OpenComponent<TableSource>(sequence++);
        //            builder.AddAttribute(sequence++, "Name", _tableSourceName);
        //            builder.AddAttribute(sequence++, "DataSource", SelectedTable);
        //            builder.CloseComponent();

        //            // TableNode and TableColumn components
        //            builder.OpenComponent<TableNode>(sequence++);
        //            builder.AddAttribute(sequence++, "DataSource", _tableSourceName);
        //            builder.AddAttribute(sequence++, "ChildContent", (RenderFragment)((nodeBuilder) =>
        //            {
        //                foreach (DataColumn column in SelectedTable.Columns)
        //                {
        //                    if (!(HiddenColumnNames?.Contains(column.ColumnName) ?? false))
        //                    {
        //                        nodeBuilder.OpenComponent<TableColumn>(sequence++);
        //                        nodeBuilder.AddAttribute(sequence++, "DataSourceName", _tableSourceName);
        //                        nodeBuilder.AddAttribute(sequence++, "FieldName", column.ColumnName);
        //                        nodeBuilder.AddAttribute(sequence++, "Type", "TextBox");
        //                        nodeBuilder.CloseComponent();
        //                    }
        //                }
        //            }));
        //            builder.CloseComponent();

        //            builder.CloseComponent(); // Closing TableGrid

        //            // Icon for Set Target Table Modal
        //            builder.OpenComponent<Icon>(sequence++);
        //            builder.AddAttribute(sequence++, "Name", "IconName.Table");
        //            builder.AddAttribute(sequence++, "Class", "text-success icon-button mb-2 cursor-pointer");
        //            builder.AddAttribute(sequence++, "title", "Step 1. Set Target Table");
        //            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, ShowSetTargetTableModalAsync));
        //            builder.CloseComponent();

        //            // Target tables grid rendering
        //            if (_targetTables != null)
        //            {
        //                foreach (var targetTable in _targetTables)
        //                {
        //                    if (targetTable != null && !string.IsNullOrEmpty(targetTable.DT))
        //                    {
        //                        var dt = targetTable.DT.DeserializeAsync<DataTable>().Result;
        //                        builder.OpenComponent<TableGrid>(sequence++);
        //                        builder.AddAttribute(sequence++, "DataTable", dt);
        //                        builder.CloseComponent();
        //                    }
        //                }
        //            }

        //            // Icon for Upload Data
        //            builder.OpenComponent<Icon>(sequence++);
        //            builder.AddAttribute(sequence++, "Name", "IconName.CloudUpload");
        //            builder.AddAttribute(sequence++, "Class", "cursor-pointer");
        //            builder.AddAttribute(sequence++, "title", "Upload to existing AccSol tables");
        //            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, UploadData));
        //            builder.CloseComponent();

        //            // Set Target Table Modal
        //            if (_showSetTargetTableModal)
        //            {
        //                builder.OpenComponent<SetTargetTableModal>(sequence++);
        //                builder.AddAttribute(sequence++, "Title", Title);
        //                builder.AddAttribute(sequence++, "ShowSetTargetTableModal", _showSetTargetTableModal);
        //                builder.AddAttribute(sequence++, "OnClose", EventCallback.Factory.Create(this, CloseSetTargetTableModal));
        //                builder.AddAttribute(sequence++, "OnSave", EventCallback.Factory.Create<List<TargetTable>>(this, SaveToTargetTableAsync));
        //                builder.AddAttribute(sequence++, "SelectedData", _selectedData);
        //                builder.AddAttribute(sequence++, "OnSelectedDataComb", EventCallback.Factory.Create<DataRow[]>(this, HandleSelectedDataComb));
        //                builder.CloseComponent();
        //            }
        //        }
    }
}
