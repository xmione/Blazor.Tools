using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DTSearchBox : ComponentBase
    {
        [CascadingParameter] private DataTable? CascadedDataTable { get; set; }

        [Parameter] public DataTable? DataTable { get; set; }
        [Parameter] public string? ColumnName { get; set; }
        [Parameter] public EventCallback<IEnumerable<DataRow>> OnFilterDataTable { get; set; }

        private string _searchQuery = "";
        private string _searchPlaceHolder = "Search...";

        protected override async Task OnParametersSetAsync()
        {
            _searchPlaceHolder = $"Search {ColumnName}...";
            // If DataTable is not provided via Parameter, use CascadingDataTable if available
            if (DataTable == null && CascadedDataTable != null)
            {
                DataTable = CascadedDataTable;
            }

            await base.OnParametersSetAsync();
        }

        private async Task SearchDataTable()
        {
            // Invoke the callback to notify the parent component of the filter change
            var filteredRows = FilterDataTable();
            await OnFilterDataTable.InvokeAsync(filteredRows);
        }

        private IEnumerable<DataRow> FilterDataTable()
        {
            IEnumerable<DataRow> dataRows = default!;

            if (DataTable == null)
            {
                throw new InvalidOperationException($"DataTable parameter must be provided or cascaded for {nameof(DTSearchBox)} component.");
            }

            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                dataRows = DataTable.AsEnumerable();
            }
            else
            {
                dataRows = DataTable.AsEnumerable()
                            .Where(row =>
                            {
                                foreach (DataColumn col in DataTable.Columns)
                                {
                                    var cellValue = row[col];
                                    if (cellValue != DBNull.Value && cellValue.ToString()?.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            });
            }

            return dataRows;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int sequence = 0;

            // Container div
            builder.OpenElement(sequence++, "div");

            // Input element
            builder.OpenElement(sequence++, "input");
            builder.AddAttribute(sequence++, "type", "text");
            builder.AddAttribute(sequence++, "value", _searchQuery);
            builder.AddAttribute(sequence++, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) =>
            {
                _searchQuery = e.Value.ToString();
                SearchDataTable();
            }));
            builder.AddAttribute(sequence++, "placeholder", _searchPlaceHolder);
            builder.AddAttribute(sequence++, "class", "form-control mb-2");
            builder.CloseElement();

            builder.CloseElement(); // Close container div
        }

    }
}

