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
using TimeClock.Core.Data.Binding.Objects;
using System.Data;
using TimeClock.RemoteStore;
using System.ComponentModel;
using TimeClock.Core;
using Newtonsoft.Json.Linq;
using TimeClock.Helpers;

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for SummaryWindow.xaml
    /// </summary>
    public partial class SummaryWindow : Window
    {
        private List<EditableWorkReport> workReports;

        public SummaryWindow()
        {
            InitializeComponent();
        }

        public List<EditableWorkReport> WorkReports
        {
            get
            {
                return this.workReports;
            }

            set
            {
                this.workReports = value;

                this.listView.ItemsSource = workReports;
            }
        }

        public IEnumerable<BaseItem> ProjectsLeads
        {
            get
            {
                return TimeClock.RemoteStore.Items.ProjectsLeads;
            }
        }

        public IEnumerable<BaseItem> WorkReportTypes
        {
            get
            {
                return Items.WorkReportTypes;
            }
        }

        void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditableWorkReport item = this.GetCurrentItem(e.OriginalSource);

            if (item != null)
            {
                this.EditWorkReport(item);
            }
        }

        /// <summary>
        /// Gets current selected item.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        EditableWorkReport GetCurrentItem(object source)
        {
            DependencyObject dependencyObject = (DependencyObject)source;

            while ((dependencyObject != null) && !(dependencyObject is ListViewItem))
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            if (dependencyObject == null)
                return null;

            return (EditableWorkReport)this.listView.ItemContainerGenerator.ItemFromContainer(dependencyObject);
        }

        void EditWorkReport(EditableWorkReport item)
        {
            WorkReportWindow window = new WorkReportWindow(this.workReports.Select(x => x.WrappedInstance), false);
            window.Owner = this;
            window.WorkReport = item;

            if (window.ShowDialog().Value)
            {
                // Save workreports.
                Core.Xml.History.SaveHistory(this.workReports);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Design.DisplayWaitCursor(() =>
            {
                for (int i = this.workReports.Count - 1; i >= 0; i--)
                {
                    EditableWorkReport item = this.workReports[i];

                    // Some information is not set
                    if (!item.WrappedInstance.IsValid)
                        continue;

                    try
                    {
                        RemoteStore.WorkReport.SaveWorkReport(item.WrappedInstance);

                        // Remove successfully saved work report from the list
                        this.workReports.RemoveAt(i);
                    }
                    catch (eWayCRM.API.Exceptions.ResponseException ex) when (ex.ReturnCode == "rcParameterError")
                    {
                        MessageBoxHelper.Warn($"Unable to save time sheet '{item.Subject}': {ex.Message}");
                        break;
                    }
                }

                // Save workreports.
                Core.Xml.History.SaveHistory(this.workReports);

                this.Close();
            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteItem(this.GetCurrentItem(e.OriginalSource));
                    break;
            }
        }

        private void DeleteItem(EditableWorkReport item)
        {
            if (item != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to remove selected item?",
                    Settings.APPLICATION_NAME,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                    );

                if (result == MessageBoxResult.Yes)
                {
                    this.workReports.Remove(item);

                    // Save workreports.
                    Core.Xml.History.SaveHistory(this.workReports);

                    // Refresh view.
                    ICollectionView view = (ICollectionView)CollectionViewSource.GetDefaultView(this.listView.ItemsSource);
                    view.Refresh();
                }
            }
        }
    }
}
