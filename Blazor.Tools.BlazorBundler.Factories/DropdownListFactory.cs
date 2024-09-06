using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Factories
{
    public static class DropdownListFactory
    {
        public static IDropdownList? CreateDropdownList(Type dropdownType, IEnumerable<object> items, string columnName, string headerName, object? value, string optionIDFieldName, string optionValueFieldName, bool isEditMode, int rowID, EventCallback<object> valueChanged)
        {
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
