using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics;

namespace SampleBlazorLibrary.Components
{
    public partial class TestComponent : ComponentBase
    {
        [Parameter] public string FirstName { get; set; } = default!;
        [Parameter] public string MiddleName { get; set; } = default!;
        [Parameter] public string LastName { get; set; } = default!;

        private string _fullName { get; set; } = default!;

        private void SetFullName(string firstName, string middleName, string lastName)
        {
            _fullName = string.Join(" ", firstName, middleName, lastName);
        }

        protected override void OnParametersSet()
        {
            SetFullName(FirstName, MiddleName, LastName);
            Debug.WriteLine($"TestComponent initialized with full name: {_fullName}");
            base.OnParametersSet();
        }

        public string FullName => _fullName;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            // Building the <h1>Hello @FullName</h1> element
            builder.OpenElement(0, "h1");
            builder.AddContent(1, $"Hello {FullName}");
            builder.CloseElement();

            // Building the <p>This is a test component only.</p> element
            builder.OpenElement(2, "p");
            builder.AddContent(3, "This is a test component only.");
            builder.CloseElement();
        }
    }
}
