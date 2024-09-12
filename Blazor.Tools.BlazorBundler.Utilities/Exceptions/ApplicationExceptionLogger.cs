namespace Blazor.Tools.BlazorBundler.Utilities.Exceptions
{
    public class ApplicationExceptionLogger
    {
        public static void HandleException(Exception ex)
        {
            // Log the primary exception
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Source: {ex.Source}");
            Console.WriteLine($"Target Site: {ex.TargetSite}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Console.WriteLine("----- Inner Exception -----");
                Console.WriteLine($"Error: {innerException.Message}");
                Console.WriteLine($"Source: {innerException.Source}");
                Console.WriteLine($"Target Site: {innerException.TargetSite}");
                Console.WriteLine($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }
        }
    }

}
