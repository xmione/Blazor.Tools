using System.Text.Json.Serialization;

namespace Blazor.Tools.ConsoleApp.Extensions
{

    [JsonSerializable(typeof(TrainingData))]
    public partial class JsonContext : JsonSerializerContext { }
}
