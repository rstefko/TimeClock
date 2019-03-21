using System;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace TimeClock.Core
{
    /// <summary>
    /// Class which manages running processes.
    /// </summary>
    public class ProcessManager
    {
        /// <summary>
        /// Delegate to let caller make some actions with a process.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="stop"></param>
        public delegate void ProcessHandler(ManagementObject process, ref bool stop);

        /// <summary>
        /// Finds all specified processes running under currently logged user.
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="filter"></param>
        /// <param name="iterator"></param>
        public static void FindProcesses(string processName, string filter, ProcessHandler iterator)
        {
            // Query which lists all processes with specified name.
            string query = string.Format(
                "SELECT * FROM Win32_Process WHERE Caption = '{0}'",
                processName
                );

            if (!String.IsNullOrEmpty(filter))
            {
                query += string.Format(
                    " AND {0}",
                    filter
                    );
            }

            /*
             * Loop throught all processes and find out if the specified process
             * is running under current user.
             */
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                searcher.Options.ReturnImmediately = false;

                using (ManagementObjectCollection processes = searcher.Get())
                {
                    foreach (ManagementObject process in processes)
                    {
                        if (Environment.UserName.Equals(GetProcessOwner(process)))
                        {
                            bool stop = false;

                            iterator(process, ref stop);

                            // Do not continue with the iteration.
                            if (stop) break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates whether a specified process is running under currently logged user.
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static bool IsProcessRunning(string processName, string filter)
        {
            bool isRunning = false;

            FindProcesses(processName, filter, delegate(ManagementObject process, ref bool stop)
            {
                isRunning = true;
                stop = true;
            });

            return isRunning;
        }

        private static string GetProcessOwner(ManagementObject process)
        {
            string[] ownerInfo = new string[2];

            // Invoke WMI method.
            process.InvokeMethod("GetOwner", ownerInfo);

            return ownerInfo[0]; // Returns User Name.
        }
    }
}
