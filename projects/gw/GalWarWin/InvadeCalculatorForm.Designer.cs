namespace GalWarWin
{
    partial class InvadeCalculatorForm
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
            this.nudPop = new System.Windows.Forms.NumericUpDown();
            this.nudDefSoldiers = new System.Windows.Forms.NumericUpDown();
            this.nudAttSoldiers = new System.Windows.Forms.NumericUpDown();
            this.nudTroops = new System.Windows.Forms.NumericUpDown();
            this.lblDefPlayer = new System.Windows.Forms.Label();
            this.lblAttPlayer = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ( (System.ComponentModel.ISupportInitialize)( this.nudPop ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDefSoldiers ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAttSoldiers ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudTroops ) ).BeginInit();
            this.SuspendLayout();
            // 
            // nudPop
            // 
            this.nudPop.Location = new System.Drawing.Point(136, 35);
            this.nudPop.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudPop.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPop.Name = "nudPop";
            this.nudPop.Size = new System.Drawing.Size(97, 20);
            this.nudPop.TabIndex = 15;
            this.nudPop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudPop.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudDefSoldiers
            // 
            this.nudDefSoldiers.DecimalPlaces = 1;
            this.nudDefSoldiers.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudDefSoldiers.Location = new System.Drawing.Point(136, 61);
            this.nudDefSoldiers.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            65536});
            this.nudDefSoldiers.Name = "nudDefSoldiers";
            this.nudDefSoldiers.Size = new System.Drawing.Size(97, 20);
            this.nudDefSoldiers.TabIndex = 17;
            this.nudDefSoldiers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // nudAttSoldiers
            // 
            this.nudAttSoldiers.DecimalPlaces = 1;
            this.nudAttSoldiers.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudAttSoldiers.Location = new System.Drawing.Point(12, 61);
            this.nudAttSoldiers.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            65536});
            this.nudAttSoldiers.Name = "nudAttSoldiers";
            this.nudAttSoldiers.Size = new System.Drawing.Size(97, 20);
            this.nudAttSoldiers.TabIndex = 16;
            this.nudAttSoldiers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // nudTroops
            // 
            this.nudTroops.Location = new System.Drawing.Point(12, 35);
            this.nudTroops.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudTroops.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTroops.Name = "nudTroops";
            this.nudTroops.Size = new System.Drawing.Size(97, 20);
            this.nudTroops.TabIndex = 13;
            this.nudTroops.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudTroops.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblDefPlayer
            // 
            this.lblDefPlayer.Location = new System.Drawing.Point(133, 9);
            this.lblDefPlayer.Name = "lblDefPlayer";
            this.lblDefPlayer.Size = new System.Drawing.Size(100, 23);
            this.lblDefPlayer.TabIndex = 21;
            this.lblDefPlayer.Text = "Planet";
            this.lblDefPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAttPlayer
            // 
            this.lblAttPlayer.Location = new System.Drawing.Point(9, 9);
            this.lblAttPlayer.Name = "lblAttPlayer";
            this.lblAttPlayer.Size = new System.Drawing.Size(100, 23);
            this.lblAttPlayer.TabIndex = 20;
            this.lblAttPlayer.Text = "Invader";
            this.lblAttPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(115, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "%";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(239, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "%";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(179, 87);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 24;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(179, 87);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 24;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // InvadeCalculatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 122);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudPop);
            this.Controls.Add(this.nudDefSoldiers);
            this.Controls.Add(this.nudAttSoldiers);
            this.Controls.Add(this.nudTroops);
            this.Controls.Add(this.lblDefPlayer);
            this.Controls.Add(this.lblAttPlayer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "InvadeCalculatorForm";
            this.Text = "InvadeCalculatorForm";
            ( (System.ComponentModel.ISupportInitialize)( this.nudPop ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDefSoldiers ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAttSoldiers ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudTroops ) ).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudPop;
        private System.Windows.Forms.NumericUpDown nudDefSoldiers;
        private System.Windows.Forms.NumericUpDown nudAttSoldiers;
        private System.Windows.Forms.NumericUpDown nudTroops;
        private System.Windows.Forms.Label lblDefPlayer;
        private System.Windows.Forms.Label lblAttPlayer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnTest;
    }
}