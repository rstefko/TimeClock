using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace TimeClock.Core
{
    public class TrayIcon : IDisposable
    {
        private const int BALOON_TIMEOUT = 2 * 1000;

        private NotifyIcon notifyIcon;

        private Icon iconRunning = ResourceManager.Instance.LoadIcon("stopwatch.ico");
        private Icon iconStopped = ResourceManager.Instance.LoadIcon("stopwatch-finish.ico");

        public event EventHandler<MouseEventArgs> Click;

        public string ToolTipText
        {
            set
            {
                this.notifyIcon.Text = string.Format(
                    "{0} - {1}",
                    Settings.APPLICATION_NAME,
                    value
                    );
            }
        }

        public TrayIcon(ContextMenu menu)
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Text = Settings.APPLICATION_NAME;
            this.notifyIcon.Icon = this.iconStopped;
            this.notifyIcon.MouseClick += notifyIcon_MouseClick;
            this.notifyIcon.Visible = true;
            this.notifyIcon.ContextMenu = menu;
        }

        void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.Click != null)
                this.Click(this, e);
        }

        public void StartAnimation()
        {
            this.notifyIcon.Icon = this.iconRunning;
        }

        public void StopAnimation()
        {
            this.notifyIcon.Icon = this.iconStopped;
        }

        /// <summary>
        /// Shows balloon tooltip in the notification area.
        /// </summary>
        public void ShowBaloon(string title)
        {
            this.notifyIcon.ShowBalloonTip(BALOON_TIMEOUT, title, Core.Settings.APPLICATION_NAME, ToolTipIcon.Info);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.MouseClick -= notifyIcon_MouseClick;
                this.notifyIcon.Dispose();
            }
        }

        #endregion
    }
}
