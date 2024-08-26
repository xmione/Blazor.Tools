using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class ModelExtendedProperties : IModelExtendedProperties
    {
        public int RowID { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsVisible { get; set; }
        public int StartCell { get; set; }
        public int EndCell { get; set; }
        public bool IsFirstCellClicked { get; set; }
    }
}
