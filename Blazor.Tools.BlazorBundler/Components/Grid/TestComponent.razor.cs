using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TestComponent : ComponentBase
    {
        [Parameter] public string FirstName { get; set; } = default!;
        [Parameter] public string MiddleName { get; set; } = default!;
        [Parameter] public string LastName { get; set; } = default!;

        // New parameter for configuration
        [Parameter] public string ConfigParameter { get; set; } = "DefaultConfig";

        // RenderFragment to hold child content
        [Parameter] public RenderFragment? ChildContent { get; set; }

        private string _fullName { get; set; } = default!;
        public string FullName => _fullName;
        private bool _isRendered = false;

        private void SetFullName(string firstName, string middleName, string lastName)
        {
            _fullName = string.Join(" ", firstName, middleName, lastName);
        }

        protected override void OnParametersSet()
        {
            SetFullName(FirstName, MiddleName, LastName);
            Debug.WriteLine($"TestComponent initialized with full name: {_fullName}");
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");

            // Render the main content
            builder.OpenElement(1, "h1");
            builder.AddContent(2, $"Hello {FullName}");
            builder.CloseElement();

            builder.OpenElement(3, "p");
            builder.AddContent(4, "This is a test component only.");
            builder.CloseElement();

            // Render child content with cascading parameters
            if (ChildContent != null)
            {
                builder.OpenComponent<CascadingValue<string>>(5);
                builder.AddAttribute(6, "Value", ConfigParameter);
                builder.AddAttribute(7, "ChildContent", ChildContent);
                builder.CloseComponent();
            }

            builder.CloseElement();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Debug.WriteLine("TestComponent: OnAfterRenderAsync (firstRender)");
                await Task.CompletedTask;
            }
        }
    }
}
