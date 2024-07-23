namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ICommonService<TServiceModel, TIModel, TReportModel> where TReportModel : IReportItem
    {
        Task<IEnumerable<TServiceModel>?> GetAllAsync();
        Task<TServiceModel?> GetAsync(int id);
        Task<TServiceModel?> GetByNameAsync(string name);
        Task<TServiceModel?> SaveAsync(TIModel? model);
        Task<TServiceModel?> SaveAsync(TServiceModel? model);
        Task DeleteAsync(int id);

    }
}
