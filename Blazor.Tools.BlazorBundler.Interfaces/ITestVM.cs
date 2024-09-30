using System.Threading.Tasks;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITestVM<TModel, TIModel> : ITestMEP
    {
        void SetMessage(string message);
        string GetMessage();
        Task<ITestVM<TModel, TIModel>> SetEditMode(bool isEditMode);
    }

}
