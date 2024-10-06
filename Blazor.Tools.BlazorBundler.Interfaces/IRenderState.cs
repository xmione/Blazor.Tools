namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IRenderState
    {
        bool ShouldRender { get; }
        void MarkAsChanged();
        void Reset();
    }
}
