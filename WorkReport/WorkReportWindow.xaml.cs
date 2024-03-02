using System;
using System.Collections.Generic;
using System.Data;
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
using TimeClock.RemoteStore;

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for WorkReportWindow.xaml
    /// </summary>
    public partial class WorkReportWindow : Window
    {
        private readonly IEnumerable<WorkReport> workReports;
        private EditableWorkReport workReport;
        private readonly bool isNew;

        private WorkReportWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkReportWindow"/> class.
        /// </summary>
        /// <param name="workReports">Collection of other work reports.</param>
        /// <param name="isNew">True if the window is displayed for new work report.</param>
        public WorkReportWindow(IEnumerable<WorkReport> workReports, bool isNew)
            : this()
        {
            if (workReports == null)
                throw new ArgumentNullException(nameof(workReports));

            this.workReports = workReports;
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

            if (this.WorkReport.ToTime.Subtract(this.WorkReport.FromTime).TotalMinutes <= 0)
            {
                if (this.isNew)
                {
                    if (MessageBoxHelper.Ask("Time sheet has to last at least one minute. Do you want to keep on counting?") == MessageBoxResult.Yes)
                        this.OnKeepCounting();
                }
                else
                {
                    MessageBoxHelper.Warn("Start time must be less than end time.");
                }

                return;
            }

            if (this.IsWorkReportInTheSameTimeAlreadyCreated(this.WorkReport.WrappedInstance.ItemGuid, this.WorkReport.FromTime, this.WorkReport.ToTime))
            {
                MessageBoxHelper.Warn("Time provided in From / To time fields would overlap with other time sheets.");
                return;
            }

            if (string.IsNullOrEmpty(this.tbxSubject.Text))
            {
                MessageBoxHelper.Warn("Subject has to be specified.");
                return;
            }

            if (this.cboProject.SelectedValue == null)
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
        
        private bool IsWorkReportInTheSameTimeAlreadyCreated(Guid itemGuid, DateTime fromTime, DateTime toTime)
        {
            foreach (var workReport in this.workReports)
            {
                // Don't check itself
                if (workReport.ItemGuid == itemGuid)
                    continue;

                if (this.IsDateRangesOverlap(workReport.FromTime, workReport.ToTime, fromTime, toTime))
                    return true;
            }

            return false;
        }

        private bool IsDateRangesOverlap(DateTime startDateA, DateTime endDateA, DateTime startDateB, DateTime endDateB)
        {
            return (startDateA < endDateB) && (startDateB < endDateA);
        }

        private void SaveUserSettings()
        {
            Core.Settings.SaveUserSetting("Project", Core.Settings.TIMECLOCK_REGISTRY_KEY, this.cboProject.SelectedValue);
            Core.Settings.SaveUserSetting("WorkReportType", Core.Settings.TIMECLOCK_REGISTRY_KEY, this.cboType.SelectedValue);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.InitializeReservedFields();

            this.cboProject.ItemsSource = RemoteStore.Items.ProjectsLeads.ToArray();
            this.cboType.ItemsSource = RemoteStore.Items.WorkReportTypes.ToArray();
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
