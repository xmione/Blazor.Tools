/*====================================================================================================
    Class Name  : ApplicationExceptionLogger
    Created By  : Solomio S. Sisante
    Created On  : September 12, 2024
    Purpose     : To handle general application exceptions and write it to the appropriate debug 
                  output medium.
  ====================================================================================================*/

using Serilog;
using System.Diagnostics;

namespace Blazor.Tools.BlazorBundler.Utilities.Exceptions
{
    public class ApplicationExceptionLogger
    {
        private const int REPL = 222;
        // Initialize Serilog with a file sink
        static ApplicationExceptionLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console() // Optionally log to console
                .WriteTo.File(
                    path: $"logs/{DateTime.UtcNow:yyyy-MM-dd}.log",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: null,
                    retainedFileCountLimit: null
                )
                .CreateLogger();
        }

        public static void HandleException(Exception ex)
        {
            // Log to the file always
            LogToFile(ex);

            // Log to the appropriate output medium
            if (Debugger.IsAttached)
            {
                LogToDebug(ex);
            }
            else
            {
                LogToConsole(ex);
            }
        }

        private static void LogToDebug(Exception ex)
        {
            Debug.WriteLine(new string('=', REPL));
            Debug.WriteLine($"Error: {ex.Message}");
            Debug.WriteLine($"Source: {ex.Source}");
            Debug.WriteLine($"Target Site: {ex.TargetSite}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Debug.WriteLine("----- Inner Exception -----");
                Debug.WriteLine($"Error: {innerException.Message}");
                Debug.WriteLine($"Source: {innerException.Source}");
                Debug.WriteLine($"Target Site: {innerException.TargetSite}");
                Debug.WriteLine($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Debug.WriteLine(new string('=', REPL));
        }

        private static void LogToConsole(Exception ex)
        {
            Console.WriteLine(new string('=', REPL));
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

            Console.WriteLine(new string('=', REPL));
        }

        private static void LogToFile(Exception ex)
        {
            Log.Error(new string('=', REPL));
            Log.Error($"Error: {ex.Message}");
            Log.Error($"Source: {ex.Source}");
            Log.Error($"Target Site: {ex.TargetSite}");
            Log.Error($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Log.Error("----- Inner Exception -----");
                Log.Error($"Error: {innerException.Message}");
                Log.Error($"Source: {innerException.Source}");
                Log.Error($"Target Site: {innerException.TargetSite}");
                Log.Error($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Log.Error(new string('=', REPL));
        }
    }
}


