using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models
{
    public class Country : IBase
    {
        public int ID { get; set; }
        public string Name { get; set; } = default!;
    }
}
