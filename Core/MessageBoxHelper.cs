using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TimeClock.Core
{
    static class MessageBoxHelper
    {
        /// <summary>
        /// Displays a message box to the user with question.
        /// </summary>
        /// <param name="text">The text.</param>
        public static DialogResult Ask(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Displays a message box to the user with question.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Ask(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Displayes a message box to the user with error message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Error(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Displayes a message box to the user with error message.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Error(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Displayes a message box to the user with warning.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Warn(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Displayes a message box to the user with warning.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Warn(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Informs a user with message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Inform(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Informs a user with message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Inform(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
