namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IModelExtendedProperties
    {
        public int RowID { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsVisible { get; set; }
        public int StartCell { get; set; }
        public int EndCell { get; set; }
        public bool IsFirstCellClicked { get; set; }

    }
}
