
namespace WinFormsApp1
{
    partial class ResearchForm
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
            lbxAvailable = new System.Windows.Forms.ListBox();
            lbxDone = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lblName = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            lblLast = new System.Windows.Forms.Label();
            lblProgress = new System.Windows.Forms.Label();
            lblCost = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            lblTotal = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lbxUnlocks = new System.Windows.Forms.ListBox();
            label8 = new System.Windows.Forms.Label();
            lvwAlso = new System.Windows.Forms.ListView();
            cbxAll = new System.Windows.Forms.CheckBox();
            cbxFilter = new System.Windows.Forms.CheckBox();
            lblUpgInf = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lbxAvailable
            // 
            lbxAvailable.FormattingEnabled = true;
            lbxAvailable.ItemHeight = 15;
            lbxAvailable.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            lbxAvailable.Location = new System.Drawing.Point(0, 17);
            lbxAvailable.Margin = new System.Windows.Forms.Padding(2);
            lbxAvailable.Name = "lbxAvailable";
            lbxAvailable.Size = new System.Drawing.Size(127, 94);
            lbxAvailable.TabIndex = 1;
            lbxAvailable.SelectedValueChanged += LB_SelectedValueChanged;
            lbxAvailable.MouseDoubleClick += LbAvailable_MouseDoubleClick;
            // 
            // lbxDone
            // 
            lbxDone.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lbxDone.FormattingEnabled = true;
            lbxDone.ItemHeight = 15;
            lbxDone.Location = new System.Drawing.Point(0, 130);
            lbxDone.Margin = new System.Windows.Forms.Padding(2);
            lbxDone.Name = "lbxDone";
            lbxDone.Size = new System.Drawing.Size(127, 229);
            lbxDone.TabIndex = 2;
            lbxDone.SelectedValueChanged += LB_SelectedValueChanged;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(126, 15);
            label1.TabIndex = 2;
            label1.Text = "Available:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(0, 111);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(126, 15);
            label2.TabIndex = 3;
            label2.Text = "Researched:";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblName
            // 
            lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblName.Location = new System.Drawing.Point(130, 17);
            lblName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(218, 30);
            lblName.TabIndex = 4;
            lblName.Text = "lblName";
            lblName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(130, 47);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(88, 15);
            label4.TabIndex = 5;
            label4.Text = "Last Researched";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(130, 62);
            label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(88, 17);
            label5.TabIndex = 6;
            label5.Text = "Progress";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.Location = new System.Drawing.Point(130, 79);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(88, 15);
            label6.TabIndex = 7;
            label6.Text = "Cost";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLast
            // 
            lblLast.Location = new System.Drawing.Point(260, 47);
            lblLast.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblLast.Name = "lblLast";
            lblLast.Size = new System.Drawing.Size(88, 15);
            lblLast.TabIndex = 8;
            lblLast.Text = "lblLast";
            lblLast.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProgress
            // 
            lblProgress.Location = new System.Drawing.Point(260, 62);
            lblProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new System.Drawing.Size(88, 17);
            lblProgress.TabIndex = 9;
            lblProgress.Text = "lblProgress";
            lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCost
            // 
            lblCost.Location = new System.Drawing.Point(260, 79);
            lblCost.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblCost.Name = "lblCost";
            lblCost.Size = new System.Drawing.Size(88, 15);
            lblCost.TabIndex = 10;
            lblCost.Text = "lblCost";
            lblCost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Location = new System.Drawing.Point(238, 330);
            btnOK.Margin = new System.Windows.Forms.Padding(2);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(78, 20);
            btnOK.TabIndex = 11;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(320, 330);
            btnCancel.Margin = new System.Windows.Forms.Padding(2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(78, 20);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblTotal
            // 
            lblTotal.Location = new System.Drawing.Point(260, 0);
            lblTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new System.Drawing.Size(88, 15);
            lblTotal.TabIndex = 14;
            lblTotal.Text = "lblTotal";
            lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            label7.Location = new System.Drawing.Point(130, 0);
            label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(88, 15);
            label7.TabIndex = 13;
            label7.Text = "Total Research";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(132, 143);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(126, 15);
            label3.TabIndex = 15;
            label3.Text = "Unlocks:";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbxUnlocks
            // 
            lbxUnlocks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lbxUnlocks.BackColor = System.Drawing.SystemColors.Window;
            lbxUnlocks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lbxUnlocks.FormattingEnabled = true;
            lbxUnlocks.ItemHeight = 15;
            lbxUnlocks.Location = new System.Drawing.Point(130, 160);
            lbxUnlocks.Margin = new System.Windows.Forms.Padding(2);
            lbxUnlocks.Name = "lbxUnlocks";
            lbxUnlocks.Size = new System.Drawing.Size(126, 150);
            lbxUnlocks.TabIndex = 3;
            lbxUnlocks.SelectedValueChanged += LbxUnlocks_SelectedValueChanged;
            // 
            // label8
            // 
            label8.Location = new System.Drawing.Point(262, 143);
            label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(136, 15);
            label8.TabIndex = 18;
            label8.Text = "Also Needs:";
            label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvwAlso
            // 
            lvwAlso.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lvwAlso.Enabled = false;
            lvwAlso.Location = new System.Drawing.Point(260, 160);
            lvwAlso.Margin = new System.Windows.Forms.Padding(2);
            lvwAlso.Name = "lvwAlso";
            lvwAlso.Size = new System.Drawing.Size(138, 143);
            lvwAlso.TabIndex = 19;
            lvwAlso.UseCompatibleStateImageBehavior = false;
            lvwAlso.View = System.Windows.Forms.View.List;
            // 
            // cbxAll
            // 
            cbxAll.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbxAll.AutoSize = true;
            cbxAll.Location = new System.Drawing.Point(131, 307);
            cbxAll.Margin = new System.Windows.Forms.Padding(2);
            cbxAll.Name = "cbxAll";
            cbxAll.Size = new System.Drawing.Size(69, 19);
            cbxAll.TabIndex = 20;
            cbxAll.Text = "Full Tree";
            cbxAll.UseVisualStyleBackColor = true;
            cbxAll.CheckedChanged += CBX_CheckedChanged;
            // 
            // cbxFilter
            // 
            cbxFilter.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbxFilter.AutoSize = true;
            cbxFilter.Checked = true;
            cbxFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxFilter.Location = new System.Drawing.Point(204, 307);
            cbxFilter.Margin = new System.Windows.Forms.Padding(2);
            cbxFilter.Name = "cbxFilter";
            cbxFilter.Size = new System.Drawing.Size(114, 19);
            cbxFilter.TabIndex = 21;
            cbxFilter.Text = "Hide Researched";
            cbxFilter.UseVisualStyleBackColor = true;
            cbxFilter.CheckedChanged += CBX_CheckedChanged;
            // 
            // lblUpgInf
            // 
            lblUpgInf.Location = new System.Drawing.Point(132, 94);
            lblUpgInf.Name = "lblUpgInf";
            lblUpgInf.Size = new System.Drawing.Size(265, 45);
            lblUpgInf.TabIndex = 22;
            lblUpgInf.Text = "lblUpgInf\r\nlblUpgInf\r\nlblUpgInf";
            lblUpgInf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ResearchForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(409, 361);
            Controls.Add(cbxFilter);
            Controls.Add(cbxAll);
            Controls.Add(lvwAlso);
            Controls.Add(label8);
            Controls.Add(lbxUnlocks);
            Controls.Add(label3);
            Controls.Add(lblTotal);
            Controls.Add(label7);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(lblCost);
            Controls.Add(lblProgress);
            Controls.Add(lblLast);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(lblName);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lbxDone);
            Controls.Add(lbxAvailable);
            Controls.Add(lblUpgInf);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Margin = new System.Windows.Forms.Padding(2);
            MinimumSize = new System.Drawing.Size(425, 376);
            Name = "ResearchForm";
            ShowIcon = false;
            Text = "Research";
            Shown += Research_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox lbxAvailable;
        private System.Windows.Forms.ListBox lbxDone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblLast;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lbxUnlocks;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListView lvwAlso;
        private System.Windows.Forms.CheckBox cbxAll;
        private System.Windows.Forms.CheckBox cbxFilter;
        private System.Windows.Forms.Label lblUpgInf;
    }
}