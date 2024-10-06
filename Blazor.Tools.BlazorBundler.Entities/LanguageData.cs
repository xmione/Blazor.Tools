using Microsoft.ML.Data;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class LanguageData
    {
        [LoadColumn(0)]
        public string Question { get; set; } = default!;

        [LoadColumn(1)]
        public string Response { get; set; } = default!;
    }

}
