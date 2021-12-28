
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
            this.lbAvailable = new System.Windows.Forms.ListBox();
            this.lbDone = new System.Windows.Forms.ListBox();
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
            this.lbxAlso = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbAvailable
            // 
            this.lbAvailable.FormattingEnabled = true;
            this.lbAvailable.ItemHeight = 25;
            this.lbAvailable.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.lbAvailable.Location = new System.Drawing.Point(0, 37);
            this.lbAvailable.Name = "lbAvailable";
            this.lbAvailable.Size = new System.Drawing.Size(180, 129);
            this.lbAvailable.TabIndex = 1;
            this.lbAvailable.SelectedValueChanged += new System.EventHandler(this.LB_SelectedValueChanged);
            this.lbAvailable.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LbAvailable_MouseDoubleClick);
            // 
            // lbDone
            // 
            this.lbDone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbDone.FormattingEnabled = true;
            this.lbDone.ItemHeight = 25;
            this.lbDone.Location = new System.Drawing.Point(0, 197);
            this.lbDone.Name = "lbDone";
            this.lbDone.Size = new System.Drawing.Size(180, 229);
            this.lbDone.TabIndex = 2;
            this.lbDone.SelectedValueChanged += new System.EventHandler(this.LB_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Available:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 169);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Researched:";
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.Location = new System.Drawing.Point(186, 34);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(388, 50);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "lblName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(186, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 50);
            this.label4.TabIndex = 5;
            this.label4.Text = "Last Researched";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(186, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 50);
            this.label5.TabIndex = 6;
            this.label5.Text = "Progress";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(186, 184);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(137, 50);
            this.label6.TabIndex = 7;
            this.label6.Text = "Cost";
            this.label6.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblLast
            // 
            this.lblLast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLast.Location = new System.Drawing.Point(480, 108);
            this.lblLast.Name = "lblLast";
            this.lblLast.Size = new System.Drawing.Size(100, 25);
            this.lblLast.TabIndex = 8;
            this.lblLast.Text = "lblLast";
            this.lblLast.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.Location = new System.Drawing.Point(480, 158);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(100, 25);
            this.lblProgress.TabIndex = 9;
            this.lblProgress.Text = "lblProgress";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblCost
            // 
            this.lblCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCost.Location = new System.Drawing.Point(480, 208);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(100, 25);
            this.lblCost.TabIndex = 10;
            this.lblCost.Text = "lblCost";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(350, 392);
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
            this.btnCancel.Location = new System.Drawing.Point(468, 392);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 34);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotal.Location = new System.Drawing.Point(480, 9);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(100, 25);
            this.lblTotal.TabIndex = 14;
            this.lblTotal.Text = "lblTotal";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(186, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(137, 25);
            this.label7.TabIndex = 13;
            this.label7.Text = "Total Research";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(186, 234);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 50);
            this.label3.TabIndex = 15;
            this.label3.Text = "Unlocks";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lbxUnlocks
            // 
            this.lbxUnlocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxUnlocks.BackColor = System.Drawing.SystemColors.Window;
            this.lbxUnlocks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbxUnlocks.FormattingEnabled = true;
            this.lbxUnlocks.ItemHeight = 25;
            this.lbxUnlocks.Location = new System.Drawing.Point(186, 287);
            this.lbxUnlocks.Name = "lbxUnlocks";
            this.lbxUnlocks.Size = new System.Drawing.Size(180, 100);
            this.lbxUnlocks.TabIndex = 3;
            this.lbxUnlocks.SelectedValueChanged += new System.EventHandler(this.LbxUnlocks_SelectedValueChanged);
            // 
            // lbxAlso
            // 
            this.lbxAlso.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxAlso.BackColor = System.Drawing.SystemColors.Control;
            this.lbxAlso.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbxAlso.FormattingEnabled = true;
            this.lbxAlso.ItemHeight = 25;
            this.lbxAlso.Location = new System.Drawing.Point(372, 287);
            this.lbxAlso.Name = "lbxAlso";
            this.lbxAlso.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lbxAlso.Size = new System.Drawing.Size(180, 100);
            this.lbxAlso.TabIndex = 17;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(372, 234);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(180, 50);
            this.label8.TabIndex = 18;
            this.label8.Text = "Also Needs";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // Research
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(592, 438);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lbxAlso);
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
            this.Controls.Add(this.lbDone);
            this.Controls.Add(this.lbAvailable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(494, 494);
            this.Name = "Research";
            this.ShowIcon = false;
            this.Text = "Research";
            this.Shown += new System.EventHandler(this.Research_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbAvailable;
        private System.Windows.Forms.ListBox lbDone;
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
        private System.Windows.Forms.ListBox lbxAlso;
        private System.Windows.Forms.Label label8;
    }
}