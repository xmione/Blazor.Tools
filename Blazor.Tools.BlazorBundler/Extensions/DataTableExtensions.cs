using System.Data;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class DataTableExtensions
    {
        public static List<T> ConvertDataTableToObjects<T>(this DataTable dataTable) where T : new()
        {
            var objects = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in dataTable.Rows)
            {
                var obj = new T();
                foreach (var prop in properties)
                {
                    if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var safeValue = Convert.ChangeType(row[prop.Name], propertyType);
                        prop.SetValue(obj, safeValue);
                    }
                }

                objects.Add(obj);
            }

            return objects;
        }

        public static DataTable ToDataTable<T>(this T data) where T : class
        {
            DataTable table = new DataTable(typeof(T).Name);

            // Get all the properties
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Create columns in the DataTable based on the properties
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            return table;
        }
    }
}
