using Microsoft.ML;
using Microsoft.ML.Data;
using HtmlAgilityPack;
using Microsoft.Data.Analysis;
using NaturalLanguageQA;

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
    private const string _columnToPredict = "ColumnToPredict";
    private static string _dataFilePath = string.Empty;
    private static string _zipFileName = "Model.zip";
    private static MLContext _mlContext;
    public static void Main(string[] args)
    {
        try 
        {
            _dataFilePath = Path.Combine(_mlFolder, _languageDataFileName);

            SaveLoadModel.Example(_mlFolder, _languageDataFileName);
            //// ML.NET model training pipeline
            //_mlContext = new MLContext();

            //// Data preparation and model training flow
            //PreprocessHtmlData();
            //TrainQAModel();
            //ValidateModel();
            //AnswerUserQuestion();
        } 
        catch (Exception ex) 
        {
            Console.WriteLine("Error: {0}", ex.Message);
        }
               
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
        _mlContext.Model.Save(model, dataView.Schema, _zipFileName);

        Console.WriteLine("Model trained and saved successfully.");
    }

    public static void ValidateModel()
    {
        Console.WriteLine("Model validation started...");

        // Load the trained model
        ITransformer model;
        using (var stream = new FileStream(_zipFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            model = _mlContext.Model.Load(stream, out var modelSchema);

            // Inspect the schema
            var columns = new List<DataFrameColumn>();
            foreach (var column in modelSchema)
            {
                var columnType = column.Type.ToString();
                switch (columnType)
                {
                    case "String":
                        columns.Add(new StringDataFrameColumn(column.Name, new string[] { "Sample " + column.Name }));
                        break;
                    case "Boolean":
                        columns.Add(new BooleanDataFrameColumn(column.Name, new bool[] { true }));
                        break;
                    case "Single":
                        columns.Add(new SingleDataFrameColumn(column.Name, new float[] { 1.0f }));
                        break;
                    default:
                        throw new NotImplementedException($"Data type {column.Type.RawType.Name} not implemented");
                }
            }

            // Create the DataFrame
            var sampleData = new DataFrame(columns);

            // Transform the sample data
            var transformedData = model.Transform(sampleData);

            // Extract predictions from transformed data
            var predictionColumn = transformedData.GetColumn<string>(_columnToPredict);

            // Retrieve the prediction (assuming single prediction in this case)
            string prediction = predictionColumn.FirstOrDefault();

            Console.WriteLine($"Predicted Answer: {prediction}");
        }

        Console.WriteLine("Model validation completed.");
    }

    public static void AnswerUserQuestion()
{
    // Initialize MLContext and load the model
    var mlContext = new MLContext();
    ITransformer model;

    using (var stream = new FileStream(_zipFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
        model = mlContext.Model.Load(stream, out var modelSchema);
    }

    // Create prediction engine
    var predictionEngine = mlContext.Model.CreatePredictionEngine<LanguageData, LanguagePrediction>(model);

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("Ask a question (or type 'exit' to quit):");
        string question = Console.ReadLine();

        if (question.ToLower() == "exit")
            break;

        // Prepare input data
        var input = new LanguageData { Question = question };

        // Predict
        var prediction = predictionEngine.Predict(input);

        // Retrieve predicted answer
        string predictedAnswer = prediction.PredictedAnswer;

        Console.WriteLine($"Predicted Answer: {predictedAnswer}");
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
        [ColumnName("PredictedAnswer")]
        public string PredictedAnswer { get; set; }
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
