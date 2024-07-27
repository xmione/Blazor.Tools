/*====================================================================================================
    Component Name: DataRowViewModel 
    Created By    : Solomio S. Sisante
    Created On    : June 3, 2024
    Purpose       : To contain a Dictionary for the Excel columns.
  ====================================================================================================*/
namespace Blazor.Tools.Components.ExcelUploader
{
    public class DataRowViewModel
    {
        public Dictionary<string, object?> Columns { get; set; } = new Dictionary<string, object?>();
    }
}