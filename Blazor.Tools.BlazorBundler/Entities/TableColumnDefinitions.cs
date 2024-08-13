using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class TableColumnDefinition
    {
        public string ColumnName { get; set; } = string.Empty;
        public string HeaderText { get; set; } = string.Empty;
        public Type ColumnType { get; set; } = default!;
        public RenderFragment<object> CellTemplate { get; set; } = default!;
        public EventCallback<object> ValueChanged { get; set; } = default!;
    }

}
