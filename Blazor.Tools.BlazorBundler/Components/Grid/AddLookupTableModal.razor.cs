using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class AddLookupTableModal : ComponentBase
    {
        [Parameter] public string Tag { get; set; } = default!;
        [Parameter] public bool ShowSearchFieldsModal { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<List<SearchField>> OnSave { get; set; }
        [Parameter] public DataRow[]? SelectedData { get; set; } = default!;
        [Parameter] public List<AssemblyTable>? TableList { get; set; } = default!;

        //private SessionManager _sessionManager = SessionManager.Instance;
        private string _baseClassName = string.Empty;
        private int _targetTableID = default!;
        private string _targetTableValue = default!;
        private string? _targetFieldValue = default!;
        private int _targetFieldMatchConditionID = default!;
        private string? _targetFieldMatchConditionValue = default!;
        //private IList<SessionItem>? _sessionItems;
        private DataTable? _searchFieldsTable;
        //private bool _isRetrieved;
        private string? _title;
        private string? _dataSource;
        private DataTable? _matchConditionsTable;

        protected override async Task OnParametersSetAsync()
        {

            await InitializeVariables();
            await RetrieveDataFromSessionTableAsync();

            await base.OnParametersSetAsync();
        }

        private async Task InitializeVariables()
        {

            _baseClassName = _searchFieldsTable?.TableName ?? "LookupTable";
            _targetTableID = 1;
            _targetFieldMatchConditionID = 1;
            _title = $"{Tag}-{_baseClassName}";
            _dataSource = $"{_title}DS";

            _matchConditionsTable = new DataTable("MatchConditionTable");
            _matchConditionsTable.Columns.Add("ID", typeof(int));
            _matchConditionsTable.Columns.Add("Name");

            var newRow = _matchConditionsTable.NewRow();
            newRow["ID"] = 1;
            newRow["Name"] = "First_Split";
            _matchConditionsTable.Rows.Add(newRow);

            newRow = _matchConditionsTable.NewRow();
            newRow["ID"] = 2;
            newRow["Name"] = "Second_Split";
            _matchConditionsTable.Rows.Add(newRow);

            newRow = _matchConditionsTable.NewRow();
            newRow["ID"] = 3;
            newRow["Name"] = "Third_Split";
            _matchConditionsTable.Rows.Add(newRow);

            newRow = _matchConditionsTable.NewRow();
            newRow["ID"] = 4;
            newRow["Name"] = "Value";
            _matchConditionsTable.Rows.Add(newRow);

            //_sessionItems = new List<SessionItem>
            //{
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_searchFieldsTable", Value = _searchFieldsTable, Type = typeof(DataTable), Serialize = true
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_baseClassName", Value = _baseClassName, Type = typeof(string), Serialize = false
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_targetTableID", Value = _targetTableID, Type = typeof(int), Serialize = true
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_targetTableValue", Value = _targetTableValue, Type = typeof(string), Serialize = false
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_targetFieldValue", Value = _targetFieldValue, Type = typeof(string), Serialize = false
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_targetFieldMatchConditionID", Value = _targetFieldMatchConditionID, Type = typeof(int), Serialize = true
            //    },
            //    new SessionItem()
            //    {
            //        Key = $"{Tag}_targetFieldMatchConditionValue", Value = _targetFieldMatchConditionValue, Type = typeof(string), Serialize = false
            //    }
            //};


            await Task.CompletedTask;
        }

        private async Task RetrieveDataFromSessionTableAsync()
        {
            //try
            //{
            //    if (!_isRetrieved && _sessionItems != null)
            //    {
            //        _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);

            //        _searchFieldsTable = (DataTable?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_searchFieldsTable"))?.Value ?? _searchFieldsTable;
            //        _baseClassName = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_baseClassName"))?.Value?.ToString() ?? string.Empty;
            //        _targetTableID = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_targetTableID"))?.Value?.ToString(), out int selectedTableIDResult) ? selectedTableIDResult : 1;
            //        _targetTableValue = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_targetTableValue"))?.Value?.ToString() ?? string.Empty;
            //        _targetFieldValue = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_targetFieldValue"))?.Value?.ToString() ?? string.Empty;
            //        _targetFieldMatchConditionID = int.TryParse(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_targetFieldMatchConditionID"))?.Value?.ToString(), out int _targetFieldMatchConditionIDResult) ? _targetFieldMatchConditionIDResult : 1;
            //        _targetFieldMatchConditionValue = _sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Tag}_targetFieldMatchConditionValue"))?.Value?.ToString() ?? string.Empty;

            //        if (_searchFieldsTable != null)
            //        {
            //            _searchFieldsTable.TableName = _baseClassName;
            //        }

            //        _targetTableID = _targetTableID == 0 ? 1 : _targetTableID;
            //        _targetFieldMatchConditionID = _targetFieldMatchConditionID == 0 ? 1 : _targetFieldMatchConditionID;
            //        _title = $"{Tag}-{_baseClassName}";
            //        _dataSource = $"{_title}DS";
            //        _isRetrieved = true;
            //        GetMatchCondition();
            //        StateHasChanged();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error: {0}", ex.Message);
            //}

            await Task.CompletedTask;
        }

        private async Task OnLookupTableValueChanged(ChangeEventArgs e)
        {
            // Parse selected value from int to TableEnum
            int targetIntValue = int.Parse(e?.Value?.ToString() ?? string.Empty);
            _targetTableID = targetIntValue;

            _targetTableValue = TableList?.FirstOrDefault(t => t.ID == _targetTableID)?.TableName ?? string.Empty;

            // Reset selected field value when table value changes
            _targetFieldValue = string.Empty;

            // Handle the selected value here
            Console.WriteLine("targetTableID: {0}", _targetTableID);
            StateHasChanged();

            //await _sessionManager.SaveToSessionTableAsync($"{Tag}_targetTableID", _targetTableID, serialize: true);
            //await _sessionManager.SaveToSessionTableAsync($"{Tag}_targetTableValue", _targetTableValue);

            await Task.CompletedTask;
        }

        private async Task OnTargetFieldValueChanged(ChangeEventArgs e)
        {
            _targetFieldValue = e?.Value?.ToString() ?? string.Empty;
            StateHasChanged();

            //await _sessionManager.SaveToSessionTableAsync($"{Tag}_targetFieldValue", _targetFieldValue);
            await Task.CompletedTask;
        }

        private async Task OnMatchConditionsValueChanged(ChangeEventArgs e)
        {
            int targetFieldMatchConditionID = int.Parse(e?.Value?.ToString() ?? string.Empty);
            _targetFieldMatchConditionID = targetFieldMatchConditionID;

            GetMatchCondition();
            StateHasChanged();
            //await _sessionManager.SaveToSessionTableAsync($"{Tag}_targetFieldMatchConditionValue", _targetFieldMatchConditionValue);
            await Task.CompletedTask;
        }

        private void GetMatchCondition()
        {
            var targetFieldMatchConditionNames = Enum.GetNames(typeof(LookupFieldConditionEnum));
            if (targetFieldMatchConditionNames != null)
            {
                int matchCondition = 0;
                foreach (string fieldName in targetFieldMatchConditionNames)
                {
                    matchCondition++;
                    if (matchCondition == _targetFieldMatchConditionID)
                    {
                        _targetFieldMatchConditionValue = fieldName;
                        break;
                    }
                }
            }
        }

        private IEnumerable<string>? GetFieldValues(int id)
        {
            var selectedTable = TableList?.FirstOrDefault(t => t.ID == id);
            var properties = selectedTable?.GetPropertyNames();

            return properties;

        }

        private async Task SaveLookupTableItemAsync()
        {
            if (_searchFieldsTable == null)
            {
                _searchFieldsTable = new DataTable(_baseClassName);
                _searchFieldsTable.Columns.Add("TableName");
                _searchFieldsTable.Columns.Add("FieldName");
                _searchFieldsTable.Columns.Add("MatchCondition", typeof(int));
            }

            var newRow = _searchFieldsTable.NewRow();
            newRow["TableName"] = _targetTableValue;
            newRow["FieldName"] = _targetFieldValue;
            newRow["MatchCondition"] = _targetFieldMatchConditionID;

            _searchFieldsTable.Rows.Add(newRow);

            //await _sessionManager.SaveToSessionTableAsync($"{Tag}_searchFieldsTable", _searchFieldsTable, serialize: true);

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task SaveAsync()
        {
            var targetTables = new List<SearchField>();
            // if (_targetTableColumnList != null)
            // {
            //     var targetTableGroups = _targetTableColumnList?
            //                             .Where(t => !string.IsNullOrEmpty(t.TargetTableName))
            //                             .GroupBy(t => t.TargetTableName)
            //                             .ToList();

            //     if (targetTableGroups != null)
            //     {
            //         foreach (var group in targetTableGroups)
            //         {
            //             var targetTableName = group.Key;
            //             var targetTableColumns = group.ToList();

            //             var targetTable = new TargetTable
            //                 {
            //                     TargetTableName = targetTableName,
            //                     TargetTableColumns = targetTableColumns,
            //                 };

            //             var dt = new DataTable(targetTableName);

            //             // Build columns for the target DataTable
            //             dt = BuildColumnsForTargetTable(dt, targetTableColumns, SelectedData);

            //             // Populate rows for the target DataTable
            //             dt = BuildRowsForTargetTable(dt, targetTableColumns, SelectedData);

            //             // Lastly, before adding the targetTable serialize the dt again and store in targetTable.DT
            //             targetTable.DT = await _sessionManager.SerializeAsync(dt);

            //             targetTables.Add(targetTable);
            //         }
            //     }
            // }        // if (_targetTableColumnList != null)
            // {
            //     var targetTableGroups = _targetTableColumnList?
            //                             .Where(t => !string.IsNullOrEmpty(t.TargetTableName))
            //                             .GroupBy(t => t.TargetTableName)
            //                             .ToList();

            //     if (targetTableGroups != null)
            //     {
            //         foreach (var group in targetTableGroups)
            //         {
            //             var targetTableName = group.Key;
            //             var targetTableColumns = group.ToList();

            //             var targetTable = new TargetTable
            //                 {
            //                     TargetTableName = targetTableName,
            //                     TargetTableColumns = targetTableColumns,
            //                 };

            //             var dt = new DataTable(targetTableName);

            //             // Build columns for the target DataTable
            //             dt = BuildColumnsForTargetTable(dt, targetTableColumns, SelectedData);

            //             // Populate rows for the target DataTable
            //             dt = BuildRowsForTargetTable(dt, targetTableColumns, SelectedData);

            //             // Lastly, before adding the targetTable serialize the dt again and store in targetTable.DT
            //             targetTable.DT = await _sessionManager.SerializeAsync(dt);

            //             targetTables.Add(targetTable);
            //         }
            //     }
            // }

            await OnSave.InvokeAsync(targetTables);
        }

        private async Task CloseAsync()
        {
            await OnClose.InvokeAsync();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            try 
            {
                int sequence = 0;

                // PageTitle
                builder.OpenElement(sequence++, "PageTitle");
                builder.AddContent(sequence++, "Search Field Entry Modal");
                builder.CloseElement();

                // Modal container
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", $"data-table-grid-modal {(ShowSearchFieldsModal ? "show" : "")}");

                // Buttons
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "data-table-grid-modal-close-button");

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", IconName.CheckCircleFill);
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, SaveAsync));
                builder.AddAttribute(sequence++, "title", "Add");
                builder.CloseComponent();

                builder.OpenComponent<Icon>(sequence++);
                builder.AddAttribute(sequence++, "Name", IconName.XCircleFill);
                builder.AddAttribute(sequence++, "title", "Close");
                builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, CloseAsync));
                builder.CloseComponent();

                builder.CloseElement();

                // Modal content
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "data-table-grid-modal-content");

                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "row");
                /* 
               Do not remove this commented code below because this is an option if you want to use Custom TableColumn usage.
               See ref: Custom DataTable Usage

               <div class="row">
                   <div class="col">
                       <h1>Set Target Table</h1>
                   </div>
               </div>
               <div class="row">
                   <div class="col">
                       <label for="_targetTableID">Lookup Table:</label>
                       <select class="form-control" id="_targetTableID" @onchange="OnLookupTableValueChanged">
                           @if (TableList != null)
                           {
                               foreach (var table in TableList)
                               {
                                   int id = (int)table.ID;
                                   string tableName = table.TableName;
                                   <option value="@id" selected="@(_targetTableID.Equals(id))">
                                       @tableName
                                   </option>
                               }
                           }
                       </select>
                   </div>
                   <div class="col">
                       <label for="_targetFieldValue">Target Field:</label>
                       <select class="form-control" id="_targetFieldValue" @onchange="OnTargetFieldValueChanged">
                           @{
                               var targetFieldValues = GetFieldValues(_targetTableID);
                               if (targetFieldValues != null)
                               {
                                   foreach (var field in targetFieldValues)
                                   {
                                       <option value="@field" selected="@(_targetFieldValue?.Equals(field))">@field</option>
                                   }
                               }
                           }
                       </select>
                   </div>
                   <div class="col">
                       <label for="_targetFieldMatchConditionValue">Match Conditions:</label>
                       <select class="form-control" id="_targetFieldMatchConditionValue" @onchange="OnMatchConditionsValueChanged">
                           @{
                               var targetFieldMatchConditionValues = Enum.GetValues(typeof(LookupFieldConditionEnum));
                               if (targetFieldMatchConditionValues != null)
                               {
                                   int matchConditionID = 0;
                                   foreach (var field in targetFieldMatchConditionValues)
                                   {
                                       matchConditionID++;
                                       <option value="@matchConditionID" selected="@(_targetFieldMatchConditionID.Equals(matchConditionID))">@field</option>
                                   }
                               }
                           }
                       </select>
                   </div>
                   <div class="col">
                       <Icon Name="IconName.Floppy" Class="cursor-pointer" @onclick="SaveLookupTableItemAsync" title="Save" />
                   </div>
               </div> 
            */
                if (_searchFieldsTable != null)
                {
                    // TableGrid
                    //builder.OpenComponent<TableGrid>(sequence++);
                    //builder.AddAttribute(sequence++, "Title", _title);

                    //builder.OpenComponent<TableSource>(sequence++);
                    //builder.AddAttribute(sequence++, "Name", _dataSource);
                    //builder.AddAttribute(sequence++, "DataSource", _searchFieldsTable);
                    //builder.CloseComponent();

                    //builder.OpenComponent<TableSource>(sequence++);
                    //builder.AddAttribute(sequence++, "Name", "MatchConditionDS");
                    //builder.AddAttribute(sequence++, "DataSource", _matchConditionsTable);
                    //builder.CloseComponent();

                    //builder.OpenComponent<TableNode>(sequence++);
                    //builder.AddAttribute(sequence++, "DataSource", _dataSource);

                    //builder.OpenComponent<TableColumn>(sequence++);
                    //builder.AddAttribute(sequence++, "DataSourceName", _dataSource);
                    //builder.AddAttribute(sequence++, "FieldName", "TableName");
                    //builder.AddAttribute(sequence++, "Type", "TextBox");
                    //builder.AddAttribute(sequence++, "HeaderName", "Table Name");
                    //builder.CloseComponent();

                    //builder.OpenComponent<TableColumn>(sequence++);
                    //builder.AddAttribute(sequence++, "DataSourceName", _dataSource);
                    //builder.AddAttribute(sequence++, "FieldName", "FieldName");
                    //builder.AddAttribute(sequence++, "Type", "TextBox");
                    //builder.AddAttribute(sequence++, "HeaderName", "Field Name");
                    //builder.CloseComponent();

                    //builder.OpenComponent<TableColumn>(sequence++);
                    //builder.AddAttribute(sequence++, "DataSourceName", "MatchConditionDS");
                    //builder.AddAttribute(sequence++, "FieldName", "MatchCondition");
                    //builder.AddAttribute(sequence++, "Type", "DropdownList");
                    //builder.AddAttribute(sequence++, "DisplayFieldName", "Name");
                    //builder.AddAttribute(sequence++, "DisplayFieldValue", "ID");
                    //builder.AddAttribute(sequence++, "HeaderName", "Match Condition");
                    //builder.CloseComponent();

                    //builder.CloseComponent(); // Close TableNode

                    //builder.CloseComponent(); // Close TableGrid
                }

                builder.CloseElement(); // Close row div
                builder.CloseElement(); // Close modal content div
                builder.CloseElement(); // Close modal container div
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
           
        }
    }
}

