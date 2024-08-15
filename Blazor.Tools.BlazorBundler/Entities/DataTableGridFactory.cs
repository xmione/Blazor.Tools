using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public static class DataTableGridFactory
    {
        public static IDropdownList? CreateDataTableGrid(Type itemType, IEnumerable<object> items, string columnName, string headerName, object? value, string optionIDFieldName, string optionValueFieldName, bool isEditMode, int rowID, EventCallback<object> valueChanged)
        {
            var dropdownType = typeof(DropdownList); // Non-generic DropdownList type
            var dropdownList = (IDropdownList?)Activator.CreateInstance(dropdownType);

            if (dropdownList != null)
            {
                dropdownList.Items = items;
                dropdownList.ColumnName = columnName;
                dropdownList.HeaderName = headerName;
                dropdownList.Value = value;
                dropdownList.OptionIDFieldName = optionIDFieldName;
                dropdownList.OptionValueFieldName = optionValueFieldName;
                dropdownList.IsEditMode = isEditMode;
                dropdownList.RowID = rowID;
                dropdownList.ValueChanged = valueChanged;
            }

            return dropdownList;
        }
    }

}
