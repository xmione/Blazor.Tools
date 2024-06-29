using Microsoft.ML.Data;

namespace Blazor.Tools.Components.AI
{
    public class SentimentPrediction
    {
        [ColumnName("Prediction")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
