/*====================================================================================================
    Class Name  : Win32Processes
    Created By  : Solomio S. Sisante
    Created On  : September 5, 2024
    Purpose     : To manage the Win32 processes.
  ====================================================================================================*/
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class Win32Processes
    {
        /// <summary>
        /// Find out what process(es) have a lock on the specified file.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <returns>Processes locking the file</returns>
        public static List<Process> GetProcessesLockingFile(string path)
        {
            uint handle;
            string key = Guid.NewGuid().ToString();
            int res = RmStartSession(out handle, 0, key);

            if (res != 0) throw new Exception("Could not begin restart session. Unable to determine file locker.");

            try
            {
                const int MORE_DATA = 234;
                uint pnProcInfoNeeded, pnProcInfo = 0, lpdwRebootReasons = RmRebootReasonNone;

                string[] resources = { path };

                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                if (res != 0) throw new Exception("Could not register resource.");

                // Check the size of the result
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                if (res == MORE_DATA)
                {
                    return EnumerateProcesses(pnProcInfoNeeded, handle, lpdwRebootReasons);
                }
                else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
            }
            finally
            {
                RmEndSession(handle);
            }

            return new List<Process>();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        const int RmRebootReasonNone = 0;
        const int CCH_RM_MAX_APP_NAME = 255;
        const int CCH_RM_MAX_SVC_NAME = 63;

        public enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)] public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)] public string strServiceShortName;
            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)] public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames,
            uint nApplications, [In] RM_UNIQUE_PROCESS[] rgApplications, uint nServices,
            string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded,
            ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
            ref uint lpdwRebootReasons);

        private static List<Process> EnumerateProcesses(uint pnProcInfoNeeded, uint handle, uint lpdwRebootReasons)
        {
            var processes = new List<Process>();
            var processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
            var pnProcInfo = pnProcInfoNeeded;

            var res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);

            if (res != 0) throw new Exception("Could not list processes locking resource.");
            for (int i = 0; i < pnProcInfo; i++)
            {
                try
                {
                    processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                }
                catch (ArgumentException) { } // Handle case where process is no longer running
            }
            return processes;
        }
    }
}
