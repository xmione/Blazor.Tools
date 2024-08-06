using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Blazor.Tools.BlazorBundler.Components.Grid
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
    }
}
