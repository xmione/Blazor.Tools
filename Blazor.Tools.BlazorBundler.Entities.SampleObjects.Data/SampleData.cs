﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Bogus;
using System.Data;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data
{
    public class SampleData : ATableGridData
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        private bool _isFirstCellClicked = true;
        private string _startCell = string.Empty;
        private string _endCell = string.Empty;
        private int _startRow;
        private int _endRow;
        private int _startCol;
        private int _endCol;

        private Employee _employee;

        public Employee Employee
        {
            get { return _employee; }
            set { _employee = value; }
        }
        
        private IViewModel<IBase, IModelExtendedProperties> _employeeVM;

        public IViewModel<IBase, IModelExtendedProperties> EmployeeVM
        {
            get { return _employeeVM; }
            set { _employeeVM = value; }
        }
        
        private Country _country;

        public Country Country
        {
            get { return _country; }
            set { _country = value; }
        }
        
        private IViewModel<IBase, IModelExtendedProperties> _countryVM;

        public IViewModel<IBase, IModelExtendedProperties> CountryVM
        {
            get { return _countryVM; }
            set { _countryVM = value; }
        }

        private List<IViewModel<IBase, IModelExtendedProperties>> _employees;

        public List<IViewModel<IBase, IModelExtendedProperties>> Employees {
            get { return _employees; }
            set { _employees = value; }
        }

        private List<IViewModel<IBase, IModelExtendedProperties>> _countries;

        public List<IViewModel<IBase, IModelExtendedProperties>> Countries
        {
            get { return _countries; }
            set { _countries = value; }
        }
        
        private DataTable _employeeDataTable;

        public DataTable EmployeeDataTable
        {
            get { return _employeeDataTable; }
            set { _employeeDataTable = value; }
        }
        
        private DataTable _countryDataTable;

        public DataTable CountryDataTable
        {
            get { return _countryDataTable; }
            set { _countryDataTable = value; }
        }

        private List<string> _hiddenEmployeeColumns;
        
        public List<string> HiddenEmployeeColumns
        {
            get { return _hiddenEmployeeColumns; }
            set { _hiddenEmployeeColumns = value; }
        }

        private List<string> _hiddenCountryColumns;

        public List<string> HiddenCountryColumns
        {
            get { return _hiddenCountryColumns; }
            set { _hiddenCountryColumns = value; }
        }

        private Dictionary<string, string> _employeeHeaderNames;
        
        public Dictionary<string, string> EmployeeHeaderNames
        {
            get { return _employeeHeaderNames; }
            set { _employeeHeaderNames = value; }
        }

        private Dictionary<string, string> _countryHeaderNames;

        public Dictionary<string, string> CountryHeaderNames
        {
            get { return _countryHeaderNames; }
            set { _countryHeaderNames = value; }
        }

        private string _modelsAssemblyName;

        public string ModelsAssemblyName
        {
            get { return _modelsAssemblyName; }
            set { _modelsAssemblyName = value; }
        }
        
        private string _viewModelsAssemblyName;

        public string ViewModelsAssemblyName
        {
            get { return _viewModelsAssemblyName; }
            set { _viewModelsAssemblyName = value; }
        }

        private List<AssemblyTable>? _tableList;

        public List<AssemblyTable>? TableList
        {
            get { return _tableList; }
            set { _tableList = value; }
        }

        public SampleData()
        {
            HostAssemblies.LoadAssemblyFromDLLFile = true; // Default value should be true;
            HostAssemblies.IsInterface = true; // Default value should be true;
            Create();
        }

        /// <summary>
        /// Creates an instance of the SampleData with options for loading the assembly needed to create the TableList.
        /// </summary>
        /// <param name="loadAssemblyFromDLLFile">(bool) - Loads the assembly from dll file path, otherwise, loads it from the assembly name.</param>
        public SampleData(bool loadAssemblyFromDLLFile)
        {
            HostAssemblies.LoadAssemblyFromDLLFile = loadAssemblyFromDLLFile;
            HostAssemblies.IsInterface = true;

            Create();
        }

        private void Create()
        {
            Title = "Employee List";
            TableID = "employee";
            DataSources = new Dictionary<string, object>();

            Items = new List<EmployeeVM>();
            _employee = new Employee();
            _country = new Country();
            _employeeVM = new EmployeeVM(new ContextProvider());
            _countryVM = new CountryVM(new ContextProvider());
            _employees = new List<IViewModel<IBase, IModelExtendedProperties>>();
            _countries = new List<IViewModel<IBase, IModelExtendedProperties>>();

            _employeeDataTable = new DataTable("Employee");
            _employeeDataTable.Columns.Add("ID", typeof(int));
            _employeeDataTable.Columns.Add("FirstName", typeof(string));
            _employeeDataTable.Columns.Add("MiddleName", typeof(string));
            _employeeDataTable.Columns.Add("LastName", typeof(string));
            _employeeDataTable.Columns.Add("DateOfBirth", typeof(DateOnly));
            _employeeDataTable.Columns.Add("CountryID", typeof(int));

            _countryDataTable = new DataTable("Country");
            _countryDataTable.Columns.Add("ID", typeof(int));
            _countryDataTable.Columns.Add("Name", typeof(string));

            _hiddenEmployeeColumns = new List<string>();
            _hiddenCountryColumns = new List<string>();
            _employeeHeaderNames = default!;
            _countryHeaderNames = default!;
            _modelsAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            _viewModelsAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels";
            _tableList = new List<AssemblyTable>();
            CreateTableColumnDefinitions();
            CreateDummyData();
            CreateAssemblyTableList();
        }

        private void CreateTableColumnDefinitions()
        {
            ColumnDefinitions = new List<TableColumnDefinition>
            {
                new TableColumnDefinition
                {
                    ColumnName = "ID",
                    HeaderText = "ID",
                    ColumnType = typeof(int),
                    CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, (EmployeeVM)vm, rowIndex)
                },
                new TableColumnDefinition
                {
                    ColumnName = "FirstName",
                    HeaderText = "First Name",
                    ColumnType = typeof(string),
                    CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, (EmployeeVM)vm, rowIndex),
                    ValueChanged = new Action<object, EmployeeVM>(OnFirstNameValueChanged)
                },
                new TableColumnDefinition
                {
                    ColumnName = "MiddleName",
                    HeaderText = "Middle Name",
                    ColumnType = typeof(string),
                    CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, (EmployeeVM)vm, rowIndex),
                    ValueChanged = new Action<object, EmployeeVM>(OnMiddleNameValueChanged)
                },
                new TableColumnDefinition
                {
                    ColumnName = "LastName",
                    HeaderText = "Last Name",
                    ColumnType = typeof(string),
                    CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, (EmployeeVM)vm, rowIndex),
                    ValueChanged = new Action<object, EmployeeVM>(OnLastNameValueChanged)
                },
                new TableColumnDefinition
                {
                    ColumnName = "DateOfBirth",
                    HeaderText = "DateOfBirth",
                    ColumnType = typeof(DateOnly?),
                    CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, (EmployeeVM)vm, rowIndex),
                    ValueChanged = new Action<DateOnly?, EmployeeVM>(OnDateOfBirthValueChanged)
                },
                new TableColumnDefinition
                {
                    Items = _countries,
                    ColumnName = "CountryID",
                    HeaderText = "Country",
                    OptionIDFieldName="ID",
                    OptionValueFieldName="Name",
                    ColumnType = typeof(IEnumerable<CountryVM>),
                    CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, (EmployeeVM)vm, rowIndex),
                    ValueChanged = new Action<object, EmployeeVM>(OnDropdownValueChanged)
                }

            };
        }

        private void CreateDummyData()
        {
            var employeeFaker = new Faker<EmployeeVM>()
               .RuleFor(e => e.ID, f => f.IndexFaker + 1)
               //.RuleFor(e => e.RowID, f => f.IndexFaker + 1)
               .RuleFor(e => e.FirstName, f => f.Name.FirstName())
               .RuleFor(e => e.MiddleName, f => f.Name.FirstName().Substring(0, 1) + ".")
               .RuleFor(e => e.LastName, f => f.Name.LastName())
               .RuleFor(e => e.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(50, DateTime.Now.AddYears(-18))))
               .RuleFor(e => e.CountryID, f => f.Random.Int(1, 2));

            var countryFaker = new Faker<CountryVM>()
            //.RuleFor(e => e.RowID, f => f.IndexFaker + 1)
            .RuleFor(c => c.ID, f => f.IndexFaker + 1)
            .RuleFor(c => c.Name, f => f.Address.Country());

            for (int i = 0; i < 101; i++)
            {
                var employee = employeeFaker.Generate();
                _employees.Add(employee);
                _employeeDataTable.Rows.Add(i + 1, employee.FirstName, employee.MiddleName, employee.LastName, employee.DateOfBirth, employee.CountryID);
            }

            // Initialize and generate country data
            for (int i = 0; i < 101; i++)
            {
                var country = countryFaker.Generate();
                _countries.Add(country);
                _countryDataTable.Rows.Add(i + 1, country.Name);
            }

            DataSources.Add("EmployeeDS", _employees);
            DataSources.Add("CountryDS", _countries);

            _employees = Items.Cast<IViewModel<IBase, IModelExtendedProperties>>().ToList();

            _hiddenEmployeeColumns.Add("ID");
            _hiddenCountryColumns.Add("ID");

            _employeeHeaderNames = new Dictionary<string, string>()
            {
                ["ID"] = "ID",
                ["FirstName"] = "First Name",
                ["MiddleName"] = "Middle Name",
                ["LastName"] = "Last Name",
                ["CountryID"] = "Country",
            };

            _countryHeaderNames = new Dictionary<string, string>()
            {
                ["ID"] = "ID",
                ["Name"] = "Country Name",
            };
        }

        private void CreateAssemblyTableList()
        {
            /*
                Note: These are the important parameters that you need to supply to create the Assembly Table List.
                      These are used to get the model classes that is used to update the database files.
             */

            // Get the assembly where the SampleData class is defined
            Assembly sampleDataAssembly = typeof(SampleData).Assembly;

            // Get the path of that assembly
            string assemblyPath = sampleDataAssembly.Location ?? string.Empty;
            string currentDLLPath = Path.GetDirectoryName(assemblyPath)!;
            HostAssemblies.ModelsAssemblyName = "AccSol.Interfaces";
            HostAssemblies.ModelsAssemblyPath = Path.Combine(currentDLLPath, @"DLLs\net8.0\AccSol.Interfaces.dll");
            HostAssemblies.ServicesAssemblyName = "AccSol.Services";
            HostAssemblies.ServicesAssemblyPath = Path.Combine(currentDLLPath, @"DLLs\net8.0\AccSol.Services.dll");

            Assembly? modelsAssembly;
            Assembly? servicesAssembly;

            if (HostAssemblies.LoadAssemblyFromDLLFile)
            {
                // Load assembly from Assembly DLL File
                modelsAssembly = HostAssemblies.ModelsAssemblyPath.LoadAssemblyFromDLLFile();
                servicesAssembly = HostAssemblies.ServicesAssemblyPath.LoadAssemblyFromDLLFile();
            }
            else 
            {
                // Load assembly from Assembly Name
                modelsAssembly = HostAssemblies.ModelsAssemblyPath.LoadAssemblyFromName();
                servicesAssembly = HostAssemblies.ServicesAssemblyPath.LoadAssemblyFromName();
            }

            var interfaceNames = modelsAssembly?.GetAssemblyInterfaceNames().ToList();
            HostAssemblies.ModelsAssemblyName = HostAssemblies.ModelsAssemblyName ?? modelsAssembly?.GetName().Name ?? string.Empty;
            HostAssemblies.ServicesAssemblyName = HostAssemblies.ServicesAssemblyName ?? servicesAssembly?.GetName().Name ?? string.Empty;
            
            if (interfaceNames != null)
            {
                for (int i = 0; i < interfaceNames.Count(); i++)
                {
                    var iFace = interfaceNames[i];
                    var assemblyTable = new AssemblyTable()
                    {
                        ID = i + 1,
                        AssemblyName = HostAssemblies.ModelsAssemblyName,
                        AssemblyPath = HostAssemblies.ModelsAssemblyPath,
                        ServiceName = HostAssemblies.ServicesAssemblyName,
                        ServicePath = HostAssemblies.ServicesAssemblyPath,
                        LoadAssemblyFromDLLFile = HostAssemblies.LoadAssemblyFromDLLFile,
                        IsInterface = HostAssemblies.IsInterface,
                        TableName = iFace.Item2.Substring(1, iFace.Item2.Length - 1),
                        TypeName = iFace.Item2
                    };

                    _tableList?.Add(assemblyTable);
                }
            }
        }

        public void OnIDValueChanged(object newValue, IViewModel<IBase, IModelExtendedProperties> employeeVM)
        {
            var newIdValue = int.Parse(newValue?.ToString() ?? "0");
            
            // Update employee list
            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.SetValue("ID", newIdValue);
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnFirstNameValueChanged(object newValue, EmployeeVM employeeVM)
        {
            var firstName = newValue?.ToString() ?? string.Empty;
            
            // Update employee list
            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.SetValue("FirstName", firstName);
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnMiddleNameValueChanged(object newValue, EmployeeVM employeeVM)
        {
            var middleName = newValue?.ToString() ?? string.Empty;
            
            // Update employee list
            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.SetValue("MiddleName", middleName);
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnLastNameValueChanged(object newValue, EmployeeVM employeeVM)
        {
            var lastName = newValue?.ToString() ?? string.Empty;

            // Update employee list
            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.SetValue("LastName", lastName);
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnDateOfBirthValueChanged(DateOnly? newValue, EmployeeVM employeeVM)
        {
            var dateOfBirth = newValue.GetValueOrDefault();

            // Update employee list
            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.SetValue("DateOfBirth", dateOfBirth);
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method
        }

        public void OnDropdownValueChanged(object newValue, EmployeeVM employeeVM)
        {
            var countryID = Convert.ToInt32(newValue);
            
            // Update employee list
            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.SetValue("CountryID", countryID);
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public async Task HandleCellClickAsync(string id, EmployeeVM employee, int column)
        {
            if (employee.IsEditMode)
            {
                return;
            }

            string cellIdentifier = $"R{employee.RowID}C{column}";
            var startCellID = $"{TableID}-start-cell";
            var endCellID = $"{TableID}-end-cell";
            // get the values from the hidden input fields first
            _startCell = await JSRuntime.InvokeAsync<string>("getValue", startCellID);
            _endCell = await JSRuntime.InvokeAsync<string>("getValue", endCellID);

            bool areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);
            if (string.IsNullOrEmpty(_startCell) || _isFirstCellClicked)
            {
                _startRow = employee.RowID;
                _startCol = column;
                _startCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : false;

                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-row", $"{_startRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-col", $"{_startCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-start-cell", cellIdentifier);
            }
            else
            {
                _endRow = employee.RowID;
                _endCol = column;
                _endCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : true;

                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-row", $"{_endRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-col", $"{_endCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{TableID}-end-cell", cellIdentifier);

            }

            // you need to ask again because it already has ran through some codes that might affect its value.
            areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);

            if (areBothFilled)
            {
                var totalRows = _employees.Count;
                var totalCols = ColumnDefinitions?.Count;
                // Now that they are both filled with values, mark them
                await JSRuntime.InvokeVoidAsync("toggleCellBorders", _startRow, _endRow, _startCol, _endCol, totalRows, totalCols, TableID, true);
            }

            //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeIsFirstCellClicked", _nodeIsFirstCellClicked, serialize: true);

            //StateHasChanged(); this should be triggered on the calling program after calling this method

            await Task.CompletedTask;
        }
    }
}
