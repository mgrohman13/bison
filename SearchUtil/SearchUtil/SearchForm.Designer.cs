namespace SearchUtil
{
    partial class SearchForm
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
            this.btnSearch = new System.Windows.Forms.Button();
            this.chxExt = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOpen = new System.Windows.Forms.Button();
            this.rtbResults = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pbSearch = new System.Windows.Forms.ProgressBar();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.chxSubDir = new System.Windows.Forms.CheckBox();
            this.chxCase = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.btnCustomize = new System.Windows.Forms.Button();
            this.txtDir = new System.Windows.Forms.ComboBox();
            this.txtExt = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnOwner = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSearch
            // 
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnSearch.Location = new System.Drawing.Point(12, 123);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(100, 50);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "&Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // chxExt
            // 
            this.chxExt.AutoSize = true;
            this.chxExt.Location = new System.Drawing.Point(12, 64);
            this.chxExt.Name = "chxExt";
            this.chxExt.Size = new System.Drawing.Size(112, 17);
            this.chxExt.TabIndex = 2;
            this.chxExt.Text = "Specify extension:";
            this.chxExt.UseVisualStyleBackColor = true;
            this.chxExt.CheckedChanged += new System.EventHandler(this.chxExt_CheckedChanged);
            this.chxExt.KeyUp += new System.Windows.Forms.KeyEventHandler(this.checkbox_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Search Directory:";
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(104, 4);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "O&pen";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // rtbResults
            // 
            this.rtbResults.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.rtbResults.BackColor = System.Drawing.Color.White;
            this.rtbResults.Cursor = System.Windows.Forms.Cursors.Default;
            this.rtbResults.DetectUrls = false;
            this.rtbResults.Location = new System.Drawing.Point(0, 179);
            this.rtbResults.Name = "rtbResults";
            this.rtbResults.ReadOnly = true;
            this.rtbResults.Size = new System.Drawing.Size(658, 460);
            this.rtbResults.TabIndex = 9;
            this.rtbResults.Text = "";
            this.rtbResults.WordWrap = false;
            this.rtbResults.TextChanged += new System.EventHandler(this.rtbResults_TextChanged);
            this.rtbResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.rtbResults_MouseDoubleClick);
            this.rtbResults.MouseUp += new System.Windows.Forms.MouseEventHandler(this.rtbResults_MouseUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Search Text:";
            // 
            // pbSearch
            // 
            this.pbSearch.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.pbSearch.Location = new System.Drawing.Point(118, 150);
            this.pbSearch.Name = "pbSearch";
            this.pbSearch.Size = new System.Drawing.Size(528, 23);
            this.pbSearch.TabIndex = 11;
            // 
            // chxSubDir
            // 
            this.chxSubDir.AutoSize = true;
            this.chxSubDir.Checked = true;
            this.chxSubDir.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxSubDir.Location = new System.Drawing.Point(346, 64);
            this.chxSubDir.Name = "chxSubDir";
            this.chxSubDir.Size = new System.Drawing.Size(130, 17);
            this.chxSubDir.TabIndex = 5;
            this.chxSubDir.Text = "Search Subdirectories";
            this.chxSubDir.UseVisualStyleBackColor = true;
            this.chxSubDir.KeyUp += new System.Windows.Forms.KeyEventHandler(this.checkbox_KeyUp);
            // 
            // chxCase
            // 
            this.chxCase.AutoSize = true;
            this.chxCase.Location = new System.Drawing.Point(482, 64);
            this.chxCase.Name = "chxCase";
            this.chxCase.Size = new System.Drawing.Size(83, 17);
            this.chxCase.TabIndex = 6;
            this.chxCase.Text = "Match Case";
            this.chxCase.UseVisualStyleBackColor = true;
            this.chxCase.KeyUp += new System.Windows.Forms.KeyEventHandler(this.checkbox_KeyUp);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(571, 4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 11;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.Location = new System.Drawing.Point(265, 60);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(75, 23);
            this.btnAdvanced.TabIndex = 4;
            this.btnAdvanced.Text = "Advanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // btnCustomize
            // 
            this.btnCustomize.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnCustomize.Location = new System.Drawing.Point(490, 4);
            this.btnCustomize.Name = "btnCustomize";
            this.btnCustomize.Size = new System.Drawing.Size(75, 23);
            this.btnCustomize.TabIndex = 10;
            this.btnCustomize.Text = "Customize";
            this.btnCustomize.UseVisualStyleBackColor = true;
            this.btnCustomize.Click += new System.EventHandler(this.btnCustomize_Click);
            // 
            // txtDir
            // 
            this.txtDir.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.txtDir.FormattingEnabled = true;
            this.txtDir.Location = new System.Drawing.Point(12, 33);
            this.txtDir.Name = "txtDir";
            this.txtDir.Size = new System.Drawing.Size(634, 21);
            this.txtDir.Sorted = true;
            this.txtDir.TabIndex = 1;
            this.txtDir.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDir_KeyDown);
            // 
            // txtExt
            // 
            this.txtExt.FormattingEnabled = true;
            this.txtExt.Location = new System.Drawing.Point(129, 60);
            this.txtExt.Name = "txtExt";
            this.txtExt.Size = new System.Drawing.Size(130, 21);
            this.txtExt.Sorted = true;
            this.txtExt.TabIndex = 3;
            this.txtExt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtExt_KeyDown);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.txtSearch.FormattingEnabled = true;
            this.txtSearch.Location = new System.Drawing.Point(86, 96);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(560, 21);
            this.txtSearch.Sorted = true;
            this.txtSearch.TabIndex = 7;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblStatus.Location = new System.Drawing.Point(118, 123);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(528, 23);
            this.lblStatus.TabIndex = 12;
            this.lblStatus.Text = "Ready.";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOwner
            // 
            this.btnOwner.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnOwner.Location = new System.Drawing.Point(386, 4);
            this.btnOwner.Name = "btnOwner";
            this.btnOwner.Size = new System.Drawing.Size(98, 23);
            this.btnOwner.TabIndex = 13;
            this.btnOwner.Text = "Search By Owner";
            this.btnOwner.UseVisualStyleBackColor = true;
            this.btnOwner.Visible = false;
            this.btnOwner.Click += new System.EventHandler(this.btnOwner_Click);
            // 
            // SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(658, 639);
            this.Controls.Add(this.btnOwner);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.txtExt);
            this.Controls.Add(this.txtDir);
            this.Controls.Add(this.btnCustomize);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.chxCase);
            this.Controls.Add(this.chxSubDir);
            this.Controls.Add(this.pbSearch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rtbResults);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chxExt);
            this.Controls.Add(this.btnSearch);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "SearchForm";
            this.Text = "Search Utility";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SearchUtil_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.CheckBox chxExt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.RichTextBox rtbResults;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar pbSearch;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox chxSubDir;
        private System.Windows.Forms.CheckBox chxCase;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.Button btnCustomize;
        private System.Windows.Forms.ComboBox txtDir;
        private System.Windows.Forms.ComboBox txtExt;
        private System.Windows.Forms.ComboBox txtSearch;
        private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Button btnOwner;
    }
}