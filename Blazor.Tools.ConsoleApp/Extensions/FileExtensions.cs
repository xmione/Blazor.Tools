using System.IO;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public static class FileExtensions
    {
        public static IEnumerable<string> ReadLines(this string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadLines(filePath);
            }
            else
            {
                throw new FileNotFoundException($"The file at path {filePath} does not exist.");
            }
        }

        public static void WriteLines(this string filePath, IEnumerable<string> lines)
        {
            File.WriteAllLines(filePath, lines);
        }

        public static void ConvertTabDelimitedFileToCsv(this string tabDelimitedFilePath, string csvFilePath)
        {
            var tabDelimitedLines = tabDelimitedFilePath.ReadLines();
            var commaDelimitedLines = tabDelimitedLines.Select(line => line.ConvertTabDelimeterToCommaDelimeter());
            csvFilePath.WriteLines(commaDelimitedLines);
        }
    }

}
