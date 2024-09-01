/*====================================================================================================
    Class Name  : EmployeeVM.cs
    Created By  : Solomio S. Sisante
    Created On  : August 15, 2024
    Purpose     : To provide a view model class for the Employee class.
  ====================================================================================================*/

using Blazor.Tools.BlazorBundler.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects
{
    public class EmployeeVM : Employee, IValidatableObject, ICloneable<EmployeeVM>, IViewModel<Employee, IModelExtendedProperties>
    {
        private List<EmployeeVM> _employees;

        private readonly IContextProvider _contextProvider;

        public int _rowID;

        public bool _isEditMode;

        public bool _isVisible;

        public int _startCell;

        public int _endCell;

        public bool _isFirstCellClicked;

        public int RowID
        {
            get
            {
                return _rowID;
            }
            set
            {
                _rowID = value;
            }
        }

        public bool IsEditMode
        {
            get
            {
                return _isEditMode;
            }
            set
            {
                _isEditMode = value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
            }
        }

        public int StartCell
        {
            get
            {
                return _startCell;
            }
            set
            {
                _startCell = value;
            }
        }

        public int EndCell
        {
            get
            {
                return _endCell;
            }
            set
            {
                _endCell = value;
            }
        }

        public bool IsFirstCellClicked
        {
            get
            {
                return _isFirstCellClicked;
            }
            set
            {
                _isFirstCellClicked = value;
            }
        }

        public EmployeeVM()
        {
            _employees = new List<EmployeeVM>();
            _contextProvider = new ContextProvider();
            _rowID = 0;
            _isEditMode = false;
            _isVisible = false;
            _startCell = 0;
            _endCell = 0;
            _isFirstCellClicked = false;
        }
        public EmployeeVM(IContextProvider contextProvider)
        {
            _contextProvider = contextProvider;
        }

        public EmployeeVM(IContextProvider contextProvider, Employee model)
        {
            _contextProvider = contextProvider;
            ID = model.ID;
            FirstName = model.FirstName;
            MiddleName = model.MiddleName;
            LastName = model.LastName;
            DateOfBirth = model.DateOfBirth;
            CountryID = model.CountryID;
        }

        public EmployeeVM(IContextProvider contextProvider, EmployeeVM modelVM)
        {
            _contextProvider = contextProvider;
            IsEditMode = modelVM.IsEditMode;
            IsVisible = modelVM.IsVisible;
            IsFirstCellClicked = modelVM.IsFirstCellClicked;
            StartCell = modelVM.StartCell;
            EndCell = modelVM.EndCell;
            RowID = modelVM.RowID;
            ID = modelVM.ID;
            FirstName = modelVM.FirstName;
            MiddleName = modelVM.MiddleName;
            LastName = modelVM.LastName;
            FirstName = modelVM.FirstName;
            DateOfBirth = modelVM.DateOfBirth;
            CountryID = modelVM.CountryID;
        }

        public EmployeeVM Clone()
        {
            return new EmployeeVM(_contextProvider)
            {
                IsEditMode = this.IsEditMode,
                IsVisible = this.IsVisible,
                IsFirstCellClicked = this.IsFirstCellClicked,
                StartCell = this.StartCell,
                EndCell = this.EndCell,
                RowID = this.RowID,
                ID = this.ID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                LastName = this.LastName,
                DateOfBirth = this.DateOfBirth,
                CountryID = this.CountryID
            };
        }

        public void SetList(List<EmployeeVM> items)
        {
            _employees = items;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure _clientVMEntryList is set before calling Validate
            if (_employees == null)
            {
                // Log or handle the situation where _clientVMEntryList is not set
                yield break; // Exit the validation early
            }

            // Implement your custom validation logic here
            //if (!IsEditing && AlreadyExists(Name, ID)) // Check existence only in editing mode
            //{
            //    yield return new ValidationResult("Name already exists.", new[] { nameof(Name) });
            //}
        }

        private bool AlreadyExists(string name, int currentItemId)
        {
            bool alreadyExists = false;

            if (name != null)
            {
                // Exclude the current item from the search
                var foundItem = _employees.FirstOrDefault(p => p.FirstName == name && p.ID != currentItemId);
                alreadyExists = foundItem != null;
            }

            return alreadyExists;
        }

        public async Task<IViewModel<Employee, IModelExtendedProperties>> FromModel(Employee model)
        {
            try
            {
                if (model != null)
                {
                    await Task.Run(() =>
                    {
                        ID = model.ID;
                        FirstName = model.FirstName;
                        MiddleName = model.MiddleName;
                        LastName = model.LastName;
                        DateOfBirth = model.DateOfBirth;
                        CountryID = model.CountryID;
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FromModel(Employee model, Dictionary<string, object> serviceList): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return this;
        }

        public Employee ToNewModel()
        {
            return new Employee
            {
                ID = this.ID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                LastName = this.LastName,
                DateOfBirth = this.DateOfBirth,
                CountryID = this.CountryID
            };
        }

        public IModelExtendedProperties ToNewIModel()
        {
            return new EmployeeVM(_contextProvider)
            {
                IsEditMode = this.IsEditMode,
                IsVisible = this.IsVisible,
                IsFirstCellClicked = this.IsFirstCellClicked,
                StartCell = this.StartCell,
                EndCell = this.EndCell,
                RowID = this.RowID,
                ID = this.ID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                LastName = this.LastName,
                DateOfBirth = this.DateOfBirth,
                CountryID = this.CountryID
            };
        }

        public async Task<IViewModel<Employee, IModelExtendedProperties>> SetEditMode(bool isEditMode)
        {
            IsEditMode = isEditMode;
            await Task.CompletedTask;
            return this;
        }

        public async Task<IViewModel<Employee, IModelExtendedProperties>> SaveModelVM()
        {
            IsEditMode = false;
            await Task.CompletedTask;
            return this;
        }

        public async Task<IViewModel<Employee, IModelExtendedProperties>> SaveModelVMToNewModelVM()
        {
            var newEmployee = new EmployeeVM(_contextProvider)
            {
                IsEditMode = IsEditMode,
                IsVisible = IsVisible,
                IsFirstCellClicked = IsFirstCellClicked,
                StartCell = StartCell,
                EndCell = EndCell,
                RowID = RowID,
                ID = ID,
                FirstName = FirstName,
                MiddleName = MiddleName,
                LastName = LastName,           
                DateOfBirth = DateOfBirth,
                CountryID = CountryID
            };

            await Task.CompletedTask;
            return newEmployee;
        }

        public async Task<IEnumerable<IViewModel<Employee, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<Employee, IModelExtendedProperties>> modelVMList)
        {
            var list = modelVMList.ToList();

            int listCount = list.Count();
            RowID = listCount + 1;
            if (listCount > 0)
            {
                var firstItem = list.First();
            }

            list.Add(this);

            await Task.CompletedTask;

            return list;
        }

        public async Task<IEnumerable<IViewModel<Employee, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<Employee, IModelExtendedProperties>> modelVMList, bool isAdding)
        {

            if (isAdding)
            {
                var list = modelVMList.ToList();
                list.Remove(this);

                modelVMList = list;
            }
            else
            {

                var foundModel = modelVMList.FirstOrDefault(e => e.RowID == RowID);
                var modelVM = foundModel == null? default : (EmployeeVM)foundModel;

                if (modelVM != null)
                {
                    modelVM.IsEditMode = IsEditMode;
                    modelVM.IsVisible = IsVisible;
                    modelVM.IsFirstCellClicked = IsFirstCellClicked;
                    modelVM.StartCell = StartCell;
                    modelVM.EndCell = EndCell;
                    modelVM.ID = ID;
                    modelVM.FirstName = FirstName;
                    modelVM.MiddleName = MiddleName;
                    modelVM.LastName = LastName;
                    modelVM.DateOfBirth = DateOfBirth;
                    modelVM.CountryID = CountryID;
                }
            }


            await Task.CompletedTask;

            return modelVMList;
        }

        public async Task<IEnumerable<IViewModel<Employee, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<Employee, IModelExtendedProperties>> modelVMList)
        {
            var list = modelVMList.ToList();

            var isDeleted = list.Remove(this);

            //TODO: sol: Add logic here for deleted and not deleted conditions
            if (isDeleted) { }
            else { }

            await Task.CompletedTask;

            return list;
        }

    }
}
