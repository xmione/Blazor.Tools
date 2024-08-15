using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Blazor.Tools.BlazorBundler.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Blazor.Tools.BlazorBundler.Extensions;
using Microsoft.AspNetCore.Components.Web;
using DocumentFormat.OpenXml.Spreadsheet;
using Blazor.Tools.BlazorBundler.Interfaces;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid : ComponentBase, ITableGrid
    {
        private DataRow[]? _selectedData;

        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string TableID { get; set; } = string.Empty;
        [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();
        [Parameter] public IBaseVM ModelVM { get; set; } = default!;
        [Parameter] public IEnumerable<IBaseVM> Items { get; set; } = Enumerable.Empty<IBaseVM>();
        [Parameter] public EventCallback<IEnumerable<IBaseVM>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;

        [Inject] protected ILogger<TableGrid> Logger { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] public ISessionTableService _sessionTableService { get; set; } = default!;

        private SessionManager _sessionManager = SessionManager.Instance;
        protected override async Task OnParametersSetAsync()
        {
            Logger.LogDebug("Parameters have been set.");
            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenComponent<TableGridInternals>(seq++);
            builder.AddAttribute(seq++, "Title", Title);
            builder.AddAttribute(seq++, "TableID", TableID);
            builder.AddAttribute(seq++, "ColumnDefinitions", ColumnDefinitions);
            builder.AddAttribute(seq++, "ModelVM", ModelVM);
            builder.AddAttribute(seq++, "Items", Items);
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

        private RenderFragment<IBaseVM> RenderRowTemplate()
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
                            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
                            {
                                await column?.CellClicked?.Invoke(id, item, colNo);
                            }));

                            builder.AddAttribute(seq++, "class", "cursor-pointer");
                            var value = item.GetPropertyValue(column.ColumnName);
                            RenderCellContent(builder, column, value, item, rowID);
                            builder.CloseElement(); // td
                        }
                 
                };
            };
        }
        private void RenderCellContent(RenderTreeBuilder builder, TableColumnDefinition column, object? value, IBaseVM item, string rowID)
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
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item)));
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
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item)));
                    builder.CloseComponent();
                    break;

                case Type t when t == typeof(DateOnly) || t == typeof(DateOnly?):
                    builder.OpenComponent<DateOnlyPicker>(seq++);
                    builder.AddAttribute(seq++, "ColumnName", column.ColumnName);
                    builder.AddAttribute(seq++, "Value", value);
                    builder.AddAttribute(seq++, "IsEditMode", isEditMode);
                    builder.AddAttribute(seq++, "RowID", rowNo);
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<DateOnly?>(this, newValue => InvokeValueChanged(column, newValue, item)));
                    builder.CloseComponent();
                    break;

                case Type t when t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>) || t.GetGenericTypeDefinition() == typeof(IEnumerable<>) || t.GetGenericTypeDefinition() == typeof(ICollection<>)):

                    var optionIDFieldName = column?.GetPropertyValue("OptionIDFieldName")?.ToString() ?? string.Empty;
                    var optionValueFieldName = column?.GetPropertyValue("OptionValueFieldName")?.ToString() ?? string.Empty;

                    // Use the factory to create an instance of DropdownList
                    var dropdownList = DropdownListFactory.Create(
                        typeof(object), // Pass the type of items here
                        column.Items,
                        column.ColumnName,
                        column.HeaderText,
                        value,
                        optionIDFieldName,
                        optionValueFieldName,
                        isEditMode,
                        rowNo,
                        EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item))
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
                    builder.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<object>(this, newValue => InvokeValueChanged(column, newValue, item)));
                    builder.CloseComponent();
                    break;

                default:
                    builder.AddContent(seq++, "Unsupported type");
                    break;
            }
        }
         
        private void InvokeValueChanged(TableColumnDefinition column, object newValue, IBaseVM item)
        {
            var valueChangedDelegate = column.ValueChanged;
            valueChangedDelegate?.DynamicInvoke(newValue, item);
        }

        private async Task LogToConsoleAsync(string message)
        {
            await JSRuntime.InvokeVoidAsync("logToConsole", message);
        }

        public async Task HandleSelectedDataComb(DataRow[] selectedData)
        {
            _selectedData = selectedData;

            //await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedData", _selectedData, serialize: true);
            
            StateHasChanged();
            await Task.CompletedTask;
        }

        public async Task<DataRow[]?> ShowSetTargetTableModalAsync()
        {
            var startCell = await JSRuntime.InvokeAsync<string>("getValue", $"{TableID}-start-cell");
            var endCell = await JSRuntime.InvokeAsync<string>("getValue", $"{TableID}-end-cell");
            
            if (!string.IsNullOrEmpty(startCell) && !string.IsNullOrEmpty(endCell))
            {
                // Extracting start row and column from startCell
                int startRow = int.Parse(startCell.Substring(1, startCell.IndexOf('C') - 1));
                int startCol = int.Parse(startCell.Substring(startCell.IndexOf('C') + 1));

                // Extracting end row and column from endCell
                int endRow = int.Parse(endCell.Substring(1, endCell.IndexOf('C') - 1));
                int endCol = int.Parse(endCell.Substring(endCell.IndexOf('C') + 1));

                _selectedData = GetDataInRange(startRow, startCol, endRow, endCol);

                //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeSelectedData", _nodeSelectedData, serialize: true);

                await Task.CompletedTask;
            }

            StateHasChanged();

            return _selectedData;
        }

        private DataRow[] GetDataInRange(int startRow, int startCol, int endRow, int endCol)
        {
            List<DataRow> dataInRange = new List<DataRow>();

            if (Items != null)
            {
                // Convert Items to DataTable
                DataTable itemsTable = Items.ToDataTable();
                // Create a new DataTable with the selected columns
                DataTable filteredDataTable = new DataTable();
                for (int i = startCol; i <= endCol; i++)
                {
                    var column = ColumnDefinitions[i];

                    filteredDataTable.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
                }

                for (int i = startRow; i <= endRow; i++)
                {
                    DataRow newRow = filteredDataTable.NewRow();

                    for (int j = startCol; j <= endCol; j++)
                    {
                        newRow[j - startCol] = itemsTable.Rows[i][j];
                    }

                    dataInRange.Add(newRow);
                }

            }

            return dataInRange.ToArray();
        }

    }
}

