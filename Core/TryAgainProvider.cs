using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TimeClock.Core
{
    /// <summary>
    /// Shows try again dialog when problem occurs
    /// </summary>
    public static class TryAgainProvider
    {
        /// <summary>
        /// Simple delegate for method calls.
        /// </summary>
        public delegate void MethodDelegate();

        /// <summary>
        /// Function which shows try again dialog when Exception occurs.
        /// </summary>
        public static void Try(MethodDelegate method)
        {
            bool retry = true;

            while (retry)
            {
                try
                {
                    // Invoke delegate method
                    method();

                    retry = false;
                }
                catch (Exception)
                {
                    DialogResult result = MessageBox.Show(
                        "Operation failed. Check your internet connection and try again later.",
                        Settings.APPLICATION_NAME,
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Warning
                        );

                    if (result == DialogResult.Retry)
                        retry = true;
                    else
                        throw;
                }
            }
        }
    }
}
