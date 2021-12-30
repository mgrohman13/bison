
namespace WinFormsApp1
{
    partial class Research
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
            this.lbxAvailable = new System.Windows.Forms.ListBox();
            this.lbxDone = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblLast = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblTotal = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbxUnlocks = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.lvwAlso = new System.Windows.Forms.ListView();
            this.cbxAll = new System.Windows.Forms.CheckBox();
            this.cbxFilter = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lbxAvailable
            // 
            this.lbxAvailable.FormattingEnabled = true;
            this.lbxAvailable.ItemHeight = 25;
            this.lbxAvailable.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.lbxAvailable.Location = new System.Drawing.Point(0, 28);
            this.lbxAvailable.Name = "lbxAvailable";
            this.lbxAvailable.Size = new System.Drawing.Size(180, 129);
            this.lbxAvailable.TabIndex = 1;
            this.lbxAvailable.SelectedValueChanged += new System.EventHandler(this.LB_SelectedValueChanged);
            this.lbxAvailable.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LbAvailable_MouseDoubleClick);
            // 
            // lbxDone
            // 
            this.lbxDone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxDone.FormattingEnabled = true;
            this.lbxDone.ItemHeight = 25;
            this.lbxDone.Location = new System.Drawing.Point(0, 188);
            this.lbxDone.Name = "lbxDone";
            this.lbxDone.Size = new System.Drawing.Size(180, 354);
            this.lbxDone.TabIndex = 2;
            this.lbxDone.SelectedValueChanged += new System.EventHandler(this.LB_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Available:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 160);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Researched:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblName.Location = new System.Drawing.Point(186, 28);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(311, 50);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "lblName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(186, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 25);
            this.label4.TabIndex = 5;
            this.label4.Text = "Last Researched";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(186, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 29);
            this.label5.TabIndex = 6;
            this.label5.Text = "Progress";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(186, 132);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 25);
            this.label6.TabIndex = 7;
            this.label6.Text = "Cost";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLast
            // 
            this.lblLast.Location = new System.Drawing.Point(372, 78);
            this.lblLast.Name = "lblLast";
            this.lblLast.Size = new System.Drawing.Size(125, 25);
            this.lblLast.TabIndex = 8;
            this.lblLast.Text = "lblLast";
            this.lblLast.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProgress
            // 
            this.lblProgress.Location = new System.Drawing.Point(372, 103);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(125, 29);
            this.lblProgress.TabIndex = 9;
            this.lblProgress.Text = "lblProgress";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCost
            // 
            this.lblCost.Location = new System.Drawing.Point(372, 132);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(125, 25);
            this.lblCost.TabIndex = 10;
            this.lblCost.Text = "lblCost";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(336, 498);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 34);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(454, 498);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 34);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblTotal
            // 
            this.lblTotal.Location = new System.Drawing.Point(372, 0);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(125, 25);
            this.lblTotal.TabIndex = 14;
            this.lblTotal.Text = "lblTotal";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(186, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 25);
            this.label7.TabIndex = 13;
            this.label7.Text = "Total Research";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(186, 188);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 25);
            this.label3.TabIndex = 15;
            this.label3.Text = "Unlocks:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbxUnlocks
            // 
            this.lbxUnlocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxUnlocks.BackColor = System.Drawing.SystemColors.Window;
            this.lbxUnlocks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbxUnlocks.FormattingEnabled = true;
            this.lbxUnlocks.ItemHeight = 25;
            this.lbxUnlocks.Location = new System.Drawing.Point(186, 216);
            this.lbxUnlocks.Name = "lbxUnlocks";
            this.lbxUnlocks.Size = new System.Drawing.Size(180, 250);
            this.lbxUnlocks.TabIndex = 3;
            this.lbxUnlocks.SelectedValueChanged += new System.EventHandler(this.LbxUnlocks_SelectedValueChanged);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(372, 188);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(180, 25);
            this.label8.TabIndex = 18;
            this.label8.Text = "Also Needs:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvwAlso
            // 
            this.lvwAlso.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwAlso.Enabled = false;
            this.lvwAlso.HideSelection = false;
            this.lvwAlso.Location = new System.Drawing.Point(372, 216);
            this.lvwAlso.Name = "lvwAlso";
            this.lvwAlso.Size = new System.Drawing.Size(194, 241);
            this.lvwAlso.TabIndex = 19;
            this.lvwAlso.UseCompatibleStateImageBehavior = false;
            this.lvwAlso.View = System.Windows.Forms.View.List;
            // 
            // cbxAll
            // 
            this.cbxAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxAll.AutoSize = true;
            this.cbxAll.Location = new System.Drawing.Point(186, 463);
            this.cbxAll.Name = "cbxAll";
            this.cbxAll.Size = new System.Drawing.Size(101, 29);
            this.cbxAll.TabIndex = 20;
            this.cbxAll.Text = "Full Tree";
            this.cbxAll.UseVisualStyleBackColor = true;
            this.cbxAll.CheckedChanged += new System.EventHandler(this.CBX_CheckedChanged);
            // 
            // cbxFilter
            // 
            this.cbxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxFilter.AutoSize = true;
            this.cbxFilter.Location = new System.Drawing.Point(293, 463);
            this.cbxFilter.Name = "cbxFilter";
            this.cbxFilter.Size = new System.Drawing.Size(169, 29);
            this.cbxFilter.TabIndex = 21;
            this.cbxFilter.Text = "Hide Researched";
            this.cbxFilter.UseVisualStyleBackColor = true;
            this.cbxFilter.CheckedChanged += new System.EventHandler(this.CBX_CheckedChanged);
            // 
            // Research
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(578, 544);
            this.Controls.Add(this.cbxFilter);
            this.Controls.Add(this.cbxAll);
            this.Controls.Add(this.lvwAlso);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lbxUnlocks);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblLast);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbxDone);
            this.Controls.Add(this.lbxAvailable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(600, 600);
            this.Name = "Research";
            this.ShowIcon = false;
            this.Text = "Research";
            this.Shown += new System.EventHandler(this.Research_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}