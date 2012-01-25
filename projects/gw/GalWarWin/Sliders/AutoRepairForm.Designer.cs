namespace GalWarWin.Sliders
{
    partial class AutoRepairForm
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
            this.rbPct = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtPct = new System.Windows.Forms.TextBox();
            this.txtConst = new System.Windows.Forms.TextBox();
            this.txtTurn = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.rbManual = new System.Windows.Forms.RadioButton();
            this.rbNone = new System.Windows.Forms.RadioButton();
            this.rbTurn = new System.Windows.Forms.RadioButton();
            this.rbConst = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rbPct
            // 
            this.rbPct.AutoSize = true;
            this.rbPct.Location = new System.Drawing.Point(6, 12);
            this.rbPct.Name = "rbPct";
            this.rbPct.Size = new System.Drawing.Size(62, 17);
            this.rbPct.TabIndex = 2;
            this.rbPct.TabStop = true;
            this.rbPct.Text = "Percent";
            this.rbPct.UseVisualStyleBackColor = true;
            this.rbPct.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtPct);
            this.groupBox1.Controls.Add(this.txtConst);
            this.groupBox1.Controls.Add(this.txtTurn);
            this.groupBox1.Controls.Add(this.btnOK);
            this.groupBox1.Controls.Add(this.btnCancel);
            this.groupBox1.Controls.Add(this.rbManual);
            this.groupBox1.Controls.Add(this.rbNone);
            this.groupBox1.Controls.Add(this.rbTurn);
            this.groupBox1.Controls.Add(this.rbConst);
            this.groupBox1.Controls.Add(this.rbPct);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(168, 162);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // txtPct
            // 
            this.txtPct.Location = new System.Drawing.Point(74, 11);
            this.txtPct.Name = "txtPct";
            this.txtPct.Size = new System.Drawing.Size(82, 20);
            this.txtPct.TabIndex = 3;
            this.txtPct.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // txtConst
            // 
            this.txtConst.Location = new System.Drawing.Point(79, 34);
            this.txtConst.Name = "txtConst";
            this.txtConst.Size = new System.Drawing.Size(77, 20);
            this.txtConst.TabIndex = 5;
            this.txtConst.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // txtTurn
            // 
            this.txtTurn.Location = new System.Drawing.Point(64, 57);
            this.txtTurn.Name = "txtTurn";
            this.txtTurn.Size = new System.Drawing.Size(92, 20);
            this.txtTurn.TabIndex = 7;
            this.txtTurn.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(6, 127);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(87, 127);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // rbManual
            // 
            this.rbManual.AutoSize = true;
            this.rbManual.Location = new System.Drawing.Point(6, 104);
            this.rbManual.Name = "rbManual";
            this.rbManual.Size = new System.Drawing.Size(60, 17);
            this.rbManual.TabIndex = 9;
            this.rbManual.TabStop = true;
            this.rbManual.Text = "Manual";
            this.rbManual.UseVisualStyleBackColor = true;
            this.rbManual.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbNone
            // 
            this.rbNone.AutoSize = true;
            this.rbNone.Location = new System.Drawing.Point(6, 81);
            this.rbNone.Name = "rbNone";
            this.rbNone.Size = new System.Drawing.Size(73, 17);
            this.rbNone.TabIndex = 8;
            this.rbNone.TabStop = true;
            this.rbNone.Text = "No Repair";
            this.rbNone.UseVisualStyleBackColor = true;
            this.rbNone.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbTurn
            // 
            this.rbTurn.AutoSize = true;
            this.rbTurn.Location = new System.Drawing.Point(6, 58);
            this.rbTurn.Name = "rbTurn";
            this.rbTurn.Size = new System.Drawing.Size(52, 17);
            this.rbTurn.TabIndex = 6;
            this.rbTurn.TabStop = true;
            this.rbTurn.Text = "Turns";
            this.rbTurn.UseVisualStyleBackColor = true;
            this.rbTurn.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbConst
            // 
            this.rbConst.AutoSize = true;
            this.rbConst.Location = new System.Drawing.Point(6, 35);
            this.rbConst.Name = "rbConst";
            this.rbConst.Size = new System.Drawing.Size(67, 17);
            this.rbConst.TabIndex = 4;
            this.rbConst.TabStop = true;
            this.rbConst.Text = "Constant";
            this.rbConst.UseVisualStyleBackColor = true;
            this.rbConst.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // AutoRepairForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(168, 162);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AutoRepairForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rbPct;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbTurn;
        private System.Windows.Forms.RadioButton rbConst;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rbManual;
        private System.Windows.Forms.RadioButton rbNone;
        private System.Windows.Forms.TextBox txtPct;
        private System.Windows.Forms.TextBox txtConst;
        private System.Windows.Forms.TextBox txtTurn;
    }
}