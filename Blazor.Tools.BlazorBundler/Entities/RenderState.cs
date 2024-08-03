using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class RenderState : IRenderState
    {
        public bool ShouldRender { get; private set; } = true;

        public void MarkAsChanged()
        {
            ShouldRender = true;
        }

        public void Reset()
        {
            ShouldRender = false;
        }
    }
}
