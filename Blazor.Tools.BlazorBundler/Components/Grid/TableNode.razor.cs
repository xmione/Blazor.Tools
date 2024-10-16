﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    //public partial class TableNode : ComponentBase
    //{
    //    [Parameter] public RenderFragment ChildContent { get; set; } = default!;
    //    [Parameter] public string DataSource { get; set; } = default!;
    //    [Parameter] public bool AllowCellSelection { get; set; } = false;
    //    [CascadingParameter] private TableGrid TableGridContext { get; set; } = default!;

    //    public List<TableColumn>? Columns;
    //    public TableNode? TableNodeContext = default!;
    //    public DataTable DataTable = default!;
    //    public int TotalPages = 0;
    //    public int TotalItems = 0;
    //    public int FilteredItems = 0;
    //    public int PageSize = 0;
    //    public int CurrentPage = 1;
    //    public IEnumerable<DataRow>? FilteredRows;
    //    public IEnumerable<DataRow>? PagedRows;
    //    public string? SelectedTableName;
    //    public string SearchQuery = string.Empty;
    //    public DataRow? EditedRow = default!;
    //    public Dictionary<string, object> EditValues = null!;
    //    public bool IsEditing = false;
    //    public bool ShowAddRowModal = false;
    //    public DataRow NewRowData = default!;
    //    // New state variables for cell selection
    //    public string StartCell = string.Empty;
    //    public string EndCell = string.Empty;
    //    public bool IsFirstCellClicked = true;
    //    public DataRow[] SelectedData = Array.Empty<DataRow>();
    //    public bool _shouldRender = true;
    //    private bool _hasDataChanged = false;
    //    protected override void OnParametersSet()
    //    {
    //        GetDefaultValues();
    //        GetExistingNodeValues();
    //        UpdateDisplayFromPageSize();

    //        if (TableNodeContext != this)
    //        {
    //            TableNodeContext = this;
    //            TableGridContext?.SetNode(TableNodeContext);
    //            _hasDataChanged = true;
    //        }

    //        Console.WriteLine("TableNode OnParametersSet");
    //    }

    //    private void GetDefaultValues()
    //    {
    //        TotalItems = DataTable?.Rows?.Count ?? 0;
    //        FilteredItems = TotalItems;
    //        PageSize = TotalItems;
    //    }

    //    private void GetExistingNodeValues()
    //    {
    //        if (TableGridContext != null)
    //        {
    //            DataTable = TableGridContext?.GetDataSource(DataSource) ?? default!;
    //            TableNodeContext = TableGridContext?.TableNodeContext;
    //            UpdateFromTableNodeContext();
    //        }
    //    }

    //    protected override bool ShouldRender()
    //    {
    //        return _hasDataChanged || _shouldRender;
    //    }

    //    public void UpdateDisplayFromPageSize()
    //    {
    //        FilteredRows = DataTable != null ? ApplyFilter() : FilteredRows;
    //        FilteredItems = FilteredRows?.Count() ?? FilteredItems;
    //        TotalItems = TotalItems == 0 ? FilteredItems : TotalItems;
    //        PageSize = PageSize == 0 ? FilteredItems : PageSize;
    //        PagedRows = FilteredRows?.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
    //        TotalPages = (int)Math.Ceiling((double)(FilteredRows?.Count() ?? 0) / PageSize);
    //        SelectedTableName = DataTable?.TableName;
    //        _shouldRender = true;  // Ensure re-render if necessary
    //        _hasDataChanged = true; // Mark that data has changed
    //    }

    //    public void UpdateFromTableNodeContext()
    //    {
    //        if (TableNodeContext != null)
    //        {
    //            DataTable = TableNodeContext.DataTable;
    //            FilteredItems = TableNodeContext.FilteredItems;
    //            TotalItems = TableNodeContext.TotalItems;
    //            TotalPages = TableNodeContext.TotalPages;
    //            CurrentPage = TableNodeContext.CurrentPage;
    //            StartCell = TableNodeContext.StartCell;
    //            EndCell = TableNodeContext.EndCell;
    //            ShowAddRowModal = TableNodeContext.ShowAddRowModal;
    //            NewRowData = TableNodeContext.NewRowData;
    //            Columns = TableNodeContext.Columns;
    //            PagedRows = TableNodeContext.PagedRows;
    //            PageSize = TableNodeContext.PageSize;
    //            IsEditing = TableNodeContext.IsEditing;
    //            EditedRow = TableNodeContext.EditedRow;
    //            EditValues = TableNodeContext.EditValues;
    //            IsFirstCellClicked = TableNodeContext.IsFirstCellClicked;
    //            SelectedData = TableNodeContext.SelectedData;
    //        }
    //    }

    //    public void FilterData(IEnumerable<DataRow> filteredRows)
    //    {
    //        FilteredRows = filteredRows;
    //        FilteredItems = FilteredRows.Count();
    //        TotalItems = TotalItems == 0 ? FilteredItems : TotalItems;
    //        PagedRows = FilteredRows?.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
    //        TotalPages = (int)Math.Ceiling((double)(FilteredRows?.Count() ?? 0) / PageSize);
    //        SelectedTableName = DataTable?.TableName;
    //    }
    //    private IEnumerable<DataRow> ApplyFilter()
    //    {
    //        IEnumerable<DataRow> rows = default!;
    //        if (string.IsNullOrWhiteSpace(SearchQuery))
    //        {
    //            rows = DataTable?.AsEnumerable() ?? rows;
    //            return rows;
    //        }

    //        rows = DataTable?
    //        .AsEnumerable()?
    //        .Where(row => DataTable
    //            .Columns
    //            .Cast<DataColumn>()
    //            .Any(column => row[column]?.ToString()?.IndexOf(SearchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
    //        ) ?? rows;

    //        return rows;
    //    }

    //    public void AddColumn(TableColumn column)
    //    {
    //        if (Columns == null)
    //        {
    //            Columns = new List<TableColumn>();
    //        }

    //        Columns.Add(column);
    //        TableNodeContext = this;
    //        TableGridContext?.SetNode(TableNodeContext);
    //    }

    //    public RenderFragment RenderTableColumn(TableColumn column, int rowNumber)
    //    {
    //        var tableColumn = new TableColumn();
    //        RenderFragment fragment = builder =>
    //        {
    //            builder.OpenComponent(0, typeof(TableColumn));
    //            builder.AddAttribute(1, nameof(tableColumn.DataSourceName), column.DataSourceName);
    //            builder.AddAttribute(2, nameof(tableColumn.FieldName), column.FieldName);
    //            builder.AddAttribute(3, nameof(tableColumn.Type), column.Type);
    //            builder.AddAttribute(4, nameof(tableColumn.DisplayFieldName), column.DisplayFieldName);
    //            builder.AddAttribute(5, nameof(tableColumn.DisplayFieldValue), column.DisplayFieldValue);
    //            builder.AddAttribute(6, nameof(tableColumn.RowNumber), rowNumber);
    //            builder.CloseComponent();
    //        };

    //        return fragment;
    //    }

    //    protected override void BuildRenderTree(RenderTreeBuilder builder)
    //    {
    //        var sequence = 0;

    //        // Open the first CascadingValue component
    //        builder.OpenComponent<CascadingValue<TableGrid>>(sequence++);
    //        builder.AddAttribute(sequence++, "Value", TableGridContext);

    //        // Open the second CascadingValue component inside the first
    //        builder.OpenComponent<CascadingValue<TableNode>>(sequence++);
    //        builder.AddAttribute(sequence++, "Value", TableNodeContext);

    //        // Add ChildContent
    //        builder.AddAttribute(sequence++, "ChildContent", ChildContent);

    //        // Close the second CascadingValue component
    //        builder.CloseComponent();

    //        // Close the first CascadingValue component
    //        builder.CloseComponent();
    //    }

    //}
}

