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
                var isEditMode = GetIsEditMode(Item);
                var message = GetMessage(Item);

                builder.OpenElement(0, "div");
                if (isEditMode)
                {
                    builder.OpenElement(1, "input");
                    builder.AddAttribute(2, "type", "text");
                    builder.AddAttribute(3, "value", message);
                    builder.CloseElement();
                }
                else
                {
                    builder.AddContent(4, message);
                }
                builder.CloseElement();
            }
        }

        private string GetMessage(T item)
        {
            var method = item.GetType().GetMethod("GetMessage");
            return method?.Invoke(item, null)?.ToString() ?? "No message available";
        }

        private bool GetIsEditMode(T item)
        {
            var property = item.GetType().GetProperty("IsEditMode");
            return (bool)(property?.GetValue(item) ?? false);
        }
    }

}
