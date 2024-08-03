using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class BBBrowserFileJsonConverter : JsonConverter<BBBrowserFile>
    {
        public override BBBrowserFile ReadJson(JsonReader reader, Type objectType, BBBrowserFile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Deserialize the JSON into a temporary object
            var tempObject = new { Name = string.Empty, LastModified = DateTimeOffset.MinValue, Size = 0L, ContentType = string.Empty };
            var tempData = serializer.Deserialize(reader, tempObject.GetType());

            // Extract properties from the temporary object
            var name = tempData?.GetType().GetProperty("Name")?.GetValue(tempData, null) as string ?? string.Empty;
            var lastModified = tempData?.GetType().GetProperty("LastModified")?.GetValue(tempData, null) as DateTimeOffset? ?? DateTimeOffset.MinValue;
            var size = tempData?.GetType().GetProperty("Size")?.GetValue(tempData, null) as long? ?? 0L;
            var contentType = tempData?.GetType().GetProperty("ContentType")?.GetValue(tempData, null) as string ?? string.Empty;

            // Create a dummy IBrowserFile implementation to wrap the data
            var dummyBrowserFile = new DummyBrowserFile
            {
                Name = name,
                LastModified = lastModified,
                Size = size,
                ContentType = contentType
            };

            return new BBBrowserFile(dummyBrowserFile);
        }

        public override void WriteJson(JsonWriter writer, BBBrowserFile? value, JsonSerializer serializer)
        {
            // Serialize properties of the inner IBrowserFile
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(value?.Name);
            writer.WritePropertyName("LastModified");
            writer.WriteValue(value?.LastModified);
            writer.WritePropertyName("Size");
            writer.WriteValue(value?.Size);
            writer.WritePropertyName("ContentType");
            writer.WriteValue(value?.ContentType);
            writer.WriteEndObject();
        }

        private class DummyBrowserFile : IBrowserFile
        {
            public string Name { get; set; }
            public DateTimeOffset LastModified { get; set; }
            public long Size { get; set; }
            public string ContentType { get; set; }

            public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
             
        }
    }

}
