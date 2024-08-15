using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public static class TableGridFactory
    {
        public static ITableGrid? Create(Type itemType, string title, string tableID, List<TableColumnDefinition> columnDefinitions,
            IBaseVM modelVM, IEnumerable<IBaseVM> items, Dictionary<string, object> dataSources, 
            EventCallback<IEnumerable<IBaseVM>> itemsChanged, bool allowCellRangeSelection
            )
        {
            var type = typeof(TableGrid); // Non-generic DropdownList type
            var instance = (ITableGrid?)Activator.CreateInstance(type);

            if (instance != null)
            {
                instance.Title = title;
                instance.TableID = tableID;
                instance.ColumnDefinitions = columnDefinitions;
                instance.ModelVM = modelVM;
                instance.Items = items;
                instance.ItemsChanged = itemsChanged;
                instance.AllowCellRangeSelection = allowCellRangeSelection;
                
            }

            return instance;
        }
    }

}
