namespace Blazor.Tools.BlazorBundler.Entities
{
    public class SessionItem
    {
        public string Key { get; set; } = default!;
        public object? Value { get; set; } 
        public bool Serialize { get; set; } 
    }
}
