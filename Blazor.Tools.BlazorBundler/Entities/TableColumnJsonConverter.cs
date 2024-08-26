using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Components.Grid;

namespace Blazor.Tools.BlazorBundler.Entities
{
    //public class TableColumnJsonConverter : JsonConverter<TableColumn>
    //{
    //    public override void WriteJson(JsonWriter writer, TableColumn? value, JsonSerializer serializer)
    //    {
    //        var jsonObject = new JObject();
    //        var tableColumn = value ?? new TableColumn();
    //        // Serialize public properties
    //        foreach (PropertyInfo prop in tableColumn.GetType().GetPropertyNames(BindingFlags.Public | BindingFlags.Instance))
    //        {
    //            if (prop.CanRead)
    //            {
    //                var propValue = prop.GetValue(tableColumn);
    //                if (propValue != null)
    //                {
    //                    jsonObject[prop.Name] = JToken.FromObject(propValue, serializer);
    //                }
    //            }
    //        }

    //        // Serialize private _dataSource field
    //        var dataSourceField = typeof(TableColumn).GetField("_dataSource", BindingFlags.NonPublic | BindingFlags.Instance);
    //        if (dataSourceField != null)
    //        {
    //            var dataSourceValue = dataSourceField.GetValue(tableColumn);
    //            if (dataSourceValue != null)
    //            {
    //                jsonObject["_dataSource"] = JToken.FromObject(dataSourceValue, serializer);
    //            }
    //        }

    //        jsonObject.WriteTo(writer);
    //    }

    //    public override TableColumn ReadJson(JsonReader reader, Type objectType, TableColumn? existingValue, bool hasExistingValue, JsonSerializer serializer)
    //    {
    //        // Load JSON into JObject
    //        var jsonObject = JObject.Load(reader);
    //        var tableColumn = existingValue ?? new TableColumn();

    //        // Deserialize public properties
    //        foreach (var prop in typeof(TableColumn).GetPropertyNames(BindingFlags.Public | BindingFlags.Instance))
    //        {
    //            var propName = prop.Name;
    //            if (jsonObject.TryGetValue(propName, out var valueToken))
    //            {
    //                var value = valueToken.ToObject(prop.PropertyType, serializer);
    //                prop.SetValue(tableColumn, value);
    //            }
    //        }

    //        // Deserialize private _dataSource field
    //        var dataSourceField = typeof(TableColumn).GetField("_dataSource", BindingFlags.NonPublic | BindingFlags.Instance);
    //        if (dataSourceField != null && jsonObject.TryGetValue("_dataSource", out var dataSourceToken))
    //        {
    //            var dataSourceValue = dataSourceToken.ToObject(dataSourceField.FieldType, serializer);
    //            dataSourceField.SetValue(tableColumn, dataSourceValue);
    //        }

    //        return tableColumn;
    //    }

    //}
}
