namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IViewModel<TModel, TIModel, TModelVM>
    {
        TModel ToNewModel();
        TIModel ToNewIModel();
        Task<TModelVM> FromModel(TModel model);
        Task<TModel> SetEditMode(TModel model, bool isEditMode);
        Task<TModel> SaveModel(TModel model);
        Task<TModel> SaveModelToNewModel(TModel model);
        Task<IEnumerable<TModel>> UpdateList(IEnumerable<TModel> modelList, TModel updatedModel);
        Task<IEnumerable<TModel>> DeleteItemFromList(IEnumerable<TModel> modelList, TModel deletedModel);
    }
}
