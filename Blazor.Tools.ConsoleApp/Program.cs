using Blazor.Tools.ConsoleApp.Extensions;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Net.Http.Headers;
using static Blazor.Tools.ConsoleApp.Program;

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

            while (true)
            {
                Console.WriteLine("Please choose an option:");
                Console.WriteLine("[1] - Convert Tab Delimited Yelp file to CSV");
                Console.WriteLine("[2] - Decompress and parse v1.0-simplified_nq-dev-all.jsonl.gz");
                Console.WriteLine("[3] - Decompress and parse v1.0-simplified_simplified-nq-train.jsonl.gz");
                Console.WriteLine("[4] - Parse and save v1.0-simplified_nq-dev-all.jsonl");
                Console.WriteLine("[5] - Parse and save v1.0-simplified_simplified-nq-train.jsonl");
                Console.WriteLine("[6] - Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConvertTabDelimitedFileToCsv(mlFolder);
                        break;
                    case "2":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_byte",
                            EndProperty = "end_byte",
                            IsBytePosition = true
                        };

                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        DecompressAndParseJsonlFile(mlFolder, fileName, answerConfig);
                        break;
                    case "3":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_token",
                            EndProperty = "end_token",
                            IsBytePosition = false
                        };

                        fileName = "v1.0-simplified-nq-train.jsonl";
                        DecompressAndParseJsonlFile(mlFolder, fileName, answerConfig);
                        break;
                    case "4":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_byte",
                            EndProperty = "end_byte",
                            IsBytePosition = true
                        };

                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        NqEntryExtensions.ParseJsonlFile(outputFilePath, answerConfig, trainingFilePath);
                        break;
                    case "5":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_token",
                            EndProperty = "end_token",
                            IsBytePosition = false
                        };

                        fileName = "v1.0-simplified-nq-train.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        NqEntryExtensions.ParseJsonlFile(outputFilePath, answerConfig, trainingFilePath);
                        break;
                    case "6":
                        return; // Exit the program
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
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
    }
}
