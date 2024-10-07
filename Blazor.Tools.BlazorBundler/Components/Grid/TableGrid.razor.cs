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
using System.Data;
using BlazorBootstrap;
using DocumentFormat.OpenXml.EMMA;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid<TModel, TIModel> : ComponentBase, ITableGrid
        where TModel : class, IBase // TModel must derive from IBase
        where TIModel : IModelExtendedProperties // Optionally constrain TIModel if applicable
    {
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string TableID { get; set; } = string.Empty;
        [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();
        [Parameter] public TModel Model { get; set; } = default!;
        [Parameter] public IViewModel<TModel, TIModel> ModelVM { get; set; } = default!;
        [Parameter] public IEnumerable<IViewModel<TModel, TIModel>> Items { get; set; } = Enumerable.Empty<IViewModel<TModel, TIModel>>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<IViewModel<TModel, TIModel>>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;

        [Inject] protected ILogger<TableGrid<TModel, TIModel>> Logger { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        private Type? _tableGridInternalsType;
        private object _tableGridInternalsComponentReference;

        protected override async Task OnParametersSetAsync()
        {
            Logger.LogDebug("Parameters have been set.");
            await InitializeVariablesAsync();
            await Task.CompletedTask;
        }

        public async Task InitializeVariablesAsync() 
        {
            // Get the TableGrid component type with the correct generic types
            _tableGridInternalsType = typeof(TableGridInternals<,>).MakeGenericType(typeof(TModel), typeof(TIModel));

            await Task.CompletedTask;
        }

        protected override async void BuildRenderTree(RenderTreeBuilder builder)
        {
            await RenderMainContentAsync(builder);
        }

        /// <summary>
        /// Renders main content. These types of methods were created for testability.
        /// </summary>
        /// <param name="builder">RenderTreeBuilder for the main content</param>
        public async Task RenderMainContentAsync(RenderTreeBuilder builder)
        {
            if (_tableGridInternalsType != null)
            {
                int seq = 0;
                builder.OpenComponent(seq++, _tableGridInternalsType);
                builder.AddAttribute(seq++, "Title", Title);
                builder.AddAttribute(seq++, "TableID", TableID);
                builder.AddAttribute(seq++, "ColumnDefinitions", ColumnDefinitions);
                builder.AddAttribute(seq++, "ModelVM", ModelVM);
                builder.AddAttribute(seq++, "Items", Items);
                builder.AddAttribute(seq++, "DataSources", DataSources);
                builder.AddAttribute(seq++, "ItemsChanged", ItemsChanged);
                builder.AddAttribute(seq++, "AllowCellRangeSelection", AllowCellRangeSelection);
                builder.AddAttribute(seq++, "StartContent", RenderStartContent());
                builder.AddAttribute(seq++, "TableHeader", RenderTableHeader());
                builder.AddAttribute(seq++, "RowTemplate", RenderRowTemplate());

                // Capture the component reference
                builder.AddComponentReferenceCapture(seq++, reference =>
                {
                    _tableGridInternalsComponentReference = reference;
                });

                builder.CloseComponent(); // TableGridInternals            
            }

            await Task.CompletedTask;
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
                            
                            if (AllowCellRangeSelection)
                            {
                                var paramString = $"CellClick('{itemString}',{colNo}, '{TableID}', {Items.Count()}, {ColumnDefinitions.Count})";
                                builder.AddAttribute(seq++, "onclick", paramString);
                            }
                        
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
                    AppLogger.WriteInfo($"ColumnName: {column.ColumnName}");
                    AppLogger.WriteInfo($"value: {value}");

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
    }
}

