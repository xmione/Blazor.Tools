using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.Components.Pages.SampleObjects
{
    public class Country : IBaseModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = default!;
    }
}
