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
            lbxDesigns = new ListBox();
            lblProdLoss = new Label();
            btnCancel = new Button();
            btnOK = new Button();
            lblProd = new Label();
            btnBuy = new Button();
            chkObsolete = new CheckBox();
            rbStr = new RadioButton();
            rbValue = new RadioButton();
            rbTrans = new RadioButton();
            rbCustom = new RadioButton();
            chkPause = new CheckBox();
            sdForm = new BuildableControl();
            SuspendLayout();
            // 
            // lbxDesigns
            // 
            lbxDesigns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lbxDesigns.DrawMode = DrawMode.OwnerDrawVariable;
            lbxDesigns.FormattingEnabled = true;
            lbxDesigns.Location = new Point(0, 39);
            lbxDesigns.Name = "lbxDesigns";
            lbxDesigns.Size = new Size(130, 333);
            lbxDesigns.TabIndex = 2;
            lbxDesigns.DrawItem += lbxDesigns_DrawItem;
            lbxDesigns.SelectedIndexChanged += lbxDesigns_SelectedIndexChanged;
            lbxDesigns.DoubleClick += lbxDesigns_DoubleClick;
            // 
            // lblProdLoss
            // 
            lblProdLoss.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProdLoss.AutoEllipsis = true;
            lblProdLoss.Location = new Point(288, 23);
            lblProdLoss.Name = "lblProdLoss";
            lblProdLoss.Size = new Size(100, 23);
            lblProdLoss.TabIndex = 7;
            lblProdLoss.Text = "lblProdLoss";
            lblProdLoss.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(288, 349);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(188, 349);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(100, 23);
            btnOK.TabIndex = 0;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // lblProd
            // 
            lblProd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProd.AutoEllipsis = true;
            lblProd.Location = new Point(188, 23);
            lblProd.Name = "lblProd";
            lblProd.Size = new Size(100, 23);
            lblProd.TabIndex = 6;
            lblProd.Text = "lblProd";
            lblProd.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnBuy
            // 
            btnBuy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBuy.Location = new Point(188, 1);
            btnBuy.Name = "btnBuy";
            btnBuy.Size = new Size(200, 23);
            btnBuy.TabIndex = 3;
            btnBuy.Text = "Buy/Sell";
            btnBuy.UseVisualStyleBackColor = true;
            btnBuy.Click += btnBuy_Click;
            // 
            // chkObsolete
            // 
            chkObsolete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkObsolete.AutoSize = true;
            chkObsolete.CheckAlign = ContentAlignment.MiddleRight;
            chkObsolete.Checked = true;
            chkObsolete.CheckState = CheckState.Checked;
            chkObsolete.Location = new Point(303, 48);
            chkObsolete.Name = "chkObsolete";
            chkObsolete.Size = new Size(73, 19);
            chkObsolete.TabIndex = 5;
            chkObsolete.Text = "Obsolete";
            chkObsolete.UseVisualStyleBackColor = true;
            chkObsolete.CheckedChanged += chkObsolete_CheckedChanged;
            // 
            // rbStr
            // 
            rbStr.AutoSize = true;
            rbStr.Location = new Point(61, 1);
            rbStr.Name = "rbStr";
            rbStr.Size = new Size(70, 19);
            rbStr.TabIndex = 11;
            rbStr.TabStop = true;
            rbStr.Text = "Strength";
            rbStr.UseVisualStyleBackColor = true;
            rbStr.CheckedChanged += rb_CheckedChanged;
            // 
            // rbValue
            // 
            rbValue.AutoSize = true;
            rbValue.Location = new Point(3, 1);
            rbValue.Name = "rbValue";
            rbValue.Size = new Size(53, 19);
            rbValue.TabIndex = 12;
            rbValue.TabStop = true;
            rbValue.Text = "Value";
            rbValue.UseVisualStyleBackColor = true;
            rbValue.CheckedChanged += rb_CheckedChanged;
            // 
            // rbTrans
            // 
            rbTrans.AutoSize = true;
            rbTrans.Location = new Point(3, 20);
            rbTrans.Name = "rbTrans";
            rbTrans.Size = new Size(75, 19);
            rbTrans.TabIndex = 13;
            rbTrans.TabStop = true;
            rbTrans.Text = "Transport";
            rbTrans.UseVisualStyleBackColor = true;
            rbTrans.CheckedChanged += rb_CheckedChanged;
            // 
            // rbCustom
            // 
            rbCustom.AutoSize = true;
            rbCustom.Location = new Point(75, 20);
            rbCustom.Name = "rbCustom";
            rbCustom.Size = new Size(55, 19);
            rbCustom.TabIndex = 14;
            rbCustom.TabStop = true;
            rbCustom.Text = "Other";
            rbCustom.UseVisualStyleBackColor = true;
            rbCustom.CheckedChanged += rb_CheckedChanged;
            rbCustom.Click += rbCustom_Click;
            // 
            // chkPause
            // 
            chkPause.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkPause.AutoSize = true;
            chkPause.Checked = true;
            chkPause.CheckState = CheckState.Checked;
            chkPause.Location = new Point(193, 48);
            chkPause.Name = "chkPause";
            chkPause.Size = new Size(57, 19);
            chkPause.TabIndex = 15;
            chkPause.Text = "Pause";
            chkPause.UseVisualStyleBackColor = true;
            chkPause.CheckedChanged += chkPause_CheckedChanged;
            // 
            // sdForm
            // 
            sdForm.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            sdForm.AutoSize = true;
            sdForm.Location = new Point(137, 75);
            sdForm.Margin = new Padding(4, 5, 4, 5);
            sdForm.Name = "sdForm";
            sdForm.Size = new Size(238, 266);
            sdForm.TabIndex = 8;
            // 
            // ProductionForm
            // 
            AcceptButton = btnOK;
            AutoScaleMode = AutoScaleMode.None;
            CancelButton = btnCancel;
            ClientSize = new Size(388, 372);
            Controls.Add(chkPause);
            Controls.Add(rbCustom);
            Controls.Add(rbTrans);
            Controls.Add(rbValue);
            Controls.Add(rbStr);
            Controls.Add(chkObsolete);
            Controls.Add(btnBuy);
            Controls.Add(lblProd);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(sdForm);
            Controls.Add(lblProdLoss);
            Controls.Add(lbxDesigns);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "ProductionForm";
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
            PerformLayout();

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