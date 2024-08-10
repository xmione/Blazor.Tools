using Blazor.Tools.BlazorBundler.Entities;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    //public partial class AddTableLookupModal : ComponentBase
    //{
    //    [Parameter] public bool ShowSearchFieldsModal { get; set; }
    //    [Parameter] public DataTable? SelectedTable { get; set; }
    //    [Parameter] public EventCallback OnClose { get; set; }
    //    [Parameter] public EventCallback<List<SearchField>> OnSave { get; set; }
    //    [Parameter] public DataRow[]? SelectedData { get; set; } = default!;

    //    private string _tableName = string.Empty;

    //    protected override async Task OnParametersSetAsync()
    //    {
    //        _tableName = SelectedTable?.TableName ?? _tableName;
    //        await base.OnParametersSetAsync();
    //    }

    //    private async Task SaveAsync()
    //    {
    //        var targetTables = new List<SearchField>();
    //        // if (_targetTableColumnList != null)
    //        // {
    //        //     var targetTableGroups = _targetTableColumnList?
    //        //                             .Where(t => !string.IsNullOrEmpty(t.TargetTableName))
    //        //                             .GroupBy(t => t.TargetTableName)
    //        //                             .ToList();

    //        //     if (targetTableGroups != null)
    //        //     {
    //        //         foreach (var group in targetTableGroups)
    //        //         {
    //        //             var targetTableName = group.Key;
    //        //             var targetTableColumns = group.ToList();

    //        //             var targetTable = new TargetTable
    //        //                 {
    //        //                     TargetTableName = targetTableName,
    //        //                     TargetTableColumns = targetTableColumns,
    //        //                 };

    //        //             var dt = new DataTable(targetTableName);

    //        //             // Build columns for the target DataTable
    //        //             dt = BuildColumnsForTargetTable(dt, targetTableColumns, SelectedData);

    //        //             // Populate rows for the target DataTable
    //        //             dt = BuildRowsForTargetTable(dt, targetTableColumns, SelectedData);

    //        //             // Lastly, before adding the targetTable serialize the dt again and store in targetTable.DT
    //        //             targetTable.DT = await _sessionManager.SerializeAsync(dt);

    //        //             targetTables.Add(targetTable);
    //        //         }
    //        //     }
    //        // }        // if (_targetTableColumnList != null)
    //        // {
    //        //     var targetTableGroups = _targetTableColumnList?
    //        //                             .Where(t => !string.IsNullOrEmpty(t.TargetTableName))
    //        //                             .GroupBy(t => t.TargetTableName)
    //        //                             .ToList();

    //        //     if (targetTableGroups != null)
    //        //     {
    //        //         foreach (var group in targetTableGroups)
    //        //         {
    //        //             var targetTableName = group.Key;
    //        //             var targetTableColumns = group.ToList();

    //        //             var targetTable = new TargetTable
    //        //                 {
    //        //                     TargetTableName = targetTableName,
    //        //                     TargetTableColumns = targetTableColumns,
    //        //                 };

    //        //             var dt = new DataTable(targetTableName);

    //        //             // Build columns for the target DataTable
    //        //             dt = BuildColumnsForTargetTable(dt, targetTableColumns, SelectedData);

    //        //             // Populate rows for the target DataTable
    //        //             dt = BuildRowsForTargetTable(dt, targetTableColumns, SelectedData);

    //        //             // Lastly, before adding the targetTable serialize the dt again and store in targetTable.DT
    //        //             targetTable.DT = await _sessionManager.SerializeAsync(dt);

    //        //             targetTables.Add(targetTable);
    //        //         }
    //        //     }
    //        // }

    //        await OnSave.InvokeAsync(targetTables);
    //    }

    //    private async Task CloseAsync()
    //    {
    //        await OnClose.InvokeAsync();
    //    }

    //    protected override void BuildRenderTree(RenderTreeBuilder builder)
    //    {
    //        int sequence = 0;

    //        // PageTitle
    //        builder.OpenElement(sequence++, "h3");
    //        builder.AddContent(sequence++, "Search Field Entry Modal");
    //        builder.CloseElement();

    //        // Modal container
    //        builder.OpenElement(sequence++, "div");
    //        builder.AddAttribute(sequence++, "class", $"data-table-grid-modal {(ShowSearchFieldsModal ? "show" : "")}");

    //        // Buttons
    //        builder.OpenElement(sequence++, "div");
    //        builder.AddAttribute(sequence++, "class", "data-table-grid-modal-close-button");

    //        // Add button
    //        builder.OpenComponent<Icon>(sequence++);
    //        builder.AddAttribute(sequence++, "Name", "IconName.CheckCircleFill");
    //        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, SaveAsync));
    //        builder.AddAttribute(sequence++, "title", "Add");
    //        builder.CloseComponent();

    //        // Close button
    //        builder.OpenComponent<Icon>(sequence++);
    //        builder.AddAttribute(sequence++, "Name", "IconName.XCircleFill");
    //        builder.AddAttribute(sequence++, "title", "Close");
    //        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, CloseAsync));
    //        builder.CloseComponent();

    //        builder.CloseElement(); // Close buttons div

    //        // Modal content
    //        builder.OpenElement(sequence++, "div");
    //        builder.AddAttribute(sequence++, "class", "data-table-grid-modal-content");

    //        // TableGrid component
    //        builder.OpenComponent<TableGrid>(sequence++);
    //        builder.AddAttribute(sequence++, "Title", _tableName);
    //        builder.AddAttribute(sequence++, "DataTable", SelectedTable);
    //        builder.CloseComponent();

    //        builder.CloseElement(); // Close modal content div
    //        builder.CloseElement(); // Close modal container div
    //    }

    //}
}

