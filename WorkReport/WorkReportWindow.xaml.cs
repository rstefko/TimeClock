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
using System.Windows.Navigation;
using System.Windows.Shapes;

using TimeClock.Core;
using TimeClock.Core.Data.Binding.Objects;
using TimeClock.Helpers;

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for WorkReportWindow.xaml
    /// </summary>
    public partial class WorkReportWindow : Window
    {
        private EditableWorkReport workReport;
        private bool isNew;

        public WorkReportWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkReportWindow"/> class.
        /// </summary>
        /// <param name="isNew">True if the window is displayed for new work report.</param>
        public WorkReportWindow(bool isNew)
            : this()
        {
            this.isNew = isNew;

            if (isNew)
            {
                this.btnKeepCounting.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public EditableWorkReport WorkReport
        {
            get
            {
                return this.workReport;
            }

            set
            {
                this.workReport = value;

                this.grid.DataContext = this.workReport;
                this.workReport.BeginEdit();
            }
        }

        /// <summary>
        /// Indicates whether the user clicked on the Keep Counting button.
        /// </summary>
        public bool KeepCounting
        {
            get;
            private set;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // Called to make sure that all current data are stored in data row
            this.btnOK.Focus();

            if (this.WorkReport.ToTime.Subtract(this.WorkReport.FromTime).TotalMinutes == 0)
            {
                if (MessageBoxHelper.Ask("Work report has to last at least one minute. Do you want to keep on counting?") == MessageBoxResult.Yes)
                    this.OnKeepCounting();

                return;
            }

            if (string.IsNullOrEmpty(this.tbxSubject.Text))
            {
                MessageBoxHelper.Warn("Subject has to be specified.");
                return;
            }

            if (this.cboProject.EditValue == null)
            {
                MessageBoxHelper.Warn("Project has to be specified.");
                return;
            }

            if (this.cboType.SelectedValue == null)
            {
                MessageBoxHelper.Warn("Type has to be specified.");
                return;
            }

            this.SaveUserSettings();

            this.workReport.EndEdit();

            this.DialogResult = true;
        }

        private void SaveUserSettings()
        {
            Core.Settings.SaveUserSetting("Project", Core.Settings.TIMECLOCK_REGISTRY_KEY, this.cboProject.EditValue);
            Core.Settings.SaveUserSetting("WorkReportType", Core.Settings.TIMECLOCK_REGISTRY_KEY, this.cboType.SelectedValue);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.InitializeReservedFields();

            this.cboProject.ItemsSource = RemoteStore.Items.ProjectsLeads;
            this.cboType.ItemsSource = RemoteStore.Items.WorkReportTypes;
        }

        private void InitializeReservedFields()
        {
            if (RemoteStore.Items.ReservedField.HasValue)
                this.lblReservedField.Content = RemoteStore.Items.ReservedField.Value.Value;
            else
                this.tbxReservedField.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult.HasValue && this.DialogResult.Value)
                return;

            // There are no changes
            if (!this.isNew && !this.workReport.HasChanges)
                return;

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to close the dialog without saving changes?",
                TimeClock.Core.Settings.APPLICATION_NAME,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
                );

            if (result == MessageBoxResult.Yes)
            {
                this.workReport.CancelEdit();

                // Close dialog
                e.Cancel = false;
                this.DialogResult = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void OnKeepCounting()
        {
            this.workReport.EndEdit();

            this.KeepCounting = true;
            this.DialogResult = true;
        }

        private void btnKeepCounting_Click(object sender, RoutedEventArgs e)
        {
            this.OnKeepCounting();
        }
    }
}
