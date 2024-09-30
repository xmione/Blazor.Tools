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

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid<TModel, TIModel> : ComponentBase where TModel : class
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

        private Type? _tableGridInternalsType;
        private object _tableGridInternalsComponentReference;

        protected override async Task OnParametersSetAsync()
        {
            Logger.LogDebug("Parameters have been set.");
            // Get the TableGrid component type with the correct generic types
            _tableGridInternalsType = typeof(TableGridInternals<,>).MakeGenericType(typeof(TModel), typeof(TIModel));
            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (_tableGridInternalsType != null)
            {
                int seq = 0;
                builder.OpenComponent(seq++, _tableGridInternalsType);
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

                // Capture the component reference
                builder.AddComponentReferenceCapture(seq++, inst =>
                {
                    _tableGridInternalsComponentReference = inst;
                });

                builder.CloseComponent(); // TableGridInternals            
            }
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

        public async Task<DataRow[]?> ShowSetTargetTableModalAsync(string startCell, string endCell)
        {
            DataRow[] selectedData = default!;

            var method = _tableGridInternalsType?.GetMethod("ShowSetTargetTableModalAsync");

            if (_tableGridInternalsComponentReference != null)
            {
                // Cast the reference to the appropriate type (generic table grid)
                var tableGridInstance = _tableGridInternalsComponentReference as dynamic;

                // Invoke the method
                if (tableGridInstance != null)
                {
                    var resultTask = tableGridInstance.ShowSetTargetTableModalAsync(startCell, endCell);

                    // Since it's a Task, you can await it or use other async handling
                    selectedData = await resultTask;

                    // Now you can use the 'rows' variable which is of type DataRow[]?
                    if (selectedData != null)
                    {
                        foreach (var row in selectedData)
                        {
                            // LoadAssembly each row here
                            Console.WriteLine(row);
                        }
                    }
                }
            }

            //if (_tableGridInternalsType != null)
            //{
            //    var instance = Activator.CreateInstance(_tableGridInternalsType);

            //    // Check if the method and instance are valid
            //    if (method != null && instance != null)
            //    {
            //        // Invoke the async method (it returns a Task)
            //        var result = method.Invoke(instance, null);

            //        // Check if the result is a Task<DataRow[]?>
            //        if (result is Task<DataRow[]?> task)
            //        {
            //            // Await the task to get the result
            //            var rows = await task;

            //            // Now you can use the 'rows' variable which is of type DataRow[]?
            //            if (rows != null)
            //            {
            //                foreach (var row in rows)
            //                {
            //                    // LoadAssembly each row here
            //                    Console.WriteLine(row);
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("Method or instance not found.");
            //    }
            //}

            await Task.CompletedTask;
            //StateHasChanged();

            return selectedData;
        }

        public async Task ReloadTableGridInternalsComponent()
        {
            var reloadComponent = _tableGridInternalsType?.GetMethod("ReloadComponent");

            if (_tableGridInternalsType != null)
            {
                // Create an instance of the type (_tableGridInternalsType refers to a type, not an instance)
                var instance = Activator.CreateInstance(_tableGridInternalsType);

                // Check if the method and instance are valid
                if (instance != null)
                {
                    // Reload TableGridInternals Component
                    if (reloadComponent != null)
                    {
                        // Invoke the async method (it returns a Task)
                        reloadComponent.Invoke(instance, null);
                    }
                    else
                    {
                        Console.WriteLine("Method ReloadTableGridInternalsComponent or instance not found.");
                    }

                }
            }
            
            await Task.CompletedTask;
        }
    }
}

