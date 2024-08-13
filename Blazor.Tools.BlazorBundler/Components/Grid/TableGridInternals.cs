﻿using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Data;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.JSInterop;
using DocumentFormat.OpenXml.Bibliography;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGridInternals<TModel, TIModel, TModelVM> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "Sample List";
        [Parameter] public string TableID { get; set; } = "table-id";
        [Parameter] public IEnumerable<TModelVM> Items { get; set; } = Enumerable.Empty<TModelVM>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public RenderFragment? StartContent { get; set; }
        [Parameter] public RenderFragment? TableHeader { get; set; }
        [Parameter] public RenderFragment<TModelVM> RowTemplate { get; set; } = default!;
        [Parameter] public TIModel IModel { get; set; } = default!;
        [Parameter] public TModelVM ModelVM { get; set; } = default!;
        [Parameter] public bool AllowAdding { get; set; } = true;
        [Parameter] public List<string>? HiddenColumnNames { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<TModelVM>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;
        [Parameter] public EventCallback OnCellClickAsync { get; set; }

        //[Parameter] public RenderFragment ChildContent { get; set; } = null!;
        //[Parameter] public DataTable DataTable { get; set; } = null!;
        //[Parameter] public List<string>? HiddenColumnNames { get; set; } = default!;
        //[Parameter] public Dictionary<string, string>? HeaderNames { get; set; } = default!;


        private SessionManager _sessionManager = SessionManager.Instance;
        private IEnumerable<TModelVM> _filteredRows = default!;
        private IEnumerable<TModelVM> _pagedRows = default!;
        private int _filteredItems = 0;
        private int _totalItems = 0;
        private int _currentPage = 0;
        private int _pageSize = 0;
        private int _totalPages = 0;
        private bool _isEditing;
        private bool _isAdding;
        private TModelVM? _editedRow;
        private TModelVM? _editedRowSaved;
        private TModelVM? _newRowData;
        private string _startCell = string.Empty;
        private string _endCell = string.Empty;
        private bool _isFirstCellClicked;

        protected override async Task OnParametersSetAsync()
        {
            await GetPageRowsAsync(Items);
        }
        protected override async void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;

            // Render StartContent if provided
            if (StartContent != null)
            {
                builder.AddContent(seq++, StartContent);
            }

            // DTSearchBox component
            builder.OpenComponent<DTSearchBox<TModelVM>>(seq++);
            builder.AddAttribute(seq++, "Data", Items);
            builder.AddAttribute(seq++, "OnFilterData", EventCallback.Factory.Create<IEnumerable<TModelVM>>(this, HandleFilterDataTableAsync));
            builder.CloseComponent();

            //Render the table with headers and rows
            //<div>
            //  <div class="data-table-grid-div">
            //      <table class="data-table-grid">
            //          <thead>
            //              <tr>
            //                  <th>
            //                  </th>
            //                  <th>
            //                     Row
            //                  </th>
            //                  <th>
            //                     ID
            //                  </th>
            //                  <th>
            //                     First Name
            //                  </th>
            //              </tr>
            //          </thead>
            builder.OpenElement(seq, "div");
            //builder.AddContent(seq++, Title);
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "id", $"{Title}-div");
            builder.AddAttribute(seq++, "class", "data-table-grid-div");

            builder.OpenElement(seq++, "table");
            //builder.AddAttribute(seq++, "title", Title);
            builder.AddAttribute(seq++, "class", "data-table-grid");

            // Render TableHeader if provided
            if (TableHeader != null)
            {
                builder.OpenElement(seq++, "thead");
                builder.OpenElement(seq++, "tr");

                // Add a blank th column for edit/delete buttons.
                builder.OpenElement(seq++, "th");
                builder.CloseElement(); // th

                // Add a blank th column for Row number.
                builder.OpenElement(seq++, "th");
                builder.AddContent(seq++, "Row");
                builder.CloseElement(); // th

                builder.AddContent(seq++, TableHeader);
                builder.CloseElement(); // tr
                builder.CloseElement(); // thead
            }

            builder.OpenElement(seq++, "tbody");
            builder.AddAttribute(seq++, "class", "data-table-grid-body-scroll");
            // Render items using RowTemplate
            if (_pagedRows != null && RowTemplate != null)
            {
                var pagedRows = _pagedRows.ToList();
                var pagedRowsCount = pagedRows.Count;

                if (pagedRowsCount > 0)
                {
                    for (int i = 0; i < pagedRowsCount; i++)
                    {
                        var item = pagedRows[i];
                        builder.OpenElement(seq++, "tr");
                        builder = RenderEditAndDeleteButtons(seq++, builder, item).Result;

                        //Row number column
                        var rowNumber = ((_currentPage - 1) * _pageSize) + i + 1;
                        builder.OpenElement(seq++, "td");
                        builder.AddContent(seq++, $"{rowNumber}");
                        builder.CloseElement(); // td

                        builder.AddContent(seq++, RowTemplate(item));
                        builder.CloseElement(); // tr

                    }
                }
            }

            builder.CloseElement(); // tbody
            builder.CloseElement(); //  <table class="data-table-grid">

            builder.CloseElement(); // <div class="data-table-grid-div">
            builder.CloseElement(); // div

            await RenderFooterAsync(builder, seq);

            await RenderAllowSelection(builder, seq);

        }
        //        private bool _renderTable = false; // Flag to control when to render RenderTable()

        //        private Dictionary<string, DataTable> _dataSources = new Dictionary<string, DataTable>();
        //        private TableGrid TableGridContext = null!;
        //        private List<TableSource> _tableSources = new List<TableSource>();
        //        public TableNode? TableNodeContext = default!;
        //        private bool _isRetrieved = false;
        //        private IList<SessionItem>? _sessionItems;
        //        private bool _showAddButton = true;
        //        // _node variables
        //        private DataTable? _nodeDataTable;
        //        private int _nodeFilteredItems;
        //        private int _nodeTotalItems;
        //        private int _nodeTotalPages;
        //        private int _nodeCurrentPage;
        //        private int _nodePageSize;
        //        private string _nodeStartCell = string.Empty;
        //        private string _nodeEndCell = string.Empty;
        //        private bool _nodeShowAddRowModal;
        //        private DataRow? _nodeNewRowData;
        //        private List<TableColumn>? _nodeColumns;
        //        private IEnumerable<DataRow>? _nodePagedRows;
        //        private bool _isEditing;
        //        private DataRow? _editedRow;
        //        private Dictionary<string, object>? _nodeEditValues;
        //        private bool _nodeIsFirstCellClicked;
        //        private DataRow[]? _nodeSelectedData;

        //        protected override async Task OnParametersSetAsync()
        //        {
        //            await base.OnParametersSetAsync();

        //            Console.WriteLine("TableGrid OnParametersSet");
        //            TableGridContext = this;
        //            await InitializeVariables();
        //            await RetrieveDataFromSessionTableAsync();
        //        }

        //        private async Task InitializeVariables()
        //        {
        //            _sessionItems = new List<SessionItem>
        //            {
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeDataTable", Value = _nodeDataTable, Type = typeof(DataTable), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeFilteredItems", Value = _nodeFilteredItems, Type = typeof(int), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeTotalItems", Value = _nodeTotalItems, Type = typeof(int), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeTotalPages", Value = _nodeTotalPages, Type = typeof(int), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeCurrentPage", Value = _nodeCurrentPage, Type = typeof(int), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodePageSize", Value = _nodePageSize, Type = typeof(int), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeStartCell", Value = _nodeStartCell, Type = typeof(string), Serialize = false
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeEndCell", Value = _nodeEndCell, Type = typeof(string), Serialize = false
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeShowAddRowModal", Value = _nodeShowAddRowModal, Type = typeof(bool), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeNewRowData", Value = _nodeNewRowData, Type = typeof(DataRow), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeColumns", Value = _nodeColumns, Type = typeof(List<TableColumn>), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodePagedRows", Value = _nodePagedRows, Type = typeof(IEnumerable<DataRow>), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_isEditing", Value = _isEditing, Type = typeof(bool), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_editedRow", Value = _editedRow, Type = typeof(DataRow), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeEditValues", Value = _nodeEditValues, Type = typeof(Dictionary<string, object>), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeIsFirstCellClicked", Value = _nodeIsFirstCellClicked, Type = typeof(bool), Serialize = true
        //                },
        //                new SessionItem()
        //                {
        //                    Key = $"{Title}_nodeSelectedData", Value = _nodeSelectedData, Type = typeof(DataRow[]), Serialize = true
        //                }
        //            };

        //            TableNodeContext = new TableNode();

        //            _nodeCurrentPage = 1;

        //            if (DataTable != null)
        //            {
        //                _nodeDataTable = DataTable;
        //                var dataSourceName = DataTable.TableName + "DS";

        //                RegisterDataSource(dataSourceName, DataTable);
        //                TableNodeContext.DataSource = dataSourceName;

        //                if (_nodeColumns == null)
        //                {
        //                    _nodeColumns = new List<TableColumn>();
        //                }

        //                bool useHeaderNames = HeaderNames != null;
        //                foreach (DataColumn col in DataTable.Columns)
        //                {
        //                    string? headerName = string.Empty;
        //                    HeaderNames?.TryGetValue(col.ColumnName, out headerName);
        //                    headerName = useHeaderNames ? headerName : col?.ColumnName;
        //                    bool visible = HiddenColumnNames?.FirstOrDefault(h => h.Equals(col?.ColumnName)) == null;
        //                    var tableColumn = new TableColumn()
        //                    {
        //                        DataSourceName = dataSourceName,
        //                        FieldName = col.ColumnName,
        //                        HeaderName = headerName,
        //                        Type = "TextBox",
        //                        Visible = visible
        //                    };

        //                    tableColumn.SetDataSource(DataTable);
        //                    _nodeColumns.Add(tableColumn);
        //                    TableNodeContext.AddColumn(tableColumn);
        //                }

        //                TableNodeContext.Columns = _nodeColumns;
        //                TableNodeContext.DataTable = DataTable;
        //                TableNodeContext.UpdateDisplayFromPageSize();
        //                TableNodeContext.UpdateFromTableNodeContext();

        //                _isRetrieved = true;
        //                _renderTable = true;
        //                _showAddButton = false;

        //            }

        //            await Task.CompletedTask;
        //        }

        //        private async Task RetrieveDataFromSessionTableAsync()
        //        {
        //            try
        //            {
        //                if (_sessionItems != null)
        //                {
        //                    _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
        //                    await PopulateNodeVariablesAsync();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error: {0}", ex.Message);
        //            }

        //            await Task.CompletedTask;
        //        }

        //        private async Task PopulateNodeVariablesAsync()
        //        {
        //            if (!_isRetrieved)
        //            {
        //                _nodeDataTable = (DataTable?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeDataTable"))?.Value ?? _nodeDataTable;
        //                _nodeFilteredItems = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeFilteredItems"))?.Value?.ToString(), out int _nodeFilteredItemsResult) ? _nodeFilteredItemsResult : _nodeFilteredItems;
        //                _nodeTotalItems = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeTotalItems"))?.Value?.ToString(), out int _nodeTotalItemsResult) ? _nodeTotalItemsResult : _nodeTotalItems;
        //                _nodeTotalPages = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeTotalPages"))?.Value?.ToString(), out int _nodeTotalPagesResult) ? _nodeTotalPagesResult : _nodeTotalPages;
        //                _nodeCurrentPage = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeCurrentPage"))?.Value?.ToString(), out int _nodeCurrentPageResult) ? _nodeCurrentPageResult : _nodeCurrentPage;
        //                _nodeStartCell = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeStartCell"))?.Value?.ToString() ?? _nodeStartCell;
        //                _nodeEndCell = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeEndCell"))?.Value?.ToString() ?? _nodeEndCell;
        //                _nodeShowAddRowModal = (bool?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeShowAddRowModal"))?.Value ?? _nodeShowAddRowModal;
        //                _nodeNewRowData = (DataRow?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeNewRowData"))?.Value ?? _nodeNewRowData;
        //                _nodeColumns = (List<TableColumn>?)(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeColumns"))?.Value ?? _nodeColumns);
        //                _nodePagedRows = (IEnumerable<DataRow>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodePagedRows"))?.Value ?? _nodePagedRows;
        //                _nodePageSize = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodePageSize"))?.Value?.ToString(), out int _nodePageSizeResult) ? _nodePageSizeResult : _nodePageSize;
        //                _isEditing = (bool?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_isEditing"))?.Value ?? _isEditing;
        //                _editedRow = (DataRow?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_editedRow"))?.Value ?? _editedRow;
        //                _nodeEditValues = (Dictionary<string, object>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeEditValues"))?.Value ?? _nodeEditValues;
        //                _nodeIsFirstCellClicked = (bool?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeIsFirstCellClicked"))?.Value ?? _nodeIsFirstCellClicked;
        //                _nodeSelectedData = (DataRow[]?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_nodeSelectedData"))?.Value ?? _nodeSelectedData;

        //                _isRetrieved = true;

        //                if (TableNodeContext != null)
        //                {
        //                    TableNodeContext._shouldRender = false;

        //                    TableNodeContext.DataTable = _nodeDataTable ?? new DataTable();
        //                    TableNodeContext.FilteredItems = _nodeFilteredItems;
        //                    TableNodeContext.TotalItems = _nodeTotalItems;
        //                    TableNodeContext.TotalPages = _nodeTotalPages;
        //                    TableNodeContext.CurrentPage = _nodeCurrentPage;
        //                    TableNodeContext.StartCell = _nodeStartCell;
        //                    TableNodeContext.EndCell = _nodeEndCell;
        //                    TableNodeContext.ShowAddRowModal = _nodeShowAddRowModal;
        //                    TableNodeContext.NewRowData = _nodeNewRowData ?? default!;
        //                    TableNodeContext.Columns = _nodeColumns;
        //                    TableNodeContext.PagedRows = _nodePagedRows;
        //                    TableNodeContext.PageSize = _nodePageSize;
        //                    TableNodeContext.IsEditing = _isEditing;
        //                    TableNodeContext.EditedRow = _editedRow;
        //                    TableNodeContext.EditValues = _nodeEditValues ?? default!;
        //                    TableNodeContext.IsFirstCellClicked = _nodeIsFirstCellClicked;
        //                    TableNodeContext.SelectedData = _nodeSelectedData ?? default!;
        //                }

        //            }
        //            else
        //            {
        //                if (TableNodeContext != null)
        //                {
        //                    _nodeDataTable = TableNodeContext.DataTable;
        //                    _nodeFilteredItems = TableNodeContext.FilteredItems;
        //                    _nodeTotalItems = TableNodeContext.TotalItems;
        //                    _nodeTotalPages = TableNodeContext.TotalPages;
        //                    _nodeCurrentPage = TableNodeContext.CurrentPage;
        //                    _nodeStartCell = TableNodeContext.StartCell;
        //                    _nodeEndCell = TableNodeContext.EndCell;
        //                    _nodeShowAddRowModal = TableNodeContext.ShowAddRowModal;
        //                    _nodeNewRowData = TableNodeContext.NewRowData;
        //                    _nodeColumns = TableNodeContext.Columns;
        //                    _nodePagedRows = TableNodeContext.PagedRows;
        //                    _nodePageSize = TableNodeContext.PageSize;
        //                    _isEditing = TableNodeContext.IsEditing;
        //                    _editedRow = TableNodeContext.EditedRow;
        //                    _nodeEditValues = TableNodeContext.EditValues;
        //                    _nodeIsFirstCellClicked = TableNodeContext.IsFirstCellClicked = _nodeIsFirstCellClicked;
        //                    _nodeSelectedData = TableNodeContext.SelectedData ?? default!;

        //                }
        //            }

        //            StateHasChanged();
        //            await Task.CompletedTask;
        //        }

        //        private RenderFragment RenderTable => builder =>
        //        {
        //            var renderTask = RenderTableContentAsync(builder);
        //            renderTask.Wait();
        //        };

        //        public void AddTableSource(TableSource source)
        //        {
        //            _tableSources.Add(source);
        //            TableGridContext = this;
        //        }

        //        public void SetNode(TableNode node)
        //        {
        //            TableNodeContext = node;
        //            PopulateNodeVariablesAsync().Wait();
        //            TableGridContext = this;
        //        }

        //        public void RegisterDataSource(string name, DataTable dataTable)
        //        {
        //            dataTable.TableName = name;
        //            _dataSources[name] = dataTable;
        //            TableGridContext = this;
        //        }

        //        public DataTable? GetDataSource(string name)
        //        {
        //            _dataSources.TryGetValue(name, out var dataTable);
        //            return dataTable;
        //        }

        //        private async Task RenderTableContentAsync(RenderTreeBuilder builder)
        //        {
        //            // <div>
        //            // <div class="data-table-grid-div">
        //            //     <table class="data-table-grid">
        //            int seq = 0;
        //            builder.OpenElement(seq, "div");
        //            builder.AddContent(seq++, Title);
        //            builder.OpenElement(seq++, "div");
        //            builder.AddAttribute(seq++, "class", "data-table-grid-div");

        //            builder.OpenElement(seq++, "table");
        //            builder.AddAttribute(seq++, "title", Title);
        //            builder.AddAttribute(seq++, "class", "data-table-grid");

        //            builder.OpenElement(seq++, "thead");
        //            builder.OpenElement(seq++, "tr");
        //            // Add a blank th column for edit/delete buttons.
        //            builder.OpenElement(seq++, "th");
        //            builder.CloseElement(); // th
        //                                    // Add a blank th column for Row number.
        //            builder.OpenElement(seq++, "th");
        //            builder.AddContent(seq++, "Row");
        //            builder.CloseElement(); // th

        //            // Start displaying the proper column headers.
        //            if (_nodeColumns != null)
        //            {
        //                foreach (var column in _nodeColumns)
        //                {
        //                    if (column.Visible)
        //                    {
        //                        builder.OpenElement(seq++, "th");
        //                        builder.AddContent(seq++, column.HeaderName);
        //                        builder.CloseElement(); // th
        //                    }
        //                }

        //            }

        //            builder.CloseElement(); // tr
        //            builder.CloseElement(); // thead

        //            builder.OpenElement(seq++, "tbody");

        //            if (_nodePagedRows != null)
        //            {
        //                var totalRows = _nodePagedRows.Count();
        //                var pagedRows = _nodePagedRows.ToList();
        //                _nodeCurrentPage = _nodeCurrentPage == 0 ? 1 : _nodeCurrentPage;
        //                if (totalRows > 0)
        //                {
        //                    for (int i = 0; i < totalRows; i++)
        //                    {
        //                        builder.OpenElement(seq++, "tr");
        //                        var currentRow = pagedRows[i];
        //                        builder = await RenderEditAndDeleteButtons(seq++, builder, currentRow);

        //                        //Row number column
        //                        var rowNumber = ((_nodeCurrentPage - 1) * _nodePageSize) + i + 1;
        //                        builder.OpenElement(seq++, "td");
        //                        builder.AddContent(seq++, $"{rowNumber}");
        //                        builder.CloseElement(); // td

        //                        if (_nodeColumns != null)
        //                        {
        //                            for (var c = 0; c < _nodeColumns.Count; c++)
        //                            {
        //                                var column = _nodeColumns[c];

        //                                if (column.Visible)
        //                                {
        //                                    int rowIndex = i;
        //                                    int columnIndex = c;
        //                                    builder.OpenElement(seq++, "td");
        //                                    builder.AddAttribute(seq++, "title", $"R{rowIndex}C{columnIndex}");
        //                                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => HandleCellClickAsync(rowIndex, columnIndex)));
        //                                    builder = await RenderTableColumn(seq++, rowIndex, builder, column);
        //                                    builder.CloseElement(); // td
        //                                }
        //                                else
        //                                {
        //                                    if (HiddenColumnNames == null)
        //                                    {
        //                                        HiddenColumnNames = new List<string>();
        //                                    }

        //                                    HiddenColumnNames.Add(column.FieldName);
        //                                }

        //                            }

        //                        }

        //                        builder.CloseElement(); // tr
        //                    }
        //                }
        //            }

        //            builder.CloseElement(); // tbody
        //            builder.CloseElement(); //  <table class="data-table-grid">

        //            builder.CloseElement(); // <div class="data-table-grid-div">
        //            builder.CloseElement(); // div

        //            await Task.CompletedTask;
        //        }

        private async Task<RenderTreeBuilder> RenderEditAndDeleteButtons(int seq, RenderTreeBuilder builder, TModelVM? row)
        {
            builder.OpenElement(seq++, "td");
            builder.AddAttribute(seq++, "class", "icons-td");

            if (!_isEditing || !Equals(row, _editedRow))
            {
                if (_isAdding)
                {
                    // Edit icon
                    // <Icon Name="IconName.PencilFill" @onclick="() => EditRow(modelVM)" title="Edit" class="text-primary icon-button" />

                    builder.OpenComponent<Icon>(seq++);
                    builder.AddAttribute(seq++, "Name", IconName.PencilFill); // Blazor.Bootstrap icon class
                    builder.AddAttribute(seq++, "Class", "text-primary icon-button cursor-pointer hide-element");
                    builder.AddAttribute(seq++, "title", "Edit");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => EditRow(row)));
                    builder.CloseComponent();

                }
                else 
                {
                    // Edit icon
                    // <Icon Name="IconName.PencilFill" @onclick="() => EditRow(modelVM)" title="Edit" class="text-primary icon-button" />

                    builder.OpenComponent<Icon>(seq++);
                    builder.AddAttribute(seq++, "Name", IconName.PencilFill); // Blazor.Bootstrap icon class
                    builder.AddAttribute(seq++, "Class", "text-primary icon-button cursor-pointer show-element");
                    builder.AddAttribute(seq++, "title", "Edit");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => EditRow(row)));
                    builder.CloseComponent();

                }

                // Delete icon
                // <Icon Name="IconName.TrashFill" @onclick="() => DeleteRowAsync(modelVM)" title="Delete" class="text-danger icon-button" />

                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "Name", IconName.TrashFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(seq++, "Class", "text-danger icon-button cursor-pointer");
                builder.AddAttribute(seq++, "title", "Delete");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => DeleteRowAsync(row)));
                builder.CloseComponent();

            }
            else
            {
                // Save icon
                // <Icon Name="IconName.CheckCircleFill" @onclick="SaveRowAsync" title="Save" class="text-success icon-button" />

                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "Name", IconName.CheckCircleFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(seq++, "Class", "text-success icon-button cursor-pointer");
                builder.AddAttribute(seq++, "title", "Save");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => SaveRowAsync(row)));
                builder.CloseComponent();

                // Cancel icon
                // <Icon Name="IconName.XCircleFill" @onclick="CancelEditAsync" title="Cancel" class="text-secondary icon-button" />
                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "Name", IconName.XCircleFill); // Blazor.Bootstrap icon class
                builder.AddAttribute(seq++, "Class", "text-secondary icon-button cursor-pointer");
                builder.AddAttribute(seq++, "title", "Cancel");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, CancelEditAsync));
                builder.CloseComponent();
            }

            builder.CloseElement();

            await Task.CompletedTask;

            return builder;
        }

        public async Task RenderFooterAsync(RenderTreeBuilder builder, int seq)
        {
            if (AllowAdding)
            {
                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "id", "add-row-icon");
                builder.AddAttribute(seq++, "Name", IconName.PlusCircleFill);
                if (_isEditing)
                {
                    builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer hide-element");
                }
                else
                {
                    builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer show-element");
                }
                
                builder.AddAttribute(seq++, "title", "Add");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, AddRowAsync));
                builder.CloseComponent();
            }

            var pageSizes = new List<int> { 10, 20, 50, 100 };
            pageSizes = pageSizes.Where(size => size <= _totalItems).ToList();

            // Add _totalItems to the list if it's not already there
            if (!pageSizes.Contains(_totalItems) && _totalItems > pageSizes.Last())
            {
                pageSizes.Add(_totalItems);
            }

            // Pagination controls
            builder.OpenElement(seq++, "div");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "pagination-container row-display");

            builder.OpenElement(seq++, "label");
            builder.AddContent(seq++, $"Total Items: {_filteredItems}/{_totalItems}");
            builder.CloseElement();

            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "for", "pageSize");
            builder.AddAttribute(seq++, "class", "left-margin-5x");
            builder.AddContent(seq++, "Page Size:");
            builder.CloseElement();

            builder.OpenElement(seq++, "select");
            builder.AddAttribute(seq++, "id", "pageSize");
            builder.AddAttribute(seq++, "class", "form-control left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, PageSizeChangedAsync));

            foreach (var size in pageSizes)
            {
                builder.OpenElement(seq++, "option");
                builder.AddAttribute(seq++, "value", size.ToString());
                if (size == _pageSize)
                {
                    builder.AddAttribute(seq++, "selected", true);
                }
                builder.AddContent(seq++, size);
                builder.CloseElement();
            }

            builder.CloseElement(); // select

            // First Page
            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.ChevronDoubleLeft);
            builder.AddAttribute(seq++, "Class", "left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "title", "First");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToFirstPageAsync));
            builder.CloseComponent();

            // Previous Page
            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.ChevronLeft);
            builder.AddAttribute(seq++, "Class", "left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "title", "Previous");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToPreviousPageAsync));
            builder.CloseComponent();

            // Next Page
            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.ChevronRight);
            builder.AddAttribute(seq++, "Class", "left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "title", "Next");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToNextPageAsync));
            builder.CloseComponent();

            // Last Page
            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.ChevronDoubleRight);
            builder.AddAttribute(seq++, "Class", "left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "title", "Last");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToLastPageAsync));
            builder.CloseComponent();

            // Go To Page
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "left-margin-5x");
            builder.AddContent(seq++, "Go to Page: ");
            builder.CloseElement();

            builder.OpenElement(seq++, "input");
            builder.AddAttribute(seq++, "id", "current-page");
            builder.AddAttribute(seq++, "type", "number");
            builder.AddAttribute(seq++, "min", "1");
            builder.AddAttribute(seq++, "max", _totalPages.ToString());
            builder.AddAttribute(seq++, "value", _currentPage);
            builder.AddAttribute(seq++, "class", "form-control left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, CurrentPageChangedAsync));
            builder.CloseElement();

            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "left-margin-5x");
            builder.AddContent(seq++, $"of {_totalPages} {(_totalPages > 1 ? "Pages" : "Page")}");
            builder.CloseElement();

            //Go to specified page
            builder.OpenComponent<Icon>(seq++);
            builder.AddAttribute(seq++, "Name", IconName.CheckCircle);
            builder.AddAttribute(seq++, "Class", "left-margin-5x cursor-pointer");
            builder.AddAttribute(seq++, "title", "Go");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToSpecifiedPageAsync));
            builder.CloseComponent();

            builder.CloseElement(); // Pagination container
            builder.CloseElement(); // div container
            /*
             <style>
                .show-element{
                    display: initial;
                }

                .hide-element{
                    display: none;
                }
             </style>
             */

            // Render the style code
            builder.OpenElement(seq++, "style");
            builder.AddContent(seq++, @"
                .show-element{
                    display: initial;
                }

                .hide-element{
                    display: none;
                }

            ");

            builder.CloseElement(); // script
            /*
             <script>
                function scrollToBottom(divId) {
                    var div = document.getElementById(divId);
                    if (div) {
                        div.scrollTop = div.scrollHeight;
                    }
                }
            </script>
             */

            // Render the JavaScript code
            builder.OpenElement(seq++, "script");
            builder.AddContent(seq++, @"
                function scrollToBottom(divId) {
                    var div = document.getElementById(divId);
                    if (div) {
                        div.scrollTop = div.scrollHeight;
                    }
                }

                function showElementById(id) {
                    var element = document.getElementById(id);
                    alert(element);
                    if (element) {
                        element.style.display = 'block';
                    }
                }

                function hideElementById(id) {
                    var element = document.getElementById(id);
                    alert(element);
                    if (element) {
                        element.style.display = 'none';
                    }
                }

            ");

            builder.CloseElement(); // script



            // Add Row Modal
            //if (_showAddRowModal)
            //{
            //    builder.OpenComponent<AddRowModal>(seq++);
            //    builder.AddAttribute(seq++, "ShowAddRowModal", _showAddRowModal);
            //    builder.AddAttribute(seq++, "Data", Items);
            //    builder.AddAttribute(seq++, "NewRowData", _newRowData);
            //    builder.AddAttribute(seq++, "OnClose", EventCallback.Factory.Create(this, CloseAddRowModal));
            //    builder.AddAttribute(seq++, "OnSave", EventCallback.Factory.Create(this, AddRowAsync));
            //    builder.AddAttribute(seq++, "HiddenColumnNames", HiddenColumnNames);
            //    builder.CloseComponent();
            //}

            await Task.CompletedTask;
        }

        private async Task RenderAllowSelection(RenderTreeBuilder builder, int seq)
        {
            if (AllowCellRangeSelection)
            {
                // Cell selection controls
                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "row-display mb-2");

                // hidden input fields for the cell range selection
                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "type", "hidden");
                builder.AddAttribute(seq++, "id", $"{TableID}-start-row");
                builder.AddAttribute(seq++, "value", "");
                builder.CloseElement();

                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "type", "hidden");
                builder.AddAttribute(seq++, "id", $"{TableID}-end-row");
                builder.AddAttribute(seq++, "value", "");
                builder.CloseElement();

                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "type", "hidden");
                builder.AddAttribute(seq++, "id", $"{TableID}-start-col");
                builder.AddAttribute(seq++, "value", "");
                builder.CloseElement();

                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "type", "hidden");
                builder.AddAttribute(seq++, "id", $"{TableID}-end-col");
                builder.AddAttribute(seq++, "value", "");
                builder.CloseElement();
                 
                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "id", $"{TableID}-clear-selection");
                builder.AddAttribute(seq++, "Name", IconName.Recycle);
                builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer");
                builder.AddAttribute(seq++, "title", "Clear");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, ClearSelectionAsync));
                builder.CloseComponent();
                  
                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "id", $"{TableID}-start-cell");
                builder.AddAttribute(seq++, "value", _startCell);
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "placeholder", "Start Cell");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleStartCellClick));
                builder.CloseElement();
  
                builder.OpenElement(seq++, "input");
                builder.AddAttribute(seq++, "id", $"{TableID}-end-cell");
                builder.AddAttribute(seq++, "value", _endCell);
                builder.AddAttribute(seq++, "class", "form-control");
                builder.AddAttribute(seq++, "placeholder", "End Cell");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleEndCellClick));
                builder.CloseElement();
                 
                builder.CloseElement(); // modelVM
            }

            await Task.CompletedTask;
        }

        private async Task AddRowAsync()
        {
            if (Items != null && ModelVM != null)
            {
                _isEditing = true;
                _isAdding = true;

                // save before adding
                _editedRow = default!;
                _editedRowSaved = default!;

                TModelVM newItem = Activator.CreateInstance<TModelVM>();
                newItem = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).SetEditMode(newItem, _isEditing);
                _editedRow = newItem;
                _editedRowSaved = newItem;

                Items = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).AddItemToList(Items, newItem);
                
                // Notify parent component that Items list has changed
                await ItemsChanged.InvokeAsync(Items);

                await GetPageRowsAsync(Items);
                //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeDataTable", _nodeDataTable, serialize: true);
                StateHasChanged(); // Ensure UI updates after adding modelVM
            }

            await Task.CompletedTask;
        }

        private async void EditRow(TModelVM? modelVM)
        {
            if (Items != null && modelVM != null && ModelVM != null)
            {
                _isEditing = true;
                
                var item = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).SetEditMode(modelVM, _isEditing);
                _editedRow = item;
                _editedRowSaved = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).SaveModelVMToNewModelVM(item);

                StateHasChanged();
            }
        }

        private async Task DeleteRowAsync(TModelVM? modelVM)
        {
            if (modelVM != null && ModelVM != null)
            {
                Items = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).DeleteItemFromList(Items, modelVM);

                await GetPageRowsAsync(Items);

                StateHasChanged();
            }

            await Task.CompletedTask;
        }

        private async Task SaveRowAsync(TModelVM? modelVM)
        {
            if (modelVM != null && _editedRow != null && ModelVM != null)
            {
                _isEditing = false;
                var item = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).SetEditMode(modelVM, _isEditing);
                _editedRow = item;

                await GetPageRowsAsync(Items);
                // Notify parent component that Items list has changed
                await ItemsChanged.InvokeAsync(Items);

                _editedRow = default!;
                _editedRowSaved = default!;
                StateHasChanged(); // Refresh UI after canceling edit
            }

        }

        private async Task CancelEditAsync()
        {
            if (_editedRow != null && _editedRowSaved != null && ModelVM != null)
            {
                var item = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).SetEditMode(_editedRowSaved, false);
                Items = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).UpdateList(Items, item, _isAdding);

                await GetPageRowsAsync(Items);

                // Notify parent component that Items list has changed
                await ItemsChanged.InvokeAsync(Items);

                _isEditing = false;
                _isAdding = false;
                _editedRow = default!;
                _editedRowSaved = default!;

                StateHasChanged(); // Refresh UI after canceling edit
            }

        }

        //        public async Task<RenderTreeBuilder> RenderTableColumn(int seq, int rowIndex, RenderTreeBuilder builder, TableColumn column)
        //        {
        //            var colDataSource = column.GetDataSource();
        //            var colDataSourceRows = colDataSource.Rows;
        //            var rows = _nodePagedRows?.ToList();

        //            if (rows != null)
        //            {
        //                var modelVM = rows[rowIndex];
        //                var isEditing = _isEditing == true && _editedRow != null && modelVM == _editedRow;
        //                var editValues = _nodeEditValues != null ? _nodeEditValues[column.FieldName]?.ToString() : null;

        //                if (isEditing)
        //                {
        //                    if (column.Type == "TextBox")
        //                    {
        //                        builder.OpenElement(seq, "input");
        //                        builder.AddAttribute(seq++, "type", "text");
        //                        builder.AddAttribute(seq++, "class", "form-control");
        //                        builder.AddAttribute(seq++, "value", editValues);
        //                        builder.AddAttribute(seq++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
        //                        {
        //                            if (TableNodeContext != null && _nodeEditValues != null)
        //                            {
        //                                _nodeEditValues[column.FieldName] = e?.Value?.ToString() ?? default!;
        //                            }
        //                        }));

        //                        builder.CloseElement();
        //                    }
        //                    else if (column.Type == "DropdownList")
        //                    {
        //                        builder.OpenElement(seq, "select");
        //                        builder.AddAttribute(seq++, "class", "form-control");
        //                        builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
        //                        {
        //                            if (TableNodeContext != null && _nodeEditValues != null)
        //                            {
        //                                _nodeEditValues[column.FieldName] = e?.Value?.ToString() ?? default!;
        //                            }
        //                        }));

        //                        for (int i = 0; i < colDataSourceRows.Count; i++)
        //                        {
        //                            DataRow iRow = colDataSourceRows[i];
        //                            var displayName = iRow[column.DisplayFieldName].ToString();
        //                            var displayValue = iRow[column.DisplayFieldValue].ToString();

        //                            builder.OpenElement(seq++, "option");
        //                            builder.AddAttribute(seq++, "value", displayValue);
        //                            if (displayValue == editValues)
        //                            {
        //                                builder.AddAttribute(seq++, "selected", "selected");
        //                            }
        //                            builder.AddContent(seq++, displayName);
        //                            builder.CloseElement();
        //                        }

        //                        builder.CloseElement();
        //                    }
        //                }
        //                else
        //                {
        //                    var columnStringValue = modelVM[column.FieldName]?.ToString();
        //                    if (column.Type == "DropdownList")
        //                    {
        //                        for (int i = 0; i < colDataSourceRows.Count; i++)
        //                        {
        //                            DataRow iRow = colDataSourceRows[i];
        //                            var displayValue = iRow[column.DisplayFieldValue].ToString();
        //                            if (displayValue == columnStringValue)
        //                            {
        //                                columnStringValue = iRow[column.DisplayFieldName].ToString();
        //                                break;
        //                            }
        //                        }
        //                    }

        //                    builder.AddContent(seq, columnStringValue);
        //                }
        //            }

        //            await Task.CompletedTask;

        //            return builder;
        //        }
        private async Task GetPageRowsAsync(IEnumerable<TModelVM> items)
        {
            _filteredRows = items;
            _filteredItems = _filteredRows.Count();
            _totalItems = Items.Count();
            if (_pageSize == 0 || _isAdding)
            {
                _pageSize = _totalItems;
            }

            _currentPage = _currentPage == 0 ? 1 : _currentPage;
            _pagedRows = _filteredRows.Skip((_currentPage - 1) * _pageSize).Take(_pageSize);
            _totalPages = (int)Math.Ceiling((double)(_filteredRows?.Count() ?? 0) / _pageSize);

            await Task.CompletedTask;
        }
        private async Task HandleFilterDataTableAsync(IEnumerable<TModelVM> filteredRows)
        {
            await GetPageRowsAsync(filteredRows);
            StateHasChanged(); 
            await Task.CompletedTask;
        }


        private async Task PageSizeChangedAsync(ChangeEventArgs e)
        {
            _pageSize = Convert.ToInt32(e.Value);
            _currentPage = 1; // Reset to first page when changing page size

            await GetPageRowsAsync(_filteredRows);
            //if (TableNodeContext != null)
            //{
            //    TableNodeContext.PageSize = _nodePageSize;
            //    TableNodeContext.CurrentPage = _nodeCurrentPage;
            //    TableNodeContext.UpdateDisplayFromPageSize();

            //    await PopulateNodeVariablesAsync();
            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodePageSize", _nodePageSize, serialize: true);
            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
            //}

            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task GoToFirstPageAsync()
        {
            _currentPage = 1;
            await GetPageRowsAsync(_filteredRows);
            //if (TableNodeContext != null)
            //{
            //    TableNodeContext.CurrentPage = _nodeCurrentPage;
            //    TableNodeContext.UpdateDisplayFromPageSize();

            //    await PopulateNodeVariablesAsync();
            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
            //}

            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task GoToPreviousPageAsync()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                //    if (TableNodeContext != null)
                //    {
                //        TableNodeContext.CurrentPage = _nodeCurrentPage;
                //        TableNodeContext.UpdateDisplayFromPageSize();

                //        await PopulateNodeVariablesAsync();
                //        await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
                //    }

                await GetPageRowsAsync(_filteredRows);
                StateHasChanged();
            }
            
            await Task.CompletedTask;
        }

        private async Task GoToNextPageAsync()
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;

                //if (TableNodeContext != null)
                //{
                //    TableNodeContext.CurrentPage = _nodeCurrentPage;
                //    TableNodeContext.UpdateDisplayFromPageSize();

                //    await PopulateNodeVariablesAsync();
                //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
                //}
                
                await GetPageRowsAsync(_filteredRows);

                StateHasChanged();

            }

            await Task.CompletedTask;
        }

        private async Task GoToLastPageAsync()
        {
            _currentPage = _totalPages;

            //if (TableNodeContext != null)
            //{
            //    TableNodeContext.CurrentPage = _nodeCurrentPage;
            //    TableNodeContext.UpdateDisplayFromPageSize();

            //    await PopulateNodeVariablesAsync();
            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
            //}
            await GetPageRowsAsync(_filteredRows);
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task CurrentPageChangedAsync(ChangeEventArgs e)
        {
            _currentPage = Convert.ToInt32(e.Value);
            if (_currentPage > _totalPages)
            {
                throw new Exception($"Page value can be up to total pages only {_totalPages}");
            }
            await GetPageRowsAsync(_filteredRows);
            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_currentPage", _currentPage, serialize: true);
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task GoToSpecifiedPageAsync()
        {
            if (_currentPage >= 1 && _currentPage <= _totalPages)
            {
                //if (TableNodeContext != null)
                //{
                //    TableNodeContext.CurrentPage = _nodeCurrentPage;
                //    TableNodeContext.UpdateDisplayFromPageSize();

                //    await PopulateNodeVariablesAsync();
                //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeCurrentPage", _nodeCurrentPage, serialize: true);
                //}
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
            //$"{TableID}-clear-selection"
            var startRow = await JSRuntime.InvokeAsync<string>("getValue", $"{TableID}-start-row");
            var endRow = await JSRuntime.InvokeAsync<string>("getValue", $"{TableID}-end-row");
            var startCol = await JSRuntime.InvokeAsync<string>("getValue", $"{TableID}-start-col");
            var endCol = await JSRuntime.InvokeAsync<string>("getValue", $"{TableID}-end-col");

            await JSRuntime.InvokeVoidAsync("toggleCellBorders", startRow, endRow, startCol, endCol, TableID, false);

            _startCell = string.Empty;
            _endCell = string.Empty;

            await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-cell", "");
            await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-cell", "");
            await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-row", "");
            await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-row", "");
            await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-col", "");
            await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-col", "");

            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
            //    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);

            await Task.CompletedTask;
        }

        private async Task HandleStartCellClick(MouseEventArgs e)
        {
            _startCell = string.Empty;

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleEndCellClick(MouseEventArgs e)
        {
            _endCell = string.Empty;

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleCellClickAsync(int rowIndex, int columnIndex)
        {
            string cellIdentifier = $"R{rowIndex}C{columnIndex}";

            if (string.IsNullOrEmpty(_startCell) || _isFirstCellClicked)
            {
                _startCell = cellIdentifier;
                _isFirstCellClicked = false;

            }
            else
            {
                _endCell = cellIdentifier;
                _isFirstCellClicked = true;

            }

                //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
                //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);
                //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeIsFirstCellClicked", _nodeIsFirstCellClicked, serialize: true);

            StateHasChanged(); // Refresh UI to reflect the changes in cell selection

            await Task.CompletedTask;
        }

        //        public async Task HandleSelectedDataComb(DataRow[] selectedData)
        //        {
        //            _nodeSelectedData = selectedData;

        //            if (TableNodeContext != null)
        //            {
        //                TableNodeContext.SelectedData = _nodeSelectedData;
        //                TableNodeContext.StartCell = _nodeStartCell;
        //                TableNodeContext.EndCell = _nodeEndCell;

        //                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);
        //                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
        //                await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);

        //            }

        //            StateHasChanged();
        //            await Task.CompletedTask;
        //        }

        //        public async Task<DataRow[]?> ShowSetTargetTableModalAsync()
        //        {
        //            if (!string.IsNullOrEmpty(_nodeStartCell) && !string.IsNullOrEmpty(_nodeEndCell))
        //            {
        //                // Extracting start modelVM and column from startCell
        //                int startRow = int.Parse(_nodeStartCell.Substring(1, _nodeStartCell.IndexOf('C') - 1));
        //                int startCol = int.Parse(_nodeStartCell.Substring(_nodeStartCell.IndexOf('C') + 1));

        //                // Extracting end modelVM and column from endCell
        //                int endRow = int.Parse(_nodeEndCell.Substring(1, _nodeEndCell.IndexOf('C') - 1));
        //                int endCol = int.Parse(_nodeEndCell.Substring(_nodeEndCell.IndexOf('C') + 1));

        //                _nodeSelectedData = GetDataInRange(startRow, startCol, endRow, endCol);

        //                if (TableNodeContext != null)
        //                {
        //                    TableNodeContext.SelectedData = _nodeSelectedData;

        //                    await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);
        //                }

        //                await Task.CompletedTask;
        //            }

        //            StateHasChanged();

        //            return _nodeSelectedData;
        //        }

        //        private DataRow[] GetDataInRange(int startRow, int startCol, int endRow, int endCol)
        //        {
        //            List<DataRow> dataInRange = new List<DataRow>();

        //            if (_nodeDataTable != null)
        //            {
        //                // Create a new DataTable with the selected columns
        //                DataTable filteredDataTable = new DataTable();
        //                for (int i = startCol; i <= endCol; i++)
        //                {
        //                    DataColumn column = _nodeDataTable.Columns[i];
        //                    filteredDataTable.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
        //                }

        //                for (int i = startRow; i <= endRow; i++)
        //                {
        //                    DataRow newRow = filteredDataTable.NewRow();

        //                    for (int j = startCol; j <= endCol; j++)
        //                    {
        //                        newRow[j - startCol] = _nodeDataTable.Rows[i][j];
        //                    }

        //                    dataInRange.Add(newRow);
        //                }

        //            }

        //            return dataInRange.ToArray();
        //        }

        //        protected override async Task OnAfterRenderAsync(bool firstRender)
        //        {
        //            if (firstRender)
        //            {
        //                // Ensure RenderTable() is rendered only after initial render
        //                _renderTable = true;
        //                StateHasChanged();
        //            }

        //            var nodeValues = new Dictionary<string, object?>
        //        {
        //            { $"{Title}_nodeDataTable", _nodeDataTable },
        //            { $"{Title}_nodeFilteredItems", _nodeFilteredItems },
        //            { $"{Title}_nodeTotalItems", _nodeTotalItems },
        //            { $"{Title}_nodeTotalPages", _nodeTotalPages },
        //            { $"{Title}_nodeCurrentPage", _nodeCurrentPage },
        //            { $"{Title}_nodeStartCell", _nodeStartCell },
        //            { $"{Title}_nodeEndCell", _nodeEndCell },
        //            { $"{Title}_nodeShowAddRowModal", _nodeShowAddRowModal },
        //            { $"{Title}_nodeNewRowData", _nodeNewRowData },
        //            { $"{Title}_nodeColumns", _nodeColumns },
        //            { $"{Title}_nodePagedRows", _nodePagedRows },
        //            { $"{Title}_nodePageSize", _nodePageSize },
        //            { $"{Title}_isEditing", _isEditing },
        //            { $"{Title}_editedRow", _editedRow },
        //            { $"{Title}_nodeEditValues", _nodeEditValues },
        //            { $"{Title}_nodeIsFirstCellClicked", _nodeIsFirstCellClicked },
        //            { $"{Title}_nodeSelectedData", _nodeSelectedData }
        //        };

        //            // Update the SessionItems with new values
        //            foreach (var item in _sessionItems)
        //            {
        //                if (nodeValues.TryGetValue(item.Key, out var newValue))
        //                {
        //                    item.Value = newValue;
        //                }
        //                else
        //                {
        //                    throw new Exception($"Unrecognized session item key: {item.Key}");
        //                }
        //            }

        //            List<SessionItem> sessionItems = _sessionItems.ToList();

        //            await _sessionManager.SaveToSessionTableAsync<List<SessionItem>>(sessionItems);
        //            await Task.CompletedTask;
        //        }

        //        protected override void BuildRenderTree(RenderTreeBuilder builder)
        //        {
        //            var seq = 0;

        //            // DTSearchBox component
        //            builder.OpenComponent<DTSearchBox>(seq++);
        //            builder.AddAttribute(seq++, "DataTable", _nodeDataTable);
        //            builder.AddAttribute(seq++, "OnFilterDataTable", EventCallback.Factory.Create<IEnumerable<DataRow>>(this, HandleFilterDataTableAsync));
        //            builder.CloseComponent();

        //            // Conditional rendering
        //            if (!_renderTable)
        //            {
        //                builder.OpenComponent<CascadingValue<TableGrid>>(seq++);
        //                builder.AddAttribute(seq++, "Value", TableGridContext);
        //                builder.AddAttribute(seq++, "ChildContent", ChildContent);
        //                builder.CloseComponent();
        //            }
        //            else
        //            {
        //                builder.AddContent(seq++, RenderTable);

        //                if (_showAddButton)
        //                {
        //                    builder.OpenComponent<Icon>(seq++);
        //                    builder.AddAttribute(seq++, "Name", "IconName.PlusCircleFill");
        //                    builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer");
        //                    builder.AddAttribute(seq++, "title", "Add");
        //                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, ShowAddRowModalAsync));
        //                    builder.CloseComponent();
        //                }

        //                // Pagination controls
        //                builder.OpenElement(seq++, "div");
        //                builder.OpenElement(seq++, "div");
        //                builder.AddAttribute(seq++, "class", "pagination-container");

        //                builder.OpenElement(seq++, "label");
        //                builder.AddContent(seq++, $"Total Items: {_nodeFilteredItems}/{_nodeTotalItems}");
        //                builder.CloseElement();

        //                builder.OpenElement(seq++, "label");
        //                builder.AddAttribute(seq++, "for", "pageSize");
        //                builder.AddContent(seq++, "Page Size:");
        //                builder.CloseElement();

        //                builder.OpenElement(seq++, "select");
        //                builder.AddAttribute(seq++, "id", "pageSize");
        //                builder.AddAttribute(seq++, "class", "cursor-pointer");
        //                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, PageSizeChangedAsync));

        //                builder.OpenElement(seq++, "option");
        //                builder.AddAttribute(seq++, "value", _nodeTotalItems.ToString());
        //                builder.AddAttribute(seq++, "selected", true);
        //                builder.AddContent(seq++, _nodeTotalItems);
        //                builder.CloseElement();

        //                foreach (var size in new[] { 5, 10, 20, 50, 100 })
        //                {
        //                    builder.OpenElement(seq++, "option");
        //                    builder.AddAttribute(seq++, "value", size.ToString());
        //                    builder.AddContent(seq++, size);
        //                    builder.CloseElement();
        //                }

        //                builder.CloseElement();

        //                // First Page
        //                builder.OpenComponent<Icon>(seq++);
        //                builder.AddAttribute(seq++, "Name", "IconName.ChevronDoubleLeft");
        //                builder.AddAttribute(seq++, "Class", "pagination-icon cursor-pointer");
        //                builder.AddAttribute(seq++, "title", "First");
        //                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToFirstPageAsync));
        //                builder.CloseComponent();

        //                // Previous Page
        //                builder.OpenComponent<Icon>(seq++);
        //                builder.AddAttribute(seq++, "Name", "IconName.ChevronLeft");
        //                builder.AddAttribute(seq++, "Class", "pagination-icon cursor-pointer");
        //                builder.AddAttribute(seq++, "title", "Previous");
        //                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToPreviousPageAsync));
        //                builder.CloseComponent();

        //                // Next Page
        //                builder.OpenComponent<Icon>(seq++);
        //                builder.AddAttribute(seq++, "Name", "IconName.ChevronRight");
        //                builder.AddAttribute(seq++, "Class", "pagination-icon cursor-pointer");
        //                builder.AddAttribute(seq++, "title", "Next");
        //                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToNextPageAsync));
        //                builder.CloseComponent();

        //                // Last Page
        //                builder.OpenComponent<Icon>(seq++);
        //                builder.AddAttribute(seq++, "Name", "IconName.ChevronDoubleRight");
        //                builder.AddAttribute(seq++, "Class", "pagination-icon cursor-pointer");
        //                builder.AddAttribute(seq++, "title", "Last");
        //                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToLastPageAsync));
        //                builder.CloseComponent();

        //                // Go To Page
        //                builder.OpenElement(seq++, "span");
        //                builder.AddContent(seq++, "Go to Page: ");
        //                builder.CloseElement();

        //                builder.OpenElement(seq++, "input");
        //                builder.AddAttribute(seq++, "type", "number");
        //                builder.AddAttribute(seq++, "min", "1");
        //                builder.AddAttribute(seq++, "max", _nodeTotalPages.ToString());
        //                builder.AddAttribute(seq++, "value", _nodeCurrentPage);
        //                builder.AddAttribute(seq++, "class", "cursor-pointer");
        //                builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => _nodeCurrentPage = int.Parse(e.Value.ToString() ?? "1")));
        //                builder.CloseElement();

        //                builder.OpenElement(seq++, "label");
        //                builder.AddContent(seq++, $"of {_nodeTotalPages} {(_nodeTotalPages > 1 ? "Pages" : "Page")}");
        //                builder.CloseElement();

        //                builder.OpenComponent<Icon>(seq++);
        //                builder.AddAttribute(seq++, "Name", "IconName.CheckCircle");
        //                builder.AddAttribute(seq++, "Class", "pagination-go-icon cursor-pointer");
        //                builder.AddAttribute(seq++, "title", "Go");
        //                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToSpecifiedPageAsync));
        //                builder.CloseComponent();

        //                builder.CloseElement(); // Pagination container

        //                if (AllowCellSelection)
        //                {
        //                    // Cell selection controls
        //                    builder.OpenElement(seq++, "div");
        //                    builder.AddAttribute(seq++, "class", "modelVM mb-2");

        //                    builder.OpenElement(seq++, "div");
        //                    builder.AddAttribute(seq++, "class", "col-auto");

        //                    builder.OpenComponent<Icon>(seq++);
        //                    builder.AddAttribute(seq++, "Name", "IconName.Recycle");
        //                    builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer");
        //                    builder.AddAttribute(seq++, "title", "Clear");
        //                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, ClearSelectionAsync));
        //                    builder.CloseComponent();

        //                    builder.CloseElement(); // col-auto

        //                    builder.OpenElement(seq++, "div");
        //                    builder.AddAttribute(seq++, "class", "col");

        //                    builder.OpenElement(seq++, "input");
        //                    builder.AddAttribute(seq++, "value", _nodeStartCell);
        //                    builder.AddAttribute(seq++, "class", "form-control");
        //                    builder.AddAttribute(seq++, "placeholder", "Start Cell");
        //                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, HandleStartCellClick));
        //                    builder.CloseElement();

        //                    builder.CloseElement(); // col

        //                    builder.OpenElement(seq++, "div");
        //                    builder.AddAttribute(seq++, "class", "col");

        //                    builder.OpenElement(seq++, "input");
        //                    builder.AddAttribute(seq++, "value", _nodeEndCell);
        //                    builder.AddAttribute(seq++, "class", "form-control");
        //                    builder.AddAttribute(seq++, "placeholder", "End Cell");
        //                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, HandleEndCellClick));
        //                    builder.CloseElement();

        //                    builder.CloseElement(); // col

        //                    builder.CloseElement(); // modelVM
        //                }

        //                // Add Row Modal
        //                if (_nodeShowAddRowModal)
        //                {
        //                    builder.OpenComponent<AddRowModal>(seq++);
        //                    builder.AddAttribute(seq++, "ShowAddRowModal", _nodeShowAddRowModal);
        //                    builder.AddAttribute(seq++, "DataTable", _nodeDataTable);
        //                    builder.AddAttribute(seq++, "NewRowData", _nodeNewRowData);
        //                    builder.AddAttribute(seq++, "OnClose", EventCallback.Factory.Create(this, CloseAddRowModal));
        //                    builder.AddAttribute(seq++, "OnSave", EventCallback.Factory.Create(this, AddRowAsync));
        //                    builder.AddAttribute(seq++, "HiddenColumnNames", HiddenColumnNames);
        //                    builder.CloseComponent();
        //                }
        //            }
        //        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                
            }

            if (_isAdding)
            {
                await JSRuntime.InvokeVoidAsync("scrollToBottom", $"{Title}-div");
            }
        }
    }
}
