using Microsoft.ML;
using Microsoft.ML.Data;
using HtmlAgilityPack;
using Microsoft.Data.Analysis;

/// <summary>
/// All-in-one Program.cs file that contains the sample implementation of Natural Language Question Answering Training
/// and Evaluation. This contains all the classes it needs to easily build and run in Visual Studio.
/// 
/// To test this:
/// 
/// 1. First create a new C# ConsoleApp using .net 8.0 framework or later.
/// 
/// 2. Add the necessary nuget packages. In a terminal, copy and run these commands:
///     dotnet add package HtmlAgilityPack
///     dotnet add package Microsoft.Data.Analysis
///     dotnet add package Microsoft.ML
///
/// 3. Create your ML training data source by adding a .txt file. Create sample data like this:
/// 
///     Column to predict	Context	Question	Answer Index
///     Capital<html> < body > Paris is the capital of France.</ body ></ html > What is the capital of France?	0
///     Author	<html><body>George Orwell wrote the novel 1984.</body></html>	Who wrote the novel "1984"?	0
///
/// 4. Copy this Program.cs code and modify it to declare your ML source data folder variables. 
///     For example:
/// 
///     private const string _mlFolder = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\ML";
///     private const string _languageDataFileName = "languageData.txt";
///     
/// 5. In Visual Studio, Press F5 to run to train and evaluate.
/// 
/// </summary>
public class Program
{
    // Declare your ML source data folder here:
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
        AnswerUserQuestion();
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

    public static void AnswerUserQuestion()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Ask a question (or type 'exit' to quit):");
            string question = Console.ReadLine();

            if (question.ToLower() == "exit")
                break;

            // Load the trained model for answering questions
            ITransformer model;
            using (var stream = new FileStream("Model.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                model = _mlContext.Model.Load(stream, out var modelSchema);
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<LanguageData, LanguagePrediction>(model);
            // Use ML.NET model to predict sentiment
            var prediction = predictionEngine.Predict(new LanguageData { Question = question });

            var response = prediction.PredictedLabel;

            Console.WriteLine($"Response: {response}");

        }
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
