namespace Blazor.Tools.Components.TableGrid
{
    public interface ITargetTableColumn
    {
        public string SourceFieldName { get; set; }
        public string TargetTableName { get; set; }
        public string TargetFieldName { get; set; }
        public string CheckOnTableName { get; set; }
        public List<CheckOnFieldValueEnum> CheckOnFieldValues { get; set; }
        public string PrimaryKey { get; set; }
        public string ForeignKey { get; set; }
        public string DataType { get; set; }
        public bool IsUnique { get; set; }
    }
}
