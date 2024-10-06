using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class TargetTable : ITargetTable
    {
        public string TargetTableName { get; set; } = default!;
        public List<ITargetTableColumn> TargetTableColumns { get; set; } = default!;
        public string? DT { get; set; } = default!;
    }
}
