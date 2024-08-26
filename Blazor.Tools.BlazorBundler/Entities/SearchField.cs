namespace Blazor.Tools.BlazorBundler.Entities
{
    public class SearchField
    {
        public string TableName { get; set; } = default!;
        public string FieldName { get; set; } = default!;
        public LookupFieldConditionEnum MatchCondition { get; set; } = default!;
    }
}
