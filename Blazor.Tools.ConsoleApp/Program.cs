using Blazor.Tools.ConsoleApp.Extensions;
using Microsoft.ML;
using System.Diagnostics;
using HtmlAgilityPack;

namespace Blazor.Tools.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var answerConfig = new AnswerConfig();
            var mlFolder = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\ML";
            var fileName = string.Empty;
            var trainingFilePath = string.Empty;
            var outputFilePath = string.Empty;
            var dateToday = DateTime.Now.ToString("yyyy-dd-MM_HH-mm-ss");
            var connectionString = "Server=(local);Database=AIDatabase;User Id=sa;Password=P@ssw0rd123;TrustServerCertificate=True;";
            var originalDataAccess = new OriginalQuestionAnsweringDataAccess(connectionString);
            var qaDataAccess = new QuestionAnsweringDataAccess(connectionString);
            IEnumerable<QuestionAnsweringData> questionAnswerList = default!;
            IEnumerable<OriginalQuestionAnsweringData> originalQuestionAnswerList = default!;
            var jsonParser = new JsonlParser();

            while (true)
            {
                Console.WriteLine("Please choose an option:");
                Console.WriteLine("[1] - Convert Tab Delimited Yelp file to CSV");
                Console.WriteLine("[2] - Log Available Properties v1.0-simplified_nq-dev-all.jsonl");
                Console.WriteLine("[3] - Log Available Properties v1.0-simplified_simplified-nq-train.jsonl");
                Console.WriteLine("[4] - Decompress and parse v1.0-simplified_nq-dev-all.jsonl.gz");
                Console.WriteLine("[5] - Decompress and parse v1.0-simplified_simplified-nq-train.jsonl.gz");
                Console.WriteLine("[6] - Parse and save v1.0-simplified_nq-dev-all.jsonl");
                Console.WriteLine("[7] - Parse and save v1.0-simplified_simplified-nq-train.jsonl");
                Console.WriteLine("[8] - Parse Json File and Save to database");
                Console.WriteLine("[9] - Get Language training data from Database then train model");
                Console.WriteLine("[10] - Train sample Language Data");
                Console.WriteLine("[11] - Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConvertTabDelimitedFileToCsv(mlFolder);
                        break;
                    case "2":
                        fileName = Path.Combine(mlFolder,"v1.0-simplified_nq-dev-all.jsonl");
                        LogAvailableProperties(fileName);
                        break;
                    case "3":
                        fileName = Path.Combine(mlFolder, "v1.0-simplified_simplified-nq-train.jsonl");
                        LogAvailableProperties(fileName);
                        break;
                    case "4":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_byte",
                            EndProperty = "end_byte",
                            IsBytePosition = true
                        };

                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        DecompressAndParseJsonlFile(mlFolder, fileName, answerConfig);
                        break;
                    case "5":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_token",
                            EndProperty = "end_token",
                            IsBytePosition = false
                        };

                        fileName = "v1.0-simplified_simplified-nq-train.jsonl";
                        DecompressAndParseJsonlFile(mlFolder, fileName, answerConfig);
                        break;
                    case "6":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_byte",
                            EndProperty = "end_byte",
                            IsBytePosition = true
                        };

                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        NqEntryExtensions.ParseJsonlFile(outputFilePath, trainingFilePath);
                        break;
                    case "7":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_token",
                            EndProperty = "end_token",
                            IsBytePosition = false
                        };

                        fileName = "v1.0-simplified_simplified-nq-train.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        NqEntryExtensions.ParseJsonlFile(outputFilePath, trainingFilePath);
                        break;
                    case "8":
                       
                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        
                        originalQuestionAnswerList = jsonParser.ParseJsonlFileToOriginal(outputFilePath);
                                                
                        foreach (var item in originalQuestionAnswerList)
                        { 
                            originalDataAccess.Create(item);
                        }
                                                
                        break;
                    case "9":
                        var progress = new Progress<double>(percentage =>
                        {
                            Console.WriteLine($"Progress: {percentage:F2}%");
                        });
                        
                        ConvertToQAData(originalDataAccess, originalQuestionAnswerList, questionAnswerList, jsonParser, trainingFilePath, progress);
                        
                        break;
                    case "10":
                        fileName = "languageData.txt";
                        TrainQuestionAnswers(mlFolder, fileName);
                        break; 
                    case "11":
                        return; 
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        public static void LogAvailableProperties(string jsonlFilePath)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var jsonlReadStopwatch = Stopwatch.StartNew();
            var dateToday = DateTime.Now.ToString("yyyy-dd-MM_HH-mm-ss");
            var logFilePath = Path.Combine(Path.GetDirectoryName(jsonlFilePath), $"parsing_log-{dateToday}.txt");

            using (var logWriter = new StreamWriter(logFilePath, append: false))
            {
                Console.WriteLine($"Reading jsonl file {jsonlFilePath}...");
                logWriter.WriteLine($"Reading jsonl file {jsonlFilePath}...");
                var lines = File.ReadLines(jsonlFilePath).ToList();
                jsonlReadStopwatch.Stop();
                var jsonlReadDuration = jsonlReadStopwatch.Elapsed;

                Console.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);
                logWriter.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);

                NqEntryExtensions.LogAvailableProperties(jsonlFilePath, lines, logWriter);

                totalStopwatch.Stop();
                var totalDuration = totalStopwatch.Elapsed;
                Console.WriteLine("Total duration for reading file: {0:hh\\:mm\\:ss}", totalDuration);
                logWriter.WriteLine("Total duration for reading file: {0:hh\\:mm\\:ss}", totalDuration);
            }
        }

        private static void ConvertTabDelimitedFileToCsv(string folder)
        {
            string tabDelimitedFilePath = Path.Combine(folder, "yelp_labelled.txt");
            string csvFilePath = Path.Combine(folder, "yelp_labelled.csv");  

            tabDelimitedFilePath.ConvertTabDelimitedFileToCsv(csvFilePath);
        }

        public static void DecompressAndParseJsonlFile(string folder, string fileName, AnswerConfig config)
        {
            string gzipFilePath = Path.Combine(folder, fileName + ".gz");
            string outputFilePath = Path.Combine(folder, fileName);

            var entry = new NqEntry();
            entry.DecompressAndParseJsonlFile(gzipFilePath, outputFilePath, config);
        }

        public static void ConvertToQAData(OriginalQuestionAnsweringDataAccess originalDataAccess,
                                   IEnumerable<OriginalQuestionAnsweringData> originalQuestionAnswerList,
                                   IEnumerable<QuestionAnsweringData> questionAnswerList,
                                   JsonlParser jsonParser, string trainingFilePath,
                                   IProgress<double> progress = null)
        {
            // Fetch total record count
            int totalRecords = originalDataAccess.GetTotalRecordCount();

            // Define batch size
            int batchSize = 100; // Adjust as per your application's needs

            // Loop through batches
            for (int pageNumber = 1; pageNumber <= (totalRecords + batchSize - 1) / batchSize; pageNumber++)
            {
                // Fetch data for current page
                originalQuestionAnswerList = originalDataAccess.ListAll(batchSize, pageNumber);

                // Convert to QuestionAnsweringData using your JsonParser
                questionAnswerList = jsonParser.ConvertToQAData(originalQuestionAnswerList);

                // Train model with current batch
                NqEntryExtensions.TrainModel(questionAnswerList, trainingFilePath);

                // Report progress
                if (progress != null)
                {
                    double progressPercentage = (double)pageNumber / ((totalRecords + batchSize - 1) / batchSize) * 100;
                    progress.Report(progressPercentage);
                }
            }
        }

        public static void TrainQuestionAnswers(string folder, string fileName)
        {
            var mlContext = new MLContext();
            var dataFile = Path.Combine(folder, fileName);

            // Load the data
            var data = mlContext.Data.LoadFromTextFile<QuestionAnswer>(dataFile, hasHeader: true, separatorChar: '\t');

            // Preprocess HTML
            var preprocessedData = mlContext.Data.CreateEnumerable<QuestionAnswer>(data, reuseRowObject: false)
                .Select(row =>
                {
                    row.Context = PreprocessHtml(row.Context);
                    return row;
                });

            var preprocessedDataView = mlContext.Data.LoadFromEnumerable(preprocessedData);

            // Define the training pipeline
            var pipeline = mlContext.Transforms.Text.FeaturizeText("ContextFeaturized", nameof(QuestionAnswer.Context))
                .Append(mlContext.Transforms.Text.FeaturizeText("QuestionFeaturized", nameof(QuestionAnswer.Question)))
                .Append(mlContext.Transforms.Concatenate("Features", "ContextFeaturized", "QuestionFeaturized"))
                .Append(mlContext.Transforms.CopyColumns("Label", nameof(QuestionAnswer.AnswerIndex)))
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", maximumNumberOfIterations: 100));

            // Train the model
            var model = pipeline.Fit(preprocessedDataView);

            Console.WriteLine("Model training complete.");
        }

        public static string PreprocessHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText;
        }
    }
}
