using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Blazor.Tools.BlazorBundler.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Blazor.Tools.BlazorBundler.Extensions;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.Json;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid<TModel, TIModel, TModelVM> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string TableID { get; set; } = string.Empty;
        [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();
        [Parameter] public TModel Model { get; set; } = default!;
        [Parameter] public TModelVM ModelVM { get; set; } = default!;
        [Parameter] public TIModel IModel { get; set; } = default!;
        [Parameter] public IEnumerable<TModelVM> Items { get; set; } = Enumerable.Empty<TModelVM>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<TModelVM>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;

        [Inject] protected ILogger<TableGrid<TModel, TIModel, TModelVM>> Logger { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            Logger.LogDebug("Parameters have been set.");
            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenComponent<TableGridInternals<TModel, TIModel, TModelVM>>(seq++);
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

        private RenderFragment<TModelVM> RenderRowTemplate()
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
        private void RenderCellContent(RenderTreeBuilder builder, TableColumnDefinition column, object? value, TModelVM item, string rowID)
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

                    if (column != null)
                    {
                        // Use the factory to create an instance of DropdownList
                        var dropdownList = DropdownListFactory.CreateDropdownList(
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
                    }
                    
                    break;

                default:
                    builder.AddContent(seq++, "Unsupported type");
                    break;
            }
        }
         
        private void InvokeValueChanged(TableColumnDefinition column, object newValue, TModelVM item)
        {
            var valueChangedDelegate = column.ValueChanged;
            valueChangedDelegate?.DynamicInvoke(newValue, item);
        }

        private async Task LogToConsoleAsync(string message)
        {
            await JSRuntime.InvokeVoidAsync("logToConsole", message);
        }

    }
}

