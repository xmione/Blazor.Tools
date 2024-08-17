namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects
{
    public interface IModelExtendedProperties
    {
        public bool IsEditMode { get; set; }
        public bool IsVisible { get; set; }
        public int StartCell { get; set; }
        public int EndCell { get; set; }
        public bool IsFirstCellClicked { get; set; }

    }
}
