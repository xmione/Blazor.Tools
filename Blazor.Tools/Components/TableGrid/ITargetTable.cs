using System.Data;
namespace Blazor.Tools.Components.TableGrid
{
    public interface ITargetTable
    {
        public string TargetTableName { get; set; }
        public List<TargetTableColumn> TargetTableColumns { get; set; }
        public string? DT { get; set; }
    }
}
