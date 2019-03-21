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

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            TimeClock.Core.Settings.AutoStart = this.cboAutoStart.IsChecked ?? false;
            TimeClock.Core.Settings.ForceOwnWebService = this.cboForceOwnWebService.IsChecked ?? false;

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cboAutoStart.IsChecked = TimeClock.Core.Settings.AutoStart;
            this.cboForceOwnWebService.IsChecked = TimeClock.Core.Settings.ForceOwnWebService;
        }
    }
}
