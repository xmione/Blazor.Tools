using Microsoft.ML;
using Microsoft.ML.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.ML.Trainers;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public static class NqEntryExtensions
    {
        public static void DecompressAndParseJsonlFile(this NqEntry entry, string gzipFilePath, string outputFilePath, AnswerConfig config)
        {
            // Decompress the gzip file to outputFilePath
            DecompressGzipFile(gzipFilePath, outputFilePath);
        }

        private static void DecompressGzipFile(string gzipFilePath, string outputFilePath)
        {
            const int bufferSize = 4096; // Define the buffer size for reading chunks
            byte[] buffer = new byte[bufferSize];
            int totalBytesRead = 0;
            int bytesRead;

            using (FileStream originalFileStream = new FileStream(gzipFilePath, FileMode.Open, FileAccess.Read))
            {
                long totalSize = originalFileStream.Length; // Get the total size of the compressed file

                using (FileStream decompressedFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    while ((bytesRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        decompressedFileStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        double percentComplete = (double)totalBytesRead / (double)totalSize * 100;
                        Console.WriteLine($"Decompressed {totalBytesRead} / {totalSize} bytes so far ({percentComplete:F2}% complete)...");
                    }
                }
            }

            Console.WriteLine("Decompression completed.");
        }

        public static void ParseJsonlFile(string jsonlFilePath, string trainingFilePath)
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

                // Get available properties
                var properties = LogAvailableProperties(jsonlFilePath, lines, logWriter);

                var logWriterLock = new object();
                var outputWriterLock = new object();

                using (var outputFileStream = new StreamWriter(trainingFilePath, append: false))
                {
                    // Write the header
                    outputFileStream.WriteLine("Context\tQuestionText\tAnnotations");

                    var linesCount = lines.Count;
                    int completedTasks = 0;

                    Parallel.For(0, linesCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                    {
                        if (i + 1 == linesCount )
                        {
                            Console.WriteLine($"Line is {i}");
                        }
                        var line = lines[i];
                        int currentCount = Interlocked.Increment(ref completedTasks);
                        double progress = (double)currentCount / (double)linesCount * 100;

                        Console.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");
                        logWriter.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");

                        try
                        {
                            var jsonDocument = JsonDocument.Parse(line);
                            var rootElement = jsonDocument.RootElement;

                            // Prepare TrainingData object
                            var trainingData = new TrainingData
                            {
                                Context = string.Empty, // Initialize Context
                                QuestionText = string.Empty,
                                Annotations = string.Empty
                            };

                            // Populate TrainingData from properties
                            foreach (var kvp in properties)
                            {
                                switch (kvp.Key)
                                {
                                    case "annotations":
                                        trainingData.Annotations = kvp.Value.ToString();
                                        break;
                                    case "question_text":
                                        trainingData.QuestionText = kvp.Value.ToString();
                                        Console.WriteLine($"Question {kvp.Value}");
                                        logWriter.WriteLine($"Question {kvp.Value}");

                                        break;
                                    case "document_text": // Example of adding non-specific property to Context
                                    case "long_answer_candidates":
                                    case "document_url":
                                    case "example_id":
                                        trainingData.Context += kvp.Value.ToString() + "\n";
                                        break;
                                    default:
                                        // Add to Context if not directly related to annotations or question_text
                                        trainingData.Context += kvp.Value.ToString() + "\n";
                                        break;
                                }
                            }

                            // Prepare tab-delimited data
                            var tabDelimitedData = $"{trainingData.Context}\t{trainingData.QuestionText}\t{trainingData.Annotations}";

                            // Write to output file in a thread-safe manner
                            lock (outputWriterLock)
                            {
                                outputFileStream.WriteLine(tabDelimitedData);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error in a thread-safe manner
                            lock (logWriterLock)
                            {
                                Console.WriteLine("Error parsing line: {0}", ex.Message);
                                logWriter.WriteLine("Error parsing line: {0}", ex.Message);
                            }
                        }
                    });

                    totalStopwatch.Stop();
                    var totalDuration = totalStopwatch.Elapsed;
                    Console.WriteLine("Total duration for reading file: {0:hh\\:mm\\:ss}", totalDuration);
                    logWriter.WriteLine("Total duration for reading file: {0:hh\\:mm\\:ss}", totalDuration);
                }
            }
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                TypeInfoResolver = JsonContext.Default
            };
        }

        public static Dictionary<string, object> LogAvailableProperties(string jsonlFilePath, IEnumerable<string> lines, TextWriter logWriter)
        {
            Console.WriteLine("Logging available properties in JSONL file: {0}", jsonlFilePath);
            logWriter.WriteLine("Logging available properties in JSONL file: {0}", jsonlFilePath);

            Dictionary<string, object> properties = new Dictionary<string, object>();

            try
            {
                string firstLine = lines.FirstOrDefault() ?? string.Empty;
                if (!string.IsNullOrEmpty(firstLine))
                {
                    var jsonDocument = JsonDocument.Parse(firstLine);
                    var rootElement = jsonDocument.RootElement;

                    foreach (var prop in rootElement.EnumerateObject())
                    {
                        Console.WriteLine($"Available property: {prop.Name}");
                        logWriter.WriteLine($"Available property: {prop.Name}");
                        properties[prop.Name] = prop.Value.Clone(); // Clone to avoid issues with disposing JsonDocument
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error logging available properties: {0}", ex.Message);
                logWriter.WriteLine("Error logging available properties: {0}", ex.Message);
            }

            return properties;

        }

        /// <summary>
        /// Extract answer text from the JSONL file (assuming it's directly embedded or referenced)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static string ExtractAnswerText(string line, int start, int end)
        {
            start = Math.Max(0, Math.Min(start, line.Length));
            end = Math.Max(start, Math.Min(end, line.Length));
            return line.Substring(start, end - start);
        }

        public static void TrainModel(IEnumerable<QuestionAnsweringData> qaDataList, string modelSavePath)
        {
            try
            {
                // Create MLContext for training
                var mlContext = new MLContext();

                // Define schema definition
                var schemaDefinition = SchemaDefinition.Create(typeof(QuestionAnsweringData));
                schemaDefinition["ID"].ColumnType = NumberDataViewType.Int32;
                schemaDefinition["Question"].ColumnType = TextDataViewType.Instance;
                schemaDefinition["Context"].ColumnType = TextDataViewType.Instance;
                schemaDefinition["Answer"].ColumnType = TextDataViewType.Instance;

                // Load data into IDataView
                var dataView = mlContext.Data.LoadFromEnumerable(qaDataList, schemaDefinition);

                // Data preprocessing pipeline
                var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(QuestionAnsweringData.Question))
                    .Append(mlContext.Transforms.Text.FeaturizeText("ContextFeatures", nameof(QuestionAnsweringData.Context)))
                    .Append(mlContext.Transforms.Concatenate("Features", "Features", "ContextFeatures"))
                    .Append(mlContext.Transforms.CopyColumns("Label", nameof(QuestionAnsweringData.Answer)));

                // Choose a model and algorithm
                var trainer = mlContext.Regression.Trainers.LbfgsPoissonRegression();

                var trainingPipeline = pipeline.Append(trainer);

                // Train the model
                var model = trainingPipeline.Fit(dataView);

                // Save the model
                mlContext.Model.Save(model, dataView.Schema, modelSavePath);

                Console.WriteLine("Model trained and saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error training model: {ex.Message}");
            }
        }

    }

}
