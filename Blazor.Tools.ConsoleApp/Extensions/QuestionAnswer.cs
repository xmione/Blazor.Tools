using Microsoft.ML.Data;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public class QuestionAnswer
    {
        [LoadColumn(0)]
        public string ColumnToPredict { get; set; }

        [LoadColumn(1)]
        public string Context { get; set; }

        [LoadColumn(2)]
        public string Question { get; set; }

        [LoadColumn(3)]
        public int AnswerIndex { get; set; }
    }
}
