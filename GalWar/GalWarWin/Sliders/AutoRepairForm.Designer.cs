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
            this.txtDefault = new System.Windows.Forms.TextBox();
            this.rbDefault = new System.Windows.Forms.RadioButton();
            this.txtCost = new System.Windows.Forms.TextBox();
            this.rbCost = new System.Windows.Forms.RadioButton();
            this.txtPct = new System.Windows.Forms.TextBox();
            this.txtConst = new System.Windows.Forms.TextBox();
            this.txtTurn = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.rbNone = new System.Windows.Forms.RadioButton();
            this.rbTurn = new System.Windows.Forms.RadioButton();
            this.rbConst = new System.Windows.Forms.RadioButton();
            this.rbManual = new System.Windows.Forms.RadioButton();
            this.cbxProdRepair = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // rbPct
            // 
            this.rbPct.AutoSize = true;
            this.rbPct.Location = new System.Drawing.Point(18, 98);
            this.rbPct.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbPct.Name = "rbPct";
            this.rbPct.Size = new System.Drawing.Size(89, 24);
            this.rbPct.TabIndex = 2;
            this.rbPct.TabStop = true;
            this.rbPct.Text = "Percent";
            this.rbPct.UseVisualStyleBackColor = true;
            this.rbPct.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // txtDefault
            // 
            this.txtDefault.Location = new System.Drawing.Point(116, 17);
            this.txtDefault.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDefault.Name = "txtDefault";
            this.txtDefault.Size = new System.Drawing.Size(126, 26);
            this.txtDefault.TabIndex = 13;
            this.txtDefault.TextChanged += new System.EventHandler(this.txtDefault_TextChanged);
            // 
            // rbDefault
            // 
            this.rbDefault.AutoSize = true;
            this.rbDefault.Location = new System.Drawing.Point(18, 18);
            this.rbDefault.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbDefault.Name = "rbDefault";
            this.rbDefault.Size = new System.Drawing.Size(86, 24);
            this.rbDefault.TabIndex = 12;
            this.rbDefault.TabStop = true;
            this.rbDefault.Text = "Default";
            this.rbDefault.UseVisualStyleBackColor = true;
            this.rbDefault.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // txtCost
            // 
            this.txtCost.Location = new System.Drawing.Point(96, 137);
            this.txtCost.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCost.Name = "txtCost";
            this.txtCost.Size = new System.Drawing.Size(145, 26);
            this.txtCost.TabIndex = 11;
            this.txtCost.TextChanged += new System.EventHandler(this.txtCost_TextChanged);
            // 
            // rbCost
            // 
            this.rbCost.AutoSize = true;
            this.rbCost.Location = new System.Drawing.Point(18, 138);
            this.rbCost.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbCost.Name = "rbCost";
            this.rbCost.Size = new System.Drawing.Size(67, 24);
            this.rbCost.TabIndex = 10;
            this.rbCost.TabStop = true;
            this.rbCost.Text = "Cost";
            this.rbCost.UseVisualStyleBackColor = true;
            this.rbCost.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // txtPct
            // 
            this.txtPct.Location = new System.Drawing.Point(120, 97);
            this.txtPct.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPct.Name = "txtPct";
            this.txtPct.Size = new System.Drawing.Size(121, 26);
            this.txtPct.TabIndex = 3;
            this.txtPct.TextChanged += new System.EventHandler(this.txtPct_TextChanged);
            // 
            // txtConst
            // 
            this.txtConst.Location = new System.Drawing.Point(128, 57);
            this.txtConst.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtConst.Name = "txtConst";
            this.txtConst.Size = new System.Drawing.Size(114, 26);
            this.txtConst.TabIndex = 5;
            this.txtConst.TextChanged += new System.EventHandler(this.txtConst_TextChanged);
            // 
            // txtTurn
            // 
            this.txtTurn.Location = new System.Drawing.Point(105, 177);
            this.txtTurn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTurn.Name = "txtTurn";
            this.txtTurn.Size = new System.Drawing.Size(136, 26);
            this.txtTurn.TabIndex = 7;
            this.txtTurn.TextChanged += new System.EventHandler(this.txtTurn_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(18, 338);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(108, 35);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(135, 338);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(108, 35);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // rbNone
            // 
            this.rbNone.AutoSize = true;
            this.rbNone.Location = new System.Drawing.Point(18, 258);
            this.rbNone.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbNone.Name = "rbNone";
            this.rbNone.Size = new System.Drawing.Size(56, 24);
            this.rbNone.TabIndex = 8;
            this.rbNone.TabStop = true;
            this.rbNone.Text = "Off";
            this.rbNone.UseVisualStyleBackColor = true;
            this.rbNone.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbTurn
            // 
            this.rbTurn.AutoSize = true;
            this.rbTurn.Location = new System.Drawing.Point(18, 178);
            this.rbTurn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbTurn.Name = "rbTurn";
            this.rbTurn.Size = new System.Drawing.Size(74, 24);
            this.rbTurn.TabIndex = 6;
            this.rbTurn.TabStop = true;
            this.rbTurn.Text = "Turns";
            this.rbTurn.UseVisualStyleBackColor = true;
            this.rbTurn.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbConst
            // 
            this.rbConst.AutoSize = true;
            this.rbConst.Location = new System.Drawing.Point(18, 58);
            this.rbConst.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbConst.Name = "rbConst";
            this.rbConst.Size = new System.Drawing.Size(99, 24);
            this.rbConst.TabIndex = 4;
            this.rbConst.TabStop = true;
            this.rbConst.Text = "Constant";
            this.rbConst.UseVisualStyleBackColor = true;
            this.rbConst.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbManual
            // 
            this.rbManual.AutoSize = true;
            this.rbManual.Location = new System.Drawing.Point(18, 218);
            this.rbManual.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbManual.Name = "rbManual";
            this.rbManual.Size = new System.Drawing.Size(86, 24);
            this.rbManual.TabIndex = 14;
            this.rbManual.TabStop = true;
            this.rbManual.Text = "Manual";
            this.rbManual.UseVisualStyleBackColor = true;
            // 
            // cbxProdRepair
            // 
            this.cbxProdRepair.AutoSize = true;
            this.cbxProdRepair.Location = new System.Drawing.Point(18, 298);
            this.cbxProdRepair.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbxProdRepair.Name = "cbxProdRepair";
            this.cbxProdRepair.Size = new System.Drawing.Size(164, 24);
            this.cbxProdRepair.TabIndex = 15;
            this.cbxProdRepair.Text = "Use Infrastructure";
            this.cbxProdRepair.UseVisualStyleBackColor = true;
            this.cbxProdRepair.CheckedChanged += new System.EventHandler(this.cbxProdRepair_CheckedChanged);
            // 
            // AutoRepairForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(261, 392);
            this.ControlBox = false;
            this.Controls.Add(this.cbxProdRepair);
            this.Controls.Add(this.rbManual);
            this.Controls.Add(this.txtDefault);
            this.Controls.Add(this.rbDefault);
            this.Controls.Add(this.txtCost);
            this.Controls.Add(this.rbPct);
            this.Controls.Add(this.rbCost);
            this.Controls.Add(this.rbConst);
            this.Controls.Add(this.txtPct);
            this.Controls.Add(this.rbTurn);
            this.Controls.Add(this.txtConst);
            this.Controls.Add(this.rbNone);
            this.Controls.Add(this.txtTurn);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "AutoRepairForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Shown += new System.EventHandler(this.AutoRepairForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbPct;
        private System.Windows.Forms.RadioButton rbTurn;
        private System.Windows.Forms.RadioButton rbConst;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rbNone;
        private System.Windows.Forms.TextBox txtPct;
        private System.Windows.Forms.TextBox txtConst;
        private System.Windows.Forms.TextBox txtTurn;
        private System.Windows.Forms.RadioButton rbDefault;
        private System.Windows.Forms.TextBox txtCost;
        private System.Windows.Forms.RadioButton rbCost;
        private System.Windows.Forms.TextBox txtDefault;
        private System.Windows.Forms.RadioButton rbManual;
        private System.Windows.Forms.CheckBox cbxProdRepair;
    }
}