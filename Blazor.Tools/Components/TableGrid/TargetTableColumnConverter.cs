using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Blazor.Tools.Components.TableGrid
{
    public class TargetTableColumnConverter : JsonConverter<TargetTableColumn>
    {
        public override TargetTableColumn? ReadJson(JsonReader reader, Type objectType, TargetTableColumn? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);

            // Example assuming TargetTableColumn as the concrete implementation
            var targetTableColumn = existingValue ?? new TargetTableColumn(); // Instantiate with the concrete type
            serializer.Populate(jsonObject.CreateReader(), targetTableColumn);

            return targetTableColumn;
        }

        public override void WriteJson(JsonWriter writer, TargetTableColumn? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

}
