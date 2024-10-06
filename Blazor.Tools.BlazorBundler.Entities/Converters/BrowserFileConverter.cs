using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor.Tools.BlazorBundler.Entities.Converters
{
    public class BrowserFileConverter : JsonConverter<IBrowserFile>
    {
        public override IBrowserFile? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Custom deserialization logic for IBrowserFile
            // Assuming a concrete implementation is known, like MyBrowserFile
            var json = JsonDocument.ParseValue(ref reader);
            var concreteInstance = JsonSerializer.Deserialize<BBBrowserFile>(json.RootElement.GetRawText(), options);
            return concreteInstance;
        }

        public override void Write(Utf8JsonWriter writer, IBrowserFile value, JsonSerializerOptions options)
        {
            // Custom serialization logic for IBrowserFile
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
