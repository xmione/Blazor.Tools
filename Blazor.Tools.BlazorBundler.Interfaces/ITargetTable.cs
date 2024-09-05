
namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITargetTable
    {
        public string TargetTableName { get; set; }
        public List<ITargetTableColumn> TargetTableColumns { get; set; }
        public string? DT { get; set; }
    }
}
