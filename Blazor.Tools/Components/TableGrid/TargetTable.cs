namespace Blazor.Tools.Components.TableGrid
{
    public class TargetTable : ITargetTable 
    {
        public string TargetTableName { get; set; } = default!;
        public List<TargetTableColumn> TargetTableColumns { get; set; } = default!;
        public string? DT { get; set; } = default!;
    }
}
