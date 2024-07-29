using Microsoft.ML.Data;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class SentimentPrediction
    {
        [ColumnName("Prediction")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
