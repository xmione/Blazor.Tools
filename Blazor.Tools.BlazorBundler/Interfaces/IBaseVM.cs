/*====================================================================================================
    Component Name: IBaseVM 
    Created By    : Solomio S. Sisante
    Created On    : August 14, 2024
    Purpose       : Base class for the table's model view classes.
  ====================================================================================================*/
namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IBaseVM: IBaseModel
    {
        public int RowID { get; set; }
    }
}
