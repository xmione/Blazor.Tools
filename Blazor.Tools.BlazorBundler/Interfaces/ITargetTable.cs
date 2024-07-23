using Blazor.Tools.BlazorBundler.Entities;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITargetTable
    {
        public string TargetTableName { get; set; }
        public List<TargetTableColumn> TargetTableColumns { get; set; }
        public string? DT { get; set; }
    }
}
