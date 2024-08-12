namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IViewModel<TModel, TIModel, TModelVM>
    {
        TModel ToNewModel();
        TIModel ToNewIModel();
        Task<TModelVM> FromModel(TModel model);
        Task<TModelVM> SetEditMode(TModelVM modelVM, bool isEditMode);
        Task<TModelVM> SaveModelVM(TModelVM modelVM);
        Task<TModelVM> SaveModelVMToNewModelVM(TModelVM modelVM);
        Task<IEnumerable<TModelVM>> AddItemToList(IEnumerable<TModelVM> modelVMList, TModelVM newModelVM);
        Task<IEnumerable<TModelVM>> UpdateList(IEnumerable<TModelVM> modelVMList, TModelVM updatedModelVM, bool isAdding);
        Task<IEnumerable<TModelVM>> DeleteItemFromList(IEnumerable<TModelVM> modelVMList, TModelVM deletedModelVM);
    }
}
