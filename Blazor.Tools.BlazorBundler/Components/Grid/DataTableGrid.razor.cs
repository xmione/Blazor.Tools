using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using BlazorBootstrap;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System.Data;
using System.Reflection;
using DataTableExtensions = Blazor.Tools.BlazorBundler.Extensions.DataTableExtensions;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DataTableGrid : ComponentBase
    {
        [Parameter] public string Title { get; set; } = default!;
        [Parameter] public string TableID { get; set; } = default!;
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public DataTable SelectedTable { get; set; } = default!;
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;
        [Parameter] public List<AssemblyTable>? TableList { get; set; } = default!;
        [Parameter] public List<string> HiddenColumnNames { get; set; } = default!;
        [Parameter] public Dictionary<string, string> HeaderNames { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<IModelExtendedProperties>> ItemsChanged { get; set; }

        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
        //[Inject] public ISessionTableService _sessionTableService { get; set; } = default!;

        private DataTable _selectedTableVM = default!;
        private bool _showSetTargetTableModal = false;
        private DataRow[]? _selectedData = default!;
        private string _selectedFieldValue = string.Empty;
        //private SessionManager _sessionManager = SessionManager.Instance;
        private bool _isRetrieved = false;
        //private TableGrid<TModel, TIModel, TModelVM> _tableGrid = default!;

        private List<TargetTable>? _targetTables;
        private string _tableName = string.Empty;
        private List<TableColumnDefinition> _columnDefinitions = default!;
        private object? _modelVMInstance;
        private Type _modelType = default!;
        private Type _modelVMType = default!;
        private Type _interfaceType = default!;
        private object? _modelInstance;
        private bool _isFirstCellClicked = true;
        private string _startCell = string.Empty;
        private string _endCell = string.Empty;
        private int _startRow;
        private int _endRow;
        private int _startCol;
        private int _endCol;
        private IEnumerable<object> _items = Enumerable.Empty<object>();


        //private IList<SessionItem>? _sessionItems;

        protected override async Task OnParametersSetAsync()
        {
            await InitializeVariables();
            //await RetrieveDataFromSessionTableAsync();
            await base.OnParametersSetAsync();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;

            // Get the TableGrid component type with the correct generic types
            var tableGridType = typeof(TableGrid<,,>).MakeGenericType(_modelType, _interfaceType, _modelVMType);

            // Open the component using the resolved type
            builder.OpenComponent(sequence++, tableGridType);
            builder.AddAttribute(sequence++, "Title", Title);
            builder.AddAttribute(sequence++, "TableID", _tableName.ToLower());
            builder.AddAttribute(sequence++, "Model", _modelInstance);
            builder.AddAttribute(sequence++, "ModelVM", _modelVMInstance);
            builder.AddAttribute(sequence++, "IModel", default(IModelExtendedProperties)); // Use default or provide an actual instance if needed
            
            // Add the items as an attribute to the TableGrid component
            builder.AddAttribute(sequence++, "Items", _items);
            
            builder.AddAttribute(sequence++, "DataSources", DataSources);
            // Handle ItemsChanged EventCallback
            var callbackMethod = typeof(EventCallbackFactory).GetMethod("Create", new[] { typeof(object), typeof(Action<>) });
            if (callbackMethod != null)
            {
                var genericCallbackMethod = callbackMethod.MakeGenericMethod(typeof(IEnumerable<>).MakeGenericType(_modelVMType));
                var callback = genericCallbackMethod.Invoke(null, new object[] { this, ItemsChanged });
                builder.AddAttribute(sequence++, "ItemsChanged", callback);
            }
            builder.AddAttribute(sequence++, "ColumnDefinitions", _columnDefinitions);
            builder.AddAttribute(sequence++, "AllowCellRangeSelection", AllowCellRangeSelection);

            builder.CloseComponent(); // Closing TableGrid

            // Icon for Set Target Table Modal
            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", IconName.Table);
            builder.AddAttribute(sequence++, "Class", "text-success icon-button mb-2 cursor-pointer");
            builder.AddAttribute(sequence++, "title", "Step 1. Set Target Table");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, ShowSetTargetTableModalAsync));
            builder.CloseComponent();

            // Target tables grid rendering
            //if (_targetTables != null)
            //{
            //    foreach (var targetTable in _targetTables)
            //    {
            //        if (targetTable != null && !string.IsNullOrEmpty(targetTable.DT))
            //        {
            //            var dt = targetTable.DT.DeserializeAsync<DataTable>().Result;
            //            builder.OpenComponent<TableGrid>(sequence++);
            //            builder.AddAttribute(sequence++, "DataTable", dt);
            //            builder.CloseComponent();
            //        }
            //    }
            //}

            // Icon for Upload Data
            builder.OpenComponent<Icon>(sequence++);
            builder.AddAttribute(sequence++, "Name", IconName.CloudUpload);
            builder.AddAttribute(sequence++, "Class", "cursor-pointer");
            builder.AddAttribute(sequence++, "title", "Upload to existing AccSol tables");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, UploadData));
            builder.CloseComponent();

            // Set Target Table Modal
            //if (_showSetTargetTableModal)
            //{
            //    builder.OpenComponent<SetTargetTableModal>(sequence++);
            //    builder.AddAttribute(sequence++, "Title", Title);
            //    builder.AddAttribute(sequence++, "ShowSetTargetTableModal", _showSetTargetTableModal);
            //    builder.AddAttribute(sequence++, "OnClose", EventCallback.Factory.Create(this, CloseSetTargetTableModal));
            //    builder.AddAttribute(sequence++, "OnSave", EventCallback.Factory.Create<List<TargetTable>>(this, SaveToTargetTableAsync));
            //    builder.AddAttribute(sequence++, "SelectedData", _selectedData);
            //    builder.AddAttribute(sequence++, "OnSelectedDataComb", EventCallback.Factory.Create<DataRow[]>(this, HandleSelectedDataComb));
            //    builder.CloseComponent();
            //}
        }
        private async Task InitializeVariables()
        {
            _tableName = SelectedTable?.TableName ?? _tableName;
            _selectedTableVM = SelectedTable?.Copy() ?? _selectedTableVM;

            // Create an instance of DynamicClassBuilder for TModel
            var modelClassBuilder = new DynamicClassBuilder(_tableName);
            // Create the TModel class from the DataTable
            modelClassBuilder.CreateClassFromDataTable(SelectedTable);
            // Create an instance of the dynamic TModel class
            _modelInstance = modelClassBuilder.CreateInstance();

            // Create an instance of DynamicClassBuilder for TModelVM
            var modelVMClassBuilder = new DynamicClassBuilder($"{_tableName}");
            // Create the TModelVM class from the DataTable
            modelVMClassBuilder.CreateClassFromDataTable(_selectedTableVM);
            // Create an instance of the dynamic TModelVM class
            _modelVMInstance = modelVMClassBuilder.CreateInstance();

            // Get the type of the dynamic TModel and TModelVM classes
            _modelType = _modelInstance?.GetType() ?? default!;
            _modelVMType = _modelVMInstance?.GetType() ?? default!;
            _interfaceType = typeof(IModelExtendedProperties);
            var modelExtendedProperties = new ModelExtendedProperties();    
            var excludedColumns = modelExtendedProperties.GetPropertyNames();
            // TableColumnDefinition should be based on model type, e.g.: Employee class and not EmployeeVM
            var props = _modelType.GetProperties(); 
            if (props != null)
            {
                _selectedTableVM.AddPropertiesFromPropertyInfoList(props);
                _columnDefinitions = new List<TableColumnDefinition>();
                foreach (PropertyInfo property in props)
                {
                    var isExcluded = excludedColumns.FirstOrDefault(p => p.Contains(property.Name)) != null;
                    if (!isExcluded)
                    {
                        var tableColumnDefinition = new TableColumnDefinition
                        {
                            ColumnName = property.Name,
                            HeaderText = property.Name,
                            ColumnType = typeof(string),
                            CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, vm, rowIndex),
                            ValueChanged = new Action<object, object>((newValue, modelInstance) => OnValueChanged(property.Name, newValue, modelInstance))
                        };

                        _columnDefinitions.Add(tableColumnDefinition);
                    }    
                }
            }

            // Use reflection to call the ConvertDataTableToObjects method
            var itemsMethod = typeof(DataTableExtensions).GetMethod(
                "ConvertDataTableToObjects",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(DataTable) }, // Specify the parameters here
                null);

            if (itemsMethod != null)
            {
                var genericMethod = itemsMethod.MakeGenericMethod(_modelVMType);
                var items = genericMethod.Invoke(null, new object[] { _selectedTableVM });

                // Cast items to IEnumerable<object>
                if (items != null)
                {
                    _items = (IEnumerable<object>)items;
                }
            }
            else
            {
                throw new InvalidOperationException("The ConvertDataTableToObjects method was not found.");
            }
            
            //_sessionItems = new List<SessionItem>
            //{
            //    new SessionItem()
            //    {
            //        Key = $"{Title}_selectedData", Value = _selectedData, Type = typeof(DataRow[]), Serialize = true
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Title}_targetTables", Value = _targetTables, Type = typeof(List<TargetTable>), Serialize = true
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Title}_tableSourceName", Value = _tableSourceName, Type = typeof(string), Serialize = true
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Title}_showSetTargetTableModal", Value = _showSetTargetTableModal, Type = typeof(bool), Serialize = true
            //    }
            //};

            await Task.CompletedTask;
        }

        public void OnValueChanged(string propertyName, object newValue, object modelInstance)
        {
            if (modelInstance == null)
                return;

            var modelType = modelInstance.GetType();
            var property = modelType.GetProperty(propertyName);

            if (property != null)
            {
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var safeValue = Convert.ChangeType(newValue, propertyType);
                property.SetValue(modelInstance, safeValue);

                // Update the items list
                foreach (var item in _items)
                {
                    var itemType = item.GetType();
                    var idProperty = itemType.GetProperty("RowID"); // Adjust based on your model
                    if (idProperty != null && idProperty.GetValue(item)?.Equals(modelInstance.GetType().GetProperty("RowID")?.GetValue(modelInstance)) == true)
                    {
                        var existingProperty = itemType.GetProperty(propertyName);
                        if (existingProperty != null)
                        {
                            existingProperty.SetValue(item, safeValue);
                        }
                        break; // Assuming RowID is unique, exit after updating
                    }
                }
            }

            // Notify that state has changed (if applicable)
            // StateHasChanged(); this should be triggered on the calling program after calling this method
        }


        public async Task HandleCellClickAsync(string id, object modelInstance, int column)
        {
            var modelVM = modelInstance as IModelExtendedProperties; // Adjust if necessary for your generic model
            if (modelVM == null || modelVM.IsEditMode)
            {
                return;
            }

            string cellIdentifier = $"R{modelVM.RowID}C{column}";
            var startCellID = $"{TableID}-start-cell";
            var endCellID = $"{TableID}-end-cell";
            _startCell = await JSRuntime.InvokeAsync<string>("getValue", startCellID);
            _endCell = await JSRuntime.InvokeAsync<string>("getValue", endCellID);

            bool areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);
            if (string.IsNullOrEmpty(_startCell) || _isFirstCellClicked)
            {
                _startRow = modelVM.RowID;
                _startCol = column;
                _startCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : false;

                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-row", $"{_startRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-col", $"{_startCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-cell", cellIdentifier);
            }
            else
            {
                _endRow = modelVM.RowID;
                _endCol = column;
                _endCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : true;

                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-row", $"{_endRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-col", $"{_endCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-cell", cellIdentifier);
            }

            areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);

            if (areBothFilled)
            {
                var totalRows = _items.Count();
                var totalCols = _columnDefinitions?.Count;
                await JSRuntime.InvokeVoidAsync("toggleCellBorders", _startRow, _endRow, _startCol, _endCol, totalRows, totalCols, TableID, true);
            }

            await Task.CompletedTask;
        }


        private async Task RetrieveDataFromSessionTableAsync()
        {
            //try
            //{
            //    if (!_isRetrieved && _sessionItems != null)
            //    {
            //        _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
            //        _selectedData = (DataRow[]?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedData"))?.Value;
            //        _targetTables = (List<TargetTable>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_targetTables"))?.Value;
            //        _showSetTargetTableModal = (bool)(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_showSetTargetTableModal"))?.Value ?? false);

            //        _isRetrieved = true;
            //        StateHasChanged();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error: {0}", ex.Message);
            //}

            await Task.CompletedTask;
        }

        private void CloseSetTargetTableModal()
        {
            _showSetTargetTableModal = false;
            StateHasChanged();

            //_sessionManager.SaveToSessionTableAsync($"{Title}_showSetTargetTableModal", _showSetTargetTableModal, serialize: true).Wait();
        }

        private async Task SaveToTargetTableAsync(List<TargetTable>? targetTables)
        {
            CloseSetTargetTableModal();

            _targetTables = targetTables;
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_targetTables", _targetTables, serialize: true);

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleSelectedDataComb(DataRow[] selectedData)
        {
            _selectedData = selectedData;

            //await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedData", _selectedData, serialize: true);
            //await _tableGrid.HandleSelectedDataComb(selectedData);

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleSetTargetTableColumnList(List<TargetTableColumn> targetTableColumnList)
        {
            await Task.CompletedTask;
        }

        private async Task HandleFieldValueChangedAsync(string newValue)
        {
            _selectedFieldValue = newValue;
            await Task.CompletedTask;
        }

        private async Task ShowSetTargetTableModalAsync()
        {
            _showSetTargetTableModal = true;
            //_selectedData = await _tableGrid.ShowSetTargetTableModalAsync();
            if (_selectedData == null)
            {
                _showSetTargetTableModal = false;
            }

            //await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedData", _selectedData, serialize: true);
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_showSetTargetTableModal", _showSetTargetTableModal, serialize: true);
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task UploadData()
        {
            if (_targetTables != null)
            {
                //await _sessionTableService.UploadTableListAsync(_targetTables);

                //TODO: sol: Optionally, show a success message or handle post-upload actions
            }

            await Task.CompletedTask;
        }

    }
}
