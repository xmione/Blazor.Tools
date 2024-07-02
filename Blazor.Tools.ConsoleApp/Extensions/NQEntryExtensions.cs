using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

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
            using (FileStream decompressedFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            {
                while ((bytesRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    decompressedFileStream.Write(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    Console.WriteLine($"Decompressed {totalBytesRead} bytes so far...");
                }
            }

            Console.WriteLine("Decompression completed.");
        }

        public static void ParseJsonlFile(string jsonlFilePath, AnswerConfig config, string trainingFilePath)
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

                var logWriterLock = new object();
                var outputWriterLock = new object();

                using (var outputFileStream = new StreamWriter(trainingFilePath, append: false))
                {
                    LogAvailableProperties(jsonlFilePath, lines, logWriter);

                    var linesCount = lines.Count;
                    // Define a counter for completed tasks
                    int completedTasks = 0;

                    Parallel.For(0, linesCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                    {
                        var line = lines[i];

                        // Increment completed tasks atomically
                        int currentCount = Interlocked.Increment(ref completedTasks);

                        // Calculate progress percentage safely
                        double progress = (double)currentCount / (double)linesCount * 100;

                        Console.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");
                        logWriter.WriteLine($"Reading Line {currentCount}/{linesCount} ({progress:F2}%)...");

                        try
                        {
                            var jsonDocument = JsonDocument.Parse(line);
                            var rootElement = jsonDocument.RootElement;

                            var trainingData = new TrainingData
                            {
                                QuestionText = rootElement.GetProperty("question_text").GetString() ?? string.Empty,
                                DocumentText = rootElement.GetProperty("document_text").GetString() ?? string.Empty,
                                LongAnswerCandidates = rootElement.GetProperty("long_answer_candidates").ToString(),
                                DocumentUrl = rootElement.GetProperty("document_url").GetString() ?? string.Empty,
                                ExampleId = rootElement.GetProperty("example_id").ToString() ?? string.Empty
                            };

                            if (rootElement.TryGetProperty("annotations", out var annotationsElement))
                            {
                                trainingData.Annotations = annotationsElement.ToString();
                            }

                            // Serialize trainingData to JSON
                            var serializedData = JsonSerializer.Serialize(trainingData, GetJsonSerializerOptions());

                            // Write to output file in a thread-safe manner
                            lock (outputWriterLock)
                            {
                                outputFileStream.WriteLine(serializedData);
                            }

                            // Log success in a thread-safe manner
                            //lock (logWriterLock)
                            //{
                            //    Console.WriteLine($"Saved Training Data: {trainingData.QuestionText}");
                            //    logWriter.WriteLine($"Saved Training Data: {trainingData.QuestionText}");
                            //}

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

        private static void LogAvailableProperties(string jsonlFilePath, IEnumerable<string> lines, TextWriter logWriter)
        {
            Console.WriteLine("Logging available properties in JSONL file: {0}", jsonlFilePath);
            logWriter.WriteLine("Logging available properties in JSONL file: {0}", jsonlFilePath);

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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error logging available properties: {0}", ex.Message);
                logWriter.WriteLine("Error logging available properties: {0}", ex.Message);
            }
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

    }
    
}
