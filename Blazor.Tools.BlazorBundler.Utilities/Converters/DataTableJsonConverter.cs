using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Utilities.Converters
{
    public class DataTableJsonConverter : JsonConverter<DataTable>
    {
        public override void WriteJson(JsonWriter writer, DataTable? value, JsonSerializer serializer)
        {
            var rows = new List<Dictionary<string, object>>();
            if (value != null)
            {
                foreach (DataRow row in value.Rows)
                {
                    var rowDict = new Dictionary<string, object>();
                    foreach (DataColumn column in value.Columns)
                    {
                        rowDict[column.ColumnName] = row[column];
                    }
                    rows.Add(rowDict);
                }

                serializer.Serialize(writer, new { value.TableName, Columns = value.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(), Rows = rows });
            }
            else
            {
                serializer.Serialize(writer, new { TableName = string.Empty, Columns = new List<string>(), Rows = rows });
            }
        }

        public override DataTable ReadJson(JsonReader reader, Type objectType, DataTable? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            DataTable table = existingValue ?? new DataTable();

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray jsonArray = JArray.Load(reader);

                if (jsonArray.Count > 0 && jsonArray[0] is JObject firstObject)
                {
                    var columnNames = firstObject.Properties().Select(p => p.Name).ToList();

                    // Ensure columns exist in the DataTable
                    foreach (var columnName in columnNames)
                    {
                        if (!table.Columns.Contains(columnName))
                        {
                            table.Columns.Add(columnName);
                        }
                    }

                    // Add rows to DataTable
                    foreach (JObject jsonRow in jsonArray)
                    {
                        DataRow dataRow = table.NewRow();
                        foreach (var columnName in columnNames)
                        {
                            if (jsonRow.TryGetValue(columnName, out JToken? value))
                            {
                                dataRow[columnName] = value.ToObject<object>();
                            }
                        }
                        table.Rows.Add(dataRow);
                    }
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                JObject jsonObject = JObject.Load(reader);

                string tableName = jsonObject["TableName"]?.ToString() ?? string.Empty;
                var columnNames = jsonObject["Columns"]?.ToObject<List<string>>();
                var rows = jsonObject["Rows"]?.ToObject<List<Dictionary<string, object>>>();

                table.TableName = tableName;

                // Ensure columns exist in the DataTable
                if (columnNames != null)
                {
                    foreach (var columnName in columnNames)
                    {
                        if (!table.Columns.Contains(columnName))
                        {
                            table.Columns.Add(columnName);
                        }
                    }
                }

                // Add rows to DataTable
                if (rows != null && columnNames != null)
                {
                    foreach (var row in rows)
                    {
                        var dataRow = table.NewRow();
                        foreach (var columnName in columnNames)
                        {
                            if (row.ContainsKey(columnName))
                            {
                                dataRow[columnName] = row[columnName];
                            }
                        }
                        table.Rows.Add(dataRow);
                    }
                }
            }

            return table;
        }

    }
}
