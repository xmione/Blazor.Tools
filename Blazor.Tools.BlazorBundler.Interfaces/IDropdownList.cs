using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IDropdownList
    {
        IEnumerable<object> Items { get; set; }
        string ColumnName { get; set; }
        string HeaderName { get; set; }
        object? Value { get; set; }
        string OptionIDFieldName { get; set; }
        string OptionValueFieldName { get; set; }
        bool IsEditMode { get; set; }
        int RowID { get; set; }
        EventCallback<object> ValueChanged { get; set; }
    }

}
