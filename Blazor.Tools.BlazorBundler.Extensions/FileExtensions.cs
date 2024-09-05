/*====================================================================================================
    Class Name  : FileExtensions
    Created By  : Solomio S. Sisante
    Created On  : September 1, 2024
    Purpose     : To help in manipulating files.
  ====================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class FileExtensions
    {
        public static List<Process> LockingProcesses;

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

        /// <summary>
        /// Checks if the specified file is currently in use or locked by a process.
        /// </summary>
        /// <param name="filePath">The full path of the specified file.</param>
        /// <returns></returns>
        public static bool IsFileInUse(this string filePath)
        {
            bool isInUse = false;
            FileInfo fileInfo = new FileInfo(filePath);

            if (fileInfo.IsFileInUse())
            {
                Console.WriteLine("File {0} is in use by the following processes:", filePath);

                LockingProcesses = fileInfo.GetProcessesUsingFile();
                foreach (var process in LockingProcesses)
                {
                    Console.WriteLine($"- {process.ProcessName} (PID: {process.Id})");
                }

                isInUse = true; 
            }
            else
            {
                Console.WriteLine("File {0} is not in use.", filePath);
            }

            return isInUse;
        }

        /// <summary>
        /// Checks if the specified file is being used by any process.
        /// </summary>
        /// <param name="fileInfo">The FileInfo object representing the file to check.</param>
        /// <returns>A list of processes that are using the file. If the file is not being used, the list will be empty.</returns>
        public static List<Process> GetProcessesUsingFile(this FileInfo fileInfo)
        {
            List<Process> lockingProcesses = new List<Process>();

            try
            {
                 lockingProcesses = Win32Processes.GetProcessesLockingFile(fileInfo.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying for file usage: {ex.Message}");
            }

            return lockingProcesses;
        }

        /// <summary>
        /// Checks if the file is currently in use by any process.
        /// </summary>
        /// <param name="fileInfo">The FileInfo object representing the file to check.</param>
        /// <returns>True if the file is in use, otherwise false.</returns>
        public static bool IsFileInUse(this FileInfo fileInfo)
        {
            return fileInfo.GetProcessesUsingFile().Count > 0;
        }

        public static void KillLockingProcesses(this string fileName)
        {
            Console.WriteLine("Locked file: {0}", fileName);
            if (LockingProcesses != null)
            {
                if (LockingProcesses.Count > 0)
                {
                    foreach (var processToKill in LockingProcesses)
                    {
                        try
                        {
                            Console.WriteLine($"Attempting to kill process ID: {processToKill.Id}, Process Name: {processToKill.ProcessName}");
                            processToKill.Kill();
                            Console.WriteLine("Process terminated successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to kill process: {ex.Message}");
                        }
                    }
                    
                }
            }
        }
    }

}
