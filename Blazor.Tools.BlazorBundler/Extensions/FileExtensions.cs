/*====================================================================================================
    Class Name  : FileExtensions
    Created By  : Solomio S. Sisante
    Created On  : September 1, 2024
    Purpose     : To help in manipulating files.
  ====================================================================================================*/
namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class FileExtensions
    {
        /// <summary>
        /// Reads the contents of a file and returns it as a string.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The contents of the file as a string.</returns>
        public static string ReadFileContents(this string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            try
            {
                return File.ReadAllText(filePath);
            }
            catch (IOException ex)
            {
                throw new IOException("Error reading the file.", ex);
            }
        }
    }

}
