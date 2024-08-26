﻿using Microsoft.AspNetCore.Components;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    //public partial class TableColumn : ComponentBase
    //{
    //    [CascadingParameter] private TableGrid TableGridContext { get; set; } = default!;
    //    [CascadingParameter] private TableNode TableNodeContext { get; set; } = default!;
    //    [Parameter] public string DataSourceName { get; set; } = default!;
    //    [Parameter] public string FieldName { get; set; } = default!;
    //    [Parameter] public string Type { get; set; } = default!;
    //    [Parameter] public string DisplayFieldName { get; set; } = default!;
    //    [Parameter] public string DisplayFieldValue { get; set; } = default!;
    //    [Parameter] public string HeaderName { get; set; } = default!;
    //    [Parameter] public int RowNumber { get; set; } = default!;
    //    [Parameter] public bool Visible { get; set; } = true;


    //    private DataTable _dataSource = default!;
    //    public TableColumn TableColumnContext = default!;
    //    public bool _shouldRender = true;
    //    private bool _hasDataChanged = false;

    //    protected override void OnParametersSet()
    //    {
    //        _dataSource = GetDataSource();

    //        if (_dataSource != null)
    //        {
    //            if (Type == "TextBox")
    //            {
    //                var row = _dataSource.Rows[RowNumber];
    //                DisplayFieldValue = GetFieldValue(row);
    //            }
    //        }

    //        if (HeaderName == null)
    //        {
    //            HeaderName = FieldName;
    //        }

    //        if (DisplayFieldName == null)
    //        {
    //            DisplayFieldName = FieldName;
    //        }

    //        GetExistingNodeValues();
    //        // Search from TableNodeContext.Columns and add if it does not exist.
    //        var tableColumnIsNotFound = TableNodeContext?.Columns?.FirstOrDefault(ctx => ctx.Equals(this)) == null;
    //        if (tableColumnIsNotFound)
    //        {
    //            TableColumnContext = this;
    //            TableNodeContext?.AddColumn(this);
    //            _hasDataChanged = true;
    //        }

    //        Console.WriteLine("TableColumn OnParametersSetAsync");

    //    }

    //    private void GetExistingNodeValues()
    //    {
    //        if (TableGridContext != null)
    //        {
    //            TableNodeContext = TableGridContext?.TableNodeContext ?? default!;
    //            TableNodeContext.UpdateFromTableNodeContext();
    //        }
    //    }

    //    protected override bool ShouldRender()
    //    {
    //        return _hasDataChanged || _shouldRender;
    //    }

    //    public void SetDataSource(DataTable dataSource)
    //    {
    //        _dataSource = dataSource;
    //    }

    //    public DataTable GetDataSource()
    //    {
    //        _dataSource = TableGridContext?.GetDataSource(DataSourceName) ?? _dataSource;
    //        return _dataSource;
    //    }

    //    private string GetFieldValue(DataRow row)
    //    {
    //        return row[FieldName]?.ToString() ?? string.Empty;
    //    }
    //}
}
