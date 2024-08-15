using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public static class TableGridInternalsFactory
    {
        public static ITableGridInternals? Create(
            Type itemType, 
            string title, 
            string tableID, 
            List<TableColumnDefinition> columnDefinitions,
            IBaseVM modelVM, 
            IEnumerable<IBaseVM> items,
            EventCallback<IEnumerable<IBaseVM>> itemsChanged, 
            bool allowCellRangeSelection,
            EventCallback onCellClickAsync, 
            bool allowAdding, 
            List<string>? hiddenColumnNames
            )
        {
            var type = typeof(TableGridInternals); // Non-generic DropdownList type
            var instance = (ITableGridInternals?)Activator.CreateInstance(type);

            if (instance != null)
            {
                instance.Title = title;
                instance.TableID = tableID;
                instance.ColumnDefinitions = columnDefinitions;
                instance.ModelVM = modelVM;
                instance.Items = items;
                instance.ItemsChanged = itemsChanged;
                instance.AllowCellRangeSelection = allowCellRangeSelection;
                instance.OnCellClickAsync = onCellClickAsync;
                instance.AllowAdding = allowAdding;
            }

            return instance;
        }
    }

}
