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
            this.btnSell = new System.Windows.Forms.Button();
            this.chkObsolete = new System.Windows.Forms.CheckBox();
            this.sdForm = new GalWarWin.BuildableControl();
            this.SuspendLayout();
            // 
            // lbxDesigns
            // 
            this.lbxDesigns.Dock = System.Windows.Forms.DockStyle.Left;
            this.lbxDesigns.FormattingEnabled = true;
            this.lbxDesigns.Location = new System.Drawing.Point(0, 0);
            this.lbxDesigns.Name = "lbxDesigns";
            this.lbxDesigns.Size = new System.Drawing.Size(130, 300);
            this.lbxDesigns.TabIndex = 2;
            this.lbxDesigns.SelectedIndexChanged += new System.EventHandler(this.lbxDesigns_SelectedIndexChanged);
            this.lbxDesigns.DoubleClick += new System.EventHandler(this.lbxDesigns_DoubleClick);
            // 
            // lblProdLoss
            // 
            this.lblProdLoss.Location = new System.Drawing.Point(230, 23);
            this.lblProdLoss.Name = "lblProdLoss";
            this.lblProdLoss.Size = new System.Drawing.Size(100, 23);
            this.lblProdLoss.TabIndex = 7;
            this.lblProdLoss.Text = "lblProdLoss";
            this.lblProdLoss.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(230, 277);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(130, 277);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // lblProd
            // 
            this.lblProd.Location = new System.Drawing.Point(130, 23);
            this.lblProd.Name = "lblProd";
            this.lblProd.Size = new System.Drawing.Size(100, 23);
            this.lblProd.TabIndex = 6;
            this.lblProd.Text = "lblProd";
            this.lblProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBuy
            // 
            this.btnBuy.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnBuy.Location = new System.Drawing.Point(130, 0);
            this.btnBuy.Name = "btnBuy";
            this.btnBuy.Size = new System.Drawing.Size(100, 23);
            this.btnBuy.TabIndex = 3;
            this.btnBuy.Text = "Buy";
            this.btnBuy.UseVisualStyleBackColor = true;
            this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
            // 
            // btnSell
            // 
            this.btnSell.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnSell.Location = new System.Drawing.Point(230, 0);
            this.btnSell.Name = "btnSell";
            this.btnSell.Size = new System.Drawing.Size(100, 23);
            this.btnSell.TabIndex = 4;
            this.btnSell.Text = "Sell";
            this.btnSell.UseVisualStyleBackColor = true;
            this.btnSell.Click += new System.EventHandler(this.btnSell_Click);
            // 
            // chkObsolete
            // 
            this.chkObsolete.AutoSize = true;
            this.chkObsolete.Checked = true;
            this.chkObsolete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkObsolete.Location = new System.Drawing.Point(303, 49);
            this.chkObsolete.Name = "chkObsolete";
            this.chkObsolete.Size = new System.Drawing.Size(15, 14);
            this.chkObsolete.TabIndex = 5;
            this.chkObsolete.UseVisualStyleBackColor = true;
            this.chkObsolete.CheckedChanged += new System.EventHandler(this.chkObsolete_CheckedChanged);
            // 
            // sdForm
            // 
            this.sdForm.Location = new System.Drawing.Point(130, 46);
            this.sdForm.Name = "sdForm";
            this.sdForm.Size = new System.Drawing.Size(200, 230);
            this.sdForm.TabIndex = 8;
            // 
            // ProductionForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(330, 300);
            this.Controls.Add(this.chkObsolete);
            this.Controls.Add(this.btnBuy);
            this.Controls.Add(this.btnSell);
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
        private System.Windows.Forms.Button btnSell;
        private System.Windows.Forms.CheckBox chkObsolete;
    }
}