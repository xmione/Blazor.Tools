using System.Diagnostics.Tracing;

namespace Blazor.Tools.ConsoleApp.Extensions
{
    public static class StringExtensions
    {
        public static string ConvertTabDelimeterToCommaDelimeter(this string tabDelimeterString)
        {
            if (string.IsNullOrEmpty(tabDelimeterString))
            {
                return string.Empty;
            }

            string[] tabDelimetedArray = tabDelimeterString.Split('\t');
            string commaDelimetedString = string.Join(",", tabDelimetedArray);

            return commaDelimetedString;
        }
    }

}
