using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TimeClock.Core.GUI
{
    public partial class UnhandledExceptionDialog : Form
    {
        private const int ERROR_DETAIL_HEIGHT = 157;

        /// <summary>
        /// Gets or sets displayed error message.
        /// </summary>
        public string ErrorMessage
        {
            get { return this.tbxError.Text; }
            set { this.tbxError.Text = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnhandledExceptionDialog()
        {
            InitializeComponent();
        }

        private void hypDetail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.tbxError.Height == 0)
                this.Height += ERROR_DETAIL_HEIGHT;
            else
                this.Height -= ERROR_DETAIL_HEIGHT;
        }
    }
}
