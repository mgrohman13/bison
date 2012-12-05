namespace GalWarWin
{
    partial class RepairAllForm
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
            if (disposing && ( components != null ))
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
            this.lblRepairs = new System.Windows.Forms.Label();
            this.label = new System.Windows.Forms.Label();
            this.lblIncome = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblGold = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMultiply = new System.Windows.Forms.TextBox();
            this.rbMultiply = new System.Windows.Forms.RadioButton();
            this.rbSet = new System.Windows.Forms.RadioButton();
            this.txtSet = new System.Windows.Forms.TextBox();
            this.cbManual = new System.Windows.Forms.CheckBox();
            this.cbOff = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRepair = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAvg = new System.Windows.Forms.TextBox();
            this.lblShips = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 233);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(93, 233);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblRepairs
            // 
            this.lblRepairs.Location = new System.Drawing.Point(96, 55);
            this.lblRepairs.Name = "lblRepairs";
            this.lblRepairs.Size = new System.Drawing.Size(40, 23);
            this.lblRepairs.TabIndex = 5;
            this.lblRepairs.Text = "+999.9";
            this.lblRepairs.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label
            // 
            this.label.Location = new System.Drawing.Point(0, 55);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(90, 23);
            this.label.TabIndex = 4;
            this.label.Text = "Repairs";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblIncome
            // 
            this.lblIncome.Location = new System.Drawing.Point(96, 32);
            this.lblIncome.Name = "lblIncome";
            this.lblIncome.Size = new System.Drawing.Size(40, 23);
            this.lblIncome.TabIndex = 7;
            this.lblIncome.Text = "+999.9";
            this.lblIncome.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "Income";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGold
            // 
            this.lblGold.Location = new System.Drawing.Point(96, 9);
            this.lblGold.Name = "lblGold";
            this.lblGold.Size = new System.Drawing.Size(40, 23);
            this.lblGold.TabIndex = 9;
            this.lblGold.Text = "+999.9";
            this.lblGold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 23);
            this.label2.TabIndex = 8;
            this.label2.Text = "Gold";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMultiply
            // 
            this.txtMultiply.Location = new System.Drawing.Point(78, 129);
            this.txtMultiply.Name = "txtMultiply";
            this.txtMultiply.Size = new System.Drawing.Size(90, 20);
            this.txtMultiply.TabIndex = 11;
            this.txtMultiply.Text = "1.0";
            this.txtMultiply.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // rbMultiply
            // 
            this.rbMultiply.AutoSize = true;
            this.rbMultiply.Checked = true;
            this.rbMultiply.Location = new System.Drawing.Point(12, 130);
            this.rbMultiply.Name = "rbMultiply";
            this.rbMultiply.Size = new System.Drawing.Size(60, 17);
            this.rbMultiply.TabIndex = 12;
            this.rbMultiply.TabStop = true;
            this.rbMultiply.Text = "Multiply";
            this.rbMultiply.UseVisualStyleBackColor = true;
            this.rbMultiply.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbSet
            // 
            this.rbSet.AutoSize = true;
            this.rbSet.Location = new System.Drawing.Point(12, 156);
            this.rbSet.Name = "rbSet";
            this.rbSet.Size = new System.Drawing.Size(41, 17);
            this.rbSet.TabIndex = 13;
            this.rbSet.Text = "Set";
            this.rbSet.UseVisualStyleBackColor = true;
            this.rbSet.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // txtSet
            // 
            this.txtSet.Enabled = false;
            this.txtSet.Location = new System.Drawing.Point(59, 155);
            this.txtSet.Name = "txtSet";
            this.txtSet.Size = new System.Drawing.Size(109, 20);
            this.txtSet.TabIndex = 14;
            this.txtSet.Text = "1.0";
            this.txtSet.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // cbManual
            // 
            this.cbManual.AutoSize = true;
            this.cbManual.Enabled = false;
            this.cbManual.Location = new System.Drawing.Point(61, 181);
            this.cbManual.Name = "cbManual";
            this.cbManual.Size = new System.Drawing.Size(61, 17);
            this.cbManual.TabIndex = 15;
            this.cbManual.Text = "Manual";
            this.cbManual.UseVisualStyleBackColor = true;
            this.cbManual.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbOff
            // 
            this.cbOff.AutoSize = true;
            this.cbOff.Enabled = false;
            this.cbOff.Location = new System.Drawing.Point(128, 181);
            this.cbOff.Name = "cbOff";
            this.cbOff.Size = new System.Drawing.Size(40, 17);
            this.cbOff.TabIndex = 16;
            this.cbOff.Text = "Off";
            this.cbOff.UseVisualStyleBackColor = true;
            this.cbOff.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "include";
            // 
            // btnRepair
            // 
            this.btnRepair.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnRepair.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnRepair.Location = new System.Drawing.Point(52, 204);
            this.btnRepair.Name = "btnRepair";
            this.btnRepair.Size = new System.Drawing.Size(76, 23);
            this.btnRepair.TabIndex = 18;
            this.btnRepair.Text = "Repair";
            this.btnRepair.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 23);
            this.label3.TabIndex = 19;
            this.label3.Text = "Average";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAvg
            // 
            this.txtAvg.Location = new System.Drawing.Point(96, 103);
            this.txtAvg.Name = "txtAvg";
            this.txtAvg.ReadOnly = true;
            this.txtAvg.Size = new System.Drawing.Size(72, 20);
            this.txtAvg.TabIndex = 20;
            // 
            // lblShips
            // 
            this.lblShips.Location = new System.Drawing.Point(96, 78);
            this.lblShips.Name = "lblShips";
            this.lblShips.Size = new System.Drawing.Size(40, 23);
            this.lblShips.TabIndex = 22;
            this.lblShips.Text = "99";
            this.lblShips.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(0, 78);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 23);
            this.label6.TabIndex = 21;
            this.label6.Text = "Ships";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // RepairAllForm
            // 
            this.AcceptButton = this.btnRepair;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(180, 268);
            this.Controls.Add(this.lblShips);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtAvg);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnRepair);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbOff);
            this.Controls.Add(this.cbManual);
            this.Controls.Add(this.txtSet);
            this.Controls.Add(this.rbSet);
            this.Controls.Add(this.rbMultiply);
            this.Controls.Add(this.txtMultiply);
            this.Controls.Add(this.lblGold);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblIncome);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblRepairs);
            this.Controls.Add(this.label);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RepairAllForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblRepairs;
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Label lblIncome;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblGold;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMultiply;
        private System.Windows.Forms.RadioButton rbMultiply;
        private System.Windows.Forms.RadioButton rbSet;
        private System.Windows.Forms.TextBox txtSet;
        private System.Windows.Forms.CheckBox cbManual;
        private System.Windows.Forms.CheckBox cbOff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRepair;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAvg;
        private System.Windows.Forms.Label lblShips;
        private System.Windows.Forms.Label label6;
    }
}