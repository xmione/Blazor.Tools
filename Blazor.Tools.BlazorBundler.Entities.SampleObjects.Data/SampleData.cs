using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Bogus;
using System.Data;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data
{
    public class SampleData : ATableGridData
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        private Dictionary<string, object> _dataSources = default!;

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
        
        private EmployeeVM _employeeVM;

        public EmployeeVM EmployeeVM
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
        
        private CountryVM _countryVM;

        public CountryVM CountryVM
        {
            get { return _countryVM; }
            set { _countryVM = value; }
        }

        private List<EmployeeVM> _employees;
        public List<EmployeeVM> Employees {
            get { return _employees; }
            set { _employees = value; }
        }

        private List<CountryVM> _countries;
        public List<CountryVM> Countries
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

        public SampleData()
        {
            Title = "Employee List";
            TableID = "employee";
            DataSources = new Dictionary<string, object>();

            Items = new List<EmployeeVM>();
            _employee = new Employee();
            _country = new Country();
            _employeeVM = new EmployeeVM(new ContextProvider());
            _countryVM = new CountryVM(new ContextProvider());
            _employees = new List<EmployeeVM>();
            _countries = new List<CountryVM>();
            
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

            CreateTableColumnDefinitions();
            CreateDummyData();
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

            _employees = Items.Cast<EmployeeVM>().ToList();

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

        public void OnIDValueChanged(object newValue, EmployeeVM employeeVM)
        {
            employeeVM.ID = int.Parse(newValue?.ToString() ?? "0");
            // Update employee list

            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.ID = employeeVM.ID;
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnFirstNameValueChanged(object newValue, EmployeeVM employeeVM)
        {
            employeeVM.FirstName = newValue?.ToString() ?? string.Empty;
            // Update employee list

            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.FirstName = employeeVM.FirstName;
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnMiddleNameValueChanged(object newValue, EmployeeVM employeeVM)
        {
            employeeVM.MiddleName = newValue?.ToString() ?? string.Empty;
            // Update employee list

            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.MiddleName = employeeVM.MiddleName;
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnLastNameValueChanged(object newValue, EmployeeVM employeeVM)
        {
            employeeVM.LastName = newValue?.ToString() ?? string.Empty;
            // Update employee list

            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.LastName = employeeVM.LastName;
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method

        }

        public void OnDateOfBirthValueChanged(DateOnly? newValue, EmployeeVM employeeVM)
        {
            employeeVM.DateOfBirth = newValue.GetValueOrDefault();

            // Update employee list

            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.DateOfBirth = employeeVM.DateOfBirth;
            }

            //StateHasChanged(); this should be triggered on the calling program after calling this method
        }

        public void OnDropdownValueChanged(object newValue, EmployeeVM employeeVM)
        {
            employeeVM.CountryID = Convert.ToInt32(newValue);
            // Update employee list

            var foundEmployee = _employees.FirstOrDefault(e => e.RowID == employeeVM.RowID);

            if (foundEmployee != null)
            {
                foundEmployee.CountryID = employeeVM.CountryID;
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
