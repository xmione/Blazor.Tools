using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Data;
using Microsoft.JSInterop;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGridInternalsCopy<TModel, TIModel, TModelVM> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "Sample List";
        [Parameter] public string TableID { get; set; } = "table-id";
        [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();
        [Parameter] public TModelVM ModelVM { get; set; } = default!;
        [Parameter] public TIModel IModel { get; set; } = default!;
        [Parameter] public IEnumerable<TModelVM> Items { get; set; } = Enumerable.Empty<TModelVM>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<TModelVM>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;
        [Parameter] public EventCallback OnCellClickAsync { get; set; }
        [Parameter] public bool AllowAdding { get; set; } = true;
        [Parameter] public List<string>? HiddenColumnNames { get; set; } = default!;
        [Parameter] public RenderFragment? StartContent { get; set; }
        [Parameter] public RenderFragment? TableHeader { get; set; }
        [Parameter] public RenderFragment<TModelVM> RowTemplate { get; set; } = default!;

        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        
        

    }
}
