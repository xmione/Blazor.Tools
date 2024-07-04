using Microsoft.ML;
using Microsoft.ML.Data;
using HtmlAgilityPack;
using Microsoft.ML.Transforms.Text;
using Microsoft.Data.Analysis;

public class Program
{
    private const string _mlFolder = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\ML";
    private const string _languageDataFileName = "languageData.txt";
    private static string _dataFilePath = string.Empty;
    private static MLContext _mlContext;
    public static void Main(string[] args)
    {
        _dataFilePath = Path.Combine(_mlFolder, _languageDataFileName);

        // ML.NET model training pipeline
        _mlContext = new MLContext();

        // Data preparation and model training flow
        PreprocessHtmlData();
        TrainQAModel();
        ValidateModel();
    }

    public static void PreprocessHtmlData()
    {
        Console.WriteLine("HTML preprocessing started...");

        // Example HTML preprocessing
        string htmlContent = "<html><body><p>This is a sample HTML content.</p></body></html>";
        string cleanedText = HtmlHelper.CleanHtml(htmlContent);

        Console.WriteLine($"Cleaned HTML text: {cleanedText}");
        Console.WriteLine("HTML preprocessing completed.");
    }

    public static void TrainQAModel()
    {
        Console.WriteLine("Model training started...");

        // Load data from languageData.txt
        var dataView = _mlContext.Data.LoadFromTextFile<LanguageData>(_dataFilePath, separatorChar: '\t', hasHeader: true);

        // Define the ML.NET data preprocessing pipeline
        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features_Context", nameof(LanguageData.Context))
            .Append(_mlContext.Transforms.Text.FeaturizeText("Features_Question", nameof(LanguageData.Question)))
            .Append(_mlContext.Transforms.Concatenate("Features", "Features_Context", "Features_Question"))
            .Append(_mlContext.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: nameof(LanguageData.AnswerIndex)));

        // Train the model
        var model = pipeline.Fit(dataView);

        // Save the model for future predictions
        _mlContext.Model.Save(model, dataView.Schema, "model.zip");

        Console.WriteLine("Model trained and saved successfully.");
    }

    public static void ValidateModel()
    {
        Console.WriteLine("Model validation started...");

        // Load the trained model
        ITransformer model;
        using (var stream = new FileStream("Model.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            model = _mlContext.Model.Load(stream, out var modelSchema);
        }

        // Sample input for prediction
        var sampleData = new DataFrame(new List<DataFrameColumn>
    {
        new StringDataFrameColumn("ColumnToPredict", new[] { "Sample Prediction" }),
        new StringDataFrameColumn("Context", new[] { "<html><body><p>This is a sample HTML content.</p></body></html>" }),
        new StringDataFrameColumn("Question", new[] { "Sample Question" })
    });

        // Transform the sample data
        var transformedData = model.Transform(sampleData);

        // Extract predictions from transformed data
        var predictionColumn = transformedData.GetColumn<string>("ColumnToPredict");

        // Retrieve the prediction (assuming single prediction in this case)
        string prediction = predictionColumn.FirstOrDefault();

        Console.WriteLine($"Predicted Answer: {prediction}");

        Console.WriteLine("Model validation completed.");
    }

    // Define the data schema
    public class LanguageData
    {
        [LoadColumn(0)]
        public string ColumnToPredict { get; set; }

        [LoadColumn(1)]
        public string Context { get; set; }

        [LoadColumn(2)]
        public string Question { get; set; }

        [LoadColumn(3)]
        public float AnswerIndex { get; set; }
    }

    // Prediction output class
    public class LanguagePrediction
    {
        [ColumnName("PredictedLabel")]
        public float PredictedLabel { get; set; }
    }

    // Helper class for HTML preprocessing
    public static class HtmlHelper
    {
        public static string CleanHtml(string html)
        {
            // Implement HTML cleaning logic here
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Example: Extracting text from paragraphs
            var text = string.Join(" ", doc.DocumentNode.SelectNodes("//p")
                                     .Select(p => p.InnerText.Trim()));

            return text;
        }
    }
}
