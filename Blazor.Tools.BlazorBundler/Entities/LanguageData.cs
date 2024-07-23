using Microsoft.ML.Data;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class LanguageData
    {
        [LoadColumn(0)]
        public string Question { get; set; }

        [LoadColumn(1)]
        public string Response { get; set; }
    }

}
