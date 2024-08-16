namespace Blazor.Tools.Components.Pages.SampleObjects
{
    public class DataSource<T>
    {
        public string DataSourceName { get; set; } = default!;
        public List<T> DataSourceList { get; set; } = default!;
    }
}
