using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.SessionManagement;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class SetTargetTableModal : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "Sample Table";
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<List<TargetTable>> OnSave { get; set; }
        [Parameter] public DataRow[]? SelectedData { get; set; } = default!;
        [Parameter] public EventCallback<DataRow[]> OnSelectedDataComb { get; set; }
        [Parameter] public EventCallback<List<TargetTableColumn>> SetTargetTableColumnList { get; set; }
        [Parameter] public List<AssemblyTable>? TableList { get; set; } = default!;

        private SessionManager _sessionManager = SessionManager.Instance;
        private string? _tag;
        //private int _selectedTableID;
        //private int _selectedLookupTableID;
        //private string? _selectedTableValue;
        //private string? _selectedLookupTableValue;
        //private string? _selectedFieldValue;
        //private string? _selectedPrimaryKeyFieldValue;
        //private string? _selectedForeignKeyFieldValue;
        //private string? _selectedSourceField;
        //private string _searchQuerySelectedData = string.Empty;
        //private IEnumerable<DataRow>? _filteredRowsSelectedData;
        //private IEnumerable<DataRow>? _pagedRowsSelectedData;
        //private int _pageSizeSelectedData = 0;
        //private int _currentPageSelectedData = 1;
        //private int _totalPagesSelectedData = 0;
        //private int _totalItemsSelectedData = 0;
        //private List<TargetTableColumn>? _targetTableColumnList;
        //private List<string>? _columnProperties;
        //private string? _uniqueField;
        //private bool _showForeignTableSearchFieldsModal = false;
        //private DataTable? _searchFieldsTable = null;

        private bool _isRetrieved = false;
        private Dictionary<string, SessionItem>? _sessionItems;

        protected override async Task OnParametersSetAsync()
        {
            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();
            await UpdateVariablesAfterSessionRetrieval();

            await base.OnParametersSetAsync();
        }

        private async Task InitializeVariables()
        {
            _sessionItems = new Dictionary<string, SessionItem>
            {
                ["_selectedData"] = 
                new SessionItem()
                {
                    Key = $"{Title}_selectedData", Value = SelectedData, Type = typeof(DataRow[]), Serialize = true
                },
                ["_uniqueField"] =
                new SessionItem()
                {
                    Key = $"{Title}_uniqueField", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedSourceField"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedSourceField", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedTableID"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedTableID", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_selectedLookupTableID"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedLookupTableID", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_selectedTableValue"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedTableValue", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedLookupTableValue"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedLookupTableValue", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedFieldValue"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedFieldValue", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedPrimaryKeyFieldValue"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedPrimaryKeyFieldValue", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedForeignKeyFieldValue"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedForeignKeyFieldValue", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_selectedLookupTableID"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedLookupTableID", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_selectedLookupTableValue"] =
                new SessionItem()
                {
                    Key = $"{Title}_selectedLookupTableValue", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_searchQuerySelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_searchQuerySelectedData", Value = string.Empty, Type = typeof(string), Serialize = false
                },
                ["_filteredRowsSelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_filteredRowsSelectedData", Value = null, Type = typeof(IEnumerable<DataRow>), Serialize = true
                },
                ["_pagedRowsSelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_pagedRowsSelectedData", Value = null, Type = typeof(IEnumerable<DataRow>), Serialize = true
                },
                ["_pageSizeSelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_pageSizeSelectedData", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_currentPageSelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_currentPageSelectedData", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_totalPagesSelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_totalPagesSelectedData", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_totalItemsSelectedData"] =
                new SessionItem()
                {
                    Key = $"{Title}_totalItemsSelectedData", Value = 0, Type = typeof(int), Serialize = true
                },
                ["_targetTableColumnList"] =
                new SessionItem()
                {
                    Key = $"{Title}_targetTableColumnList", Value = null, Type = typeof(List<TargetTableColumn>), Serialize = true
                },
                ["_columnProperties"] =
                new SessionItem()
                {
                    Key = $"{Title}_columnProperties", Value = null, Type = typeof(List<string>), Serialize = true
                },
                ["_showForeignTableSearchFieldsModal"] =
                new SessionItem()
                {
                    Key = $"{Title}_showForeignTableSearchFieldsModal", Value = false, Type = typeof(bool), Serialize = true
                },
                ["_searchFieldsTable"] =
                new SessionItem()
                {
                    Key = $"{Title}_searchFieldsTable", Value = null, Type = typeof(DataTable), Serialize = true
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

                    _tag = $"{_sessionItems["_selectedTableValue"]}-{_sessionItems["_selectedFieldValue"]}";
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

        private async Task UpdateVariablesAfterSessionRetrieval()
        {
            DataRow[]? selectedData = _sessionItems!["_selectedData"];
            List<TargetTableColumn>? targetTableColumnList = _sessionItems["_targetTableColumnList"];
            List<string>? columnProperties = _sessionItems["_columnProperties"];
            if (selectedData != null)
            {
                var dataLength = selectedData.Length;
                _sessionItems["_totalItemsSelectedData"] = dataLength;
                _sessionItems["_pageSizeSelectedData"] = dataLength;
                _sessionItems["_filteredRowsSelectedData"].SetI(ApplyFilterSelectedData());
                
                CalculateTotalPagesAndPagedData();

                if (targetTableColumnList != null)
                {
                    if (targetTableColumnList.Any())
                    {
                        columnProperties = targetTableColumnList.First().GetPropertyNames().ToList();
                    }
                    else
                    {
                        columnProperties = new List<string>();
                    }
                }
                else
                {
                    columnProperties = new List<string>();
                }

                // Set the initial selected value
                int totalItemsSelectedData = _sessionItems["_totalItemsSelectedData"];
                string? selectedSourceField = _sessionItems["_selectedSourceField"];
                if (totalItemsSelectedData > 0 && string.IsNullOrEmpty(selectedSourceField))
                {
                    _sessionItems["_selectedSourceField"] = selectedData[0].Table.Columns[0].ColumnName;
                    var selectedTableValue = TableList?.FirstOrDefault();
                    _sessionItems["_selectedTableID"] = selectedTableValue?.ID ?? 0;
                    var selectedFieldValue = GetFieldValues(_sessionItems["_selectedTableID"])?.FirstOrDefault();
                    _sessionItems["_selectedFieldValue"] = selectedFieldValue ?? string.Empty;
                    _sessionItems["_selectedPrimaryKeyFieldValue"] = selectedFieldValue ?? string.Empty;
                    _sessionItems["_selectedLookupTableID"] = 0;
                    _sessionItems["_uniqueField"] = _sessionItems["_selectedSourceField"]!;
                }
            }

            await Task.CompletedTask;
        }

        private async Task OnSourceFieldValueChanged(ChangeEventArgs e)
        {
            var selectedSourceField = e?.Value?.ToString() ?? string.Empty;
            _sessionItems!["_selectedSourceField"] = selectedSourceField;
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedSourceField", selectedSourceField);
            await Task.CompletedTask;
        }

        private async Task OnTargetTableValueChanged(ChangeEventArgs e)
        {
            // Parse selected value from int to TableEnum
            int selectedTableID = int.Parse(e?.Value?.ToString() ?? string.Empty);
            _sessionItems!["_selectedTableID"] = selectedTableID;

            _sessionItems["_selectedTableValue"] = TableList?.FirstOrDefault(t => t.ID == selectedTableID)?.TableName ?? string.Empty;

            // Reset selected field value when table value changes
            _sessionItems["_selectedFieldValue"] = string.Empty;
            _tag = $"{ _sessionItems["_selectedTableValue"]}-{_sessionItems["_selectedFieldValue"]}";

            // Handle the selected value here
            AppLogger.WriteInfo($"_selectedTableID: {selectedTableID}");

            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedTableID", selectedTableID, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedTableValue", _sessionItems["_selectedTableValue"]);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedFieldValue", _sessionItems["_selectedFieldValue"]);

            await Task.CompletedTask;
        }

        private async Task OnSelectedLookupTableValueChanged(ChangeEventArgs e)
        {
            // Parse selected value from int to TableEnum
            int selectedLookupTableID = int.Parse(e?.Value?.ToString() ?? string.Empty);
            _sessionItems!["_selectedLookupTableID"] = selectedLookupTableID;
            _sessionItems["_selectedLookupTableValue"] = TableList?.FirstOrDefault(t => t.ID == selectedLookupTableID)?.TableName ?? string.Empty;

            // Reset selected field value when table value changes
            _sessionItems["_selectedPrimaryKeyFieldValue"] = string.Empty;
            _tag = $"{_sessionItems["_selectedTableValue"]}-{_sessionItems["_selectedFieldValue"]}";

            // Handle the selected value here
            AppLogger.WriteInfo($"selectedValue: {selectedLookupTableID}" );

            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedLookupTableID", selectedLookupTableID, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedLookupTableValue", _sessionItems["_selectedLookupTableValue"], serialize: true);
            await Task.CompletedTask;
        }

        private async Task OnTargetFieldChanged(ChangeEventArgs e)
        {
            _sessionItems!["_selectedTableValue"] = TableList?.FirstOrDefault(t => t.ID == _sessionItems["_selectedTableID"])?.TableName ?? string.Empty;
            _sessionItems["_selectedFieldValue"] = e?.Value?.ToString() ?? string.Empty;
            _tag = $"{_sessionItems["_selectedTableValue"]}-{_sessionItems["_selectedFieldValue"]}";

            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedFieldValue", _sessionItems["_selectedFieldValue"]);
            await Task.CompletedTask;
        }

        private async Task OnPrimaryKeyFieldChanged(ChangeEventArgs e)
        {
            _sessionItems!["_selectedLookupTableValue"] = TableList?.FirstOrDefault(t => t.ID == _sessionItems["_selectedLookupTableID"])?.TableName ?? string.Empty;
            _sessionItems["_selectedPrimaryKeyFieldValue"] = e?.Value?.ToString() ?? string.Empty;
            await Task.CompletedTask;
        }

        private async Task OnForeignKeyFieldChanged(ChangeEventArgs e)
        {
            _sessionItems!["_selectedLookupTableValue"] = TableList?.FirstOrDefault(t => t.ID == _sessionItems["_selectedLookupTableID"])?.TableName ?? string.Empty;
            _sessionItems["_selectedForeignKeyFieldValue"] = e?.Value?.ToString() ?? string.Empty;
            await Task.CompletedTask;
        }

        private async Task SetTargetTableFieldAsync()
        {
            List<TargetTableColumn>? targetTableColumnList = _sessionItems!["_targetTableColumnList"];
            string? selectedSourceField = _sessionItems["_selectedSourceField"];
            string? selectedTableValue = _sessionItems["_selectedTableValue"];
            string? selectedFieldValue = _sessionItems["_selectedFieldValue"];
            string? selectedLookupTableValue = _sessionItems["_selectedLookupTableValue"];
            List<string>? columnProperties = _sessionItems["_columnProperties"];

            int selectedTableID = _sessionItems["_selectedTableID"];
            if (targetTableColumnList == null)
            {
                targetTableColumnList = new List<TargetTableColumn>();
            }

            var targetTableColumn = targetTableColumnList.FirstOrDefault(s => s.SourceFieldName == selectedSourceField)
                                    ?? new TargetTableColumn();
            selectedTableValue = TableList?.FirstOrDefault(t => t.ID == selectedTableID)?.TableName ?? string.Empty;
            var sourceFieldName = selectedSourceField ?? string.Empty;
            var targetTableName = selectedTableValue ?? string.Empty;
            var targetFieldName = selectedFieldValue ?? string.Empty;
            var checkOnTableName = selectedLookupTableValue ?? string.Empty;

            Type dataType = typeof(object);
            if (!string.IsNullOrEmpty(selectedSourceField) && SelectedData?.Length > 0 && SelectedData[0].Table.Columns.Contains(selectedSourceField))
            {
                dataType = SelectedData[0][selectedSourceField]?.GetType() ?? typeof(object);
            }

            targetTableColumn.SourceFieldName = sourceFieldName;
            targetTableColumn.TargetTableName = targetTableName;
            targetTableColumn.TargetFieldName = targetFieldName;
            targetTableColumn.CheckOnTableName = checkOnTableName;
            targetTableColumn.TargetFieldName = targetFieldName;

            targetTableColumn.DataType = dataType.Name;

            if (!targetTableColumnList.Contains(targetTableColumn))
            {
                targetTableColumnList.Add(targetTableColumn);
            }

            if (targetTableColumnList != null)
            {
                if (targetTableColumnList.Any())
                {
                    columnProperties = targetTableColumnList.First().GetPropertyNames().ToList();
                }
                else
                {
                    columnProperties = new List<string>();
                }
            }
            else
            {
                columnProperties = new List<string>();
            }

            _sessionItems["_selectedTableValue"] = selectedTableValue!;
            _sessionItems["_targetTableColumnList"] = targetTableColumnList!;
            _sessionItems["_columnProperties"] = columnProperties;
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedTableValue", selectedTableValue, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_targetTableColumnList", targetTableColumnList, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_columnProperties", columnProperties, serialize: true);
            await SetTargetTableColumnList.InvokeAsync(targetTableColumnList);
            StateHasChanged();
        }

        private IEnumerable<string>? GetFieldValues(int id)
        {
            var selectedTable = TableList?.FirstOrDefault(t => t.ID == id);
            var properties = selectedTable?.GetPropertyNames();

            return properties;

        }

        private IEnumerable<DataRow>? ApplyFilterSelectedData()
        {
            if (string.IsNullOrWhiteSpace(_sessionItems!["_searchQuerySelectedData"]))
            {
                return SelectedData;
            }

            // Assuming SelectedData is DataRow[]
            return SelectedData?.Where(row =>
            {
                // Check each column in the row for the search query
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (row[column]?.ToString()?.IndexOf(_sessionItems["_searchQuerySelectedData"]!, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true; // Return true if search query is found in any column
                    }
                }
                return false; // Return false if search query is not found in any column
            });
        }

        private void ChangePage(int pageNumber)
        {
            _sessionItems!["_currentPageSelectedData"] = pageNumber;
            StateHasChanged(); // Refresh UI after changing page
        }

        private void SelectedDataPageSizeChanged(ChangeEventArgs e)
        {
            _sessionItems!["_pageSizeSelectedData"] = Convert.ToInt32(e.Value);
            _sessionItems["_currentPageSelectedData"] = 1; // Reset to first page when changing page size
            CalculateTotalPagesAndPagedData();
        }

        private void CalculateTotalPagesAndPagedData()
        {
            IEnumerable<DataRow>? filteredRowsSelectedData = _sessionItems!["_filteredRowsSelectedData"].GetI();
            int pageSizeSelectedData = _sessionItems["_pageSizeSelectedData"];
            int currentPageSelectedData = _sessionItems["_currentPageSelectedData"];
            if (filteredRowsSelectedData != null)
            {
                _sessionItems["_totalPagesSelectedData"] = (int)Math.Ceiling((double)(filteredRowsSelectedData?.Count() ?? 0) / pageSizeSelectedData);
                _sessionItems["_pagedRowsSelectedData"].SetI(filteredRowsSelectedData?.Skip((currentPageSelectedData - 1) * pageSizeSelectedData).Take(pageSizeSelectedData));
            }
            else
            {
                _sessionItems["_totalPagesSelectedData"] = 0;
                _sessionItems["_pagedRowsSelectedData"].SetI(Enumerable.Empty<DataRow>());
            }

            StateHasChanged(); // Refresh UI after calculating pages and paged data
        }

        private async Task GoToFirstPage()
        {
            _sessionItems!["_currentPageSelectedData"] = 1;
            StateHasChanged();
            await Task.CompletedTask; // If you have no async operations, you can use Task.CompletedTask
        }

        private async Task GoToPreviousPage()
        {
            var curPage = _sessionItems!["_currentPageSelectedData"];
            if (curPage > 1)
            {
                curPage--;
                _sessionItems!["_currentPageSelectedData"] = curPage;
                StateHasChanged();
            }

            await Task.CompletedTask;
        }

        private async Task GoToNextPage()
        {
            var curPage = _sessionItems!["_currentPageSelectedData"];
            var totalPagesSelectedData = _sessionItems!["_totalPagesSelectedData"];
            if (curPage < totalPagesSelectedData)
            {
                curPage++;
                _sessionItems!["_currentPageSelectedData"] = curPage;
                StateHasChanged();
            }

            await Task.CompletedTask;
        }

        private async Task GoToLastPage()
        {
            var curPage = _sessionItems!["_currentPageSelectedData"];
            var totalPagesSelectedData = _sessionItems!["_totalPagesSelectedData"];

            curPage = totalPagesSelectedData;
            _sessionItems!["_currentPageSelectedData"] = curPage;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task GoToSpecifiedPage()
        {
            var curPage = _sessionItems!["_currentPageSelectedData"];
            var totalPagesSelectedData = _sessionItems!["_totalPagesSelectedData"];

            if (curPage >= 1 && curPage <= totalPagesSelectedData)
            {
                StateHasChanged();
            }
            else
            {
                // Handle invalid page number
                // For example, display a toast message or an error message
            }

            await Task.CompletedTask;
        }

        private async Task DataCombAsync()
        {
            string? uniqueField = _sessionItems!["_uniqueField"];
            if (SelectedData != null && SelectedData.Length > 1 && !string.IsNullOrEmpty(uniqueField))
            {
                SelectedData = SelectedData
                    .Where(row => row[uniqueField] != DBNull.Value &&
                                  row[uniqueField] != null &&
                                  !string.IsNullOrEmpty(row[uniqueField].ToString()))
                    .ToArray();  // Explicitly convert to DataRow[]


                await OnSelectedDataComb.InvokeAsync(SelectedData);
            }

            await Task.CompletedTask;

            StateHasChanged();
        }

        private async Task OnUniqueFieldValueChanged(ChangeEventArgs e)
        {
            _sessionItems!["_uniqueField"] = e?.Value?.ToString() ?? string.Empty;

            await _sessionManager.SaveToSessionTableAsync($"{Title}_uniqueField", _sessionItems["_uniqueField"]);
            await DataCombAsync();

        }

        private async Task SaveTargetTablesAsync()
        {
            List<TargetTableColumn>? targetTableColumnList = _sessionItems!["_targetTableColumnList"];
            var targetTables = new List<TargetTable>();
            if (targetTableColumnList != null)
            {
                var targetTableGroups = targetTableColumnList?
                                        .Where(t => !string.IsNullOrEmpty(t.TargetTableName))
                                        .GroupBy(t => t.TargetTableName)
                                        .ToList();

                if (targetTableGroups != null)
                {
                    foreach (var group in targetTableGroups)
                    {
                        var targetTableName = group.Key;
                        var targetTableColumns = group.ToList();

                        var targetTable = new TargetTable
                        {
                            TargetTableName = targetTableName,
                            //TargetTableColumns = targetTableColumns, TODO: sol: Correct this
                        };

                        var dt = new DataTable(targetTableName);

                        // Build columns for the target DataTable
                        dt = BuildColumnsForTargetTable(dt, targetTableColumns, SelectedData);

                        // Populate rows for the target DataTable
                        dt = BuildRowsForTargetTable(dt, targetTableColumns, SelectedData);

                        // Lastly, before adding the targetTable serialize the dt again and store in targetTable.DT
                        targetTable.DT = dt != null ? await dt.SerializeAsync() : default!;


                        targetTables.Add(targetTable);
                    }
                }
            }

            await OnSave.InvokeAsync(targetTables);
        }

        private async Task CloseAsync()
        {
            await OnClose.InvokeAsync();
        }

        private DataTable BuildColumnsForTargetTable(DataTable targetTable, List<TargetTableColumn> targetTableColumns, DataRow[]? selectedData)
        {
            if (selectedData?.Length > 0)
            {
                foreach (var targetColumn in targetTableColumns)
                {
                    if (!targetTable.Columns.Contains(targetColumn.TargetFieldName))
                    {
                        var columnType = selectedData[0]?.Table?.Columns[targetColumn.SourceFieldName]?.DataType;
                        if (columnType != null)
                        {
                            var newColumn = new DataColumn(targetColumn.TargetFieldName, columnType);
                            targetTable.Columns.Add(newColumn);
                        }
                    }
                }
            }

            return targetTable;
        }

        private DataTable? BuildRowsForTargetTable(DataTable targetTable, List<TargetTableColumn> targetTableColumns, DataRow[]? selectedData)
        {
            if (selectedData != null)
            {
                foreach (var selectedRow in selectedData)
                {
                    var tableRow = targetTable.NewRow();

                    foreach (var targetColumn in targetTableColumns)
                    {
                        var sourceFieldName = targetColumn.SourceFieldName;
                        var targetFieldName = targetColumn.TargetFieldName;

                        if (selectedRow.Table.Columns.Contains(sourceFieldName))
                        {
                            tableRow[targetFieldName] = selectedRow[sourceFieldName];
                        }
                    }

                    targetTable.Rows.Add(tableRow);
                }
            }

            return targetTable;
        }

        private async Task AddLookupTableAsync()
        {
            _sessionItems!["_showForeignTableSearchFieldsModal"] = true;

            await _sessionManager.SaveToSessionTableAsync($"{Title}_showForeignTableSearchFieldsModal", _sessionItems["_showForeignTableSearchFieldsModal"], serialize: true);
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task SaveSearchFieldsAsync()
        {
            DataTable? searchFieldsTable = _sessionItems!["_searchFieldsTable"];
            List<SearchField> searchFields = searchFieldsTable?.ConvertDataTableToObjects<SearchField>() ?? default!;
            await Task.CompletedTask;
        }

        private async Task CloseSearchFieldsEntryModalAsync()
        {
            _sessionItems!["_showForeignTableSearchFieldsModal"] = false;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private void RenderTitleButtons(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "data-table-grid-modal-close-button");

            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.CheckCircleFill);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, SaveTargetTablesAsync));
            builder.AddAttribute(seq++, "title", "Add");
            builder.CloseComponent();

            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.XCircleFill);
            builder.AddAttribute(seq++, "title", "Close");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, CloseAsync));
            builder.CloseComponent();

            builder.CloseElement();
        }
        
        private void RenderSelectedDataHeader(RenderTreeBuilder builder, int seq) 
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "row");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "h1");
            builder.AddContent(seq++, "Selected Data");
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();
        }

        private void RenderSelectedDataTable(RenderTreeBuilder builder, int seq)
        {
            IEnumerable<DataRow>? pagedRowsSelectedData = _sessionItems!["_pagedRowsSelectedData"].GetI();
            if (SelectedData != null && pagedRowsSelectedData != null)
            {
                var pagedColumns = pagedRowsSelectedData?.FirstOrDefault()?.Table?.Columns;

                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "data-table-grid-div");
                builder.OpenElement(seq++, "table");
                builder.AddAttribute(seq++, "class", "data-table-grid");

                // Table Header
                builder.OpenElement(seq++, "thead");
                builder.OpenElement(seq++, "tr");
                if (pagedColumns != null)
                {
                    foreach (DataColumn column in pagedColumns)
                    {
                        builder.OpenElement(seq++, "td");
                        builder.AddContent(seq++, column.ColumnName);
                        builder.CloseElement();
                    }
                }
                builder.CloseElement();
                builder.CloseElement();

                // Table Body
                builder.OpenElement(seq++, "tbody");
                if (pagedRowsSelectedData != null)
                {
                    foreach (DataRow row in pagedRowsSelectedData)
                    {
                        builder.OpenElement(seq++, "tr");
                        foreach (DataColumn column in row.Table.Columns)
                        {
                            builder.OpenElement(seq++, "td");
                            builder.AddContent(seq++, row[column]);
                            builder.CloseElement();
                        }

                        builder.CloseElement();
                    }
                }

                builder.CloseElement();
                builder.CloseElement();
                builder.CloseElement();

                // Pagination Controls
                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "pagination-container");

                builder.OpenElement(seq++, "label");
                builder.AddContent(seq++, $"Page Size:");
                builder.CloseElement();

                builder.OpenElement(seq++, "select");
                builder.AddAttribute(seq++, "id", "_pageSizeSelectedData");
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, SelectedDataPageSizeChanged));

                builder.OpenElement(seq++, "option");
                builder.AddAttribute(seq++, "value", _sessionItems["_totalItemsSelectedData"].ToString());
                builder.AddAttribute(seq++, "selected", "selected");
                builder.AddContent(seq++, _sessionItems["_totalItemsSelectedData"].ToString());
                builder.CloseElement();

                foreach (var size in new[] { 5, 10, 20, 50, 100 })
                {
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", size.ToString());
                    builder.AddContent(seq++, size.ToString());
                    builder.CloseElement();
                }

                builder.CloseElement();

                // Pagination Icons
                // Define the tuple array with explicit types
                var icons = new (IconName iconName, string title, Func<Task> action)[]
                {
                        (IconName.ChevronDoubleLeft, "First", GoToFirstPage),
                        (IconName.ChevronLeft, "Previous", GoToPreviousPage),
                        (IconName.ChevronRight, "Next", GoToNextPage),
                        (IconName.ChevronDoubleRight, "Last", GoToLastPage)
                };

                // Use the explicit tuple type for deconstruction
                foreach ((IconName iconName, string title, Func<Task> action) in icons)
                {
                    builder.OpenComponent<Icon>(seq++);
                    builder.AddAttribute(seq++, "Name", iconName);
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, action));
                    builder.AddAttribute(seq++, "class", "pagination-icon");
                    builder.AddAttribute(seq++, "title", title);
                    builder.CloseComponent();
                }

                // Go To Page
                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "type", "number");
                builder.AddAttribute(seq++, "min", "1");
                builder.AddAttribute(seq++, "max", _sessionItems["_totalItemsSelectedData"].ToString());
                builder.AddAttribute(seq++, "bind", _sessionItems["_currentPageSelectedData"].ToString());
                builder.CloseElement();

                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "Name", IconName.CheckCircle);
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToSpecifiedPage));
                builder.AddAttribute(seq++, "class", "pagination-go-icon");
                builder.AddAttribute(seq++, "title", "Go");
                builder.CloseComponent();

                builder.CloseElement();
            }
        }

        private void RenderSetTargetTableHeader(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "row");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "h1");
            builder.AddContent(seq++, "SetI Target Table");
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();
        }

        private void RenderTableColumnsHeader(RenderTreeBuilder builder, int seq)
        {
            List<string>? columnProperties = _sessionItems!["_columnProperties"];
            builder.OpenElement(seq++, "thead");
            if (columnProperties != null)
            {
                builder.OpenElement(seq++, "tr");
                foreach (var prop in columnProperties)
                {
                    builder.OpenElement(seq++, "th");
                    builder.AddContent(seq++, prop);
                    builder.CloseElement();
                }
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        private void RenderTableColumnsBody(RenderTreeBuilder builder, int seq)
        {
            List<TargetTableColumn>? targetTableColumnList = _sessionItems!["_targetTableColumnList"];
            List<string>? columnProperties = _sessionItems["_columnProperties"];

            builder.OpenElement(seq++, "tbody");
            if (targetTableColumnList != null && columnProperties != null)
            {
                foreach (var col in targetTableColumnList)
                {
                    builder.OpenElement(seq++, "tr");
                    foreach (var prop in columnProperties)
                    {
                        var colProperty = col.GetProperty(prop);
                        builder.OpenElement(seq++, "td");
                        builder.AddContent(seq++, colProperty);
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
            }
        }

        private void RenderTableColumns(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "row");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "for", "sourceField");
            builder.AddContent(seq++, "Table Columns:");
            builder.CloseElement();

            builder.OpenElement(seq++, "table");
            builder.AddAttribute(seq++, "id", "tableColumns-div");
            builder.AddAttribute(seq++, "class", "table");

            // Table Columns Header
            RenderTableColumnsHeader(builder, seq);

            // Table Columns Body
            RenderTableColumnsBody(builder, seq);

            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();
        }

        private void RenderLookupTableModalEntry(RenderTreeBuilder builder, int seq)
        {
            bool showForeignTableSearchFieldsModal = _sessionItems!["_showForeignTableSearchFieldsModal"];
            builder.OpenComponent<AddLookupTableModal>(seq++);
            builder.AddAttribute(seq++, "Tag", _tag);
            builder.AddAttribute(seq++, "ShowSearchFieldsModal", showForeignTableSearchFieldsModal);
            builder.AddAttribute(seq++, "OnSave", EventCallback.Factory.Create(this, SaveSearchFieldsAsync));
            builder.AddAttribute(seq++, "OnClose", EventCallback.Factory.Create(this, CloseSearchFieldsEntryModalAsync));
            builder.AddAttribute(seq++, "TableList", TableList);
            builder.CloseComponent();
        }

        private void RenderButtons(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");

            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.Table);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, AddLookupTableAsync));
            builder.AddAttribute(seq++, "class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(seq++, "title", "Add Lookup Table");
            builder.CloseComponent();

            builder.CloseElement();

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, AddLookupTableAsync));
            builder.AddAttribute(seq++, "class", "form-control cursor-pointer");
            builder.AddAttribute(seq++, "title", "Search Foreign Table Search Fields");
            builder.AddAttribute(seq++, "value", "Search Foreign Table Fields");
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.PlusCircleFill);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, SetTargetTableFieldAsync));
            builder.AddAttribute(seq++, "class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(seq++, "title", "Add");
            builder.CloseComponent();

            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.Tornado);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, DataCombAsync));
            builder.AddAttribute(seq++, "class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(seq++, "title", "Data Comb");
            builder.CloseComponent();

            builder.CloseElement();
        }

        private void RenderTargetField(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "for", "targetField");
            builder.AddContent(seq++, "Target Field:");
            builder.CloseElement();

            builder.OpenElement(seq++, "select");
            builder.AddAttribute(seq++, "class", "form-control");
            builder.AddAttribute(seq++, "id", "targetField");
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, OnTargetFieldChanged));

            int selectedTableID = _sessionItems!["_selectedTableID"];
            string? selectedFieldValue = _sessionItems!["_selectedFieldValue"];
            var fieldValues = GetFieldValues(selectedTableID);
            if (fieldValues != null)
            {
                foreach (var field in fieldValues)
                {
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", field);
                    builder.AddAttribute(seq++, "selected", selectedFieldValue?.Equals(field) ?? false ? "selected" : null);
                    builder.AddContent(seq++, field);
                    builder.CloseElement();
                }
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        private void RenderTargetTable(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "for", "targetTable");
            builder.AddContent(seq++, "Target Table:");
            builder.CloseElement();

            builder.OpenElement(seq++, "select");
            builder.AddAttribute(seq++, "class", "form-control");
            builder.AddAttribute(seq++, "id", "targetTable");
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, OnTargetTableValueChanged));

            if (TableList != null)
            {
                int selectedTableID = _sessionItems!["_selectedTableID"];
                foreach (var table in TableList)
                {
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", table.ID.ToString());
                    builder.AddAttribute(seq++, "selected", selectedTableID.Equals(table.ID) ? "selected" : null);
                    builder.AddContent(seq++, table.TableName);
                    builder.CloseElement();
                }
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        private void RenderSourceField(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "row");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "for", "sourceField");
            builder.AddContent(seq++, "Source Field:");
            builder.CloseElement();

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "id", "sourceField-div");
            if (SelectedData != null)
            {
                builder.OpenElement(seq++, "select");
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "id", "sourceField");
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, OnSourceFieldValueChanged));
                string? selectedSourceField = _sessionItems!["_selectedSourceField"];
                foreach (DataColumn sourceFieldCol in SelectedData[0].Table.Columns)
                {
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", sourceFieldCol.ColumnName);
                    builder.AddAttribute(seq++, "selected", selectedSourceField?.Equals(sourceFieldCol.ColumnName) ?? false ? "selected" : null);
                    builder.AddContent(seq++, sourceFieldCol.ColumnName);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();

            // Target Table
            RenderTargetTable(builder, seq);

            // Target Field
            RenderTargetField(builder, seq);

            // Buttons
            RenderButtons(builder, seq);

            // Table Columns
            RenderTableColumns(builder, seq);
            builder.CloseElement();
        }

        private void RenderUniqueField(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "row");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "col");
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "for", "_uniqueField");
            builder.AddContent(seq++, "Unique Field:");
            builder.CloseElement();

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "id", "sourceField-div");
            if (SelectedData != null)
            {
                builder.OpenElement(seq++, "select");
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "id", "_uniqueField");
                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, OnUniqueFieldValueChanged));

                foreach (DataColumn sourceFieldCol in SelectedData[0].Table.Columns)
                {
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", sourceFieldCol.ColumnName);
                    builder.AddContent(seq++, sourceFieldCol.ColumnName);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }
            builder.CloseElement();
            builder.CloseElement();

            // Source Field
            RenderSourceField(builder, seq);
            builder.CloseElement();
        }

        private void RenderModalContent(RenderTreeBuilder builder, int seq)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "data-table-grid-modal-content");

            // Selected Data Header
            RenderSelectedDataHeader(builder, seq);

            // Display Selected Data Table
            RenderSelectedDataTable(builder, seq);

            // SetI Target Table Header
            RenderSetTargetTableHeader(builder, seq);

            // Unique Field
            RenderUniqueField(builder, seq);
            builder.CloseElement();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            try 
            {
                var seq = 0;
                
                // Modal container
                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "id", $"{Title.ToLower()}-set-target-table-modal");
                builder.AddAttribute(seq++, "class", $"data-table-grid-modal");

                // PageTitle
                builder.OpenElement(seq++, "PageTitle");
                builder.AddContent(seq++, "SetTargetTableModal");
                builder.CloseElement();

                // Buttons
                RenderTitleButtons(builder, seq);

                // Modal content
                RenderModalContent(builder, seq);

                bool showForeignTableSearchFieldsModal = _sessionItems!["_showForeignTableSearchFieldsModal"];
                // AddLookupTableModal
                if (showForeignTableSearchFieldsModal)
                {
                    RenderLookupTableModalEntry(builder, seq);
                }

                builder.CloseElement();
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
            
        }
    }
}

