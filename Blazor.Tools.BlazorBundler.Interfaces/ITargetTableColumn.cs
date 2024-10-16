namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITargetTableColumn
    {
        public string SourceFieldName { get; set; }
        public string TargetTableName { get; set; }
        public string TargetFieldName { get; set; }
        public string CheckOnTableName { get; set; }
        public IDictionary<string, object> CheckOnFieldNames { get; set; }
        public string PrimaryKey { get; set; }
        public string ForeignKey { get; set; }
        public string DataType { get; set; }
        public bool IsUnique { get; set; }
    }
}
