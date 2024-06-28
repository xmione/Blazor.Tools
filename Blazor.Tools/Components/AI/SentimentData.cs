using Microsoft.ML.Data;

namespace Blazor.Tools.Components.AI
{
    public class SentimentData
    {
        [LoadColumn(0)]
        public bool Sentiment { get; set; }

        [LoadColumn(1)]
        public string SentimentText { get; set; }
    }

}
