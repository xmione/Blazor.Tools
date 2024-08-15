using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class SetTargetTableModal : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "Sample Table";
        [Parameter] public bool ShowSetTargetTableModal { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<List<TargetTable>> OnSave { get; set; }
        [Parameter] public DataRow[]? SelectedData { get; set; } = default!;
        [Parameter] public EventCallback<DataRow[]> OnSelectedDataComb { get; set; }
        [Parameter] public EventCallback<List<TargetTableColumn>> SetTargetTableColumnList { get; set; }
        [Parameter] public List<AssemblyTable>? TableList { get; set; } = default!;

        private SessionManager _sessionManager = SessionManager.Instance;
        private string? _tag;
        private int _selectedTableID;
        private int _selectedLookupTableID;
        private string? _selectedTableValue;
        private string? _selectedLookupTableValue;
        private string? _selectedFieldValue;
        private string? _selectedPrimaryKeyFieldValue;
        private string? _selectedForeignKeyFieldValue;
        private string? _selectedSourceField;
        private string _searchQuerySelectedData = string.Empty;
        private IEnumerable<DataRow>? _filteredRowsSelectedData;
        private IEnumerable<DataRow>? _pagedRowsSelectedData;
        private int _pageSizeSelectedData = 0;
        private int _currentPageSelectedData = 1;
        private int _totalPagesSelectedData = 0;
        private int _totalItemsSelectedData = 0;
        private List<TargetTableColumn>? _targetTableColumnList;
        private List<string>? _columnProperties;
        private string? _uniqueField;
        private bool _isRetrieved = false;
        private bool _showForeignTableSearchFieldsModal = false;
        private DataTable? _searchFieldsTable;
        private IList<SessionItem>? _sessionItems;

        protected override async Task OnParametersSetAsync()
        {
            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();
            await UpdateVariablesAfterSessionRetrieval();

            await base.OnParametersSetAsync();
        }

        private async Task InitializeVariables()
        {
            _sessionItems = new List<SessionItem>
        {
            new SessionItem()
            {
                Key = $"{Title}_selectedData", Value = SelectedData, Type = typeof(DataRow[]), Serialize = true
            },
            new SessionItem()
            {
                Key = $"{Title}_uniqueField", Value = _uniqueField, Type = typeof(string), Serialize = false
            },
            new SessionItem()
            {
                Key = $"{Title}_selectedSourceField", Value = _selectedSourceField, Type = typeof(string), Serialize = false
            },
            new SessionItem()
            {
                Key = $"{Title}_selectedTableID", Value = _selectedTableID, Type = typeof(int), Serialize = true
            },
            new SessionItem()
            {
                Key = $"{Title}_selectedTableValue", Value = _selectedTableValue, Type = typeof(string), Serialize = false
            },
            new SessionItem()
            {
                Key = $"{Title}_selectedFieldValue", Value = _selectedFieldValue, Type = typeof(string), Serialize = false
            },
            new SessionItem()
            {
                Key = $"{Title}_selectedLookupTableID", Value = _selectedLookupTableID, Type = typeof(int), Serialize = true
            },
            new SessionItem()
            {
                Key = $"{Title}_selectedLookupTableValue", Value = _selectedLookupTableValue, Type = typeof(string), Serialize = false
            },
            new SessionItem()
            {
                Key = $"{Title}_targetTableColumnList", Value = _targetTableColumnList, Type = typeof(List<TargetTableColumn>), Serialize = true
            },
            new SessionItem()
            {
                Key = $"{Title}_columnProperties", Value = _columnProperties, Type = typeof(List<string>), Serialize = true
            },
            new SessionItem()
            {
                Key = $"{Title}_showForeignTableSearchFieldsModal", Value = _showForeignTableSearchFieldsModal, Type = typeof(bool), Serialize = true
            }
        };

            _tag = $"{_selectedTableValue}-{_selectedFieldValue}";
            await Task.CompletedTask;
        }

        private async Task RetrieveDataFromSessionTableAsync()
        {
            try
            {
                if (!_isRetrieved && _sessionItems != null)
                {
                    _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
                    SelectedData = (DataRow[]?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedData"))?.Value;
                    _uniqueField = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_uniqueField"))?.Value?.ToString() ?? string.Empty;
                    _selectedSourceField = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedSourceField"))?.Value?.ToString() ?? string.Empty;
                    _selectedTableID = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedTableID"))?.Value?.ToString(), out int selectedTableIDResult) ? selectedTableIDResult : 0;
                    _selectedTableValue = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedTableValue"))?.Value?.ToString() ?? string.Empty;
                    _selectedFieldValue = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedFieldValue"))?.Value?.ToString() ?? string.Empty;
                    _selectedLookupTableID = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedLookupTableID"))?.Value?.ToString(), out int selectedLookupTableIDResult) ? selectedLookupTableIDResult : 0;
                    _selectedLookupTableValue = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedLookupTableValue"))?.Value?.ToString() ?? string.Empty;
                    _targetTableColumnList = (List<TargetTableColumn>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_targetTableColumnList"))?.Value;
                    _columnProperties = (List<string>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_columnProperties"))?.Value;
                    _showForeignTableSearchFieldsModal = (bool)(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_showForeignTableSearchFieldsModal"))?.Value ?? false);

                    _tag = $"{_selectedTableValue}-{_selectedFieldValue}";
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

        private async Task UpdateVariablesAfterSessionRetrieval()
        {

            if (SelectedData != null)
            {
                _totalItemsSelectedData = SelectedData.Length;
                _pageSizeSelectedData = _totalItemsSelectedData;
                _filteredRowsSelectedData = ApplyFilterSelectedData();
                CalculateTotalPagesAndPagedData();

                if (_targetTableColumnList != null)
                {
                    if (_targetTableColumnList.Any())
                    {
                        _columnProperties = _targetTableColumnList.First().GetPropertyNames().ToList();
                    }
                    else
                    {
                        _columnProperties = new List<string>();
                    }
                }
                else
                {
                    _columnProperties = new List<string>();
                }

                // Set the initial selected value
                if (_totalItemsSelectedData > 0 && string.IsNullOrEmpty(_selectedSourceField))
                {
                    _selectedSourceField = SelectedData[0].Table.Columns[0].ColumnName;
                    var selectedTableValue = TableList?.FirstOrDefault();
                    _selectedTableID = selectedTableValue?.ID ?? 0;
                    var selectedFieldValue = GetFieldValues(_selectedTableID)?.FirstOrDefault();
                    _selectedFieldValue = selectedFieldValue ?? string.Empty;

                    _selectedLookupTableID = 0;
                    _selectedPrimaryKeyFieldValue = _selectedFieldValue;

                    _uniqueField = _selectedSourceField;
                }
            }

            await Task.CompletedTask;
        }

        private async Task OnSourceFieldValueChanged(ChangeEventArgs e)
        {
            _selectedSourceField = e?.Value?.ToString() ?? string.Empty;
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedSourceField", _selectedSourceField);
            await Task.CompletedTask;
        }

        private async Task OnTargetTableValueChanged(ChangeEventArgs e)
        {
            // Parse selected value from int to TableEnum
            int selectedIntValue = int.Parse(e?.Value?.ToString() ?? string.Empty);
            _selectedTableID = selectedIntValue;

            _selectedTableValue = TableList?.FirstOrDefault(t => t.ID == _selectedTableID)?.TableName ?? string.Empty;

            // Reset selected field value when table value changes
            _selectedFieldValue = string.Empty;
            _tag = $"{_selectedTableValue}-{_selectedFieldValue}";

            // Handle the selected value here
            Console.WriteLine("selectedValue: {0}", _selectedTableID);

            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedTableID", _selectedTableID, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedTableValue", _selectedTableValue);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedFieldValue", _selectedFieldValue);

            await Task.CompletedTask;
        }

        private async Task OnSelectedLookupTableValueChanged(ChangeEventArgs e)
        {
            // Parse selected value from int to TableEnum
            int selectedIntValue = int.Parse(e?.Value?.ToString() ?? string.Empty);
            _selectedLookupTableID = selectedIntValue;
            _selectedLookupTableValue = TableList?.FirstOrDefault(t => t.ID == _selectedLookupTableID)?.TableName ?? string.Empty;

            // Reset selected field value when table value changes
            _selectedPrimaryKeyFieldValue = string.Empty;
            _tag = $"{_selectedTableValue}-{_selectedFieldValue}";

            // Handle the selected value here
            Console.WriteLine("selectedValue: {0}", _selectedLookupTableID);

            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedLookupTableID", _selectedLookupTableID, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedLookupTableValue", _selectedLookupTableValue, serialize: true);
            await Task.CompletedTask;
        }

        private async Task OnTargetFieldChanged(ChangeEventArgs e)
        {
            _selectedTableValue = TableList?.FirstOrDefault(t => t.ID == _selectedTableID)?.TableName ?? string.Empty;
            _selectedFieldValue = e?.Value?.ToString() ?? string.Empty;
            _tag = $"{_selectedTableValue}-{_selectedFieldValue}";

            await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedFieldValue", _selectedFieldValue);
            await Task.CompletedTask;
        }

        private async Task OnPrimaryKeyFieldChanged(ChangeEventArgs e)
        {
            _selectedLookupTableValue = TableList?.FirstOrDefault(t => t.ID == _selectedLookupTableID)?.TableName ?? string.Empty;
            _selectedPrimaryKeyFieldValue = e?.Value?.ToString() ?? string.Empty;
            await Task.CompletedTask;
        }

        private async Task OnForeignKeyFieldChanged(ChangeEventArgs e)
        {
            _selectedLookupTableValue = TableList?.FirstOrDefault(t => t.ID == _selectedLookupTableID)?.TableName ?? string.Empty;
            _selectedForeignKeyFieldValue = e?.Value?.ToString() ?? string.Empty;
            await Task.CompletedTask;
        }

        private async Task SetTargetTableFieldAsync()
        {
            if (_targetTableColumnList == null)
            {
                _targetTableColumnList = new List<TargetTableColumn>();
            }

            var targetTableColumn = _targetTableColumnList.FirstOrDefault(s => s.SourceFieldName == _selectedSourceField)
                                    ?? new TargetTableColumn();
            _selectedTableValue = TableList?.FirstOrDefault(t => t.ID == _selectedTableID)?.TableName ?? string.Empty;
            var sourceFieldName = _selectedSourceField ?? string.Empty;
            var targetTableName = _selectedTableValue ?? string.Empty;
            var targetFieldName = _selectedFieldValue ?? string.Empty;
            var checkOnTableName = _selectedLookupTableValue ?? string.Empty;

            Type dataType = typeof(object);
            if (!string.IsNullOrEmpty(_selectedSourceField) && SelectedData?.Length > 0 && SelectedData[0].Table.Columns.Contains(_selectedSourceField))
            {
                dataType = SelectedData[0][_selectedSourceField]?.GetType() ?? typeof(object);
            }

            targetTableColumn.SourceFieldName = sourceFieldName;
            targetTableColumn.TargetTableName = targetTableName;
            targetTableColumn.TargetFieldName = targetFieldName;
            targetTableColumn.CheckOnTableName = checkOnTableName;
            targetTableColumn.TargetFieldName = targetFieldName;

            targetTableColumn.DataType = dataType.Name;

            if (!_targetTableColumnList.Contains(targetTableColumn))
            {
                _targetTableColumnList.Add(targetTableColumn);
            }

            if (_targetTableColumnList != null)
            {
                if (_targetTableColumnList.Any())
                {
                    _columnProperties = _targetTableColumnList.First().GetPropertyNames().ToList();
                }
                else
                {
                    _columnProperties = new List<string>();
                }
            }
            else
            {
                _columnProperties = new List<string>();
            }

            await _sessionManager.SaveToSessionTableAsync($"{Title}_targetTableColumnList", _targetTableColumnList, serialize: true);
            await _sessionManager.SaveToSessionTableAsync($"{Title}_columnProperties", _columnProperties, serialize: true);
            await SetTargetTableColumnList.InvokeAsync(_targetTableColumnList);
            StateHasChanged();
        }

        private IEnumerable<string>? GetFieldValues(int id)
        {
            var selectedTable = TableList?.FirstOrDefault(t => t.ID == id);
            var properties = selectedTable?.GetPropertyNames();

            return properties;

        }

        private IEnumerable<DataRow> ApplyFilterSelectedData()
        {
            if (string.IsNullOrWhiteSpace(_searchQuerySelectedData))
            {
                return SelectedData;
            }

            // Assuming SelectedData is DataRow[]
            return SelectedData.Where(row =>
            {
                // Check each column in the row for the search query
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (row[column]?.ToString()?.IndexOf(_searchQuerySelectedData, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true; // Return true if search query is found in any column
                    }
                }
                return false; // Return false if search query is not found in any column
            });
        }

        private void ChangePage(int pageNumber)
        {
            _currentPageSelectedData = pageNumber;
            StateHasChanged(); // Refresh UI after changing page
        }

        private void SelectedDataPageSizeChanged(ChangeEventArgs e)
        {
            _pageSizeSelectedData = Convert.ToInt32(e.Value);
            _currentPageSelectedData = 1; // Reset to first page when changing page size
            CalculateTotalPagesAndPagedData();
        }

        private void CalculateTotalPagesAndPagedData()
        {
            if (_filteredRowsSelectedData != null)
            {
                _totalPagesSelectedData = (int)Math.Ceiling((double)(_filteredRowsSelectedData?.Count() ?? 0) / _pageSizeSelectedData);
                _pagedRowsSelectedData = _filteredRowsSelectedData?.Skip((_currentPageSelectedData - 1) * _pageSizeSelectedData).Take(_pageSizeSelectedData);
            }
            else
            {
                _totalPagesSelectedData = 0;
                _pagedRowsSelectedData = Enumerable.Empty<DataRow>();
            }

            StateHasChanged(); // Refresh UI after calculating pages and paged data
        }

        private async Task GoToFirstPage()
        {
            _currentPageSelectedData = 1;
            StateHasChanged();
            await Task.CompletedTask; // If you have no async operations, you can use Task.CompletedTask
        }

        private async Task GoToPreviousPage()
        {
            if (_currentPageSelectedData > 1)
            {
                _currentPageSelectedData--;
                StateHasChanged();
            }
            await Task.CompletedTask;
        }

        private async Task GoToNextPage()
        {
            if (_currentPageSelectedData < _totalPagesSelectedData)
            {
                _currentPageSelectedData++;
                StateHasChanged();
            }
            await Task.CompletedTask;
        }

        private async Task GoToLastPage()
        {
            _currentPageSelectedData = _totalPagesSelectedData;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task GoToSpecifiedPage()
        {
            if (_currentPageSelectedData >= 1 && _currentPageSelectedData <= _totalPagesSelectedData)
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
            if (SelectedData != null && SelectedData.Length > 1 && !string.IsNullOrEmpty(_uniqueField))
            {
                SelectedData = SelectedData
                    .Where(row => row[_uniqueField] != DBNull.Value &&
                                  row[_uniqueField] != null &&
                                  !string.IsNullOrEmpty(row[_uniqueField].ToString()))
                    .ToArray();  // Explicitly convert to DataRow[]


                await OnSelectedDataComb.InvokeAsync(SelectedData);
            }

            await Task.CompletedTask;

            StateHasChanged();
        }

        private async Task OnUniqueFieldValueChanged(ChangeEventArgs e)
        {
            _uniqueField = e?.Value?.ToString() ?? string.Empty;

            await _sessionManager.SaveToSessionTableAsync($"{Title}_uniqueField", _uniqueField);
            await DataCombAsync();

        }

        private async Task SaveTargetTablesAsync()
        {
            var targetTables = new List<TargetTable>();
            if (_targetTableColumnList != null)
            {
                var targetTableGroups = _targetTableColumnList?
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
                            TargetTableColumns = targetTableColumns,
                        };

                        var dt = new DataTable(targetTableName);

                        // Build columns for the target DataTable
                        dt = BuildColumnsForTargetTable(dt, targetTableColumns, SelectedData);

                        // Populate rows for the target DataTable
                        dt = BuildRowsForTargetTable(dt, targetTableColumns, SelectedData);

                        // Lastly, before adding the targetTable serialize the dt again and store in targetTable.DT
                        targetTable.DT = await dt.SerializeAsync();

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

        private DataTable BuildColumnsForTargetTable(DataTable targetTable, List<TargetTableColumn> targetTableColumns, DataRow[] selectedData)
        {
            if (selectedData.Length > 0)
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

        private DataTable BuildRowsForTargetTable(DataTable targetTable, List<TargetTableColumn> targetTableColumns, DataRow[] selectedData)
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

            return targetTable;
        }

        private async Task AddLookupTableAsync()
        {
            _showForeignTableSearchFieldsModal = true;

            await _sessionManager.SaveToSessionTableAsync($"{Title}_showForeignTableSearchFieldsModal", _showForeignTableSearchFieldsModal, serialize: true);
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task SaveSearchFieldsAsync()
        {
            List<SearchField> searchFields = _searchFieldsTable?.ConvertDataTableToObjects<SearchField>() ?? default!;
            await Task.CompletedTask;
        }
        private async Task CloseSearchFieldsEntryModalAsync()
        {
            _showForeignTableSearchFieldsModal = false;
            StateHasChanged();
            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;

            // PageTitle
            builder.OpenElement(sequence++, "PageTitle");
            builder.AddContent(sequence++, "SetTargetTableModal");
            builder.CloseElement();

            // Modal container
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", $"data-table-grid-modal {(ShowSetTargetTableModal ? "show" : "")}");

            // Buttons
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "data-table-grid-modal-close-button");

            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", "IconName.CheckCircleFill");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, SaveTargetTablesAsync));
            builder.AddAttribute(sequence++, "title", "Add");
            builder.CloseComponent();

            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", "IconName.XCircleFill");
            builder.AddAttribute(sequence++, "title", "Close");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, CloseAsync));
            builder.CloseComponent();

            builder.CloseElement();

            // Modal content
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "data-table-grid-modal-content");

            // Selected Data Header
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "h1");
            builder.AddContent(sequence++, "Selected Data");
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();

            // Display Selected Data Table
            if (SelectedData != null && _pagedRowsSelectedData != null)
            {
                var pagedColumns = _pagedRowsSelectedData?.FirstOrDefault()?.Table?.Columns;

                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "data-table-grid-div");
                builder.OpenElement(sequence++, "table");
                builder.AddAttribute(sequence++, "class", "data-table-grid");

                // Table Header
                builder.OpenElement(sequence++, "thead");
                builder.OpenElement(sequence++, "tr");
                if (pagedColumns != null)
                {
                    foreach (DataColumn column in pagedColumns)
                    {
                        builder.OpenElement(sequence++, "td");
                        builder.AddContent(sequence++, column.ColumnName);
                        builder.CloseElement();
                    }
                }
                builder.CloseElement();
                builder.CloseElement();

                // Table Body
                builder.OpenElement(sequence++, "tbody");
                if (_pagedRowsSelectedData != null)
                {
                    foreach (DataRow row in _pagedRowsSelectedData)
                    {
                        builder.OpenElement(sequence++, "tr");
                        foreach (DataColumn column in row.Table.Columns)
                        {
                            builder.OpenElement(sequence++, "td");
                            builder.AddContent(sequence++, row[column]);
                            builder.CloseElement();
                        }
                        builder.CloseElement();
                    }
                }
                builder.CloseElement();

                builder.CloseElement();
                builder.CloseElement();

                // Pagination Controls
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "pagination-container");

                builder.OpenElement(sequence++, "label");
                builder.AddContent(sequence++, $"Page Size:");
                builder.CloseElement();

                builder.OpenElement(sequence++, "select");
                builder.AddAttribute(sequence++, "id", "_pageSizeSelectedData");
                builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create(this, SelectedDataPageSizeChanged));

                builder.OpenElement(sequence++, "option");
                builder.AddAttribute(sequence++, "value", _totalItemsSelectedData.ToString());
                builder.AddAttribute(sequence++, "selected", "selected");
                builder.AddContent(sequence++, _totalItemsSelectedData.ToString());
                builder.CloseElement();

                foreach (var size in new[] { 5, 10, 20, 50, 100 })
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", size.ToString());
                    builder.AddContent(sequence++, size.ToString());
                    builder.CloseElement();
                }

                builder.CloseElement();

                // Pagination Icons
                // Define the tuple array with explicit types
                var icons = new (string iconName, string title, Func<Task> action)[]
                {
                    ("IconName.ChevronDoubleLeft", "First", GoToFirstPage),
                    ("IconName.ChevronLeft", "Previous", GoToPreviousPage),
                    ("IconName.ChevronRight", "Next", GoToNextPage),
                    ("IconName.ChevronDoubleRight", "Last", GoToLastPage)
                };

                // Use the explicit tuple type for deconstruction
                foreach ((string iconName, string title, Func<Task> action) in icons)
                {
                    builder.OpenComponent<Icon>(sequence++);
                    builder.AddAttribute(sequence++, "Name", iconName);
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, action));
                    builder.AddAttribute(sequence++, "class", "pagination-icon");
                    builder.AddAttribute(sequence++, "title", title);
                    builder.CloseComponent();
                }


                // Go To Page
                builder.OpenElement(sequence++, "input");
                builder.AddAttribute(sequence++, "type", "number");
                builder.AddAttribute(sequence++, "min", "1");
                builder.AddAttribute(sequence++, "max", _totalPagesSelectedData.ToString());
                builder.AddAttribute(sequence++, "bind", _currentPageSelectedData.ToString());
                builder.CloseElement();

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", "IconName.CheckCircle");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, GoToSpecifiedPage));
                builder.AddAttribute(sequence++, "class", "pagination-go-icon");
                builder.AddAttribute(sequence++, "title", "Go");
                builder.CloseComponent();

                builder.CloseElement();
            }

            // Set Target Table Header
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "h1");
            builder.AddContent(sequence++, "Set Target Table");
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();

            // Unique Field
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "for", "_uniqueField");
            builder.AddContent(sequence++, "Unique Field:");
            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "id", "sourceField-div");
            if (SelectedData != null)
            {
                builder.OpenElement(sequence++, "select");
                builder.AddAttribute(sequence++, "class", "form-control");
                builder.AddAttribute(sequence++, "id", "_uniqueField");
                builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create(this, OnUniqueFieldValueChanged));

                foreach (DataColumn sourceFieldCol in SelectedData[0].Table.Columns)
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", sourceFieldCol.ColumnName);
                    builder.AddContent(sequence++, sourceFieldCol.ColumnName);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }
            builder.CloseElement();
            builder.CloseElement();

            // Source Field
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "for", "sourceField");
            builder.AddContent(sequence++, "Source Field:");
            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "id", "sourceField-div");
            if (SelectedData != null)
            {
                builder.OpenElement(sequence++, "select");
                builder.AddAttribute(sequence++, "class", "form-control");
                builder.AddAttribute(sequence++, "id", "sourceField");
                builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create(this, OnSourceFieldValueChanged));

                foreach (DataColumn sourceFieldCol in SelectedData[0].Table.Columns)
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", sourceFieldCol.ColumnName);
                    builder.AddAttribute(sequence++, "selected", _selectedSourceField?.Equals(sourceFieldCol.ColumnName) ?? false ? "selected" : null);
                    builder.AddContent(sequence++, sourceFieldCol.ColumnName);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }
            builder.CloseElement();
            builder.CloseElement();

            // Target Table
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "for", "targetTable");
            builder.AddContent(sequence++, "Target Table:");
            builder.CloseElement();

            builder.OpenElement(sequence++, "select");
            builder.AddAttribute(sequence++, "class", "form-control");
            builder.AddAttribute(sequence++, "id", "targetTable");
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create(this, OnTargetTableValueChanged));

            if (TableList != null)
            {
                foreach (var table in TableList)
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", table.ID.ToString());
                    builder.AddAttribute(sequence++, "selected", _selectedTableID.Equals(table.ID) ? "selected" : null);
                    builder.AddContent(sequence++, table.TableName);
                    builder.CloseElement();
                }
            }

            builder.CloseElement();
            builder.CloseElement();

            // Target Field
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "for", "targetField");
            builder.AddContent(sequence++, "Target Field:");
            builder.CloseElement();

            builder.OpenElement(sequence++, "select");
            builder.AddAttribute(sequence++, "class", "form-control");
            builder.AddAttribute(sequence++, "id", "targetField");
            builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create(this, OnTargetFieldChanged));

            var fieldValues = GetFieldValues(_selectedTableID);
            if (fieldValues != null)
            {
                foreach (var field in fieldValues)
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", field);
                    builder.AddAttribute(sequence++, "selected", _selectedFieldValue?.Equals(field) ?? false ? "selected" : null);
                    builder.AddContent(sequence++, field);
                    builder.CloseElement();
                }
            }

            builder.CloseElement();
            builder.CloseElement();

            // Buttons
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");

            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", "IconName.Table");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, AddLookupTableAsync));
            builder.AddAttribute(sequence++, "class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(sequence++, "title", "Add Lookup Table");
            builder.CloseComponent();

            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "button");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, AddLookupTableAsync));
            builder.AddAttribute(sequence++, "class", "form-control cursor-pointer");
            builder.AddAttribute(sequence++, "title", "Search Foreign Table Search Fields");
            builder.AddAttribute(sequence++, "value", "Search Foreign Table Fields");
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", "IconName.PlusCircleFill");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, SetTargetTableFieldAsync));
            builder.AddAttribute(sequence++, "class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(sequence++, "title", "Add");
            builder.CloseComponent();

            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", "IconName.Tornado");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, DataCombAsync));
            builder.AddAttribute(sequence++, "class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(sequence++, "title", "Data Comb");
            builder.CloseComponent();

            builder.CloseElement();

            // Table Columns
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col");
            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "for", "sourceField");
            builder.AddContent(sequence++, "Table Columns:");
            builder.CloseElement();

            builder.OpenElement(sequence++, "table");
            builder.AddAttribute(sequence++, "id", "tableColumns-div");
            builder.AddAttribute(sequence++, "class", "table");

            // Table Columns Header
            builder.OpenElement(sequence++, "thead");
            if (_columnProperties != null)
            {
                builder.OpenElement(sequence++, "tr");
                foreach (var prop in _columnProperties)
                {
                    builder.OpenElement(sequence++, "th");
                    builder.AddContent(sequence++, prop);
                    builder.CloseElement();
                }
                builder.CloseElement();
            }
            builder.CloseElement();

            // Table Columns Body
            builder.OpenElement(sequence++, "tbody");
            if (_targetTableColumnList != null && _columnProperties != null)
            {
                foreach (var col in _targetTableColumnList)
                {
                    builder.OpenElement(sequence++, "tr");
                    foreach (var prop in _columnProperties)
                    {
                        var colProperty = col.GetProperty(prop);
                        builder.OpenElement(sequence++, "td");
                        builder.AddContent(sequence++, colProperty);
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
            }
            builder.CloseElement();

            builder.CloseElement();
            builder.CloseElement();

            // AddLookupTableModal
            if (_showForeignTableSearchFieldsModal)
            {
                builder.OpenComponent<AddLookupTableModal>(sequence++);
                builder.AddAttribute(sequence++, "Tag", _tag);
                builder.AddAttribute(sequence++, "ShowSearchFieldsModal", _showForeignTableSearchFieldsModal);
                builder.AddAttribute(sequence++, "OnSave", EventCallback.Factory.Create(this, SaveSearchFieldsAsync));
                builder.AddAttribute(sequence++, "OnClose", EventCallback.Factory.Create(this, CloseSearchFieldsEntryModalAsync));
                builder.AddAttribute(sequence++, "TableList", TableList);
                builder.CloseComponent();
            }

            builder.CloseElement();
        }
    }
}

