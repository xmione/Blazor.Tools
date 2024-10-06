/*====================================================================================================
    Class Name  : StringExtensions
    Created By  : Solomio S. Sisante
    Created On  : August 30, 2024
    Purpose     : To provide an extension method for string manipulations.
  ====================================================================================================*/
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class StringExtensions
    {
        // Convert to PascalCase
        public static string ToPascalCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            StringBuilder resultBuilder = new StringBuilder();

            // Split the text by non-alphanumeric characters
            string[] words = Regex.Split(text, @"[^a-zA-Z0-9]+");

            // Capitalize the first letter of each word and add it to the result
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    // Capitalize the first letter and add the rest of the word
                    resultBuilder.Append(char.ToUpper(word[0]) + word.Substring(1));
                }
            }

            return resultBuilder.ToString();
        }

        // Convert to camelCase
        public static string ToCamelCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Convert to PascalCase first
            string pascalCase = text.ToPascalCase();

            // Lowercase the first letter of PascalCase string
            if (!string.IsNullOrEmpty(pascalCase))
            {
                return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
            }

            return pascalCase;
        }

        public static string RemoveLines(this string content, string keyword)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keyword))
                return content; // Return the content as is if content or keyword is null or empty.

            // Split the content into lines, filter out lines containing the keyword, and join them back together
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filteredLines = lines.Where(line => !line.Contains(keyword));
            return string.Join(Environment.NewLine, filteredLines);
        }
    }
}
