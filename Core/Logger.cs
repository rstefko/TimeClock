using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeClock.Core
{
    /// <summary>
    /// Initializes logger class.
    /// </summary>
    public static class Logger
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Diagnostics.Process.GetCurrentProcess().ProcessName);

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        public static log4net.ILog Instance
        {
            get
            {
                return _log;
            }
        }
    }
}
