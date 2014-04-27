namespace FileSearch {
    partial class FilesListWindow {
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
            this.matchedFilesListBox = new System.Windows.Forms.ListBox();
            this.status = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // matchedFilesListBox
            // 
            this.matchedFilesListBox.FormattingEnabled = true;
            this.matchedFilesListBox.Location = new System.Drawing.Point(12, 12);
            this.matchedFilesListBox.Name = "matchedFilesListBox";
            this.matchedFilesListBox.Size = new System.Drawing.Size(481, 355);
            this.matchedFilesListBox.TabIndex = 0;
            // 
            // status
            // 
            this.status.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(9, 376);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(240, 13);
            this.status.TabIndex = 1;
            this.status.Text = "Found in 0 out of 0 (+ 0 unaccessible). 0MB read.";
            // 
            // FilesListWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 398);
            this.Controls.Add(this.status);
            this.Controls.Add(this.matchedFilesListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FilesListWindow";
            this.Text = "File Search";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FilesListWindow_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label status;
        private System.Windows.Forms.ListBox matchedFilesListBox;
    }
}

