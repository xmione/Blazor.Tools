using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.Components.Pages
{
    public class MessageDisplay<T> : ComponentBase where T : class
    {
        [Parameter]
        public T Item { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            if (Item != null)
            {
                var message = GetMessage(Item);
                builder.OpenElement(0, "div");
                builder.AddContent(1, message);
                builder.CloseElement();
            }
        }

        private string GetMessage(T item)
        {
            var method = item.GetType().GetMethod("GetMessage");
            return method?.Invoke(item, null)?.ToString() ?? "No message available";
        }
    }

}
