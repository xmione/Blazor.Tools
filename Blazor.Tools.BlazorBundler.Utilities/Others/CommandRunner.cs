/*====================================================================================================
    Class Name  : CommandRunner
    Created By  : Solomio S. Sisante
    Created On  : October 5, 2024
    Purpose     : To manage the running of desired process.
  ====================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Tools.BlazorBundler.Utilities.Others
{
    using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
    using System;
    using System.Diagnostics;

    public class CommandRunner
    {
        private string _commandId;
        private bool _isRunning;
        public static Dictionary<string, object> Commands = new Dictionary<string, object>();
        public CommandRunner(string commandId)
        {
            _isRunning = Commands.ContainsKey(commandId);
            _commandId = commandId;
        }

        public async Task RunDotnetCommandAsync(Dictionary<string, object> commandRunnerArgs, int timeOut)
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                Commands.Add(_commandId, this);
                bool useShellExecute = bool.Parse(commandRunnerArgs["UseShellExecute"].ToString()!);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = commandRunnerArgs["FileName"].ToString(), // e.g., "cmd.exe"
                    Arguments = commandRunnerArgs["Arguments"].ToString(), // e.g., "/k dotnet path"
                    RedirectStandardOutput = !useShellExecute,
                    RedirectStandardError = !useShellExecute,
                    UseShellExecute = useShellExecute,
                    CreateNoWindow = !useShellExecute
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    // Start the process asynchronously
                    await Task.Run(() => process.Start());

                    if (!useShellExecute)
                    {
                        var outputTask = process.StandardOutput.ReadToEndAsync();
                        var errorTask = process.StandardError.ReadToEndAsync();

                        // Wait for the process to exit or timeout
                        var delayTask = Task.Delay(timeOut);
                        await Task.WhenAny(process.WaitForExitAsync(), delayTask);

                        // Check if the process timed out
                        if (!process.HasExited)
                        {
                            process.Kill(); // Kill if still running
                            AppLogger.WriteInfo("Process timed out.");
                        }
                        else
                        {
                            string output = await outputTask;
                            string error = await errorTask;

                            // Log the output
                            AppLogger.WriteInfo("Output: " + output);
                            AppLogger.WriteInfo("Error: " + error);
                        }
                    }
                    else
                    {
                        // If using shell execution, wait for the process to finish
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.HandleError(ex);
            }
        }

    }

}
