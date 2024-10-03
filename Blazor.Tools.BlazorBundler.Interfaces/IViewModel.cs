namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IViewModel<TModel, TIModel> : IModelExtendedProperties where TModel : IBase
    {
        TModel ToNewModel();
        TIModel ToNewIModel();
        Task<IViewModel<TModel, TIModel>> FromModel(TModel model);
        Task<IViewModel<TModel, TIModel>> SetEditMode(bool isEditMode);
        Task<IViewModel<TModel, TIModel>> SaveModelVM();
        Task<IViewModel<TModel, TIModel>> SaveModelVMToNewModelVM();
        Task<IEnumerable<IViewModel<TModel, TIModel>>> AddItemToList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList);
        Task<IEnumerable<IViewModel<TModel, TIModel>>> UpdateList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList, bool isAdding);
        Task<IEnumerable<IViewModel<TModel, TIModel>>> DeleteItemFromList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList);
    }

}
