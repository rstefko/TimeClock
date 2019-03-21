namespace TimeClock.Core.GUI
{
    partial class UnhandledExceptionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picError = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.hypDetail = new System.Windows.Forms.LinkLabel();
            this.tbxError = new System.Windows.Forms.TextBox();
            this.btnExit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picError)).BeginInit();
            this.SuspendLayout();
            // 
            // picError
            // 
            this.picError.Image = global::TimeClock.Core.Properties.Resources._109_AllAnnotations_Error_48x48_72;
            this.picError.Location = new System.Drawing.Point(12, 12);
            this.picError.Name = "picError";
            this.picError.Size = new System.Drawing.Size(48, 48);
            this.picError.TabIndex = 0;
            this.picError.TabStop = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(67, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(329, 47);
            this.label1.TabIndex = 1;
            this.label1.Text = "There has been an error in the application causing it to exit. To view more infor" +
    "mation about the error, click the link below.";
            // 
            // hypDetail
            // 
            this.hypDetail.AutoSize = true;
            this.hypDetail.Location = new System.Drawing.Point(67, 47);
            this.hypDetail.Name = "hypDetail";
            this.hypDetail.Size = new System.Drawing.Size(134, 13);
            this.hypDetail.TabIndex = 2;
            this.hypDetail.TabStop = true;
            this.hypDetail.Text = "Show whole error message";
            this.hypDetail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.hypDetail_LinkClicked);
            // 
            // tbxError
            // 
            this.tbxError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxError.Location = new System.Drawing.Point(12, 66);
            this.tbxError.Multiline = true;
            this.tbxError.Name = "tbxError";
            this.tbxError.ReadOnly = true;
            this.tbxError.Size = new System.Drawing.Size(384, 0);
            this.tbxError.TabIndex = 3;
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnExit.Location = new System.Drawing.Point(321, 72);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 6;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            // 
            // UnhandledExceptionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 107);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.tbxError);
            this.Controls.Add(this.hypDetail);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picError);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "UnhandledExceptionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UnhandledExceptionDialog";
            ((System.ComponentModel.ISupportInitialize)(this.picError)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picError;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel hypDetail;
        private System.Windows.Forms.TextBox tbxError;
        private System.Windows.Forms.Button btnExit;
    }
}