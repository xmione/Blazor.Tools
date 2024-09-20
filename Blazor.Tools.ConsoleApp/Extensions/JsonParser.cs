using Newtonsoft.Json;
using System.Diagnostics;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public class JsonlParser
    {
        public IEnumerable<QuestionAnsweringData> ParseJsonlFile(string filePath)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var jsonlReadStopwatch = Stopwatch.StartNew();
            var dateToday = DateTime.Now.ToString("yyyy-dd-MM_HH-mm-ss");
            var logFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? default!, $"parsing_log-{dateToday}.txt");

            List<QuestionAnsweringData> qaDataList = new List<QuestionAnsweringData>();

            using (var logWriter = new StreamWriter(logFilePath, append: false))
            {
                Console.WriteLine($"Reading jsonl file {filePath}...");
                logWriter.WriteLine($"Reading jsonl file {filePath}...");
                var lines = File.ReadLines(filePath).ToList();
                jsonlReadStopwatch.Stop();
                var jsonlReadDuration = jsonlReadStopwatch.Elapsed;

                Console.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);
                logWriter.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);

                var linesCount = lines.Count;
                int completedTasks = 0;
                var logWriterLock = new object();

                Parallel.For(0, linesCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var line = lines[i];
                    int currentCount = Interlocked.Increment(ref completedTasks);
                    double progress = (double)currentCount / (double)linesCount * 100;

                    Console.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");
                    lock (logWriterLock)
                    {
                        logWriter.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");
                    }

                    try
                    {
                        // Deserialize JSON line into dictionary
                        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);

                        // Create QuestionAnsweringData object
                        QuestionAnsweringData qaData = new QuestionAnsweringData();
                        if (jsonData != null)
                        {
                            // Map properties from jsonData to qaData
                            foreach (var kvp in jsonData)
                            {
                                var kvpKey = kvp.Key;
                                var kvpValue = kvp.Value?.ToString() ?? string.Empty;
                                switch (kvpKey)
                                {
                                    case "question_text":
                                        qaData.Question = kvpValue;
                                        break;
                                    case "document_text":
                                        qaData.Answer = kvpValue;
                                        break;
                                    default:
                                        if (qaData.Context == null)
                                            qaData.Context = kvpValue + "\n";
                                        else
                                            qaData.Context += kvpValue + "\n";
                                        break;
                                }
                            }

                        }

                        // Add the constructed QuestionAnsweringData to the list
                        lock (qaDataList)
                        {
                            qaDataList.Add(qaData);
                        }
                    }
                    catch (Exception ex)
                    {
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

            return qaDataList;
        }

        public IEnumerable<OriginalQuestionAnsweringData> ParseJsonlFileToOriginal(string filePath)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var jsonlReadStopwatch = Stopwatch.StartNew();
            var dateToday = DateTime.Now.ToString("yyyy-dd-MM_HH-mm-ss");
            var fileFolderName = Path.GetDirectoryName(filePath) ?? string.Empty;
            var logFilePath = Path.Combine(fileFolderName, $"parsing_log-{dateToday}.txt");

            List<OriginalQuestionAnsweringData> qaDataList = new List<OriginalQuestionAnsweringData>();

            using (var logWriter = new StreamWriter(logFilePath, append: false))
            {
                Console.WriteLine($"Reading jsonl file {filePath}...");
                logWriter.WriteLine($"Reading jsonl file {filePath}...");
                var lines = File.ReadLines(filePath).ToList();
                jsonlReadStopwatch.Stop();
                var jsonlReadDuration = jsonlReadStopwatch.Elapsed;

                Console.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);
                logWriter.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);

                var linesCount = lines.Count;
                int completedTasks = 0;
                var logWriterLock = new object();

                Parallel.For(0, linesCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var line = lines[i];
                    int currentCount = Interlocked.Increment(ref completedTasks);
                    double progress = (double)currentCount / (double)linesCount * 100;

                    Console.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");
                    lock (logWriterLock)
                    {
                        logWriter.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");
                    }

                    try
                    {
                        // Deserialize JSON line into dictionary
                        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(line);

                        // Create QuestionAnsweringData object
                        OriginalQuestionAnsweringData qaData = new OriginalQuestionAnsweringData();

                        if (jsonData != null)
                        {
                            // Map properties from jsonData to qaData
                            foreach (var kvp in jsonData)
                            {
                                var kvpKey = kvp.Key;
                                var kvpValue = kvp.Value.ToString() ?? string.Empty;
                                switch (kvpKey)
                                {
                                    case "annotations":
                                        qaData.Annotations = kvpValue;
                                        break;
                                    case "example_id":
                                        qaData.ExampleId = kvpValue;
                                        break;
                                    case "long_answer_candidates":
                                        qaData.LongAnswerCandidates = kvpValue;
                                        break;
                                    case "question_text":
                                        qaData.QuestionText = kvpValue;
                                        break;
                                    case "document_url":
                                        qaData.DocumentUrl = kvpValue;
                                        break;
                                    case "document_html":
                                        qaData.DocumentHtml = kvpValue;
                                        break;
                                    case "document_title":
                                        qaData.DocumentTitle = kvpValue;
                                        break;
                                    case "document_tokens":
                                        qaData.DocumentTokens = kvpValue;
                                        break;
                                    case "questions_tokens":
                                        qaData.QuestionTokens = kvpValue;
                                        break;
                                }
                            }
                        }

                        // Add the constructed QuestionAnsweringData to the list
                        lock (qaDataList)
                        {
                            qaDataList.Add(qaData);
                        }
                    }
                    catch (Exception ex)
                    {
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

            return qaDataList;
        }

        public IEnumerable<QuestionAnsweringData> ConvertToQAData(IEnumerable<OriginalQuestionAnsweringData> originalDataList) 
        {
            List<QuestionAnsweringData> qaData = new List<QuestionAnsweringData>();

            foreach (OriginalQuestionAnsweringData originalData in originalDataList)
            {
                var qa = new QuestionAnsweringData 
                {
                    Question = originalData.QuestionText,
                    Answer = originalData.DocumentHtml,
                    Context = originalData.LongAnswerCandidates 
                };

                qaData.Add(qa);
            }

            return qaData;
        }
    }
}
