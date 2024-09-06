using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Blazor.Tools.BlazorBundler.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Blazor.Tools.BlazorBundler.Extensions;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.Json;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Factories;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid<TModel, TIModel> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string TableID { get; set; } = string.Empty;
        [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();
        [Parameter] public TModel Model { get; set; } = default!;
        [Parameter] public TIModel IModel { get; set; } = default!;
        [Parameter] public IViewModel<TModel, TIModel> ModelVM { get; set; } = default!;
        [Parameter] public IEnumerable<IViewModel<TModel, TIModel>> Items { get; set; } = Enumerable.Empty<IViewModel<TModel, TIModel>>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<IViewModel<TModel, TIModel>>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;

        [Inject] protected ILogger<TableGrid<TModel, TIModel>> Logger { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            Logger.LogDebug("Parameters have been set.");
            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenComponent<TableGridInternals<TModel, TIModel>>(seq++);
            builder.AddAttribute(seq++, "Title", Title);
            builder.AddAttribute(seq++, "TableID", TableID);
            builder.AddAttribute(seq++, "ColumnDefinitions", ColumnDefinitions);
            builder.AddAttribute(seq++, "ModelVM", ModelVM);
            builder.AddAttribute(seq++, "IModel", IModel);
            builder.AddAttribute(seq++, "Items", Items);
            builder.AddAttribute(seq++, "DataSources", DataSources);
            builder.AddAttribute(seq++, "ItemsChanged", ItemsChanged);
            builder.AddAttribute(seq++, "AllowCellRangeSelection", AllowCellRangeSelection);
            builder.AddAttribute(seq++, "StartContent", RenderStartContent());
            builder.AddAttribute(seq++, "TableHeader", RenderTableHeader());
            builder.AddAttribute(seq++, "RowTemplate", RenderRowTemplate());

            builder.CloseComponent(); // TableGridInternals
        }
        private RenderFragment RenderStartContent()
        {
            return new RenderFragment(BuildStartContent);
        }

        private void BuildStartContent(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenElement(seq++, "h2");
            builder.AddContent(seq++, Title);
            builder.CloseElement(); // h2
        }

        private RenderFragment RenderTableHeader()
        {
            var fragment = new RenderFragment(BuildTableHeader);
            return fragment;
        }

        private void BuildTableHeader(RenderTreeBuilder builder)
        {
            Logger.LogInformation("Building table header...");
            //LogToConsoleAsync("Building table header...").Wait();
            int seq = 0;

            foreach (var column in ColumnDefinitions)
            {
                builder.OpenElement(seq++, "th");
                builder.AddContent(seq++, column.HeaderText);

                Logger.LogDebug($"Column header text: {column.HeaderText}");
                //LogToConsoleAsync($"Column header text: {column.HeaderText}").Wait();
                builder.CloseElement(); // th
            }
        }

        private RenderFragment<IViewModel<TModel, TIModel>> RenderRowTemplate()
        {
            return item =>
            {
                return builder =>
                {
                    Logger.LogInformation("Building row template...");
                    int seq = 0;
                     
                        int colNo = 0;
                        foreach (var column in ColumnDefinitions)
                        {
                            colNo++;
                            Logger.LogDebug($"ColumnName: {column.ColumnName}");
                            //LogToConsoleAsync($"ColumnName: {column.ColumnName}").Wait();
                            builder.OpenElement(seq++, "td");
                            var rowID = item.GetPropertyValue("RowID")?.ToString() ?? string.Empty;
                            var id = $"{TableID}-{rowID}-{colNo}";
                            builder.AddAttribute(seq++, "id", id);

                            var itemString = JsonSerializer.Serialize(item);
                            var paramString = $"CellClick('{itemString}',{colNo}, '{TableID}', {Items.Count()}, {ColumnDefinitions.Count})";
                            builder.AddAttribute(seq++, "onclick", paramString);
                            builder.AddAttribute(seq++, "class", "cursor-pointer");
                            object? value;
                            if (column != null)
                            {
                                value = item.GetPropertyValue(column.ColumnName);
                                RenderCellContent(builder, column, value, item, rowID);
                            }
                            
                            builder.CloseElement(); // td
                        }
                 
                };
            };
        }
        private void RenderCellContent(RenderTreeBuilder builder, TableColumnDefinition column, object? value, IViewModel<TModel, TIModel> item, string rowID)
        {
            int seq = 0;
            int.TryParse(rowID, out int rowNo);
            string isEditModeStringValue = item?.GetPropertyValue("IsEditMode")?.ToString() ?? "false";
            bool.TryParse(isEditModeStringValue, out bool isEditMode);

            switch (column.ColumnType)
            {
                case Type t when t == typeof(int):
                    builder.OpenComponent<NumberInput>(seq++);
                    builder.AddAttribute(seq++, "ColumnName", column.ColumnName);
                    builder.AddAttribute(seq++, "Value", value);
                    builder.AddAttribute(seq++, "IsEditMode", isEditMode);
                    builder.AddAttribute(seq++, "RowID", rowNo);
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item ?? default!)));
                    builder.CloseComponent();

                    Logger.LogDebug($"ColumnName: {column.ColumnName}");
                    Logger.LogDebug($"value: {value}");
                    Console.WriteLine($"ColumnName: {column.ColumnName}");
                    Console.WriteLine($"value: {value}");

                    break;

                case Type t when t == typeof(string):
                    builder.OpenComponent<TextInput>(seq++);
                    builder.AddAttribute(seq++, "ColumnName", column.ColumnName);
                    builder.AddAttribute(seq++, "Value", value);
                    builder.AddAttribute(seq++, "IsEditMode", isEditMode);
                    builder.AddAttribute(seq++, "RowID", rowNo);
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item ?? default!)));
                    builder.CloseComponent();
                    break;

                case Type t when t == typeof(DateOnly) || t == typeof(DateOnly?):
                    builder.OpenComponent<DateOnlyPicker>(seq++);
                    builder.AddAttribute(seq++, "ColumnName", column.ColumnName);
                    builder.AddAttribute(seq++, "Value", value);
                    builder.AddAttribute(seq++, "IsEditMode", isEditMode);
                    builder.AddAttribute(seq++, "RowID", rowNo);
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<DateOnly?>(this, newValue => InvokeValueChanged(column, newValue ?? default, item ?? default!)));
                    builder.CloseComponent();
                    break;

                case Type t when t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>) || t.GetGenericTypeDefinition() == typeof(IEnumerable<>) || t.GetGenericTypeDefinition() == typeof(ICollection<>)):

                    var optionIDFieldName = column?.GetPropertyValue("OptionIDFieldName")?.ToString() ?? string.Empty;
                    var optionValueFieldName = column?.GetPropertyValue("OptionValueFieldName")?.ToString() ?? string.Empty;

                    if (column != null)
                    {
                        // Use the factory to create an instance of DropdownList
                        var dropdownList = DropdownListFactory.CreateDropdownList(
                            typeof(DropdownList), // Pass the type of items here
                            column.Items,
                            column.ColumnName,
                            column.HeaderText,
                            value,
                            optionIDFieldName,
                            optionValueFieldName,
                            isEditMode,
                            rowNo,
                            EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item ?? default!))
                        );

                        var type = dropdownList?.GetType() ?? default!;
                        // Use builder to add the component to the render tree
                        builder.OpenComponent(seq++, type); // or DropdownList if specific type is used
                        builder.AddAttribute(seq++, "Items", column.Items);
                        builder.AddAttribute(seq++, "ColumnName", column.ColumnName);
                        builder.AddAttribute(seq++, "Value", value);
                        builder.AddAttribute(seq++, "IsEditMode", isEditMode);
                        builder.AddAttribute(seq++, "RowID", rowNo);
                        builder.AddAttribute(seq++, "OptionIDFieldName", optionIDFieldName);
                        builder.AddAttribute(seq++, "OptionValueFieldName", optionValueFieldName);
                        builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item ?? default!)));
                        builder.CloseComponent();
                    }
                    
                    break;

                default:
                    builder.AddContent(seq++, "Unsupported type");
                    break;
            }
        }
         
        private void InvokeValueChanged(TableColumnDefinition column, object newValue, IViewModel<TModel, TIModel> item)
        {
            var valueChangedDelegate = column.ValueChanged;
            valueChangedDelegate?.DynamicInvoke(newValue, item);
        }

        private async Task LogToConsoleAsync(string message)
        {
            await JSRuntime.InvokeVoidAsync("logToConsole", message);
        }

        //public async Task HandleSelectedDataComb(DataRow[] selectedData)
        //{
        //    _nodeSelectedData = selectedData;

        //    if (TableNodeContext != null)
        //    {
        //        TableNodeContext.SelectedData = _nodeSelectedData;
        //        TableNodeContext.StartCell = _nodeStartCell;
        //        TableNodeContext.EndCell = _nodeEndCell;

        //        await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);
        //        await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
        //        await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);

        //    }

        //    StateHasChanged();
        //    await Task.CompletedTask;
        //}

        //public async Task<DataRow[]?> ShowSetTargetTableModalAsync()
        //{
        //    if (!string.IsNullOrEmpty(_nodeStartCell) && !string.IsNullOrEmpty(_nodeEndCell))
        //    {
        //        // Extracting start row and column from startCell
        //        int startRow = int.Parse(_nodeStartCell.Substring(1, _nodeStartCell.IndexOf('C') - 1));
        //        int startCol = int.Parse(_nodeStartCell.Substring(_nodeStartCell.IndexOf('C') + 1));

        //        // Extracting end row and column from endCell
        //        int endRow = int.Parse(_nodeEndCell.Substring(1, _nodeEndCell.IndexOf('C') - 1));
        //        int endCol = int.Parse(_nodeEndCell.Substring(_nodeEndCell.IndexOf('C') + 1));

        //        _nodeSelectedData = GetDataInRange(startRow, startCol, endRow, endCol);

        //        if (TableNodeContext != null)
        //        {
        //            TableNodeContext.SelectedData = _nodeSelectedData;

        //            await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);
        //        }

        //        await Task.CompletedTask;
        //    }

        //    StateHasChanged();

        //    return _nodeSelectedData;
        //}

        //private DataRow[] GetDataInRange(int startRow, int startCol, int endRow, int endCol)
        //{
        //    List<DataRow> dataInRange = new List<DataRow>();

        //    if (_nodeDataTable != null)
        //    {
        //        // Create a new DataTable with the selected columns
        //        DataTable filteredDataTable = new DataTable();
        //        for (int i = startCol; i <= endCol; i++)
        //        {
        //            DataColumn column = _nodeDataTable.Columns[i];
        //            filteredDataTable.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
        //        }

        //        for (int i = startRow; i <= endRow; i++)
        //        {
        //            DataRow newRow = filteredDataTable.NewRow();

        //            for (int j = startCol; j <= endCol; j++)
        //            {
        //                newRow[j - startCol] = _nodeDataTable.Rows[i][j];
        //            }

        //            dataInRange.Add(newRow);
        //        }

        //    }

        //    return dataInRange.ToArray();
        //}
    }
}

