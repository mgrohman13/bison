namespace GalWarWin
{
    partial class ProductionForm
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
            this.lbxDesigns = new System.Windows.Forms.ListBox();
            this.lblProdLoss = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblProd = new System.Windows.Forms.Label();
            this.btnBuy = new System.Windows.Forms.Button();
            this.chkObsolete = new System.Windows.Forms.CheckBox();
            this.rbStr = new System.Windows.Forms.RadioButton();
            this.rbValue = new System.Windows.Forms.RadioButton();
            this.rbTrans = new System.Windows.Forms.RadioButton();
            this.rbCustom = new System.Windows.Forms.RadioButton();
            this.chkPause = new System.Windows.Forms.CheckBox();
            this.sdForm = new GalWarWin.BuildableControl();
            this.SuspendLayout();
            // 
            // lbxDesigns
            // 
            this.lbxDesigns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxDesigns.FormattingEnabled = true;
            this.lbxDesigns.Location = new System.Drawing.Point(0, 39);
            this.lbxDesigns.Name = "lbxDesigns";
            this.lbxDesigns.Size = new System.Drawing.Size(130, 277);
            this.lbxDesigns.TabIndex = 2;
            this.lbxDesigns.SelectedIndexChanged += new System.EventHandler(this.lbxDesigns_SelectedIndexChanged);
            this.lbxDesigns.DoubleClick += new System.EventHandler(this.lbxDesigns_DoubleClick);
            // 
            // lblProdLoss
            // 
            this.lblProdLoss.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProdLoss.AutoEllipsis = true;
            this.lblProdLoss.Location = new System.Drawing.Point(230, 23);
            this.lblProdLoss.Name = "lblProdLoss";
            this.lblProdLoss.Size = new System.Drawing.Size(100, 23);
            this.lblProdLoss.TabIndex = 7;
            this.lblProdLoss.Text = "lblProdLoss";
            this.lblProdLoss.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(230, 293);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(130, 293);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // lblProd
            // 
            this.lblProd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProd.AutoEllipsis = true;
            this.lblProd.Location = new System.Drawing.Point(130, 23);
            this.lblProd.Name = "lblProd";
            this.lblProd.Size = new System.Drawing.Size(100, 23);
            this.lblProd.TabIndex = 6;
            this.lblProd.Text = "lblProd";
            this.lblProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBuy
            // 
            this.btnBuy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuy.Location = new System.Drawing.Point(130, 1);
            this.btnBuy.Name = "btnBuy";
            this.btnBuy.Size = new System.Drawing.Size(200, 23);
            this.btnBuy.TabIndex = 3;
            this.btnBuy.Text = "Buy/Sell";
            this.btnBuy.UseVisualStyleBackColor = true;
            this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
            // 
            // chkObsolete
            // 
            this.chkObsolete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkObsolete.AutoSize = true;
            this.chkObsolete.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkObsolete.Checked = true;
            this.chkObsolete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkObsolete.Location = new System.Drawing.Point(250, 48);
            this.chkObsolete.Name = "chkObsolete";
            this.chkObsolete.Size = new System.Drawing.Size(68, 17);
            this.chkObsolete.TabIndex = 5;
            this.chkObsolete.Text = "Obsolete";
            this.chkObsolete.UseVisualStyleBackColor = true;
            this.chkObsolete.CheckedChanged += new System.EventHandler(this.chkObsolete_CheckedChanged);
            // 
            // rbStr
            // 
            this.rbStr.AutoSize = true;
            this.rbStr.Location = new System.Drawing.Point(61, 1);
            this.rbStr.Name = "rbStr";
            this.rbStr.Size = new System.Drawing.Size(65, 17);
            this.rbStr.TabIndex = 11;
            this.rbStr.TabStop = true;
            this.rbStr.Text = "Strength";
            this.rbStr.UseVisualStyleBackColor = true;
            this.rbStr.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbValue
            // 
            this.rbValue.AutoSize = true;
            this.rbValue.Location = new System.Drawing.Point(3, 1);
            this.rbValue.Name = "rbValue";
            this.rbValue.Size = new System.Drawing.Size(52, 17);
            this.rbValue.TabIndex = 12;
            this.rbValue.TabStop = true;
            this.rbValue.Text = "Value";
            this.rbValue.UseVisualStyleBackColor = true;
            this.rbValue.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbTrans
            // 
            this.rbTrans.AutoSize = true;
            this.rbTrans.Location = new System.Drawing.Point(3, 20);
            this.rbTrans.Name = "rbTrans";
            this.rbTrans.Size = new System.Drawing.Size(70, 17);
            this.rbTrans.TabIndex = 13;
            this.rbTrans.TabStop = true;
            this.rbTrans.Text = "Transport";
            this.rbTrans.UseVisualStyleBackColor = true;
            this.rbTrans.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbCustom
            // 
            this.rbCustom.AutoSize = true;
            this.rbCustom.Location = new System.Drawing.Point(75, 20);
            this.rbCustom.Name = "rbCustom";
            this.rbCustom.Size = new System.Drawing.Size(51, 17);
            this.rbCustom.TabIndex = 14;
            this.rbCustom.TabStop = true;
            this.rbCustom.Text = "Other";
            this.rbCustom.UseVisualStyleBackColor = true;
            this.rbCustom.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            this.rbCustom.Click += new System.EventHandler(this.rbCustom_Click);
            // 
            // chkPause
            // 
            this.chkPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPause.AutoSize = true;
            this.chkPause.Checked = true;
            this.chkPause.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPause.Location = new System.Drawing.Point(136, 48);
            this.chkPause.Name = "chkPause";
            this.chkPause.Size = new System.Drawing.Size(56, 17);
            this.chkPause.TabIndex = 15;
            this.chkPause.Text = "Pause";
            this.chkPause.UseVisualStyleBackColor = true;
            // 
            // sdForm
            // 
            this.sdForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sdForm.Location = new System.Drawing.Point(130, 62);
            this.sdForm.Name = "sdForm";
            this.sdForm.Size = new System.Drawing.Size(200, 230);
            this.sdForm.TabIndex = 8;
            // 
            // ProductionForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(330, 316);
            this.Controls.Add(this.chkPause);
            this.Controls.Add(this.rbCustom);
            this.Controls.Add(this.rbTrans);
            this.Controls.Add(this.rbValue);
            this.Controls.Add(this.rbStr);
            this.Controls.Add(this.chkObsolete);
            this.Controls.Add(this.btnBuy);
            this.Controls.Add(this.lblProd);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.sdForm);
            this.Controls.Add(this.lblProdLoss);
            this.Controls.Add(this.lbxDesigns);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ProductionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbxDesigns;
        private System.Windows.Forms.Label lblProdLoss;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblProd;
        public BuildableControl sdForm;
        private System.Windows.Forms.Button btnBuy;
        private System.Windows.Forms.CheckBox chkObsolete;
        private System.Windows.Forms.RadioButton rbStr;
        private System.Windows.Forms.RadioButton rbValue;
        private System.Windows.Forms.RadioButton rbTrans;
        private System.Windows.Forms.RadioButton rbCustom;
        private System.Windows.Forms.CheckBox chkPause;
    }
}