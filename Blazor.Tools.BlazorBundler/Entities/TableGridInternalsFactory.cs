using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public static class TableGridInternalsFactory
    {
        public static ITableGridInternals? Create(Type itemType, string title, string tableID, List<TableColumnDefinition> columnDefinitions,
            IBaseModel model, IBaseVM modelVM, IModelExtendedProperties iModel, IEnumerable<IBaseVM> items,
            Dictionary<string, object> dataSources, EventCallback<IEnumerable<IBaseVM>> itemsChanged, bool allowCellRangeSelection,
            EventCallback onCellClickAsync, bool allowAdding, List<string>? hiddenColumnNames
            )
        {
            var type = typeof(TableGridInternals); // Non-generic DropdownList type
            var instance = (ITableGridInternals?)Activator.CreateInstance(type);

            if (instance != null)
            {
                instance.Title = title;
                instance.TableID = tableID;
                instance.ColumnDefinitions = columnDefinitions;
                instance.Model = model;
                instance.ModelVM = modelVM;
                instance.IModel = iModel;
                instance.Items = items;
                instance.DataSources = dataSources;
                instance.ItemsChanged = itemsChanged;
                instance.AllowCellRangeSelection = allowCellRangeSelection;
                instance.OnCellClickAsync = onCellClickAsync;
                instance.AllowAdding = allowAdding;
                instance.HiddenColumnNames = hiddenColumnNames;
            }

            return instance;
        }
    }

}
