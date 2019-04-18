using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net;

using TimeClock.Core.Data.Binding.Objects;
using TimeClock.Core;
using TimeClock.Helpers;
using eWayCRM.API.Exceptions;

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private BusinessLogic businessLogic = new BusinessLogic();
        private SplashScreen splashScreen = new SplashScreen("Resources/Splash.png");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.businessLogic.SummaryShow += new EventHandler<BusinessLogic.WorkReportsEventArgs>(businessLogic_SummaryShow);
            this.businessLogic.LoginShow += new EventHandler<BusinessLogic.LoginEventArgs>(businessLogic_LoginShow);
            this.businessLogic.ShutdownApplication += new EventHandler<BusinessLogic.ShutdownEventArgs>(businessLogic_ShutdownApplication);
            this.businessLogic.NewWorkReportShow += new EventHandler<BusinessLogic.WorkReportEventArgs>(businessLogic_NewWorkReportShow);
            this.businessLogic.StartCountingRequested += new EventHandler<BusinessLogic.StartCountingEventArgs>(businessLogic_StartCountingRequested);
            this.businessLogic.OptionsShow += new EventHandler(businessLogic_OptionsShow);
            this.businessLogic.LoadingFinished += businessLogic_LoadingFinished;
            this.businessLogic.LoadingStarted += businessLogic_LoadingStarted;

            this.businessLogic.Initialize();
        }

        private void businessLogic_LoadingStarted(object sender, EventArgs e)
        {
            this.splashScreen.Show(false, true);
        }

        private void businessLogic_LoadingFinished(object sender, EventArgs e)
        {
            this.splashScreen.Close(new TimeSpan(0, 0, 1));
        }

        void businessLogic_OptionsShow(object sender, EventArgs e)
        {
            var optionsWindow = new OptionsWindow();
            optionsWindow.Show();
        }

        void businessLogic_StartCountingRequested(object sender, BusinessLogic.StartCountingEventArgs e)
        {
            string message;

            switch (e.Reason)
            {
                case StartCountingReasons.AutoContinued:
                    message = "Do you want to continue with counting?";
                    break;

                case StartCountingReasons.SystemUnlocked:
                    message = "Computer has been unlocked. Do you want to continue with counting?";
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Unknown StartCountingReasons reason '{0}'", e.Reason));
            }

            if (MessageBoxHelper.Ask(message) == MessageBoxResult.Yes)
            {
                e.Handled = e.Continue = true;
            }
        }

        void businessLogic_NewWorkReportShow(object sender, BusinessLogic.WorkReportEventArgs e)
        {
            WorkReportWindow workReportWindow = new WorkReportWindow(true);
            workReportWindow.WorkReport = e.WorkReport;

            if (workReportWindow.ShowDialog().Value)
            {
                e.Handled = true;
                e.KeepCounting = workReportWindow.KeepCounting;
            }
        }

        void businessLogic_ShutdownApplication(object sender, BusinessLogic.ShutdownEventArgs e)
        {
            this.Shutdown(e.ExitCode);
        }

        void businessLogic_LoginShow(object sender, BusinessLogic.LoginEventArgs e)
        {
            LoginDialog login = new LoginDialog();
            login.UserName = e.UserName;
            login.Server = e.WebService;

            if (login.ShowDialog().Value)
            {
                e.Handled = true;

                e.RememberPassword = login.RememberPassword;
                e.WebService = login.Server;
                e.UserName = login.UserName;
                e.Password = login.Password;
            }
        }

        void businessLogic_SummaryShow(object sender, BusinessLogic.WorkReportsEventArgs e)
        {
            SummaryWindow summary = new SummaryWindow();
            summary.WorkReports = e.WorkReports;

            summary.ShowDialog();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.businessLogic.Dispose();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                if (!this.ProcessUnhandledException(e.Exception))
                {
                    using (var dialog = new Core.GUI.UnhandledExceptionDialog())
                    {
                        dialog.Text = Core.Settings.APPLICATION_NAME;
                        dialog.ErrorMessage = e.Exception.ToString();

                        // Show error dialog
                        dialog.ShowDialog();
                    }
                }
                
                // Whatever the problem is, close application
                this.Shutdown(-1);

                // Prevent default exception handling
                e.Handled = true;
            }
            catch (Exception)
            {
                // Use default error dialog
                e.Handled = false;
            }
        }

        private bool ProcessUnhandledException(Exception exception)
        {
            var baseException = exception.GetBaseException();
            if (baseException is WebException)
            {
                HttpWebResponse response = (HttpWebResponse)((WebException)baseException).Response;

                // When the exception is 401 (Unauthorized)
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    this.ShowLoginFailedMessage();

                    return true;
                }
            }
            else if (baseException is ResponseException)
            {
                var webServiceException = baseException as ResponseException;
                if (webServiceException.ReturnCode == "rcLoginFailed")
                {
                    this.ShowLoginFailedMessage();

                    return true;
                }
                else if (webServiceException.ReturnCode == "rcLoginAccountNotActive")
                {
                    MessageBoxHelper.Error("Your account is not active. The application will exit.");

                    return true;
                }
            }

            return false;
        }

        private void ShowLoginFailedMessage()
        {
            MessageBoxHelper.Error("Login failed. The application will exit.");
        }
    }
}
