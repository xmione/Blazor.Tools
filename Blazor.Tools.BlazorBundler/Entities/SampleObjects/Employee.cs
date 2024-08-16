using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects
{
    public class Employee : IBaseModel
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = default!;
        public string MiddleName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public DateOnly DateOfBirth { get; set; } = default!;
        public int CountryID { get; set; }
    }

}
