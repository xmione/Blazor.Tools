﻿using Blazor.Tools.BlazorBundler.Entities;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Data;
using System.Collections.Generic;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid : ComponentBase
    {

        [Parameter] public string Title { get; set; } = "Sample Table";
        [Parameter] public RenderFragment ChildContent { get; set; } = null!;
        [Parameter] public DataTable DataTable { get; set; } = null!;
        [Parameter] public List<string>? HiddenColumnNames { get; set; } = default!;
        [Parameter] public Dictionary<string, string>? HeaderNames { get; set; } = default!;
        [Parameter] public bool AllowCellSelection { get; set; } = false;

        private SessionManager _sessionManager = SessionManager.Instance;
        private bool _renderTable = false; // Flag to control when to render RenderTable()

        private Dictionary<string, DataTable> _dataSources = new Dictionary<string, DataTable>();
        private TableGrid TableGridContext = null!;
        private List<TableSource> _tableSources = new List<TableSource>();
        public TableNode? TableNodeContext = default!;
        private bool _isRetrieved = false;
        private IList<SessionItem>? _sessionItems;
        private bool _showAddButton = true;
        // _node variables
        private DataTable? _nodeDataTable;
        private int _nodeFilteredItems;
        private int _nodeTotalItems;
        private int _nodeTotalPages;
        private int _nodeCurrentPage;
        private int _nodePageSize;
        private string _nodeStartCell = string.Empty;
        private string _nodeEndCell = string.Empty;
        private bool _nodeShowAddRowModal;
        private DataRow? _nodeNewRowData;
        private List<TableColumn>? _nodeColumns;
        private IEnumerable<DataRow>? _nodePagedRows;
        private bool _nodeIsEditing;
        private DataRow? _nodeEditedRow;
        private Dictionary<string, object>? _nodeEditValues;
        private bool _nodeIsFirstCellClicked;
        private DataRow[]? _nodeSelectedData;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            Console.WriteLine("TableGrid OnParametersSet");
            TableGridContext = this;
            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();
        }

        private async Task InitializeVariables()
        {
            _sessionItems = new List<SessionItem>
            {
                new SessionItem()
                {
                    Key = $"{Title}_nodeDataTable", Value = _nodeDataTable, Type = typeof(DataTable), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeFilteredItems", Value = _nodeFilteredItems, Type = typeof(int), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeTotalItems", Value = _nodeTotalItems, Type = typeof(int), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeTotalPages", Value = _nodeTotalPages, Type = typeof(int), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeCurrentPage", Value = _nodeCurrentPage, Type = typeof(int), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodePageSize", Value = _nodePageSize, Type = typeof(int), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeStartCell", Value = _nodeStartCell, Type = typeof(string), Serialize = false
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeEndCell", Value = _nodeEndCell, Type = typeof(string), Serialize = false
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeShowAddRowModal", Value = _nodeShowAddRowModal, Type = typeof(bool), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeNewRowData", Value = _nodeNewRowData, Type = typeof(DataRow), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeColumns", Value = _nodeColumns, Type = typeof(List<TableColumn>), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodePagedRows", Value = _nodePagedRows, Type = typeof(IEnumerable<DataRow>), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeIsEditing", Value = _nodeIsEditing, Type = typeof(bool), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeEditedRow", Value = _nodeEditedRow, Type = typeof(DataRow), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeEditValues", Value = _nodeEditValues, Type = typeof(Dictionary<string, object>), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeIsFirstCellClicked", Value = _nodeIsFirstCellClicked, Type = typeof(bool), Serialize = true
                },
                new SessionItem()
                {
                    Key = $"{Title}_nodeSelectedData", Value = _nodeSelectedData, Type = typeof(DataRow[]), Serialize = true
                }
            };

            TableNodeContext = new TableNode();

            _nodeCurrentPage = 1;

            if (DataTable != null)
            {
                _nodeDataTable = DataTable;
                var dataSourceName = DataTable.TableName + "DS";

                RegisterDataSource(dataSourceName, DataTable);
                TableNodeContext.DataSource = dataSourceName;

                if (_nodeColumns == null)
                {
                    _nodeColumns = new List<TableColumn>();
                }

                bool useHeaderNames = HeaderNames != null;
                foreach (DataColumn col in DataTable.Columns)
                {
                    string? headerName = string.Empty;
                    HeaderNames?.TryGetValue(col.ColumnName, out headerName);
                    headerName = useHeaderNames ? headerName : col?.ColumnName;
                    bool visible = HiddenColumnNames?.FirstOrDefault(h => h.Equals(col?.ColumnName)) == null;
                    var tableColumn = new TableColumn()
                    {
                        DataSourceName = dataSourceName,
                        FieldName = col.ColumnName,
                        HeaderName = headerName,
                        Type = "TextBox",
                        Visible = visible
                    };

                    tableColumn.SetDataSource(DataTable);
                    _nodeColumns.Add(tableColumn);
                    TableNodeContext.AddColumn(tableColumn);
                }

                TableNodeContext.Columns = _nodeColumns;
                TableNodeContext.DataTable = DataTable;
                TableNodeContext.UpdateDisplayFromPageSize();
                TableNodeContext.UpdateFromTableNodeContext();

                _isRetrieved = true;
                _renderTable = true;
                _showAddButton = false;

            }

            await Task.CompletedTask;
        }

        private async Task RetrieveDataFromSessionTableAsync()
        {
            try
            {
                if (_sessionItems != null)
                {
                    _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
                    await PopulateNodeVariablesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            await Task.CompletedTask;
        }

        private async Task PopulateNodeVariablesAsync()
        {
            if (!_isRetrieved)
            {
                _nodeDataTable = (DataTable?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeDataTable"))?.Value ?? _nodeDataTable;
                _nodeFilteredItems = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeFilteredItems"))?.Value?.ToString(), out int _nodeFilteredItemsResult) ? _nodeFilteredItemsResult : _nodeFilteredItems;
                _nodeTotalItems = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeTotalItems"))?.Value?.ToString(), out int _nodeTotalItemsResult) ? _nodeTotalItemsResult : _nodeTotalItems;
                _nodeTotalPages = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeTotalPages"))?.Value?.ToString(), out int _nodeTotalPagesResult) ? _nodeTotalPagesResult : _nodeTotalPages;
                _nodeCurrentPage = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeCurrentPage"))?.Value?.ToString(), out int _nodeCurrentPageResult) ? _nodeCurrentPageResult : _nodeCurrentPage;
                _nodeStartCell = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeStartCell"))?.Value?.ToString() ?? _nodeStartCell;
                _nodeEndCell = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeEndCell"))?.Value?.ToString() ?? _nodeEndCell;
                _nodeShowAddRowModal = (bool?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeShowAddRowModal"))?.Value ?? _nodeShowAddRowModal;
                _nodeNewRowData = (DataRow?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeNewRowData"))?.Value ?? _nodeNewRowData;
                _nodeColumns = (List<TableColumn>?)(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeColumns"))?.Value ?? _nodeColumns);
                _nodePagedRows = (IEnumerable<DataRow>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodePagedRows"))?.Value ?? _nodePagedRows;
                _nodePageSize = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodePageSize"))?.Value?.ToString(), out int _nodePageSizeResult) ? _nodePageSizeResult : _nodePageSize;
                _nodeIsEditing = (bool?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeIsEditing"))?.Value ?? _nodeIsEditing;
                _nodeEditedRow = (DataRow?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeEditedRow"))?.Value ?? _nodeEditedRow;
                _nodeEditValues = (Dictionary<string, object>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeEditValues"))?.Value ?? _nodeEditValues;
                _nodeIsFirstCellClicked = (bool?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeIsFirstCellClicked"))?.Value ?? _nodeIsFirstCellClicked;
                _nodeSelectedData = (DataRow[]?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeSelectedData"))?.Value ?? _nodeSelectedData;

                _isRetrieved = true;

                if (TableNodeContext != null)
                {
                    TableNodeContext._shouldRender = false;

                    TableNodeContext.DataTable = _nodeDataTable ?? new DataTable();
                    TableNodeContext.FilteredItems = _nodeFilteredItems;
                    TableNodeContext.TotalItems = _nodeTotalItems;
                    TableNodeContext.TotalPages = _nodeTotalPages;
                    TableNodeContext.CurrentPage = _nodeCurrentPage;
                    TableNodeContext.StartCell = _nodeStartCell;
                    TableNodeContext.EndCell = _nodeEndCell;
                    TableNodeContext.ShowAddRowModal = _nodeShowAddRowModal;
                    TableNodeContext.NewRowData = _nodeNewRowData ?? default!;
                    TableNodeContext.Columns = _nodeColumns;
                    TableNodeContext.PagedRows = _nodePagedRows;
                    TableNodeContext.PageSize = _nodePageSize;
                    TableNodeContext.IsEditing = _nodeIsEditing;
                    TableNodeContext.EditedRow = _nodeEditedRow;
                    TableNodeContext.EditValues = _nodeEditValues ?? default!;
                    TableNodeContext.IsFirstCellClicked = _nodeIsFirstCellClicked;
                    TableNodeContext.SelectedData = _nodeSelectedData ?? default!;
                }

            }
            else
            {
                if (TableNodeContext != null)
                {
                    _nodeDataTable = TableNodeContext.DataTable;
                    _nodeFilteredItems = TableNodeContext.FilteredItems;
                    _nodeTotalItems = TableNodeContext.TotalItems;
                    _nodeTotalPages = TableNodeContext.TotalPages;
                    _nodeCurrentPage = TableNodeContext.CurrentPage;
                    _nodeStartCell = TableNodeContext.StartCell;
                    _nodeEndCell = TableNodeContext.EndCell;
                    _nodeShowAddRowModal = TableNodeContext.ShowAddRowModal;
                    _nodeNewRowData = TableNodeContext.NewRowData;
                    _nodeColumns = TableNodeContext.Columns;
                    _nodePagedRows = TableNodeContext.PagedRows;
                    _nodePageSize = TableNodeContext.PageSize;
                    _nodeIsEditing = TableNodeContext.IsEditing;
                    _nodeEditedRow = TableNodeContext.EditedRow;
                    _nodeEditValues = TableNodeContext.EditValues;
                    _nodeIsFirstCellClicked = TableNodeContext.IsFirstCellClicked = _nodeIsFirstCellClicked;
                    _nodeSelectedData = TableNodeContext.SelectedData ?? default!;

                }
            }

            StateHasChanged();
            await Task.CompletedTask;
        }

        private RenderFragment RenderTable => builder =>
        {
            var renderTask = RenderTableContentAsync(builder);
            renderTask.Wait();
        };

        public void AddTableSource(TableSource source)
        {
            _tableSources.Add(source);
            TableGridContext = this;
        }

        public void SetNode(TableNode node)
        {
            TableNodeContext = node;
            PopulateNodeVariablesAsync().Wait();
            TableGridContext = this;
        }

        public void RegisterDataSource(string name, DataTable dataTable)
        {
            dataTable.TableName = name;
            _dataSources[name] = dataTable;
            TableGridContext = this;
        }

        public DataTable? GetDataSource(string name)
        {
            _dataSources.TryGetValue(name, out var dataTable);
            return dataTable;
        }

        private async Task RenderTableContentAsync(RenderTreeBuilder builder)
        {
            // <div>
            // <div class="data-table-grid-div">
            //     <table class="data-table-grid">
            int sequence = 0;
            builder.OpenElement(sequence, "div");
            builder.AddContent(sequence++, Title);
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "data-table-grid-div");

            builder.OpenElement(sequence++, "table");
            builder.AddAttribute(sequence++, "title", Title);
            builder.AddAttribute(sequence++, "class", "data-table-grid");

            builder.OpenElement(sequence++, "thead");
            builder.OpenElement(sequence++, "tr");
            // Add a blank th column for edit/delete buttons.
            builder.OpenElement(sequence++, "th");
            builder.CloseElement(); // th
                                    // Add a blank th column for Row number.
            builder.OpenElement(sequence++, "th");
            builder.AddContent(sequence++, "Row");
            builder.CloseElement(); // th

            // Start displaying the proper column headers.
            if (_nodeColumns != null)
            {
                foreach (var column in _nodeColumns)
                {
                    if (column.Visible)
                    {
                        builder.OpenElement(sequence++, "th");
                        builder.AddContent(sequence++, column.HeaderName);
                        builder.CloseElement(); // th
                    }
                }

            }

            builder.CloseElement(); // tr
            builder.CloseElement(); // thead

            builder.OpenElement(sequence++, "tbody");

            if (_nodePagedRows != null)
            {
                var totalRows = _nodePagedRows.Count();
                var pagedRows = _nodePagedRows.ToList();
                _nodeCurrentPage = _nodeCurrentPage == 0 ? 1 : _nodeCurrentPage;
                if (totalRows > 0)
                {
                    for (int i = 0; i < totalRows; i++)
                    {
                        builder.OpenElement(sequence++, "tr");
                        var currentRow = pagedRows[i];
                        builder = await RenderEditAndDeleteButtons(sequence++, builder, currentRow);

                        //Row number column
                        var rowNumber = ((_nodeCurrentPage - 1) * _nodePageSize) + i + 1;
                        builder.OpenElement(sequence++, "td");
                        builder.AddContent(sequence++, $"{rowNumber}");
                        builder.CloseElement(); // td

                        if (_nodeColumns != null)
                        {
                            for (var c = 0; c < _nodeColumns.Count; c++)
                            {
                                var column = _nodeColumns[c];

                                if (column.Visible)
                                {
                                    int rowIndex = i;
                                    int columnIndex = c;
                                    builder.OpenElement(sequence++, "td");
                                    builder.AddAttribute(sequence++, "title", $"R{rowIndex}C{columnIndex}");
                                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => HandleCellClickAsync(rowIndex, columnIndex)));
                                    builder = await RenderTableColumn(sequence++, rowIndex, builder, column);
                                    builder.CloseElement(); // td
                                }
                                else
                                {
                                    if (HiddenColumnNames == null)
                                    {
                                        HiddenColumnNames = new List<string>();
                                    }

                                    HiddenColumnNames.Add(column.FieldName);
                                }

                            }

                        }

                        builder.CloseElement(); // tr
                    }
                }
            }

            builder.CloseElement(); // tbody
            builder.CloseElement(); //  <table class="data-table-grid">

            builder.CloseElement(); // <div class="data-table-grid-div">
            builder.CloseElement(); // div

            await Task.CompletedTask;
        }

        private async Task<RenderTreeBuilder> RenderEditAndDeleteButtons(int sequence, RenderTreeBuilder builder, DataRow? row)
        {
            builder.OpenElement(sequence++, "td");
            builder.AddAttribute(sequence++, "class", "icons-td");

            if (!_nodeIsEditing || row != _nodeEditedRow)
            {
                // Edit icon
                // <Icon Name="IconName.PencilFill" @onclick="() => EditRow(row)" title="Edit" class="text-primary icon-button" />

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", IconName.PencilFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(sequence++, "Class", "text-primary icon-button cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Edit");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => EditRow(row)));
                builder.CloseComponent();

                // Delete icon
                // <Icon Name="IconName.TrashFill" @onclick="() => DeleteRowAsync(row)" title="Delete" class="text-danger icon-button" />

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", IconName.TrashFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(sequence++, "Class", "text-danger icon-button cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Delete");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => DeleteRowAsync(row)));
                builder.CloseComponent();
            }
            else
            {
                // Save icon
                // <Icon Name="IconName.CheckCircleFill" @onclick="SaveRowAsync" title="Save" class="text-success icon-button" />

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", IconName.CheckCircleFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(sequence++, "Class", "text-success icon-button cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Save");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, SaveRowAsync));
                builder.CloseComponent();

                // Cancel icon
                // <Icon Name="IconName.XCircleFill" @onclick="CancelEdit" title="Cancel" class="text-secondary icon-button" />
                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", IconName.XCircleFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(sequence++, "Class", "text-secondary icon-button cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Cancel");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, CancelEdit));
                builder.CloseComponent();
            }

            builder.CloseElement();

            await Task.CompletedTask;

            return builder;
        }

        private void EditRow(DataRow? row)
        {
            if (_nodeDataTable != null && row != null)
            {
                _nodeEditedRow = row;
                _nodeEditValues = _nodeDataTable.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => row[col]);
                _nodeIsEditing = true;
            }
        }

        private async Task DeleteRowAsync(DataRow? row)
        {
            if (_nodeDataTable != null && row != null)
            {
                _nodeDataTable.Rows.Remove(row);
                if (TableNodeContext != null)
                {
                    TableNodeContext.DataTable = _nodeDataTable;

                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeDataTable", _nodeDataTable, serialize: true);
                }

                StateHasChanged(); // Refresh UI after deleting row
            }

            await Task.CompletedTask;
        }

        private async Task SaveRowAsync()
        {
            if (_nodeEditedRow != null && _nodeEditValues != null && _nodeDataTable != null)
            {
                foreach (var column in _nodeDataTable.Columns.Cast<DataColumn>())
                {
                    _nodeEditedRow[column.ColumnName] = _nodeEditValues[column.ColumnName];
                }

                _nodeEditedRow = default!;
                _nodeIsEditing = false;

                if (TableNodeContext != null)
                {
                    TableNodeContext.DataTable = _nodeDataTable;
                    TableNodeContext.EditedRow = _nodeEditedRow;
                    TableNodeContext.IsEditing = _nodeIsEditing;

                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeDataTable", _nodeDataTable, serialize: true);
                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEditedRow", _nodeEditedRow, serialize: true);
                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeIsEditing", _nodeIsEditing, serialize: true);
                }

                StateHasChanged();
            }

            await Task.CompletedTask;
        }

        private void CancelEdit()
        {
            _nodeEditedRow = default!;
            _nodeIsEditing = false;
            StateHasChanged(); // Refresh UI after canceling edit
        }

        public async Task<RenderTreeBuilder> RenderTableColumn(int sequence, int rowIndex, RenderTreeBuilder builder, TableColumn column)
        {
            var colDataSource = column.GetDataSource();
            var colDataSourceRows = colDataSource.Rows;
            var rows = _nodePagedRows?.ToList();

            if (rows != null)
            {
                var row = rows[rowIndex];
                var isEditing = _nodeIsEditing == true && _nodeEditedRow != null && row == _nodeEditedRow;
                var editValues = _nodeEditValues != null ? _nodeEditValues[column.FieldName]?.ToString() : null;

                if (isEditing)
                {
                    if (column.Type == "TextBox")
                    {
                        builder.OpenElement(sequence, "input");
                        builder.AddAttribute(sequence++, "type", "text");
                        builder.AddAttribute(sequence++, "class", "form-control");
                        builder.AddAttribute(sequence++, "value", editValues);
                        builder.AddAttribute(sequence++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
                        {
                            if (TableNodeContext != null && _nodeEditValues != null)
                            {
                                _nodeEditValues[column.FieldName] = e?.Value?.ToString() ?? default!;
                            }
                        }));

                        builder.CloseElement();
                    }
                    else if (column.Type == "DropdownList")
                    {
                        builder.OpenElement(sequence, "select");
                        builder.AddAttribute(sequence++, "class", "form-control");
                        builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
                        {
                            if (TableNodeContext != null && _nodeEditValues != null)
                            {
                                _nodeEditValues[column.FieldName] = e?.Value?.ToString() ?? default!;
                            }
                        }));

                        for (int i = 0; i < colDataSourceRows.Count; i++)
                        {
                            DataRow iRow = colDataSourceRows[i];
                            var displayName = iRow[column.DisplayFieldName].ToString();
                            var displayValue = iRow[column.DisplayFieldValue].ToString();

                            builder.OpenElement(sequence++, "option");
                            builder.AddAttribute(sequence++, "value", displayValue);
                            if (displayValue == editValues)
                            {
                                builder.AddAttribute(sequence++, "selected", "selected");
                            }
                            builder.AddContent(sequence++, displayName);
                            builder.CloseElement();
                        }

                        builder.CloseElement();
                    }
                }
                else
                {
                    var columnStringValue = row[column.FieldName]?.ToString();
                    if (column.Type == "DropdownList")
                    {
                        for (int i = 0; i < colDataSourceRows.Count; i++)
                        {
                            DataRow iRow = colDataSourceRows[i];
                            var displayValue = iRow[column.DisplayFieldValue].ToString();
                            if (displayValue == columnStringValue)
                            {
                                columnStringValue = iRow[column.DisplayFieldName].ToString();
                                break;
                            }
                        }
                    }

                    builder.AddContent(sequence, columnStringValue);
                }
            }

            await Task.CompletedTask;

            return builder;
        }

        private async Task HandleFilterDataTableAsync(IEnumerable<DataRow> filteredRows)
        {
            TableNodeContext?.FilterData(filteredRows);
            await PopulateNodeVariablesAsync();
            StateHasChanged(); // Ensure UI updates after filtering
            await Task.CompletedTask;
        }

        private async Task ShowAddRowModalAsync()
        {
            if (_nodeDataTable != null)
            {
                _nodeNewRowData = _nodeDataTable.NewRow();

                if (TableNodeContext != null)
                {
                    TableNodeContext.NewRowData = _nodeNewRowData;

                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeNewRowData", _nodeNewRowData, serialize: true);
                }

                _nodeShowAddRowModal = true;
                StateHasChanged();
            }

            await Task.CompletedTask;
        }

        private async Task PageSizeChangedAsync(ChangeEventArgs e)
        {
            _nodePageSize = Convert.ToInt32(e.Value);
            _nodeCurrentPage = 1; // Reset to first page when changing page size

            if (TableNodeContext != null)
            {
                TableNodeContext.PageSize = _nodePageSize;
                TableNodeContext.CurrentPage = _nodeCurrentPage;
                TableNodeContext.UpdateDisplayFromPageSize();

                await PopulateNodeVariablesAsync();
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodePageSize", _nodePageSize, serialize: true);
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
            }

            await Task.CompletedTask;
        }
        private async Task GoToFirstPageAsync()
        {
            _nodeCurrentPage = 1;

            if (TableNodeContext != null)
            {
                TableNodeContext.CurrentPage = _nodeCurrentPage;
                TableNodeContext.UpdateDisplayFromPageSize();

                await PopulateNodeVariablesAsync();
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
            }

            await Task.CompletedTask;
        }

        private async Task GoToPreviousPageAsync()
        {
            if (_nodeCurrentPage > 1)
            {
                _nodeCurrentPage--;
                if (TableNodeContext != null)
                {
                    TableNodeContext.CurrentPage = _nodeCurrentPage;
                    TableNodeContext.UpdateDisplayFromPageSize();

                    await PopulateNodeVariablesAsync();
                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
                }

            }

            await Task.CompletedTask;
        }

        private async Task GoToNextPageAsync()
        {
            if (_nodeCurrentPage < _nodeTotalPages)
            {
                _nodeCurrentPage++;

                if (TableNodeContext != null)
                {
                    TableNodeContext.CurrentPage = _nodeCurrentPage;
                    TableNodeContext.UpdateDisplayFromPageSize();

                    await PopulateNodeVariablesAsync();
                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
                }
            }

            await Task.CompletedTask;
        }

        private async Task GoToLastPageAsync()
        {
            _nodeCurrentPage = _nodeTotalPages;

            if (TableNodeContext != null)
            {
                TableNodeContext.CurrentPage = _nodeCurrentPage;
                TableNodeContext.UpdateDisplayFromPageSize();

                await PopulateNodeVariablesAsync();
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
            }

            await Task.CompletedTask;
        }

        private async Task GoToSpecifiedPageAsync()
        {
            if (_nodeCurrentPage >= 1 && _nodeCurrentPage <= _nodeTotalPages)
            {
                if (TableNodeContext != null)
                {
                    TableNodeContext.CurrentPage = _nodeCurrentPage;
                    TableNodeContext.UpdateDisplayFromPageSize();

                    await PopulateNodeVariablesAsync();
                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
                }
            }
            else
            {
                //TODO: sol
                // Handle invalid page number
                // For example, display a toast message or an error message
            }

            await Task.CompletedTask;
        }

        private async Task ClearSelectionAsync(MouseEventArgs e)
        {
            _nodeStartCell = string.Empty;
            _nodeEndCell = string.Empty;

            if (TableNodeContext != null)
            {
                TableNodeContext.StartCell = _nodeStartCell;
                TableNodeContext.EndCell = _nodeEndCell;

                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);
            }

            await Task.CompletedTask;
        }

        private async Task HandleStartCellClick(MouseEventArgs e)
        {
            _nodeStartCell = string.Empty;

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleEndCellClick(MouseEventArgs e)
        {
            _nodeStartCell = string.Empty;

            StateHasChanged();

            await Task.CompletedTask;
        }


        private void CloseAddRowModal()
        {
            _nodeShowAddRowModal = false;
            StateHasChanged(); // Ensure UI updates to hide the modal
        }

        private async Task AddRowAsync()
        {
            if (_nodeDataTable != null && _nodeNewRowData != null)
            {
                DataRow newRow = _nodeDataTable.NewRow();
                foreach (DataColumn column in _nodeDataTable.Columns)
                {
                    newRow[column.ColumnName] = _nodeNewRowData[column.ColumnName];
                }

                _nodeDataTable.Rows.Add(newRow);

                if (TableNodeContext != null)
                {
                    TableNodeContext.DataTable = _nodeDataTable;

                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeDataTable", _nodeDataTable, serialize: true);

                }

                StateHasChanged(); // Ensure UI updates after adding row
            }

            CloseAddRowModal();

            await Task.CompletedTask;
        }

        private async Task HandleCellClickAsync(int rowIndex, int columnIndex)
        {
            string cellIdentifier = $"R{rowIndex}C{columnIndex}";

            if (string.IsNullOrEmpty(_nodeStartCell) || _nodeIsFirstCellClicked)
            {
                _nodeStartCell = cellIdentifier;
                _nodeIsFirstCellClicked = false;

            }
            else
            {
                _nodeEndCell = cellIdentifier;
                _nodeIsFirstCellClicked = true;

            }

            if (TableNodeContext != null)
            {
                TableNodeContext.StartCell = _nodeStartCell;
                TableNodeContext.EndCell = _nodeEndCell;
                TableNodeContext.IsFirstCellClicked = _nodeIsFirstCellClicked;

                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeIsFirstCellClicked", _nodeIsFirstCellClicked, serialize: true);
            }

            StateHasChanged(); // Refresh UI to reflect the changes in cell selection

            await Task.CompletedTask;
        }

        public async Task HandleSelectedDataComb(DataRow[] selectedData)
        {
            _nodeSelectedData = selectedData;

            if (TableNodeContext != null)
            {
                TableNodeContext.SelectedData = _nodeSelectedData;
                TableNodeContext.StartCell = _nodeStartCell;
                TableNodeContext.EndCell = _nodeEndCell;

                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);

            }

            StateHasChanged();
            await Task.CompletedTask;
        }

        public async Task<DataRow[]?> ShowSetTargetTableModalAsync()
        {
            if (!string.IsNullOrEmpty(_nodeStartCell) && !string.IsNullOrEmpty(_nodeEndCell))
            {
                // Extracting start row and column from startCell
                int startRow = int.Parse(_nodeStartCell.Substring(1, _nodeStartCell.IndexOf('C') - 1));
                int startCol = int.Parse(_nodeStartCell.Substring(_nodeStartCell.IndexOf('C') + 1));

                // Extracting end row and column from endCell
                int endRow = int.Parse(_nodeEndCell.Substring(1, _nodeEndCell.IndexOf('C') - 1));
                int endCol = int.Parse(_nodeEndCell.Substring(_nodeEndCell.IndexOf('C') + 1));

                _nodeSelectedData = GetDataInRange(startRow, startCol, endRow, endCol);

                if (TableNodeContext != null)
                {
                    TableNodeContext.SelectedData = _nodeSelectedData;

                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);
                }

                await Task.CompletedTask;
            }

            StateHasChanged();

            return _nodeSelectedData;
        }

        private DataRow[] GetDataInRange(int startRow, int startCol, int endRow, int endCol)
        {
            List<DataRow> dataInRange = new List<DataRow>();

            if (_nodeDataTable != null)
            {
                // Create a new DataTable with the selected columns
                DataTable filteredDataTable = new DataTable();
                for (int i = startCol; i <= endCol; i++)
                {
                    DataColumn column = _nodeDataTable.Columns[i];
                    filteredDataTable.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
                }

                for (int i = startRow; i <= endRow; i++)
                {
                    DataRow newRow = filteredDataTable.NewRow();

                    for (int j = startCol; j <= endCol; j++)
                    {
                        newRow[j - startCol] = _nodeDataTable.Rows[i][j];
                    }

                    dataInRange.Add(newRow);
                }

            }

            return dataInRange.ToArray();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Ensure RenderTable() is rendered only after initial render
                _renderTable = true;
                StateHasChanged();
            }

            var nodeValues = new Dictionary<string, object?>
        {
            { $"{Title}_nodeDataTable", _nodeDataTable },
            { $"{Title}_nodeFilteredItems", _nodeFilteredItems },
            { $"{Title}_nodeTotalItems", _nodeTotalItems },
            { $"{Title}_nodeTotalPages", _nodeTotalPages },
            { $"{Title}_nodeCurrentPage", _nodeCurrentPage },
            { $"{Title}_nodeStartCell", _nodeStartCell },
            { $"{Title}_nodeEndCell", _nodeEndCell },
            { $"{Title}_nodeShowAddRowModal", _nodeShowAddRowModal },
            { $"{Title}_nodeNewRowData", _nodeNewRowData },
            { $"{Title}_nodeColumns", _nodeColumns },
            { $"{Title}_nodePagedRows", _nodePagedRows },
            { $"{Title}_nodePageSize", _nodePageSize },
            { $"{Title}_nodeIsEditing", _nodeIsEditing },
            { $"{Title}_nodeEditedRow", _nodeEditedRow },
            { $"{Title}_nodeEditValues", _nodeEditValues },
            { $"{Title}_nodeIsFirstCellClicked", _nodeIsFirstCellClicked },
            { $"{Title}_nodeSelectedData", _nodeSelectedData }
        };

            // Update the SessionItems with new values
            foreach (var item in _sessionItems)
            {
                if (nodeValues.TryGetValue(item.Key, out var newValue))
                {
                    item.Value = newValue;
                }
                else
                {
                    throw new Exception($"Unrecognized session item key: {item.Key}");
                }
            }

            List<SessionItem> sessionItems = _sessionItems.ToList();

            await _sessionManager.SaveToSessionTableAsync<List<SessionItem>>(sessionItems);
            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;

            // DTSearchBox component
            builder.OpenComponent<DTSearchBox>(sequence++);
            builder.AddAttribute(sequence++, "DataTable", _nodeDataTable);
            builder.AddAttribute(sequence++, "OnFilterDataTable", EventCallback.Factory.Create<IEnumerable<DataRow>>(this, HandleFilterDataTableAsync));
            builder.CloseComponent();

            // Conditional rendering
            if (!_renderTable)
            {
                builder.OpenComponent<CascadingValue<TableGrid>>(sequence++);
                builder.AddAttribute(sequence++, "Value", TableGridContext);
                builder.AddAttribute(sequence++, "ChildContent", ChildContent);
                builder.CloseComponent();
            }
            else
            {
                builder.AddContent(sequence++, RenderTable);

                if (_showAddButton)
                {
                    builder.OpenComponent<Icon>(sequence++);
                    builder.AddAttribute(sequence++, "Name", "IconName.PlusCircleFill");
                    builder.AddAttribute(sequence++, "Class", "text-success icon-button mb-2 cursor-pointer");
                    builder.AddAttribute(sequence++, "title", "Add");
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, ShowAddRowModalAsync));
                    builder.CloseComponent();
                }

                // Pagination controls
                builder.OpenElement(sequence++, "div");
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "pagination-container");

                builder.OpenElement(sequence++, "label");
                builder.AddContent(sequence++, $"Total Items: {_nodeFilteredItems}/{_nodeTotalItems}");
                builder.CloseElement();

                builder.OpenElement(sequence++, "label");
                builder.AddAttribute(sequence++, "for", "pageSize");
                builder.AddContent(sequence++, "Page Size:");
                builder.CloseElement();

                builder.OpenElement(sequence++, "select");
                builder.AddAttribute(sequence++, "id", "pageSize");
                builder.AddAttribute(sequence++, "class", "cursor-pointer");
                builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, PageSizeChangedAsync));

                builder.OpenElement(sequence++, "option");
                builder.AddAttribute(sequence++, "value", _nodeTotalItems.ToString());
                builder.AddAttribute(sequence++, "selected", true);
                builder.AddContent(sequence++, _nodeTotalItems);
                builder.CloseElement();

                foreach (var size in new[] { 5, 10, 20, 50, 100 })
                {
                    builder.OpenElement(sequence++, "option");
                    builder.AddAttribute(sequence++, "value", size.ToString());
                    builder.AddContent(sequence++, size);
                    builder.CloseElement();
                }

                builder.CloseElement();

                // First Page
                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", "IconName.ChevronDoubleLeft");
                builder.AddAttribute(sequence++, "Class", "pagination-icon cursor-pointer");
                builder.AddAttribute(sequence++, "title", "First");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, GoToFirstPageAsync));
                builder.CloseComponent();

                // Previous Page
                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", "IconName.ChevronLeft");
                builder.AddAttribute(sequence++, "Class", "pagination-icon cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Previous");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, GoToPreviousPageAsync));
                builder.CloseComponent();

                // Next Page
                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", "IconName.ChevronRight");
                builder.AddAttribute(sequence++, "Class", "pagination-icon cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Next");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, GoToNextPageAsync));
                builder.CloseComponent();

                // Last Page
                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", "IconName.ChevronDoubleRight");
                builder.AddAttribute(sequence++, "Class", "pagination-icon cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Last");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, GoToLastPageAsync));
                builder.CloseComponent();

                // Go To Page
                builder.OpenElement(sequence++, "span");
                builder.AddContent(sequence++, "Go to Page: ");
                builder.CloseElement();

                builder.OpenElement(sequence++, "input");
                builder.AddAttribute(sequence++, "type", "number");
                builder.AddAttribute(sequence++, "min", "1");
                builder.AddAttribute(sequence++, "max", _nodeTotalPages.ToString());
                builder.AddAttribute(sequence++, "value", _nodeCurrentPage);
                builder.AddAttribute(sequence++, "class", "cursor-pointer");
                builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => _nodeCurrentPage = int.Parse(e.Value.ToString() ?? "1")));
                builder.CloseElement();

                builder.OpenElement(sequence++, "label");
                builder.AddContent(sequence++, $"of {_nodeTotalPages} {(_nodeTotalPages > 1 ? "Pages" : "Page")}");
                builder.CloseElement();

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", "IconName.CheckCircle");
                builder.AddAttribute(sequence++, "Class", "pagination-go-icon cursor-pointer");
                builder.AddAttribute(sequence++, "title", "Go");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, GoToSpecifiedPageAsync));
                builder.CloseComponent();

                builder.CloseElement(); // Pagination container

                if (AllowCellSelection)
                {
                    // Cell selection controls
                    builder.OpenElement(sequence++, "div");
                    builder.AddAttribute(sequence++, "class", "row mb-2");

                    builder.OpenElement(sequence++, "div");
                    builder.AddAttribute(sequence++, "class", "col-auto");

                    builder.OpenComponent<Icon>(sequence++);
                    builder.AddAttribute(sequence++, "Name", "IconName.Recycle");
                    builder.AddAttribute(sequence++, "Class", "text-success icon-button mb-2 cursor-pointer");
                    builder.AddAttribute(sequence++, "title", "Clear");
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, ClearSelectionAsync));
                    builder.CloseComponent();

                    builder.CloseElement(); // col-auto

                    builder.OpenElement(sequence++, "div");
                    builder.AddAttribute(sequence++, "class", "col");

                    builder.OpenElement(sequence++, "input");
                    builder.AddAttribute(sequence++, "value", _nodeStartCell);
                    builder.AddAttribute(sequence++, "class", "form-control");
                    builder.AddAttribute(sequence++, "placeholder", "Start Cell");
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, HandleStartCellClick));
                    builder.CloseElement();

                    builder.CloseElement(); // col

                    builder.OpenElement(sequence++, "div");
                    builder.AddAttribute(sequence++, "class", "col");

                    builder.OpenElement(sequence++, "input");
                    builder.AddAttribute(sequence++, "value", _nodeEndCell);
                    builder.AddAttribute(sequence++, "class", "form-control");
                    builder.AddAttribute(sequence++, "placeholder", "End Cell");
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, HandleEndCellClick));
                    builder.CloseElement();

                    builder.CloseElement(); // col

                    builder.CloseElement(); // row
                }

                // Add Row Modal
                if (_nodeShowAddRowModal)
                {
                    builder.OpenComponent<AddRowModal>(sequence++);
                    builder.AddAttribute(sequence++, "ShowAddRowModal", _nodeShowAddRowModal);
                    builder.AddAttribute(sequence++, "DataTable", _nodeDataTable);
                    builder.AddAttribute(sequence++, "NewRowData", _nodeNewRowData);
                    builder.AddAttribute(sequence++, "OnClose", EventCallback.Factory.Create(this, CloseAddRowModal));
                    builder.AddAttribute(sequence++, "OnSave", EventCallback.Factory.Create(this, AddRowAsync));
                    builder.AddAttribute(sequence++, "HiddenColumnNames", HiddenColumnNames);
                    builder.CloseComponent();
                }
            }
        }
    }
}
