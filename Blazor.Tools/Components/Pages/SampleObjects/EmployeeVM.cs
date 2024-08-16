using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Blazor.Tools.Components.Pages.SampleObjects
{
    public class EmployeeVM : Employee, IBaseVM, IModelExtendedProperties, IValidatableObject, ICloneable<EmployeeVM>, IViewModel<Employee, IModelExtendedProperties, EmployeeVM>
    {
        private List<EmployeeVM> _employees = new List<EmployeeVM>(); // Initialize the list
        public int RowID { get; set; } // Integer property to indicate row ID
        public bool IsEditMode { get; set; } // Boolean property to indicate editing mode
        public bool IsVisible { get; set; } // Boolean property to indicate if visible
        public int StartCell { get; set; } // Integer property to indicate start of cell range selection
        public int EndCell { get; set; } // Integer property to indicate end of cell range selection
        public bool IsFirstCellClicked { get; set; } // Boolean property to indicate if StartCell control is clicked
        private readonly IContextProvider _contextProvider;

        public EmployeeVM()
        {
            _contextProvider = new ContextProvider();
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
        }

        public void SetList(List<EmployeeVM> items)
        {
            _employees = items;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure _employeeVMEntryList is set before calling Validate
            if (_employees == null || !_employees.Any())
            {
                //yield return new ValidationResult("Employee list cannot be null or empty.", new[] { nameof(_employees) });
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

        public async Task<EmployeeVM> FromModel(Employee model)
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
                ID = ID,
                FirstName = FirstName,
                MiddleName = MiddleName,
                LastName = LastName,
                DateOfBirth = DateOfBirth,
                CountryID = CountryID
            };
        }

        public IModelExtendedProperties ToNewIModel()
        {
            return new EmployeeVM(_contextProvider)
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
        }

        public async Task<EmployeeVM> SetEditMode(EmployeeVM modelVM, bool isEditMode)
        {
            modelVM.IsEditMode = isEditMode;
            await Task.CompletedTask;
            return modelVM;
        }

        public async Task<EmployeeVM> SaveModelVM(EmployeeVM modelVM)
        {
            modelVM.IsEditMode = false;
            await Task.CompletedTask;
            return modelVM;
        }

        public async Task<EmployeeVM> SaveModelVMToNewModelVM(EmployeeVM modelVM)
        {
            var newEmployee = new EmployeeVM(_contextProvider)
            {
                IsEditMode = modelVM.IsEditMode,
                IsVisible = modelVM.IsVisible,
                IsFirstCellClicked = modelVM.IsFirstCellClicked,
                StartCell = modelVM.StartCell,
                EndCell = modelVM.EndCell,
                RowID = modelVM.RowID,
                ID = modelVM.ID,
                FirstName = modelVM.FirstName,
                MiddleName = modelVM.MiddleName,
                LastName = modelVM.LastName,
                DateOfBirth = modelVM.DateOfBirth,
                CountryID = modelVM.CountryID
            };

            await Task.CompletedTask;

            return newEmployee;
        }

        public async Task<IEnumerable<EmployeeVM>> AddItemToList(IEnumerable<EmployeeVM> modelVMList, EmployeeVM newModelVM)
        {
            var list = modelVMList.ToList();

            int listCount = list.Count();
            RowID = listCount + 1;
            newModelVM.RowID = RowID;
            if (listCount > 0)
            {
                var firstItem = list.First();
                newModelVM.IsFirstCellClicked = firstItem.IsFirstCellClicked;
                newModelVM.StartCell = firstItem.StartCell;
                newModelVM.EndCell = firstItem.EndCell;
            }

            list.Add(newModelVM);

            await Task.CompletedTask;

            return list;
        }

        public async Task<IEnumerable<EmployeeVM>> UpdateList(IEnumerable<EmployeeVM> modelVMList, EmployeeVM updatedModelVM, bool isAdding)
        {
            EmployeeVM? modelVM = null;

            if (isAdding)
            {
                var list = modelVMList.ToList();
                list.Remove(updatedModelVM);

                modelVMList = list;
            }
            else
            {
                modelVM = modelVMList.FirstOrDefault(e => e.ID == updatedModelVM.ID);

                if (modelVM != null)
                {
                    modelVM.IsEditMode = updatedModelVM.IsEditMode;
                    modelVM.IsVisible = updatedModelVM.IsVisible;
                    modelVM.IsFirstCellClicked = updatedModelVM.IsFirstCellClicked;
                    modelVM.StartCell = updatedModelVM.StartCell;
                    modelVM.EndCell = updatedModelVM.EndCell;
                    modelVM.ID = updatedModelVM.ID;
                    modelVM.FirstName = updatedModelVM.FirstName;
                    modelVM.MiddleName = updatedModelVM.MiddleName;
                    modelVM.LastName = updatedModelVM.LastName;
                    modelVM.DateOfBirth = updatedModelVM.DateOfBirth;
                    modelVM.CountryID = updatedModelVM.CountryID;
                }
            }


            await Task.CompletedTask;

            return modelVMList;
        }

        public async Task<IEnumerable<EmployeeVM>> DeleteItemFromList(IEnumerable<EmployeeVM> modelVMList, EmployeeVM deletedModelVM)
        {
            var list = modelVMList.ToList();

            var isDeleted = list.Remove(deletedModelVM);

            //TODO: sol: Add logic here for deleted and not deleted conditions
            if (isDeleted) { }
            else { }

            await Task.CompletedTask;

            return list;
        }

    }
}
