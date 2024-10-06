using Blazor.Tools.BlazorBundler.Interfaces;
using System.Data;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class ListExtensions
    {
        public static DataTable ToDataTable<T>(this List<T> data) where T : class
        {
            DataTable table = new DataTable(typeof(T).Name);

            // Get all the properties
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Create columns in the DataTable based on the properties
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Iterate over the list of objects
            foreach (T item in data)
            {
                DataRow row = table.NewRow();

                // Populate the DataRow with the values from the object properties
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    row[prop.Name] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public static DataTable? GetDataTableFromClass<T>(this List<T> data) where T : class
        {
            DataTable? table = null;

            if (data != null)
            {
                var item = data.FirstOrDefault();
                var tableName = item?.GetType().Name;
                table = new DataTable(tableName);

                var excludedProperties = typeof(IModelExtendedProperties).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .ToArray();
                
                //var excludedProperties = typeof(IModelExtendedProperties).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                //                        .Concat(typeof(IBase)
                //                        .GetProperties(BindingFlags.Public | BindingFlags.Instance))
                //                        .ToArray();

                // Get all the properties
                PropertyInfo[] properties = item?.GetType()?
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)!;

                // Filter out the excluded properties
                var filteredProperties = properties
                    .Where(p => !excludedProperties.Any(ep => ep.Name == p.Name))
                    .ToArray();

                // Create columns in the DataTable based on the properties
                foreach (var prop in filteredProperties)
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }

                // Iterate over the list of objects
                foreach (T i in data)
                {
                    DataRow row = table.NewRow();

                    // Populate the DataRow with the values from the object properties
                    foreach (var prop in filteredProperties)
                    {
                        var value = prop.GetValue(i);
                        row[prop.Name] = value ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }
            }
            

            return table;
        }

    }
}
