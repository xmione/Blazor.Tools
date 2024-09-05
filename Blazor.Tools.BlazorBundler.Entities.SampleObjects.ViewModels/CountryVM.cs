using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels
{

    public class CountryVM : Country, IValidatableObject, ICloneable<CountryVM>, IViewModel<Country, IModelExtendedProperties>
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
                IsEditMode = this.IsEditMode,
                IsVisible = this.IsVisible,
                IsFirstCellClicked = this.IsFirstCellClicked,
                StartCell = this.StartCell,
                EndCell = this.EndCell,
                RowID = this.RowID,
                ID = this.ID,
                Name = this.Name,
            };
        }

        public void SetList(List<CountryVM> items)
        {
            _countries = items;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure _clientVMEntryList is set before calling Validate
            if (_countries == null)
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
                var foundItem = _countries.FirstOrDefault(p => p.Name == name && p.ID != currentItemId);
                alreadyExists = foundItem != null;
            }

            return alreadyExists;
        }
        public async Task<IViewModel<Country, IModelExtendedProperties>> FromModel(Country model)
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
                ID = this.ID,
                Name = this.Name,
            };
        }

        public IModelExtendedProperties ToNewIModel()
        {
            return new CountryVM(_contextProvider)
            {
                IsEditMode = this.IsEditMode,
                IsVisible = this.IsVisible,
                IsFirstCellClicked = this.IsFirstCellClicked,
                StartCell = this.StartCell,
                EndCell = this.EndCell,
                RowID = this.RowID,
                ID = this.ID,
                Name = this.Name,
            };
        }

        public async Task<IViewModel<Country, IModelExtendedProperties>> SetEditMode(bool isEditMode)
        {
            IsEditMode = isEditMode;
            await Task.CompletedTask;
            return this;
        }

        public async Task<IViewModel<Country, IModelExtendedProperties>> SaveModelVM()
        {
            IsEditMode = false;
            await Task.CompletedTask;

            return this;
        }

        public async Task<IViewModel<Country, IModelExtendedProperties>> SaveModelVMToNewModelVM()
        {
            var newModelVM = new CountryVM(_contextProvider)
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

            await Task.CompletedTask;

            return newModelVM;
        }

        public async Task<IEnumerable<IViewModel<Country, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<Country, IModelExtendedProperties>> modelVMList)
        {
            var list = modelVMList.ToList();

            int listCount = list.Count();
            RowID = listCount + 1;
            if (listCount > 0)
            {
                var firstItem = list.First();
                IsFirstCellClicked = firstItem.IsFirstCellClicked;
                StartCell = firstItem.StartCell;
                EndCell = firstItem.EndCell;
            }

            list.Add(this);

            await Task.CompletedTask;

            return list;
        }

        public async Task<IEnumerable<IViewModel<Country, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<Country, IModelExtendedProperties>> modelVMList, bool isAdding)
        {
            CountryVM? modelVM = null;

            if (isAdding)
            {
                var list = modelVMList.ToList();
                list.Remove(this);
                modelVMList = list;
            }
            else
            {

                var foundModel = modelVMList.FirstOrDefault(e => e.RowID == RowID);

                modelVM = foundModel == null? default: (CountryVM)foundModel;

                if (modelVM != null)
                {
                    modelVM.IsEditMode = IsEditMode;
                    modelVM.IsVisible = IsVisible;
                    modelVM.IsFirstCellClicked = IsFirstCellClicked;
                    modelVM.StartCell = StartCell;
                    modelVM.EndCell = EndCell;
                    modelVM.RowID = RowID;
                    modelVM.ID = ID;
                    modelVM.Name = Name;
                }

            }

            await Task.CompletedTask;

            return modelVMList;
        }

        public async Task<IEnumerable<IViewModel<Country, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<Country, IModelExtendedProperties>> modelVMList)
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
