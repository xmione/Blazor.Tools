using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DTSearchBox<T> : ComponentBase
    {
        [CascadingParameter] private IEnumerable<T>? CascadedData { get; set; }

        [Parameter] public IEnumerable<T>? Data { get; set; }
        [Parameter] public string? ColumnName { get; set; }
        [Parameter] public EventCallback<IEnumerable<T>> OnFilterData { get; set; }

        private string _searchQuery = "";
        private string _searchPlaceHolder = "Search...";
        private IEnumerable<T>? _dataList;

        protected override void OnParametersSet()
        {
            _searchPlaceHolder = $"Search {ColumnName}...";

            // Use Data if provided, else fall back to CascadedData
            if (Data != null)
            {
                _dataList = Data.ToList();
            }
            else if (CascadedData != null)
            {
                _dataList = CascadedData.ToList();
            }

            base.OnParametersSet();
        }

        private async Task SearchData()
        {
            var filteredData = FilterData();
            await OnFilterData.InvokeAsync(filteredData);
        }

        private IEnumerable<T>? FilterData()
        {
            if (_dataList == null || !_dataList.Any())
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                return _dataList;
            }

            Console.WriteLine($"Searching for: {_searchQuery}");

            var filteredData = _dataList.Where(item => GetItem(item)).ToList();

            return filteredData;
        }

        private bool GetItem(T item)
        {
            // Ensure item is not null
            if (item == null)
            {
                Console.WriteLine("Item is null");
                return false;
            }

            // Set a breakpoint on the following line
            var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Ensure properties are being retrieved
            if (!properties.Any())
            {
                Console.WriteLine("No properties found");
            }

            // Use PropertyMatchesSearch method to process each item
            return PropertyMatchesSearch(item, _searchQuery);
        }
        // Helper method to handle nullability and search logic
        private bool PropertyMatchesSearch<TItem>(TItem item, string searchQuery)
        {

            bool isFound = false;
            var properties = item?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    string? propValue;
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            propValue = (string?)prop?.GetValue(item);
                        }
                        else
                        {
                            propValue = prop?.GetValue(item)?.ToString();
                        }

                        if (propValue != null && propValue.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // Debug output
                            Console.WriteLine($"Match found in property: {prop?.Name}, Value: {propValue}");
                            isFound = true;
                        }
                    }
                }
            }

            // Debug output if no match found
            Console.WriteLine("No match found in this item.");
            return isFound;
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
                _searchQuery = e.Value?.ToString();
                SearchData();
            }));
            builder.AddAttribute(sequence++, "placeholder", _searchPlaceHolder);
            builder.AddAttribute(sequence++, "class", "form-control mb-2");
            builder.CloseElement();

            builder.CloseElement(); // Close container div
        }
    }
}
