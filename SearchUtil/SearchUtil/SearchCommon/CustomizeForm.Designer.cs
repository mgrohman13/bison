namespace SearchCommon
{
    partial class CustomizeForm
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
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnApply = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.nudResPerFile = new System.Windows.Forms.NumericUpDown();
			this.btnDefaults = new System.Windows.Forms.Button();
			this.cbxNotepad = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.chxAutoScroll = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.lblHighlightColor = new System.Windows.Forms.Label();
			this.lblTextColor = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.lblBackColor = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.label5 = new System.Windows.Forms.Label();
			this.nudSpan = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.nudMaxHist = new System.Windows.Forms.NumericUpDown();
			this.nudDropDownItems = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.btnRandom = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.nudResPerFile)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSpan)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxHist)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudDropDownItems)).BeginInit();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(143, 288);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 12;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(224, 288);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 13;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnApply
			// 
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnApply.Location = new System.Drawing.Point(305, 288);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(75, 23);
			this.btnApply.TabIndex = 15;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Show up to";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(129, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(74, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "results per file.";
			// 
			// nudResPerFile
			// 
			this.nudResPerFile.Location = new System.Drawing.Point(79, 7);
			this.nudResPerFile.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.nudResPerFile.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudResPerFile.Name = "nudResPerFile";
			this.nudResPerFile.Size = new System.Drawing.Size(44, 20);
			this.nudResPerFile.TabIndex = 0;
			this.nudResPerFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.nudResPerFile.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.nudResPerFile.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
			// 
			// btnDefaults
			// 
			this.btnDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDefaults.Location = new System.Drawing.Point(286, 12);
			this.btnDefaults.Name = "btnDefaults";
			this.btnDefaults.Size = new System.Drawing.Size(94, 23);
			this.btnDefaults.TabIndex = 16;
			this.btnDefaults.Text = "Restore Defaults";
			this.btnDefaults.UseVisualStyleBackColor = true;
			this.btnDefaults.Click += new System.EventHandler(this.btnDefaults_Click);
			// 
			// cbxNotepad
			// 
			this.cbxNotepad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxNotepad.FormattingEnabled = true;
			this.cbxNotepad.Items.AddRange(new object[] {
            "Right Click",
            "Doubleclick",
            "Both",
            "None"});
			this.cbxNotepad.Location = new System.Drawing.Point(112, 33);
			this.cbxNotepad.Name = "cbxNotepad";
			this.cbxNotepad.Size = new System.Drawing.Size(100, 21);
			this.cbxNotepad.TabIndex = 1;
			this.cbxNotepad.SelectedIndexChanged += new System.EventHandler(this.cbxNotepad_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 36);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(94, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Open Notepad by ";
			// 
			// chxAutoScroll
			// 
			this.chxAutoScroll.AutoSize = true;
			this.chxAutoScroll.Location = new System.Drawing.Point(12, 60);
			this.chxAutoScroll.Name = "chxAutoScroll";
			this.chxAutoScroll.Size = new System.Drawing.Size(108, 17);
			this.chxAutoScroll.TabIndex = 6;
			this.chxAutoScroll.Text = "Auto scroll results";
			this.chxAutoScroll.UseVisualStyleBackColor = true;
			this.chxAutoScroll.CheckedChanged += new System.EventHandler(this.chxAutoScroll_CheckedChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(28, 139);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(132, 13);
			this.label4.TabIndex = 10;
			this.label4.Text = "Search text highlight color:";
			// 
			// lblHighlightColor
			// 
			this.lblHighlightColor.BackColor = System.Drawing.Color.White;
			this.lblHighlightColor.Location = new System.Drawing.Point(166, 131);
			this.lblHighlightColor.Name = "lblHighlightColor";
			this.lblHighlightColor.Size = new System.Drawing.Size(30, 30);
			this.lblHighlightColor.TabIndex = 7;
			this.lblHighlightColor.Click += new System.EventHandler(this.lblColor_Click);
			// 
			// lblTextColor
			// 
			this.lblTextColor.BackColor = System.Drawing.Color.White;
			this.lblTextColor.Location = new System.Drawing.Point(166, 162);
			this.lblTextColor.Name = "lblTextColor";
			this.lblTextColor.Size = new System.Drawing.Size(30, 30);
			this.lblTextColor.TabIndex = 9;
			this.lblTextColor.Click += new System.EventHandler(this.lblColor_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(52, 170);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(108, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "Result field text color:";
			// 
			// lblBackColor
			// 
			this.lblBackColor.BackColor = System.Drawing.Color.White;
			this.lblBackColor.Location = new System.Drawing.Point(166, 193);
			this.lblBackColor.Name = "lblBackColor";
			this.lblBackColor.Size = new System.Drawing.Size(30, 30);
			this.lblBackColor.TabIndex = 10;
			this.lblBackColor.Click += new System.EventHandler(this.lblColor_Click);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 201);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(148, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "Result field background color:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 82);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(83, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Keep history for ";
			// 
			// nudSpan
			// 
			this.nudSpan.DecimalPlaces = 1;
			this.nudSpan.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.nudSpan.Location = new System.Drawing.Point(101, 80);
			this.nudSpan.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            65536});
			this.nudSpan.Name = "nudSpan";
			this.nudSpan.Size = new System.Drawing.Size(52, 20);
			this.nudSpan.TabIndex = 2;
			this.nudSpan.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.nudSpan.Value = new decimal(new int[] {
            9999,
            0,
            0,
            65536});
			this.nudSpan.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(159, 82);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(118, 13);
			this.label7.TabIndex = 18;
			this.label7.Text = "days with a maximum of";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(333, 82);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(41, 13);
			this.label9.TabIndex = 19;
			this.label9.Text = "entries.";
			// 
			// nudMaxHist
			// 
			this.nudMaxHist.Location = new System.Drawing.Point(283, 80);
			this.nudMaxHist.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.nudMaxHist.Name = "nudMaxHist";
			this.nudMaxHist.Size = new System.Drawing.Size(44, 20);
			this.nudMaxHist.TabIndex = 3;
			this.nudMaxHist.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.nudMaxHist.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.nudMaxHist.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
			// 
			// nudDropDownItems
			// 
			this.nudDropDownItems.Location = new System.Drawing.Point(52, 106);
			this.nudDropDownItems.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudDropDownItems.Name = "nudDropDownItems";
			this.nudDropDownItems.Size = new System.Drawing.Size(44, 20);
			this.nudDropDownItems.TabIndex = 5;
			this.nudDropDownItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.nudDropDownItems.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudDropDownItems.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(12, 108);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(34, 13);
			this.label10.TabIndex = 22;
			this.label10.Text = "Show";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(102, 108);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(171, 13);
			this.label11.TabIndex = 23;
			this.label11.Text = "items at once in drop down menus.";
			// 
			// btnRandom
			// 
			this.btnRandom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRandom.Location = new System.Drawing.Point(286, 41);
			this.btnRandom.Name = "btnRandom";
			this.btnRandom.Size = new System.Drawing.Size(94, 23);
			this.btnRandom.TabIndex = 24;
			this.btnRandom.Text = "Randomize";
			this.btnRandom.UseVisualStyleBackColor = true;
			this.btnRandom.Click += new System.EventHandler(this.btnRandom_Click);
			// 
			// CustomizeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(392, 323);
			this.Controls.Add(this.btnRandom);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.nudDropDownItems);
			this.Controls.Add(this.nudMaxHist);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.nudSpan);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.lblBackColor);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.lblTextColor);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.lblHighlightColor);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.chxAutoScroll);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cbxNotepad);
			this.Controls.Add(this.btnDefaults);
			this.Controls.Add(this.nudResPerFile);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CustomizeForm";
			this.Text = "Customize";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CustomizeForm_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.nudResPerFile)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSpan)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxHist)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudDropDownItems)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudResPerFile;
        private System.Windows.Forms.Button btnDefaults;
        private System.Windows.Forms.ComboBox cbxNotepad;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chxAutoScroll;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblHighlightColor;
        private System.Windows.Forms.Label lblTextColor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblBackColor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudSpan;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nudMaxHist;
        private System.Windows.Forms.NumericUpDown nudDropDownItems;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnRandom;
    }
}