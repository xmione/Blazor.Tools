namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IViewModel<TModel, TIModel, TModelVM>
    {
        TModel ToModel();
        TIModel ToIModel();
        Task<TModelVM> FromModel(TModel model);
        Task<TModel> SetEditMode(TModel model, bool isEditMode);
    }
}
