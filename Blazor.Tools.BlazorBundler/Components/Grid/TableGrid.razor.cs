using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class TableGrid<TModel, TIModel, TModelVM> : ComponentBase
    {

        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string TableID { get; set; } = string.Empty;
        [Parameter] public Dictionary<string,string> HeaderNames { get; set; } = default!;
        [Parameter] public TModel Model { get; set; } = default!;
        [Parameter] public TModelVM ModelVM { get; set; } = default!;
        [Parameter] public TIModel IModel { get; set; } = default!;
        [Parameter] public IEnumerable<TModelVM> Items { get; set; } = Enumerable.Empty<TModelVM>();
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<TModelVM>> ItemsChanged { get; set; }
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;

        protected override async Task OnParametersSetAsync()
        {


            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenComponent<TableGridInternals<TModel, TIModel, TModelVM>>(seq++);
            builder.AddAttribute(seq++, "Title", Title);
            builder.AddAttribute(seq++, "TableID", TableID);
            builder.AddAttribute(seq++, "HeaderNames", HeaderNames);
            builder.AddAttribute(seq++, "TModel", Model);
            builder.AddAttribute(seq++, "TModelVM", ModelVM);
            builder.AddAttribute(seq++, "TIModel", IModel);
            builder.AddAttribute(seq++, "Items", Items);
            builder.AddAttribute(seq++, "DataSources", DataSources);
            builder.AddAttribute(seq++, "Context", ModelVM);
            builder.AddAttribute(seq++, "ItemsChanged", ItemsChanged);
            builder.AddAttribute(seq++, "AllowCellRangeSelection", AllowCellRangeSelection);
            builder.AddAttribute(seq++, "StartContent", (RenderFragment)(headerBuilder =>
            {
                headerBuilder.OpenElement(seq++, "h2");
                headerBuilder.AddContent(seq++, Title);
                headerBuilder.CloseElement(); // th
                    
            }));

            // Render the TableHeader inside the TableGridInternals
            builder.AddAttribute(seq++, "TableHeader", (RenderFragment)(headerBuilder =>
            {
                if (HeaderNames != null)
                {
                    foreach (var column in HeaderNames)
                    {
                        headerBuilder.OpenElement(seq++, "th");
                        headerBuilder.AddContent(seq++, column.Value);
                        headerBuilder.CloseElement(); // th
                    }
                }
            }));

            builder.AddAttribute(seq++, "RowTemplate", (RenderFragment)(headerBuilder =>
            {
                if (HeaderNames != null)
                {
                    foreach (var column in HeaderNames)
                    {
                        headerBuilder.OpenElement(seq++, "td");
                        headerBuilder.AddAttribute(seq++, "id", $"{TableID}-{employee.RowID}-1");
                        headerBuilder.AddContent(seq++, column.Value);
                        headerBuilder.CloseElement(); // th
                    }
                }
            }));

            builder.CloseComponent(); // TableGridInternals

        }

        private int GetRowID<TModelVM>(TModelVM model)
        {
            // Attempt to get the RowID property dynamically
            var rowIDProperty = typeof(TModelVM).GetProperty("RowID");

            if (rowIDProperty != null && rowIDProperty.PropertyType == typeof(int))
            {
                return (int)rowIDProperty.GetValue(model);
            }

            throw new InvalidOperationException("RowID property not found or not of type int.");
        }


    }
}

