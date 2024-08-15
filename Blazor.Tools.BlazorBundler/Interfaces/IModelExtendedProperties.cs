/*====================================================================================================
    Component Name: IModelExtendedProperties 
    Created By    : Solomio S. Sisante
    Created On    : August 14, 2024
    Purpose       : Adds extended properties to table's model view properties.
  ====================================================================================================*/
namespace Blazor.Tools.BlazorBundler.Interfaces
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
