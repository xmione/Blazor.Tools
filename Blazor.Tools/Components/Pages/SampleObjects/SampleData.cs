using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using Bogus;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Tools.Components.Pages.SampleObjects
{
    public class SampleData: ComponentBase
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        private Dictionary<string, object> _dataSources = default!;
        private List<EmployeeVM> _employees = new List<EmployeeVM>();
        private List<CountryVM> _countries = new List<CountryVM>();
        private List<string>? _hiddenEmployeeColumns;
        private List<string>? _hiddenCountryColumns;
        private EmployeeVM _employeeVM = new EmployeeVM(new ContextProvider());
        private CountryVM? _selectedCountry;
        private string _title = "Employee List";
        private string _tableID = "employee";
        private bool _isFirstCellClicked = true;
        private string _startCell = string.Empty;
        private string _endCell = string.Empty;
        private int _startRow;
        private int _endRow;
        private int _startCol;
        private int _endCol;

        private List<TableColumnDefinition> _columnDefinitions = default!;

        public string Title
        {
            get{ return _title; }
            private set{ _title = value; }
        }
        
        public string TableID
        {
            get{ return _tableID; }
            private set{ _tableID = value; }
        }

        public List<TableColumnDefinition> ColumnDefinitions 
        {
            get { return _columnDefinitions; }
            private set { _columnDefinitions = value; }
        }

        public EmployeeVM EmployeeVM
        {
            get { return _employeeVM; }
            private set { _employeeVM = value; }
        }
        
        public CountryVM? SelectedCountry
        {
            get { return _selectedCountry; }
            private set { _selectedCountry = value; }
        }
        
        public List<EmployeeVM> Employees 
        {
            get { return _employees; }
            private set { _employees = value; }
        }
        
        public List<CountryVM> Countries 
        {
            get { return _countries; }
            private set { _countries = value; }
        }

        public SampleData() 
        {
            CreateTableColumnDefinitions();
            CreateDummyData();
        }

        private void CreateTableColumnDefinitions()
        {
            var columnDefinitions = new List<TableColumnDefinition>
            {
                new TableColumnDefinition
                {
                    ColumnName = "ID",
                    HeaderText = "ID",
                    ColumnType = typeof(int)
                },
                new TableColumnDefinition
                {
                    ColumnName = "FirstName",
                    HeaderText = "First Name",
                    ColumnType = typeof(string),
                    ValueChanged = new Action<object, EmployeeVM>(OnFirstNameValueChanged)
                },
                new TableColumnDefinition
                {
                    ColumnName = "MiddleName",
                    HeaderText = "Middle Name",
                    ColumnType = typeof(string),
                    ValueChanged = new Action<object, EmployeeVM>(OnMiddleNameValueChanged)
                },
                new TableColumnDefinition
                {
                    ColumnName = "LastName",
                    HeaderText = "Last Name",
                    ColumnType = typeof(string),
                    ValueChanged = new Action<object, EmployeeVM>(OnLastNameValueChanged)
                },
                new TableColumnDefinition
                {
                    Items = _countries,
                    ColumnName = "CountryID",
                    HeaderText = "Country",
                    OptionIDFieldName="ID",
                    OptionValueFieldName="Name",
                    ColumnType = typeof(IEnumerable<CountryVM>),
                    ValueChanged = new Action<object, EmployeeVM>(OnIDValueChanged)
                }

            };

            _columnDefinitions = columnDefinitions;
        }

        private void CreateDummyData()
        {
            var employeeFaker = new Faker<EmployeeVM>()
               .RuleFor(e => e.ID, f => f.IndexFaker + 1)
               .RuleFor(e => e.RowID, f => f.IndexFaker + 1)
               .RuleFor(e => e.FirstName, f => f.Name.FirstName())
               .RuleFor(e => e.MiddleName, f => f.Name.FirstName().Substring(0, 1) + ".")
               .RuleFor(e => e.LastName, f => f.Name.LastName())
               .RuleFor(e => e.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(50, DateTime.Now.AddYears(-18))))
               .RuleFor(e => e.CountryID, f => f.Random.Int(1, 2));

            var countryFaker = new Faker<CountryVM>()
            .RuleFor(e => e.RowID, f => f.IndexFaker + 1)
            .RuleFor(c => c.ID, f => f.IndexFaker + 1)
            .RuleFor(c => c.Name, f => f.Address.Country());

            for (int i = 0; i < 101; i++)
            {
                var employee = employeeFaker.Generate();
                _employees.Add(employee);
            }

            // Initialize and generate country data
            for (int i = 0; i < 101; i++)
            {
                var country = countryFaker.Generate();
                _countries.Add(country);
            }

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

        public void OnItemsChanged(IEnumerable<IBaseVM> updatedItems)
        {
            _employees = ((IEnumerable<EmployeeVM>)updatedItems).ToList();
        }

        public async Task HandleCellClickAsync(string id, EmployeeVM employee, int column)
        {
            string cellIdentifier = $"R{employee.RowID}C{column}";
            var startCellID = $"{_tableID}-start-cell";
            var endCellID = $"{_tableID}-end-cell";
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

                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-start-row", $"{_startRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-start-col", $"{_startCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-start-cell", cellIdentifier);
            }
            else
            {
                _endRow = employee.RowID;
                _endCol = column;
                _endCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : true;

                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-end-row", $"{_endRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-end-col", $"{_endCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-end-cell", cellIdentifier);

            }

            // you need to ask again because it already has ran through some codes that might affect its value.
            areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);

            if (areBothFilled)
            {
                var totalRows = _employees.Count;
                var totalCols = _columnDefinitions?.Count;
                // Now that they are both filled with values, mark them
                await JSRuntime.InvokeVoidAsync("toggleCellBorders", _startRow, _endRow, _startCol, _endCol, totalRows, totalCols, _tableID, true);
            }

            //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeStartCell", _nodeStartCell, serialize: false);
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeEndCell", _nodeEndCell, serialize: false);
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_nodeIsFirstCellClicked", _nodeIsFirstCellClicked, serialize: true);

            //StateHasChanged(); this should be triggered on the calling program after calling this method

            await Task.CompletedTask;
        }

    }
}
