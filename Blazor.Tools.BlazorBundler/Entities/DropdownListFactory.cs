using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public static class DropdownListFactory
    {
        public static IDropdownList? Create(Type itemType, IEnumerable<object> items, string columnName, string headerName, object? value, string optionIDFieldName, string optionValueFieldName, bool isEditMode, int rowID, EventCallback<object> valueChanged)
        {
            var type = typeof(DropdownList); // Non-generic DropdownList type
            var instance = (IDropdownList?)Activator.CreateInstance(type);

            if (instance != null)
            {
                instance.Items = items;
                instance.ColumnName = columnName;
                instance.HeaderName = headerName;
                instance.Value = value;
                instance.OptionIDFieldName = optionIDFieldName;
                instance.OptionValueFieldName = optionValueFieldName;
                instance.IsEditMode = isEditMode;
                instance.RowID = rowID;
                instance.ValueChanged = valueChanged;
            }

            return instance;
        }
    }

}
