/*====================================================================================================
    Interface Name: ITableGrid
    Created By    : Solomio S. Sisante
    Created On    : August 14, 2024
    Purpose       : TableGrid interface.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Entities;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITableGrid
    {
        string Title { get; set; }
        string TableID { get; set; }
        List<TableColumnDefinition> ColumnDefinitions { get; set; }
        IBaseVM ModelVM { get; set; }
        IEnumerable<IBaseVM> Items { get; set; }
        Dictionary<string, object> DataSources { get; set; }
        EventCallback<IEnumerable<IBaseVM>> ItemsChanged { get; set; }
        bool AllowCellRangeSelection { get; set; }
    }

}
