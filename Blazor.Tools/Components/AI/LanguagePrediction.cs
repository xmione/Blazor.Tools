using Microsoft.ML.Data;

namespace Blazor.Tools.Components.AI
{
    public class LanguagePrediction
    {
        [ColumnName("Response")]
        public string Response { get; set; }
        public float Probability { get; set; }
        public float[] Score { get; set; }
    }
}
