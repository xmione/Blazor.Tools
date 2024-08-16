using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Blazor.Tools.Components.Pages.SampleObjects
{
    public class CountryVM : Country, IBaseVM, IModelExtendedProperties, IValidatableObject, ICloneable<CountryVM>, IViewModel<Country, IModelExtendedProperties, CountryVM>
    {
        private List<CountryVM> _countries = new List<CountryVM>(); // Initialize the list
        public int RowID { get; set; } // Integer property to indicate row ID
        public bool IsEditMode { get; set; } // Boolean property to indicate editing mode
        public bool IsVisible { get; set; } // Boolean property to indicate editing mode
        public int StartCell { get; set; } // Integer property to indicate start of cell range selection
        public int EndCell { get; set; } // Integer property to indicate end of cell range selection
        public bool IsFirstCellClicked { get; set; } // Boolean property to indicate if StartCell control is clicked
        private readonly IContextProvider _contextProvider;
        public CountryVM()
        {
            _contextProvider = new ContextProvider();
        }
        public CountryVM(IContextProvider contextProvider)
        {
            _contextProvider = contextProvider;
        }
        public CountryVM(IContextProvider contextProvider, Country model)
        {
            _contextProvider = contextProvider;
            ID = model.ID;
            Name = model.Name;
        }

        public CountryVM(IContextProvider contextProvider, CountryVM modelVM)
        {
            _contextProvider = contextProvider;
            IsEditMode = modelVM.IsEditMode;
            IsVisible = modelVM.IsVisible;
            IsFirstCellClicked = modelVM.IsFirstCellClicked;
            StartCell = modelVM.StartCell;
            EndCell = modelVM.EndCell;
            RowID = modelVM.RowID;
            ID = modelVM.ID;
            Name = modelVM.Name;
        }
        public CountryVM Clone()
        {
            return new CountryVM(_contextProvider)
            {
                IsEditMode = IsEditMode,
                IsVisible = IsVisible,
                IsFirstCellClicked = IsFirstCellClicked,
                StartCell = StartCell,
                EndCell = EndCell,
                RowID = RowID,
                ID = ID,
                Name = Name,
            };
        }

        public void SetList(List<CountryVM> items)
        {
            _countries = items;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure _countryVMEntryList is set before calling Validate
            if (_countries == null || !_countries.Any())
            {
                //yield return new ValidationResult("Employee list cannot be null or empty.", new[] { nameof(_employees) });
                // Log or handle the situation where _countryVMEntryList is not set
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
                var foundItem = _countries.FirstOrDefault(p => p.Name == name && p.ID != currentItemId);
                alreadyExists = foundItem != null;
            }

            return alreadyExists;
        }
        public async Task<CountryVM> FromModel(Country model)
        {
            try
            {
                if (model != null)
                {
                    await Task.Run(() =>
                    {
                        ID = model.ID;
                        Name = model.Name;
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FromModel(Country model, Dictionary<string, object> serviceList): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return this;
        }
        public Country ToNewModel()
        {
            return new Country
            {
                ID = ID,
                Name = Name,
            };
        }

        public IModelExtendedProperties ToNewIModel()
        {
            return new CountryVM(_contextProvider)
            {
                IsEditMode = IsEditMode,
                IsVisible = IsVisible,
                IsFirstCellClicked = IsFirstCellClicked,
                StartCell = StartCell,
                EndCell = EndCell,
                RowID = RowID,
                ID = ID,
                Name = Name,
            };
        }

        public async Task<CountryVM> SetEditMode(CountryVM modelVM, bool isEditMode)
        {
            modelVM.IsEditMode = isEditMode;
            await Task.CompletedTask;
            return modelVM;
        }

        public async Task<CountryVM> SaveModelVM(CountryVM modelVM)
        {
            modelVM.IsEditMode = false;
            await Task.CompletedTask;

            return modelVM;
        }

        public async Task<CountryVM> SaveModelVMToNewModelVM(CountryVM modelVM)
        {
            var newModelVM = new CountryVM(_contextProvider)
            {
                IsEditMode = modelVM.IsEditMode,
                IsVisible = modelVM.IsVisible,
                IsFirstCellClicked = modelVM.IsFirstCellClicked,
                StartCell = modelVM.StartCell,
                EndCell = modelVM.EndCell,
                RowID = modelVM.RowID,
                ID = modelVM.ID,
                Name = modelVM.Name,
            };

            await Task.CompletedTask;

            return newModelVM;
        }

        public async Task<IEnumerable<CountryVM>> AddItemToList(IEnumerable<CountryVM> modelVMList, CountryVM newModelVM)
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

        public async Task<IEnumerable<CountryVM>> UpdateList(IEnumerable<CountryVM> modelVMList, CountryVM updatedModelVM, bool isAdding)
        {
            CountryVM? modelVM = null;

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
                    modelVM.RowID = updatedModelVM.RowID;
                    modelVM.ID = updatedModelVM.ID;
                    modelVM.Name = updatedModelVM.Name;
                }

            }

            await Task.CompletedTask;

            return modelVMList;
        }

        public async Task<IEnumerable<CountryVM>> DeleteItemFromList(IEnumerable<CountryVM> modelVMList, CountryVM deletedModelVM)
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
