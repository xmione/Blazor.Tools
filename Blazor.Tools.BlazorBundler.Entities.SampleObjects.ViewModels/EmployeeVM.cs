/*====================================================================================================
    Class Name  : EmployeeVM.cs
    Created By  : Solomio S. Sisante
    Created On  : August 15, 2024
    Purpose     : To provide a view model class for the Employee class.
  ====================================================================================================*/

// Do not remove the declared usings here even if they are unnecessary.
// They are used to dynamically create the assembly dll file.
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels
{
    public class EmployeeVM : IBase, IValidatableObject, ICloneable<EmployeeVM>, IViewModel<IBase, IModelExtendedProperties>
    {
        private List<EmployeeVM> _employees;
        private readonly IContextProvider _contextProvider;

        public int _id;
        public string _firstName;
        public string _middleName;
        public string _lastName;
        public DateOnly _dateOfBirth;
        public int _countryID;

        public int _rowID;
        public bool _isEditMode;
        public bool _isVisible;
        public int _startCell;
        public int _endCell;
        public bool _isFirstCellClicked;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }
        
        public string MiddleName
        {
            get { return _middleName; }
            set { _middleName = value; }
        }
        
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }
        
        public DateOnly DateOfBirth
        {
            get { return _dateOfBirth; }
            set { _dateOfBirth = value; }
        }
        
        public int CountryID
        {
            get { return _countryID; }
            set { _countryID = value; }
        }

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
            _employees = new List<EmployeeVM>();
            _contextProvider = contextProvider;
            _rowID = 0;
            _isEditMode = false;
            _isVisible = false;
            _startCell = 0;
            _endCell = 0;
            _isFirstCellClicked = false;
        }

        public EmployeeVM(IContextProvider contextProvider, IBase model)
        {
            _contextProvider = contextProvider;
            _id = model.ID;
            _firstName = model.GetPropertyValue<string>("FirstName");
            _middleName = model.GetPropertyValue<string>("MiddleName");
            _lastName = model.GetPropertyValue<string>("LastName");
            _dateOfBirth = model.GetPropertyValue<DateOnly>("DateOfBirth");
            _countryID = model.GetPropertyValue<int>("CountryID");
        }

        public EmployeeVM(IContextProvider contextProvider, EmployeeVM modelVM)
        {
            _contextProvider = contextProvider;
            _isEditMode = modelVM.IsEditMode;
            _isVisible = modelVM.IsVisible;
            _isFirstCellClicked = modelVM.IsFirstCellClicked;
            _startCell = modelVM.StartCell;
            _endCell = modelVM.EndCell;
            _rowID = modelVM.RowID;
            _id = modelVM.ID;
            _firstName = modelVM.FirstName;
            _middleName = modelVM.MiddleName;
            _lastName = modelVM.LastName;
            _dateOfBirth = modelVM.DateOfBirth;
            _countryID = modelVM.CountryID;
        }

        public EmployeeVM Clone()
        {
            return new EmployeeVM(_contextProvider)
            {
                _isEditMode = this.IsEditMode,
                _isVisible = this.IsVisible,
                _isFirstCellClicked = this.IsFirstCellClicked,
                _startCell = this.StartCell,
                _endCell = this.EndCell,
                _rowID = this.RowID,
                _id = this.ID,
                _firstName = this.FirstName,
                _middleName = this.MiddleName,
                _lastName = this.LastName,
                _dateOfBirth = this.DateOfBirth,
                _countryID = this.CountryID
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

        public async Task<IViewModel<IBase, IModelExtendedProperties>> FromModel(IBase model)
        {
            try
            {
                if (model != null)
                {
                    await Task.Run(() =>
                    {
                        _id = model.GetPropertyValue<int>("ID")!;
                        _firstName = model.GetPropertyValue<string>("FirstName");
                        _middleName = model.GetPropertyValue<string>("MiddleName");
                        _lastName = model.GetPropertyValue<string>("LastName");
                        _dateOfBirth = model.GetPropertyValue<DateOnly>("DateOfBirth")!;
                        _countryID = model.GetPropertyValue<int>("CountryID")!;
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FromModel(Employee model, Dictionary<string, object> serviceList): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return this;
        }

        public IBase ToNewModel()
        {
            var type = typeof(IBase);
            var typeInstance = (IBase)Activator.CreateInstance(type)!;

            typeInstance.SetValue("ID", this.ID);
            typeInstance.SetValue("FirstName", this.FirstName);
            typeInstance.SetValue("MiddleName", this.MiddleName);
            typeInstance.SetValue("LastName", this.LastName);
            typeInstance.SetValue("DateOfBirth", this.DateOfBirth);
            typeInstance.SetValue("CountryID", this.CountryID);

            return typeInstance;
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

        public async Task<IViewModel<IBase, IModelExtendedProperties>> SetEditMode(bool isEditMode)
        {
            IsEditMode = isEditMode;
            await Task.CompletedTask;
            return this;
        }

        public async Task<IViewModel<IBase, IModelExtendedProperties>> SaveModelVM()
        {
            IsEditMode = false;
            await Task.CompletedTask;
            return this;
        }

        public async Task<IViewModel<IBase, IModelExtendedProperties>> SaveModelVMToNewModelVM()
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

        public async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList)
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

        public async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList, bool isAdding)
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
                var modelVM = foundModel == null ? default : (EmployeeVM)foundModel;

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

        public async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList)
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

