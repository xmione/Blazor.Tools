using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class TargetTableColumn : ITargetTableColumn
    {
        public string SourceFieldName { get; set; } = default!;
        public string TargetTableName { get; set; } = default!;
        public string TargetFieldName { get; set; } = default!;
        public string CheckOnTableName { get; set; } = default!;
        public List<string> CheckOnFieldNames { get; set; } = new List<string>();
        public List<CheckOnFieldValueEnum> CheckOnFieldValues { get; set; } = new List<CheckOnFieldValueEnum>();
        public string PrimaryKey { get; set; } = default!;
        public string ForeignKey { get; set; } = default!;
        public string DataType { get; set; } = default!;
        public bool IsUnique { get; set; }
    }
}
