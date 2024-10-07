using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
namespace Blazor.Tools.BlazorBundler.Utilities.Others
{
    public class ThreadMonitor
    {
        private static readonly Lazy<ThreadMonitor> _instance = new Lazy<ThreadMonitor>(() => new ThreadMonitor());
        private readonly List<Thread> _activeThreads;

        // Private constructor to prevent external instantiation
        private ThreadMonitor()
        {
            _activeThreads = new List<Thread>();
        }

        // Singleton instance access
        public static ThreadMonitor Instance => _instance.Value;

        // Check and store all active threads in the current process
        public void UpdateActiveThreads()
        {
            _activeThreads.Clear(); // Clear the list before updating

            // Get all threads of the current process
            ProcessThreadCollection processThreads = Process.GetCurrentProcess().Threads;

            foreach (ProcessThread processThread in processThreads)
            {
                // Only add running threads
                if (processThread.ThreadState == System.Diagnostics.ThreadState.Running)
                {
                    try
                    {
                        // Add a Thread object corresponding to the ProcessThread
                        Thread thread = Thread.CurrentThread;
                        _activeThreads.Add(thread);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not retrieve thread details: {ex.Message}");
                    }
                }
            }
        }

        // Retrieve a list of active threads
        public List<Thread> GetActiveThreads()
        {
            return _activeThreads;
        }

        // Method to check if there are any active threads
        public bool AreThreadsRunning()
        {
            return _activeThreads.Count > 0;
        }
    }

}
