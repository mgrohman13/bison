
namespace WinFormsApp1
{
    partial class Trade
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.pnlBurn = new System.Windows.Forms.Panel();
            this.lblBurn = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.nudBurn = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlFabricate = new System.Windows.Forms.Panel();
            this.lblFabricate = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.nudFabricate = new System.Windows.Forms.NumericUpDown();
            this.pnlScrap = new System.Windows.Forms.Panel();
            this.lblScrap = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.nudScrap = new System.Windows.Forms.NumericUpDown();
            this.pnlBurn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBurn)).BeginInit();
            this.pnlFabricate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFabricate)).BeginInit();
            this.pnlScrap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScrap)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(405, 150);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 34);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(287, 150);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 34);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // pnlBurn
            // 
            this.pnlBurn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBurn.Controls.Add(this.lblBurn);
            this.pnlBurn.Controls.Add(this.label1);
            this.pnlBurn.Controls.Add(this.label7);
            this.pnlBurn.Controls.Add(this.label6);
            this.pnlBurn.Controls.Add(this.nudBurn);
            this.pnlBurn.Location = new System.Drawing.Point(12, 86);
            this.pnlBurn.Name = "pnlBurn";
            this.pnlBurn.Size = new System.Drawing.Size(505, 31);
            this.pnlBurn.TabIndex = 15;
            // 
            // lblBurn
            // 
            this.lblBurn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBurn.Location = new System.Drawing.Point(328, 2);
            this.lblBurn.Name = "lblBurn";
            this.lblBurn.Size = new System.Drawing.Size(90, 29);
            this.lblBurn.TabIndex = 4;
            this.lblBurn.Text = "-9999999";
            this.lblBurn.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Burn Mass:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(232, 2);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 25);
            this.label7.TabIndex = 2;
            this.label7.Text = "Energy";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(424, 2);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 25);
            this.label6.TabIndex = 3;
            this.label6.Text = "Mass";
            // 
            // nudBurn
            // 
            this.nudBurn.Location = new System.Drawing.Point(140, 0);
            this.nudBurn.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudBurn.Name = "nudBurn";
            this.nudBurn.Size = new System.Drawing.Size(86, 31);
            this.nudBurn.TabIndex = 2;
            this.nudBurn.Value = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudBurn.ValueChanged += new System.EventHandler(this.NUD_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(232, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Mass";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(424, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 25);
            this.label3.TabIndex = 3;
            this.label3.Text = "Energy";
            // 
            // pnlFabricate
            // 
            this.pnlFabricate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlFabricate.Controls.Add(this.lblFabricate);
            this.pnlFabricate.Controls.Add(this.label2);
            this.pnlFabricate.Controls.Add(this.label3);
            this.pnlFabricate.Controls.Add(this.label5);
            this.pnlFabricate.Controls.Add(this.label8);
            this.pnlFabricate.Controls.Add(this.nudFabricate);
            this.pnlFabricate.Location = new System.Drawing.Point(12, 49);
            this.pnlFabricate.Name = "pnlFabricate";
            this.pnlFabricate.Size = new System.Drawing.Size(505, 31);
            this.pnlFabricate.TabIndex = 16;
            // 
            // lblFabricate
            // 
            this.lblFabricate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFabricate.Location = new System.Drawing.Point(328, 2);
            this.lblFabricate.Name = "lblFabricate";
            this.lblFabricate.Size = new System.Drawing.Size(90, 29);
            this.lblFabricate.TabIndex = 5;
            this.lblFabricate.Text = "-9999999";
            this.lblFabricate.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(-2982, 2);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 29);
            this.label5.TabIndex = 4;
            this.label5.Text = "-9999999";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(0, 2);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(132, 25);
            this.label8.TabIndex = 1;
            this.label8.Text = "Fabricate Mass:";
            // 
            // nudFabricate
            // 
            this.nudFabricate.Location = new System.Drawing.Point(140, 0);
            this.nudFabricate.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudFabricate.Name = "nudFabricate";
            this.nudFabricate.Size = new System.Drawing.Size(86, 31);
            this.nudFabricate.TabIndex = 3;
            this.nudFabricate.Value = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudFabricate.ValueChanged += new System.EventHandler(this.NUD_ValueChanged);
            // 
            // pnlScrap
            // 
            this.pnlScrap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlScrap.Controls.Add(this.lblScrap);
            this.pnlScrap.Controls.Add(this.label15);
            this.pnlScrap.Controls.Add(this.label10);
            this.pnlScrap.Controls.Add(this.label11);
            this.pnlScrap.Controls.Add(this.label12);
            this.pnlScrap.Controls.Add(this.label13);
            this.pnlScrap.Controls.Add(this.label14);
            this.pnlScrap.Controls.Add(this.nudScrap);
            this.pnlScrap.Location = new System.Drawing.Point(12, 12);
            this.pnlScrap.Name = "pnlScrap";
            this.pnlScrap.Size = new System.Drawing.Size(505, 31);
            this.pnlScrap.TabIndex = 17;
            // 
            // lblScrap
            // 
            this.lblScrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblScrap.Location = new System.Drawing.Point(328, 2);
            this.lblScrap.Name = "lblScrap";
            this.lblScrap.Size = new System.Drawing.Size(90, 29);
            this.lblScrap.TabIndex = 6;
            this.lblScrap.Text = "-9999999";
            this.lblScrap.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(232, 2);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(53, 25);
            this.label15.TabIndex = 6;
            this.label15.Text = "Mass";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.Location = new System.Drawing.Point(-2882, 2);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(90, 29);
            this.label10.TabIndex = 5;
            this.label10.Text = "-9999999";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.Location = new System.Drawing.Point(-2628, 2);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(90, 29);
            this.label11.TabIndex = 4;
            this.label11.Text = "-9999999";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(-2786, 2);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 25);
            this.label12.TabIndex = 3;
            this.label12.Text = "Mass";
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(424, 2);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(81, 25);
            this.label13.TabIndex = 2;
            this.label13.Text = "Research";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(0, 2);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(134, 25);
            this.label14.TabIndex = 1;
            this.label14.Text = "Scrap Research:";
            // 
            // nudScrap
            // 
            this.nudScrap.Location = new System.Drawing.Point(140, 0);
            this.nudScrap.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudScrap.Name = "nudScrap";
            this.nudScrap.Size = new System.Drawing.Size(86, 31);
            this.nudScrap.TabIndex = 1;
            this.nudScrap.Value = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudScrap.ValueChanged += new System.EventHandler(this.NUD_ValueChanged);
            // 
            // Trade
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(529, 196);
            this.Controls.Add(this.pnlScrap);
            this.Controls.Add(this.pnlFabricate);
            this.Controls.Add(this.pnlBurn);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Trade";
            this.Text = "Trade";
            this.pnlBurn.ResumeLayout(false);
            this.pnlBurn.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBurn)).EndInit();
            this.pnlFabricate.ResumeLayout(false);
            this.pnlFabricate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFabricate)).EndInit();
            this.pnlScrap.ResumeLayout(false);
            this.pnlScrap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScrap)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel pnlBurn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudBurn;
        private System.Windows.Forms.Label lblBurn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel pnlFabricate;
        private System.Windows.Forms.Label lblFabricate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nudFabricate;
        private System.Windows.Forms.Panel pnlScrap;
        private System.Windows.Forms.Label lblScrap;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown nudScrap;
    }
}