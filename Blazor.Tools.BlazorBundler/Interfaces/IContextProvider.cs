namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IContextProvider
    {
        T GetContext<T>(string key);
    }

}
