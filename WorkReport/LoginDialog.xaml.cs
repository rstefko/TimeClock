using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimeClock.Interfaces;

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window, ILoginDialog
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        public string UserName
        {
            get { return this.tbxUserName.Text; }
            set { this.tbxUserName.Text = value; }
        }

        public string Password
        {
            get { return this.tbxPassword.Password; }
        }

        public string Server
        {
            get { return this.tbxServer.Text; }
            set { this.tbxServer.Text = value; }
        }

        /// <summary>
        /// Indicates whether the user wants to save password or not.
        /// </summary>
        public bool RememberPassword
        {
            get { return this.chkRememberPassword.IsChecked ?? false; }
        }

        public string AccessToken => null;

        public bool UseLegacyLogin => false;

        /// <summary>
        /// Hides the remember password control.
        /// </summary>
        /// <returns></returns>
        public void HideRememberPasswordControl()
        {
            this.chkRememberPassword.Visibility = Visibility.Hidden;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Server) && !string.IsNullOrEmpty(this.UserName))
                this.tbxPassword.Focus();
        }
    }
}
