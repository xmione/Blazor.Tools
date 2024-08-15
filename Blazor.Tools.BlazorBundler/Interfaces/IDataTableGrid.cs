using Blazor.Tools.BlazorBundler.Entities;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IDataTableGrid
    {
        string Title { get; set; }
        DataTable SelectedTable { get; set; }
        bool AllowCellSelection { get; set; }
        List<AssemblyTable>? TableList { get; set; }
        List<string> HiddenColumnNames { get; set; }
        Dictionary<string, string> HeaderNames { get; set; }
    }

}
