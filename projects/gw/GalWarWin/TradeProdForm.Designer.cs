namespace GalWarWin
{
    partial class TradeProdForm
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
            this.lblName = new System.Windows.Forms.Label();
            this.nudProd = new System.Windows.Forms.NumericUpDown();
            this.lblCost = new System.Windows.Forms.Label();
            this.nudShips = new System.Windows.Forms.NumericUpDown();
            this.lblInf = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDiff = new System.Windows.Forms.Label();
            this.lblGoldDiff = new System.Windows.Forms.Label();
            this.lblCurGold = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudProd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShips)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(165, 61);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(271, 61);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblName
            // 
            this.lblName.AutoEllipsis = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(12, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(126, 23);
            this.lblName.TabIndex = 13;
            this.lblName.Text = "lblName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudProd
            // 
            this.nudProd.Location = new System.Drawing.Point(144, 12);
            this.nudProd.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudProd.Name = "nudProd";
            this.nudProd.Size = new System.Drawing.Size(47, 20);
            this.nudProd.TabIndex = 14;
            this.nudProd.Value = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            // 
            // lblCost
            // 
            this.lblCost.AutoEllipsis = true;
            this.lblCost.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCost.Location = new System.Drawing.Point(197, 9);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(45, 23);
            this.lblCost.TabIndex = 15;
            this.lblCost.Text = "/ 9999";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudShips
            // 
            this.nudShips.Location = new System.Drawing.Point(248, 12);
            this.nudShips.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudShips.Name = "nudShips";
            this.nudShips.Size = new System.Drawing.Size(35, 20);
            this.nudShips.TabIndex = 16;
            this.nudShips.Value = new decimal(new int[] {
            99,
            0,
            0,
            0});
            // 
            // lblInf
            // 
            this.lblInf.AutoEllipsis = true;
            this.lblInf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInf.Location = new System.Drawing.Point(197, 9);
            this.lblInf.Name = "lblInf";
            this.lblInf.Size = new System.Drawing.Size(126, 23);
            this.lblInf.TabIndex = 17;
            this.lblInf.Text = "lblInf";
            this.lblInf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Gold:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Click += new System.EventHandler(this.lblGoldDiff_Click);
            // 
            // lblDiff
            // 
            this.lblDiff.AutoEllipsis = true;
            this.lblDiff.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiff.Location = new System.Drawing.Point(329, 9);
            this.lblDiff.Name = "lblDiff";
            this.lblDiff.Size = new System.Drawing.Size(42, 23);
            this.lblDiff.TabIndex = 19;
            this.lblDiff.Text = "+9999";
            this.lblDiff.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGoldDiff
            // 
            this.lblGoldDiff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGoldDiff.AutoEllipsis = true;
            this.lblGoldDiff.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGoldDiff.Location = new System.Drawing.Point(55, 35);
            this.lblGoldDiff.Name = "lblGoldDiff";
            this.lblGoldDiff.Size = new System.Drawing.Size(46, 23);
            this.lblGoldDiff.TabIndex = 20;
            this.lblGoldDiff.Text = "+999.9";
            this.lblGoldDiff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblGoldDiff.Click += new System.EventHandler(this.lblGoldDiff_Click);
            // 
            // lblCurGold
            // 
            this.lblCurGold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCurGold.AutoEllipsis = true;
            this.lblCurGold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurGold.Location = new System.Drawing.Point(107, 35);
            this.lblCurGold.Name = "lblCurGold";
            this.lblCurGold.Size = new System.Drawing.Size(56, 23);
            this.lblCurGold.TabIndex = 21;
            this.lblCurGold.Text = "/ 9999.9";
            this.lblCurGold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCurGold.Click += new System.EventHandler(this.lblCurGold_Click);
            // 
            // TradeProdForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(383, 96);
            this.Controls.Add(this.lblCurGold);
            this.Controls.Add(this.lblGoldDiff);
            this.Controls.Add(this.lblDiff);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudShips);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.nudProd);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblInf);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "TradeProdForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            ((System.ComponentModel.ISupportInitialize)(this.nudProd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShips)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.NumericUpDown nudProd;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.NumericUpDown nudShips;
        private System.Windows.Forms.Label lblInf;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDiff;
        private System.Windows.Forms.Label lblGoldDiff;
        private System.Windows.Forms.Label lblCurGold;
    }
}