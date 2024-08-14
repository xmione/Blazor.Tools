namespace Blazor.Tools.BlazorBundler.Entities
{
    public class TableColumnDefinition
    {
        public string ColumnName { get; set; } = string.Empty;
        public string HeaderText { get; set; } = string.Empty;
        public Type ColumnType { get; set; } = typeof(string);
        public Delegate ValueChanged { get; set; } = default!;
        public Func<string, object, int, Task> CellClicked { get; set; } = default!;
        // Additional properties for DropdownList support
        public IEnumerable<object> Items { get; set; } = Enumerable.Empty<object>();
        public string OptionIDFieldName { get; set; } = string.Empty;
        public string OptionValueFieldName { get; set; } = string.Empty;
    }

}
