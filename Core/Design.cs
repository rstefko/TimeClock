using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TimeClock.Core
{
    /// <summary>
    /// Class with various design helper methods.
    /// </summary>
    public static class Design
    {
        /// <summary>
        /// Displays the wait cursor during the action and restores old one after the action is completed.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void DisplayWaitCursor(Action action)
        {
            var oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                action();
            }
            finally
            {
                Cursor.Current = oldCursor;
            }
        }
    }
}
