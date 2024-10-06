using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Entities.Converters;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serializes the specified value to a JSON string.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>A Task representing the asynchronous operation, with a string containing the serialized JSON.</returns>
        public static async Task<string> SerializeAsync(this object value)
        {
            string serializedObject = string.Empty;

            if (value is IEnumerable<DataRow> dataRows)
            {
                serializedObject = SerializeDataRows(dataRows);
            }
            else if (value is DataRow[] dataRowArray)
            {
                serializedObject = SerializeDataRows(dataRowArray);
            }
            else if (value is DataTable dataTable)
            {
                serializedObject = SerializeDataTable(dataTable);
            }
            else
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                // Conditionally add converters based on value type
                if (value != null)
                {
                    if (value.GetType() == typeof(DataTable))
                    {
                        settings.Converters.Add(new DataTableJsonConverter());
                    }

                }

                serializedObject = JsonConvert.SerializeObject(value, settings);
            }

            await Task.CompletedTask;
            return serializedObject;
        }

        /// <summary>
        /// Deserializes the specified data to a type T? object.
        /// </summary>
        /// <param name="serializedData">The string to be deserialized.</param>
        /// <returns>A Task representing the asynchronous operation, with a type T? containing the deserialized object.</returns>
        public static async Task<T?> DeserializeAsync<T>(this string serializedData)
        {
            T? deserializedData = default;

            if (typeof(T) == typeof(IEnumerable<DataRow>) || typeof(T) == typeof(DataRow[]))
            {
                var templateTable = new DataTable(); // Define or pass the schema of the DataTable according to your needs
                var deserializedRows = DeserializeDataRows(serializedData, templateTable).ToArray();

                if (typeof(T) == typeof(DataRow[]))
                {
                    deserializedData = (T)(object)deserializedRows;
                }
                else if (typeof(T) == typeof(IEnumerable<DataRow>))
                {
                    deserializedData = (T)(object)deserializedRows.AsEnumerable();
                }
            }
            else if (typeof(T) == typeof(DataTable))
            {
                deserializedData = DeserializeDataTable<T>(serializedData);
            }
            else
            {
                var settings = new JsonSerializerSettings();

                // Conditionally add the converter based on T and its properties
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    var innerType = typeof(T).GetGenericArguments()[0];
                    if (innerType == typeof(ITargetTable))
                    {
                        settings.Converters.Add(new TargetTableColumnConverter());
                    }
                }
                else if (typeof(T) == typeof(DataTable))
                {
                    settings.Converters.Add(new DataTableJsonConverter());
                }
                else if (typeof(T) == typeof(IBrowserFile))
                {
                    settings.Converters.Add(new BBBrowserFileJsonConverter());
                }

                deserializedData = JsonConvert.DeserializeObject<T>(serializedData, settings);
            }

            await Task.CompletedTask;
            return deserializedData;
        }

        /// <summary>
        /// Serializes IEnumerable<DataRow> types of objects.
        /// </summary>
        /// <param name="dataRows">The IEnumerable<DataRow> object to serialize.</param>
        /// <returns>(string) The serialized IEnumerable<DataRow> object.</returns>
        private static string SerializeDataRows(IEnumerable<DataRow> dataRows)
        {
            var rowsAsDictionaries = dataRows.Select(row =>
                row.Table.Columns.Cast<DataColumn>()
                .ToDictionary(column => column.ColumnName, column => row[column]));
            return JsonConvert.SerializeObject(rowsAsDictionaries);
        }

        /// <summary>
        /// Deserializes IEnumerable<DataRow> types of objects.
        /// </summary>
        /// <param name="serializedData">The serialized IEnumerable<DataRow> object.</param>
        /// <param name="templateTable">The table to be used as template.</param>
        /// <returns></returns>
        private static IEnumerable<DataRow> DeserializeDataRows(string serializedData, DataTable templateTable)
        {
            var rowsAsDictionaries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(serializedData);

            if (rowsAsDictionaries != null)
            {
                foreach (var rowDict in rowsAsDictionaries)
                {
                    if (rowDict != null)
                    {
                        var newRow = templateTable.NewRow();
                        foreach (var kvp in rowDict)
                        {
                            if (kvp.Key != null && kvp.Value != null)
                            {
                                if (!templateTable.Columns.Contains(kvp.Key))
                                {
                                    templateTable.Columns.Add(kvp.Key, kvp.Value.GetType());
                                }
                                newRow[kvp.Key] = kvp.Value;
                            }
                        }
                        yield return newRow;
                    }
                }
            }
        }

        private static string SerializeDataTable(DataTable dataTable)
        {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                rows.Add(dict);
            }

            return JsonConvert.SerializeObject(rows);
        }

        private static T? DeserializeDataTable<T>(string serializedData)
        {
            var rows = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(serializedData);

            DataTable dataTable = new DataTable();
            if (rows?.Count > 0)
            {
                foreach (var columnName in rows[0].Keys)
                {
                    dataTable.Columns.Add(columnName, typeof(object)); // Assuming all columns are of type object
                }
                foreach (var row in rows)
                {
                    var dataRow = dataTable.NewRow();
                    foreach (var columnName in row.Keys)
                    {
                        dataRow[columnName] = row[columnName];
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return (T)(object)dataTable;
        }
    }
}
