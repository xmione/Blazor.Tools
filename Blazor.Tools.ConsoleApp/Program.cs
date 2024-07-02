using Blazor.Tools.ConsoleApp.Extensions;

namespace Blazor.Tools.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            args = ["2"];

            switch (args[0])
            {
                case "1":
                    ConvertTabDelimitedFileToCsv();
                    break;
                case "2":
                    DecompressAndParseJsonlFile();
                    break;
            }
        }

        private static void ConvertTabDelimitedFileToCsv()
        {
            string tabDelimitedFilePath = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\yelp_labelled.txt";
            string csvFilePath = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\yelp_labelled.csv"; 

            tabDelimitedFilePath.ConvertTabDelimitedFileToCsv(csvFilePath);
        }

        public static void DecompressAndParseJsonlFile()
        {
            string gzipFilePath = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\v1.0-simplified_nq-dev-all.jsonl.gz";
            string outputFilePath = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\v1.0-simplified_nq-dev-all.jsonl";

            var entry = new NqEntry();
            entry.DecompressAndParseJsonlFile(gzipFilePath, outputFilePath);

            // Use the entry for training your model or other operations
            Console.WriteLine($"Question: {entry.Question}");
            foreach (var answer in entry.Answers)
            {
                Console.WriteLine($"Answer: {answer}");
            }
        }
    }
}
