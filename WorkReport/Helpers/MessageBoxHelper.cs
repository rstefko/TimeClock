using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using TimeClock.Core;

namespace TimeClock.Helpers
{
    public static class MessageBoxHelper
    {
        /// <summary>
        /// Displays a message box to the user with question.
        /// </summary>
        /// <param name="text">The text.</param>
        public static MessageBoxResult Ask(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        /// <summary>
        /// Displays a message box to the user with question.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Ask(Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        /// <summary>
        /// Displayes a message box to the user with error message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Error(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Displayes a message box to the user with error message.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Error(Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Displayes a message box to the user with warning.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Warn(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Displayes a message box to the user with warning.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Warn(Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Informs a user with message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Inform(string text)
        {
            return MessageBox.Show(text, Settings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Informs a user with message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static MessageBoxResult Inform(Window owner, string text)
        {
            return MessageBox.Show(owner, text, Settings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
