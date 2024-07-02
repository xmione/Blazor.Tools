using System.IO.Compression;
using System.Text.Json;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public static class NqEntryExtensions
    {
        public static void DecompressAndParseJsonlFile(this NqEntry entry, string gzipFilePath, string outputFilePath)
        {
            // Decompress the gzip file to outputFilePath
            DecompressGzipFile(gzipFilePath, outputFilePath);

            // Parse the JSONL file from outputFilePath
            var entries = ParseJsonlFile(outputFilePath);

            // Set entry's question and answers
            entry.Question = entries.Count > 0 ? entries[0].Question : null;
            entry.Answers = entries.Count > 0 ? entries[0].Answers : null;
        }

        private static void DecompressGzipFile(string gzipFilePath, string outputFilePath)
        {
            using (FileStream originalFileStream = new FileStream(gzipFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream decompressedFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedFileStream);
            }
        }

        public static List<NqEntry> ParseJsonlFile(string jsonlFilePath)
        {
            var entries = new List<NqEntry>();
            foreach (var line in File.ReadLines(jsonlFilePath))
            {
                var jsonDocument = JsonDocument.Parse(line);

                var entry = new NqEntry
                {
                    Question = jsonDocument.RootElement.GetProperty("question_text").GetString(),
                    Answers = new List<string>()
                };
                
                Console.WriteLine("Question: {0}", entry.Question);

                var annotations = jsonDocument.RootElement.GetProperty("annotations").EnumerateArray();
                foreach (var annotation in annotations)
                {
                    var shortAnswersArray = annotation.GetProperty("short_answers").EnumerateArray();
                    foreach (var answer in shortAnswersArray)
                    {
                        var startByte = answer.GetProperty("start_byte").GetInt32();
                        var endByte = answer.GetProperty("end_byte").GetInt32();

                        // Extract answer text from the JSONL file (assuming it's directly embedded or referenced)
                        var answerText = ExtractAnswerText(line, startByte, endByte);
                        entry.Answers.Add(answerText);

                        Console.WriteLine("Answer: {0}", answerText);
                    }
                }

                entries.Add(entry);
            }

            return entries;
        }

        private static string ExtractAnswerText(string line, int startByte, int endByte)
        {
            // Assuming line contains the entire JSON object as a string, parse it again if needed
            var jsonDocument = JsonDocument.Parse(line);

            // Ensure startByte and endByte are within valid range of line length
            startByte = Math.Max(0, Math.Min(startByte, line.Length));
            endByte = Math.Max(startByte, Math.Min(endByte, line.Length));

            // Extract substring based on byte positions
            return line.Substring(startByte, endByte - startByte);
        }
    }
}
