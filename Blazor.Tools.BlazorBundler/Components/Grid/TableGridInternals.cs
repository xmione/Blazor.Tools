﻿using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Data;
using Microsoft.JSInterop;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGridInternals<TModel, TIModel, TModelVM> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "Sample List";
        [Parameter] public string TableID { get; set; } = "table-id";
        [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();
        [Parameter] public TModelVM ModelVM { get; set; } = default!;
        [Parameter] public TIModel IModel { get; set; } = default!;
        [Parameter] public IEnumerable<TModelVM> Items { get; set; } = Enumerable.Empty<TModelVM>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<TModelVM>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;
        [Parameter] public bool AllowAdding { get; set; } = true;
        [Parameter] public List<string>? HiddenColumnNames { get; set; } = default!;
        [Parameter] public RenderFragment? StartContent { get; set; }
        [Parameter] public RenderFragment? TableHeader { get; set; }
        [Parameter] public RenderFragment<TModelVM> RowTemplate { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        
        private SessionManager _sessionManager = SessionManager.Instance;
        public IEnumerable<TModelVM> _items { get; set; } = Enumerable.Empty<TModelVM>();
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
        private bool _isComponentInitialized;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _isComponentInitialized = true;
        }

        protected override async Task OnParametersSetAsync()
        {
            // Important note to self:
            // It is best not to change the value of the [Parameter] variables within the component
            // because it will trigger this method and will affect the data displayed.
            // It is better to use private variables to play with within the component.

            _items = Items;
            await GetPageRowsAsync(_items);
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
            builder.AddAttribute(seq++, "Data", _items);
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
            builder.AddAttribute(seq++, "id", $"{TableID}-div");
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
        
        private async Task<RenderTreeBuilder> RenderEditAndDeleteButtons(int seq, RenderTreeBuilder builder, TModelVM? row)
        {
            builder.OpenElement(seq++, "td");
            builder.AddAttribute(seq++, "class", "icons-td");

            if (!_isEditing || !Equals(row, _editedRow))
            {
                if (_isAdding)
                {
                    // Edit icon
                    // <Icon Name="IconName.PencilFill" @onclick="() => EditRowAsync(modelVM)" title="Edit" class="text-primary icon-button" />

                    builder.OpenComponent<Icon>(seq++);
                    builder.AddAttribute(seq++, "Name", IconName.PencilFill); // Blazor.Bootstrap icon class
                    builder.AddAttribute(seq++, "Class", "text-primary icon-button cursor-pointer hide-element");
                    builder.AddAttribute(seq++, "title", "Edit");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => EditRowAsync(row)));
                    builder.CloseComponent();

                }
                else 
                {
                    // Edit icon
                    // <Icon Name="IconName.PencilFill" @onclick="() => EditRowAsync(modelVM)" title="Edit" class="text-primary icon-button" />

                    builder.OpenComponent<Icon>(seq++);
                    builder.AddAttribute(seq++, "Name", IconName.PencilFill); // Blazor.Bootstrap icon class
                    builder.AddAttribute(seq++, "Class", "text-primary icon-button cursor-pointer show-element");
                    builder.AddAttribute(seq++, "title", "Edit");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => EditRowAsync(row)));
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
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, async () => await CancelEditAsync()));

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

            // Add Row Modal
            //if (_showAddRowModal)
            //{
            //    builder.OpenComponent<AddRowModal>(seq++);
            //    builder.AddAttribute(seq++, "ShowAddRowModal", _showAddRowModal);
            //    builder.AddAttribute(seq++, "Data", _items);
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
                builder.AddAttribute(seq++, "Name", IconName.ArrowClockwise);
                builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer icon-large");
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
            if (_items != null && ModelVM != null)
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

                _items = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).AddItemToList(_items, newItem);
                 
                await GetPageRowsAsync(_items);
                //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeDataTable", _nodeDataTable, serialize: true);
                StateHasChanged(); // Ensure UI updates after adding modelVM
            }

            await Task.CompletedTask;
        }

        private async void EditRowAsync(TModelVM? modelVM)
        {
            if (_items != null && modelVM != null && ModelVM != null)
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
                _items = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).DeleteItemFromList(_items, modelVM);

                await GetPageRowsAsync(_items);

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

                await GetPageRowsAsync(_items);
                 
                _editedRow = default!;
                _editedRowSaved = default!;
                StateHasChanged(); // Refresh UI after canceling edit
            }

        }

        private async Task CancelEditAsync()
        {
            if (!_isComponentInitialized)
            {
                Console.WriteLine("Component is not initialized yet.");
                return;
            }

            if (_editedRow != null && _editedRowSaved != null && ModelVM != null)
            {
                var item = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).SetEditMode(_editedRowSaved, false);
                _items = await ((IViewModel<TModel, TIModel, TModelVM>)ModelVM).UpdateList(_items, item, _isAdding);

                await GetPageRowsAsync(_items);
                 
                _isEditing = false;
                _isAdding = false;
                _editedRow = default!;
                _editedRowSaved = default!;

                await InvokeAsync(() => StateHasChanged());
            }

        }

        private async Task GetPageRowsAsync(IEnumerable<TModelVM> items)
        {
            _filteredRows = items;
            _filteredItems = _filteredRows.Count();
            _totalItems = _items.Count();
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
            var startRow = 1;
            var endRow = _items.Count();
            var startCol = 1;
            var endCol = ColumnDefinitions.Count();

            await JSRuntime.InvokeVoidAsync("toggleCellBorders", startRow, endRow, startCol, endCol, _totalItems, ColumnDefinitions.Count, TableID, false);

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

            await JSRuntime.InvokeVoidAsync("StartCellClicked", true, TableID);
            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleEndCellClick(MouseEventArgs e)
        {
            _endCell = string.Empty;

            await JSRuntime.InvokeVoidAsync("StartCellClicked", false, TableID);
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("StartCellClicked", true, TableID);
            }

            if (_isAdding)
            {
                await JSRuntime.InvokeVoidAsync("scrollToBottom", $"{TableID}-div");
            }
        }
    }
}
