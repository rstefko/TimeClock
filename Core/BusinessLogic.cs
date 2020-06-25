using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

using TimeClock.Core.Data.Binding.Objects;
using eWayCRM.API;
using eWayCRM.API.Exceptions;

namespace TimeClock.Core
{
    /// <summary>
    /// Business logic of the application.
    /// </summary>
    public class BusinessLogic : IDisposable
    {
        private Core.TrayIcon trayIcon;
        private DateTime? startTime;
        private System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
        private List<EditableWorkReport> workReports = new List<EditableWorkReport>(5);
        private System.Windows.Forms.Timer hoursTimer = new System.Windows.Forms.Timer();
        private bool saveNewWorkReportWindowDisplayed;

        private const string MENU_ITEM_START = "Start";
        private const string MENU_ITEM_STOP = "Stop";
        private const string MENU_ITEM_COMMIT = "Commit";
        private const string MENU_ITEM_EXIT = "Exit";
        private const string MENU_ITEM_SETTINGS = "Settings";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BusinessLogic()
        {
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        /// <summary>
        /// Executes application initialization logic.
        /// </summary>
        public async void Initialize()
        {
#if  (!DEBUG)
            // Allow just one instance.
            if (Running())
            {
                this.Shutdown();
                return;
            }
#endif

            if (!LogIn())
            {
                this.Shutdown();
                return;
            }

            this.LoadingStarted?.Invoke(this, EventArgs.Empty);

            await Task.Run(() =>
            {
                this.PreloadData();
            });

            this.LoadingFinished?.Invoke(this, EventArgs.Empty);

            this.workReports = Core.Xml.History.LoadHistory();

            this.CreateContextMenu();

            this.trayIcon = new TrayIcon(this.menu);

            this.trayIcon.Click += new EventHandler<MouseEventArgs>(trayIcon_Click);
            this.ToolTipText = "Idle";

            this.hoursTimer.Interval = 1000;
            this.hoursTimer.Tick += new EventHandler(hoursTimer_Tick);
            this.hoursTimer.Start();

            // Restart unfinished counting if necessary
            this.CheckForUnfinishedWorkReport();

            if (!this.IsRunning)
            {
                if (this.workReports.Count != 0)
                {
                    if (MessageBoxHelper.Ask("There are uncommited work reports. Would you like to commit them now?") == DialogResult.Yes)
                        this.ShowSummary();
                }
                else if (Settings.AutoStart)
                {
                    this.StartCounting();
                }
            }
        }

        private void PreloadData()
        {
            var superiorItems = RemoteStore.Items.ProjectsLeads;
            var workReportTypes = RemoteStore.Items.WorkReportTypes;
        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason != Microsoft.Win32.SessionSwitchReason.SessionUnlock)
                return;

            // Another session already running
            if (this.IsRunning)
                return;

            // No work reports created
            if (this.workReports.Count == 0)
                return;

            // No work reports from today
            if (this.workReports[this.workReports.Count - 1].FromTime.DayOfYear != DateTime.Now.DayOfYear)
                return;

            // Continue counting
            this.StartCounting(StartCountingReasons.SystemUnlocked, DateTime.Now);
        }

        private void Shutdown()
        {
            this.ShutdownApplication?.Invoke(this, new ShutdownEventArgs());
        }

        private void CheckForUnfinishedWorkReport()
        {
            EditableWorkReport item;
            if (this.IsUnfinishedWorkReport(out item))
                this.ActivateCounting(item.WrappedInstance.FromTime);
        }

        public string ToolTipText
        {
            get;
            set;
        }

        void hoursTimer_Tick(object sender, EventArgs e)
        {
            this.trayIcon.ToolTipText = string.Format(
                    "{0}{1}Today worked: {2}h",
                    this.ToolTipText,
                    Environment.NewLine,
                    this.WorkedHours
                    );
        }

#if  (!DEBUG)
        private bool Running()
        {
            string filter = string.Format("Handle <> {0}", Process.GetCurrentProcess().Id);

            // Allow just one instance.
			try
			{
            	return ProcessManager.IsProcessRunning(Settings.APPLICATION_PROCESS_NAME, filter);
			}
			catch (NotImplementedException)
			{
				// Probably in Linux
				return false;
			}
        }
#endif

        void CreateContextMenu()
        {
            this.menu.MenuItems.Add(MENU_ITEM_START, menuItem_Click);
            this.menu.MenuItems.Add(MENU_ITEM_STOP, menuItem_Click);
            this.menu.MenuItems.Add("-");
            this.menu.MenuItems.Add(MENU_ITEM_COMMIT, menuItem_Click);
            this.menu.MenuItems.Add("-");
            this.menu.MenuItems.Add(MENU_ITEM_SETTINGS, menuItem_Click);
            this.menu.MenuItems.Add("-");
            this.menu.MenuItems.Add(MENU_ITEM_EXIT, menuItem_Click);
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = sender as System.Windows.Forms.MenuItem;

            if (item != null)
            {
                switch (item.Text)
                {
                    case MENU_ITEM_EXIT:
                        this.Quit();
                        break;

                    case MENU_ITEM_START:
                        this.StartCounting();
                        break;

                    case MENU_ITEM_STOP:
                        this.StopCounting();
                        break;

                    case MENU_ITEM_COMMIT:
                        this.ShowSummary();
                        break;

                    case MENU_ITEM_SETTINGS:
                        this.ShowOptionsDialog();
                        break;
                }
            }
        }

        private void ShowOptionsDialog()
        {
            if (OptionsShow != null)
                OptionsShow(this, EventArgs.Empty);
        }

        private void Quit()
        {
            // Stop counting if it is running.
            if (this.startTime.HasValue)
            {
                if (MessageBoxHelper.Ask("Do you want to stop counting?") == DialogResult.Yes)
                {
                    if (!this.StopCounting(allowNew: false))
                        return;
                }
            }

            // Close tray icon.
            this.trayIcon.Dispose();

            // Dispose timer
            this.hoursTimer.Dispose();

            if (!this.startTime.HasValue && this.workReports.Count != 0)
            {
                if (MessageBoxHelper.Ask("Would you like to commit your work reports now?") == DialogResult.Yes)
                    this.ShowSummary();
            }

            this.Shutdown();
        }

        /// <summary>
        /// Gets worked hours for today
        /// </summary>
        private double WorkedHours
        {
            get
            {
                double workedHours = 0;

                // Get time from work reports
                foreach (EditableWorkReport item in this.workReports)
                {
                    RemoteStore.WorkReport report = item.WrappedInstance;

                    if (report.FromTime.Year == DateTime.Today.Year && report.FromTime.DayOfYear == DateTime.Today.DayOfYear)
                    {
                        DateTime toTime = report.ToTime == DateTime.MinValue ? DateTime.Now : report.ToTime;
                        workedHours += ((TimeSpan)toTime.Subtract(report.FromTime)).TotalHours;
                    }
                }

                return Math.Round(workedHours, 2);
            }
        }

        void trayIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                bool start = false;

                if (this.startTime.HasValue)
                {
                    this.StopCounting();
                }
                else
                {
                    start = true; // First counting should start
                }

                if (start)
                    this.StartCounting();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                // Stop counting without displaying new work report dialog
                this.StopCounting(true);
            }
        }

        /// <summary>
        /// Indicates whether the timer is running a session
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.startTime.HasValue;
            }
        }

        /// <summary>
        /// Start new counting session
        /// </summary>
        /// <param name="startTime"></param>
        public void StartCounting(DateTime startTime = default(DateTime))
        {
            System.Diagnostics.Debug.Assert(!this.IsRunning, "Another counting session already running");

            if (saveNewWorkReportWindowDisplayed)
            {
                MessageBoxHelper.Inform("There is unsaved window with new work report. Please close it first.");
                return;
            }

            this.ActivateCounting(startTime == default(DateTime) ? DateTime.Now : startTime);

            // Create new workreport record and save it
            this.CreateEditableWorkReport(this.startTime.Value);
            this.SaveWorkReports();
        }

        void ActivateCounting(DateTime startTime)
        {
            if (this.startTime.HasValue)
                return;

            this.trayIcon.StartAnimation();
            this.startTime = startTime;

            // Set ToolTip
            string toolTip = this.GetActivateAtText(this.startTime.Value);

            this.ToolTipText = toolTip;
            this.trayIcon.ShowBaloon(toolTip);
        }

        void KeepCounting(DateTime startTime)
        {
            if (this.startTime.HasValue)
                throw new InvalidOperationException("StartTime should not be set");

            this.trayIcon.StartAnimation();
            this.startTime = startTime;

            string balloonTitle = "Kept on counting from " + this.startTime.Value.ToString("HH:mm");

            this.ToolTipText = this.GetActivateAtText(this.startTime.Value);
            this.trayIcon.ShowBaloon(balloonTitle);
        }

        private string GetActivateAtText(DateTime time)
        {
            return "Activated at " + time.ToString("HH:mm");
        }

        bool StopCounting(bool force = false, bool allowNew = true)
        {
            if (!this.startTime.HasValue)
               return true;

            this.startTime = null;
            this.trayIcon.StopAnimation();
            this.ToolTipText = "Idle";

            EditableWorkReport item = this.GetLatestWorkReport();

            System.Diagnostics.Debug.Assert(item != null, "Cannot get latest work report");
            System.Diagnostics.Debug.Assert(item.WrappedInstance.ToTime == DateTime.MinValue, "There is some inconsistency in work report");

            bool keepCounting = false;
            if (force || !this.ShowNewWorkReport(item, DateTime.Now, out keepCounting))
            {
                this.workReports.Remove(item);

                allowNew = false;
            }

            // We have to save workreports because there is information about unfinished work report
            this.SaveWorkReports();

            // Continue counting
            if (!keepCounting)
            {
                if (!allowNew)
                    return true;
                
                this.StartCounting(StartCountingReasons.AutoContinued);

                return false;
            }
            else
            {
                this.KeepCounting(item.WrappedInstance.FromTime);

                return false;
            }
        }

        private void StartCounting(StartCountingReasons reason, DateTime? start = null)
        {
            if (StartCountingRequested == null)
                return;

            if (this.workReports.Count == 0)
                return;

            var args = new StartCountingEventArgs(reason);
            StartCountingRequested(this, args);

            if (!args.Handled)
                return;

            if (!args.Continue)
                return;

            // Counting already started -> do not continue
            if (this.IsRunning)
                return;

            // Use start time from parameter or continue right after the last work report
            var startTime = start.HasValue ? start.Value : this.workReports[this.workReports.Count - 1].ToTime;
            this.StartCounting(startTime);
        }

        EditableWorkReport CreateEditableWorkReport(DateTime startTime)
        {
            RemoteStore.WorkReport item = new RemoteStore.WorkReport();
            item.FromTime = startTime;

            // Insert item into the workreports list
            this.workReports.Add(new EditableWorkReport(item));

            // Return new item
            return GetLatestWorkReport();
        }

        private EditableWorkReport GetLatestWorkReport()
        {
            if (this.workReports.Count > 0)
                return this.workReports[this.workReports.Count - 1];
            else
                return null;
        }

        private bool IsUnfinishedWorkReport(out EditableWorkReport item)
        {
            item = this.GetLatestWorkReport();

            // When ToTime is not specified, work report has only started
            if (item != null && item.WrappedInstance.ToTime == DateTime.MinValue)
                return true;

            return false;
        }

        private void SaveWorkReports()
        {
            // Save workreports.
            Core.Xml.History.SaveHistory(this.workReports);
        }

        #region Events

        /// <summary>
        /// Event called when summary window should be displayed.
        /// </summary>
        public event EventHandler<WorkReportsEventArgs> SummaryShow;

        /// <summary>
        /// Event called when login window should be displayed.
        /// </summary>
        public event EventHandler<LoginEventArgs> LoginShow;

        /// <summary>
        /// Event called when the application should exit.
        /// </summary>
        public event EventHandler<ShutdownEventArgs> ShutdownApplication;

        /// <summary>
        /// Event called when window with new WorkReport object should be displayed.
        /// </summary>
        public event EventHandler<WorkReportEventArgs> NewWorkReportShow;

        /// <summary>
        /// Event called when counting is requested.
        /// </summary>
        public event EventHandler<StartCountingEventArgs> StartCountingRequested;

        /// <summary>
        /// Event called when options window should be displayed.
        /// </summary>
        public event EventHandler OptionsShow;

        /// <summary>
        /// Event called when application start to load data from API.
        /// </summary>
        public event EventHandler LoadingStarted;

        /// <summary>
        /// Event called when application stops loading data from API.
        /// </summary>
        public event EventHandler LoadingFinished;

        /// <summary>
        /// Event arguments with EditableWorkReport list.
        /// </summary>
        public class WorkReportsEventArgs : EventArgs
        {
            /// <summary>
            /// Collection of EditableWorkReport objects
            /// </summary>
            public List<EditableWorkReport> WorkReports
            {
                get;
                private set;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public WorkReportsEventArgs(List<EditableWorkReport> workReports)
            {
                this.WorkReports = workReports;
            }
        }

        /// <summary>
        /// Event arguments with login information.
        /// </summary>
        public class LoginEventArgs : EventArgs
        {
            /// <summary>
            /// Name of the user.
            /// </summary>
            public string UserName
            {
                get;
                set;
            }

            /// <summary>
            /// User password.
            /// </summary>
            public string Password
            {
                get;
                set;
            }

            /// <summary>
            /// Web service to log into.
            /// </summary>
            public string WebService
            {
                get;
                set;
            }

            /// <summary>
            /// Indicates whether the event has been handled.
            /// </summary>
            public bool Handled
            {
                get;
                set;
            }

            /// <summary>
            /// Indicates whether the password will be saved into registry or not.
            /// </summary>
            public bool RememberPassword
            {
                get;
                set;
            }

            /// <summary>
            /// Indicates whether user wants to remember his password.
            /// </summary>
            public bool AllowRememberPassword
            {
                get;
                set;
            }

            /// <summary>
            /// Indicates whether OAuth 2.0 is used.
            /// </summary>
            public bool UseOAuth
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the access token.
            /// </summary>
            public string AccessToken
            {
                get;
                set;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public LoginEventArgs(string webService, string userName, string password)
            {
                this.WebService = webService;
                this.UserName = userName;
                this.Password = password;
            }

            /// <summary>
            /// Constructor without parameters.
            /// </summary>
            public LoginEventArgs() { }
        }

        /// <summary>
        /// Event arguments with shutdown information.
        /// </summary>
        public class ShutdownEventArgs : EventArgs
        {
            /// <summary>
            /// Exit code of the application.
            /// </summary>
            public int ExitCode
            {
                get;
                private set;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ShutdownEventArgs(int exitCode)
            {
                this.ExitCode = exitCode;
            }
			
			/// <summary>
			/// Constructor without parameters. 
			/// </summary>
			public ShutdownEventArgs()
				: this(0)
			{
				
			}
        }

        /// <summary>
        /// Event arguments with EditableWorkReport.
        /// </summary>
        public class WorkReportEventArgs : EventArgs
        {
            /// <summary>
            /// Instance of EditableWorkReport object.
            /// </summary>
            public EditableWorkReport WorkReport
            {
                get;
                private set;
            }

            /// <summary>
            /// Indicates whether the event has been handled.
            /// </summary>
            public bool Handled
            {
                get;
                set;
            }

            /// <summary>
            /// Indicates whether the user wants to keep counting.
            /// </summary>
            public bool KeepCounting
            {
                get;
                set;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public WorkReportEventArgs(EditableWorkReport workReport)
            {
                this.WorkReport = workReport;
            }
        }

        public class StartCountingEventArgs : EventArgs
        {
            /// <summary>
            /// Indicates whether the event has been handled.
            /// </summary>
            public bool Handled
            {
                get;
                set;
            }

            /// <summary>
            /// Indicates whether counting will start after previous counting has been stopped and new workreport has been saved.
            /// </summary>
            public bool Continue
            {
                get;
                set;
            }

            /// <summary>
            /// Reason of the event.
            /// </summary>
            public StartCountingReasons Reason
            {
                get;
                private set;
            }

            /// <summary>
            /// Default constructor
            /// </summary>
            public StartCountingEventArgs(StartCountingReasons reason)
            {
                this.Reason = reason;
            }
        }

        #endregion

        #region Dialogs

        private void ShowSummary()
        {
            // Stop counting if it is running.
            this.StopCounting();

            if (this.SummaryShow != null)
            {
                this.SummaryShow(this, new WorkReportsEventArgs(this.workReports));
            }
        }

        private bool LogIn()
        {
            var loginEventArgs = new LoginEventArgs();
            bool result = false;

            // Try to login using credentials from registry.
            string webService = Settings.TryGetUserSetting("Server", null) as string;
            bool isHttpAuthentification = false;

            if (webService != null)
            {
                string userName = Settings.TryGetUserSetting("Username", string.Empty) as string;
                string password = Settings.TryGetUserSetting("Password", null) as string;

                if (!string.IsNullOrEmpty(userName) && password != string.Empty)
                {
                    try
                    {
                        result = RemoteStore.ItemStore.Instance.LogIn(
                            webService,
                            userName,
                            password,
                            Settings.ClientVersion,
                            useDefaultCredentials: webService.StartsWith("https://") && password == null,
                            accessToken: Settings.AccessToken
                            );
                    }
                    catch (OAuthRequiredException)
                    {
                        loginEventArgs.UseOAuth = true;
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse response = (HttpWebResponse)ex.Response;
                        if (response == null)
                            throw;

                        // When the exception is 401 (Unauthorized) -> display login dialog
                        if (response.StatusCode != HttpStatusCode.Unauthorized)
                            throw;

                        isHttpAuthentification = true;
                    }

                    if (result)
                        return true;
                }

                loginEventArgs.AllowRememberPassword = !isHttpAuthentification;
                loginEventArgs.UserName = userName;
                loginEventArgs.WebService = webService;
            }

            // Show login dialog
            if (this.LoginShow != null)
            {
                this.LoginShow(this, loginEventArgs);

                if (loginEventArgs.Handled)
                {
                    bool canSavePassword = true;
                    try
                    {
                        if (!isHttpAuthentification)
                        {
                            result = RemoteStore.ItemStore.Instance.LogIn(
                                loginEventArgs.WebService,
                                loginEventArgs.UserName,
                                loginEventArgs.Password == null ? null : Connection.HashPassword(loginEventArgs.Password),
                                Settings.ClientVersion,
                                accessToken: loginEventArgs.AccessToken
                                );
                        }
                    }
                    catch (WebException ex)
                    {
                        if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                        {
                            isHttpAuthentification = true;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    if (isHttpAuthentification)
                    {
                        result = RemoteStore.ItemStore.Instance.LogIn(
                            loginEventArgs.WebService,
                            loginEventArgs.UserName,
                            null,
                            Settings.ClientVersion,
                            networkCredential: new NetworkCredential(loginEventArgs.UserName, loginEventArgs.Password)
                            );

                        canSavePassword = false;
                    }

                    if (!result)
                    {
                        MessageBoxHelper.Error("Could not login to the Web Service.");
                    }
                    else
                    {
                        // Save login information
                        Settings.SaveTimeClockUserSetting("Server", loginEventArgs.WebService);
                        Settings.SaveTimeClockUserSetting("Username", loginEventArgs.UserName);

                        if (loginEventArgs.RememberPassword && canSavePassword)
                        {
                            Settings.SaveTimeClockUserSetting("Password", Connection.HashPassword(loginEventArgs.Password));
                        }
                        else
                        {
                            Settings.SaveTimeClockUserSetting("Password", null);
                        }

                        return true;
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Fail("LoginShow event not implemented");
            }

            return false;
        }

        bool ShowNewWorkReport(EditableWorkReport item, DateTime endTime, out bool keepCounting)
        {
            keepCounting = false;

            this.saveNewWorkReportWindowDisplayed = true;
            try
            {
                item.WrappedInstance.ToTime = endTime;
                item.WrappedInstance.Project = new Guid(Core.Settings.GetUserSetting("Project", Core.Settings.TIMECLOCK_REGISTRY_KEY, Guid.Empty.ToString()) as string);
                item.WrappedInstance.Type = new Guid(Core.Settings.GetUserSetting("WorkReportType", Core.Settings.TIMECLOCK_REGISTRY_KEY, Guid.Empty.ToString()) as string);

                if (this.NewWorkReportShow != null)
                {
                    var workReportEventArgs = new WorkReportEventArgs(item);
                    this.NewWorkReportShow(this, workReportEventArgs);

                    // Item information accepted
                    if (workReportEventArgs.Handled)
                    {
                        keepCounting = workReportEventArgs.KeepCounting;

                        // Reset ToTime
                        if (keepCounting)
                        {
                            item.WrappedInstance.ToTime = default(DateTime);
                        }

                        return true;
                    }
                }

                return false;
            }
            finally
            {
                this.saveNewWorkReportWindowDisplayed = false;
            }
        }

        #endregion

        /// <summary>
        /// Disposes all used resources.
        /// </summary>
        public void Dispose()
        {
            if (this.trayIcon != null)
                this.trayIcon.Dispose();

            this.menu.Dispose();
        }
    }
}
