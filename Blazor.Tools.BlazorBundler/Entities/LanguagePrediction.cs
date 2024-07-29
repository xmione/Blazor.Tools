using Microsoft.ML.Data;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class LanguagePrediction
    {
        [ColumnName("Response")]
        public string Response { get; set; }
        public float Probability { get; set; }
        public float[] Score { get; set; }
    }
}
