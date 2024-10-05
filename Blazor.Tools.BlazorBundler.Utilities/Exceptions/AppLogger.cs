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
    public class AppLogger
    {
        private const int REPL = 222;
        // Initialize Serilog with a file sink
        private enum ExceptionType
        {
            Info = 1,
            Warning = 2,
            Error = 3
        }
        private static ExceptionType _exceptionType = ExceptionType.Info;
        static AppLogger()
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

        public static void WriteInfo(string message)
        {

            // Log to the file always
            LogToFile(message);

            // Log to the appropriate output medium
            if (Debugger.IsAttached)
            {
                LogToDebug(message);
            }
            else
            {
                LogToConsole(message);
            }
        }

        public static void HandleInfo(Exception ex, string? message = null)
        {
            _exceptionType = ExceptionType.Info;
            // Log to the file always
            LogToFile(ex, message);

            // Log to the appropriate output medium
            if (Debugger.IsAttached)
            {
                LogToDebug(ex, message);
            }
            else
            {
                LogToConsole(ex, message);
            }
        }

        public static void HandleWarning(Exception ex, string? message = null)
        {
            _exceptionType = ExceptionType.Warning;
            // Log to the file always
            LogToFile(ex, message);

            // Log to the appropriate output medium
            if (Debugger.IsAttached)
            {
                LogToDebug(ex, message);
            }
            else
            {
                LogToConsole(ex, message);
            }
        }

        public static void HandleError(Exception ex, string? message = null)
        {
            _exceptionType = ExceptionType.Error;
            // Log to the file always
            LogToFile(ex, message);

            // Log to the appropriate output medium
            if (Debugger.IsAttached)
            {
                LogToDebug(ex, message);
            }
            else
            {
                LogToConsole(ex, message);
            }
        }

        private static void LogToDebug(string message)
        {
            Debug.WriteLine(new string('=', REPL));
            Debug.WriteLine($"{_exceptionType.ToString()!}: {message}");
            Debug.WriteLine(new string('=', REPL));
        }

        private static void LogToDebug(Exception ex, string? message = null)
        {
            Debug.WriteLine(new string('=', REPL));
            Debug.WriteLine($"{_exceptionType.ToString()!}: [{message}]{ex.Message}");
            Debug.WriteLine($"Source: {ex.Source}");
            Debug.WriteLine($"Target Site: {ex.TargetSite}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Debug.WriteLine("----- Inner Exception -----");
                Debug.WriteLine($"{_exceptionType.ToString()!}: {innerException.Message}");
                Debug.WriteLine($"Source: {innerException.Source}");
                Debug.WriteLine($"Target Site: {innerException.TargetSite}");
                Debug.WriteLine($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Debug.WriteLine(new string('=', REPL));
        }

        private static void LogToConsole(string message)
        {
            Console.WriteLine(new string('=', REPL));
            Console.WriteLine($"{_exceptionType.ToString()!}: {message}");
            Console.WriteLine(new string('=', REPL));
        }

        private static void LogToConsole(Exception ex, string? message = null)
        {

            Console.WriteLine(new string('=', REPL));
            Console.WriteLine($"{_exceptionType.ToString()!}: [{message}]{ex.Message}");
            Console.WriteLine($"Source: {ex.Source}");
            Console.WriteLine($"Target Site: {ex.TargetSite}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Console.WriteLine("----- Inner Exception -----");
                Console.WriteLine($"{_exceptionType.ToString()!}: {innerException.Message}");
                Console.WriteLine($"Source: {innerException.Source}");
                Console.WriteLine($"Target Site: {innerException.TargetSite}");
                Console.WriteLine($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Console.WriteLine(new string('=', REPL));
        }

        private static void LogToFile(string message)
        {
            switch (_exceptionType)
            {

                case ExceptionType.Info:

                    Log.Information(new string('=', REPL));
                    Log.Information($"{_exceptionType.ToString()!}: {message}");
                    Log.Information(new string('=', REPL));

                    break;

                case ExceptionType.Warning:

                    Log.Warning(new string('=', REPL));
                    Log.Warning($"{_exceptionType.ToString()!}: {message}");
                    Log.Warning(new string('=', REPL));

                    break;

                case ExceptionType.Error:

                    Log.Error(new string('=', REPL));
                    Log.Error($"{_exceptionType.ToString()!}: {message}");
                    Log.Error(new string('=', REPL));

                    break;

            }

        }

        private static void LogToFile(Exception ex, string? message = null)
        {
            switch (_exceptionType)
            {

                case ExceptionType.Info:
                    LogInfoToFile(ex, message);
                    break;

                case ExceptionType.Warning:
                    LogWarningToFile(ex, message);
                    break;

                case ExceptionType.Error:
                    LogErrorToFile(ex, message);
                    break;

            }

        }

        private static void LogInfoToFile(Exception ex, string? message = null)
        {

            Log.Information(new string('=', REPL));
            Log.Information($"{_exceptionType.ToString()!}: [{message}]{ex.Message}");
            Log.Information($"Source: {ex.Source}");
            Log.Information($"Target Site: {ex.TargetSite}");
            Log.Information($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Log.Information("----- Inner Exception -----");
                Log.Information($"{_exceptionType.ToString()!}: {innerException.Message}");
                Log.Information($"Source: {innerException.Source}");
                Log.Information($"Target Site: {innerException.TargetSite}");
                Log.Information($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Log.Information(new string('=', REPL));
        }

        private static void LogWarningToFile(Exception ex, string? message = null)
        {

            Log.Warning(new string('=', REPL));
            Log.Warning($"{_exceptionType.ToString()!}: [{message}]{ex.Message}");
            Log.Warning($"Source: {ex.Source}");
            Log.Warning($"Target Site: {ex.TargetSite}");
            Log.Warning($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Log.Warning("----- Inner Exception -----");
                Log.Warning($"{_exceptionType.ToString()!}: {innerException.Message}");
                Log.Warning($"Source: {innerException.Source}");
                Log.Warning($"Target Site: {innerException.TargetSite}");
                Log.Warning($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Log.Warning(new string('=', REPL));
        }

        private static void LogErrorToFile(Exception ex, string? message = null)
        {

            Log.Error(new string('=', REPL));
            Log.Error($"{_exceptionType.ToString()!}: [{message}]{ex.Message}");
            Log.Error($"Source: {ex.Source}");
            Log.Error($"Target Site: {ex.TargetSite}");
            Log.Error($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions if they exist
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Log.Error("----- Inner Exception -----");
                Log.Error($"{_exceptionType.ToString()!}: {innerException.Message}");
                Log.Error($"Source: {innerException.Source}");
                Log.Error($"Target Site: {innerException.TargetSite}");
                Log.Error($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException; // Move to the next inner exception
            }

            Log.Error(new string('=', REPL));
        }

    }
}


