using System.Data;
using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable AddPropertiesFromPropertyInfoList(this DataTable source, PropertyInfo[] propertyInfoList)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Source DataTable cannot be null.");

            if (propertyInfoList == null)
                throw new ArgumentNullException(nameof(propertyInfoList), "PropertyInfo array cannot be null.");

            foreach (var propertyInfo in propertyInfoList)
            {
                if (!source.Columns.Contains(propertyInfo.Name))
                {
                    // Determine the column type
                    var columnType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                    // Add the column to the DataTable
                    source.Columns.Add(propertyInfo.Name, columnType);
                }
            }

            return source;
        }

        public static List<T> ConvertDataTableToObjects<T>(this DataTable dataTable) where T : new()
        {
            var objects = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in dataTable.Rows)
            {
                var obj = new T();
                foreach (var prop in properties)
                {
                    if (dataTable.Columns.Contains(prop.Name))
                    {
                        var value = row[prop.Name];
                        if (value != DBNull.Value)
                        {
                            try
                            {
                                var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                var safeValue = Convert.ChangeType(value, propertyType);
                                prop.SetValue(obj, safeValue);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error setting property '{prop.Name}': {ex.Message} {ex.StackTrace}");
                            }
                        }
                    }
                }
                objects.Add(obj);
            }

            return objects;
        }

        public static List<object> ConvertDataTableToObjects(this DataTable dataTable, Type type)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable), "DataTable cannot be null.");
            if (type == null)
                throw new ArgumentNullException(nameof(type), "Type cannot be null.");

            var method = typeof(DataTableExtensions).GetMethod(
                "ConvertDataTableToObjects",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(DataTable) },
                null);

            if (method == null)
                throw new InvalidOperationException("Method 'ConvertDataTableToObjects' not found.");

            var genericMethod = method.MakeGenericMethod(type);

            var resultObject = genericMethod.Invoke(null, new object[] { dataTable });

            if (resultObject is List<object> list)
                return list;

            throw new InvalidOperationException("The result of the method invocation is not of type List<object>.");
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

        public static List<(string TableName, string ColumnName, Type ColumnType)>? GetColumnDefinitions(this DataTable source)
        {
            List<(string TableName, string ColumnName, Type ColumnType)>? columns = null!;

            var sourceColumns = source.Columns;
            if (sourceColumns != null)
            {
                columns = new List<(string TableName, string ColumnName, Type ColumnType)>();
                foreach (DataColumn dc in source.Columns)
                {
                    columns.Add((source.TableName, dc.ColumnName, dc.DataType));
                }
            }
            
            return columns;
        }

    }
}
