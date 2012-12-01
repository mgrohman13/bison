namespace GalWarWin
{
    partial class CostCalculatorForm
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
            this.nudProd = new System.Windows.Forms.NumericUpDown();
            this.nudResearch = new System.Windows.Forms.NumericUpDown();
            this.nudAtt = new System.Windows.Forms.NumericUpDown();
            this.nudDef = new System.Windows.Forms.NumericUpDown();
            this.nudHP = new System.Windows.Forms.NumericUpDown();
            this.nudSpeed = new System.Windows.Forms.NumericUpDown();
            this.nudUpk = new System.Windows.Forms.NumericUpDown();
            this.nudTrans = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.nudDS = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cbCol = new System.Windows.Forms.CheckBox();
            this.btnDone = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.txtStr = new System.Windows.Forms.TextBox();
            this.txtCost = new System.Windows.Forms.TextBox();
            this.lblOverflow = new System.Windows.Forms.Label();
            this.cbDS = new System.Windows.Forms.CheckBox();
            ( (System.ComponentModel.ISupportInitialize)( this.nudProd ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudResearch ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAtt ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDef ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudHP ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudSpeed ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudUpk ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudTrans ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDS ) ).BeginInit();
            this.SuspendLayout();
            // 
            // nudProd
            // 
            this.nudProd.DecimalPlaces = 1;
            this.nudProd.Location = new System.Drawing.Point(109, 12);
            this.nudProd.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudProd.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudProd.Name = "nudProd";
            this.nudProd.Size = new System.Drawing.Size(100, 20);
            this.nudProd.TabIndex = 1;
            this.nudProd.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudProd.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudResearch
            // 
            this.nudResearch.Location = new System.Drawing.Point(109, 38);
            this.nudResearch.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudResearch.Name = "nudResearch";
            this.nudResearch.Size = new System.Drawing.Size(100, 20);
            this.nudResearch.TabIndex = 3;
            this.nudResearch.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudAtt
            // 
            this.nudAtt.Location = new System.Drawing.Point(109, 64);
            this.nudAtt.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudAtt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAtt.Name = "nudAtt";
            this.nudAtt.Size = new System.Drawing.Size(100, 20);
            this.nudAtt.TabIndex = 5;
            this.nudAtt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAtt.ValueChanged += new System.EventHandler(this.nudAtt_ValueChanged);
            // 
            // nudDef
            // 
            this.nudDef.Location = new System.Drawing.Point(109, 90);
            this.nudDef.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudDef.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDef.Name = "nudDef";
            this.nudDef.Size = new System.Drawing.Size(100, 20);
            this.nudDef.TabIndex = 7;
            this.nudDef.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDef.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudHP
            // 
            this.nudHP.Location = new System.Drawing.Point(109, 116);
            this.nudHP.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudHP.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHP.Name = "nudHP";
            this.nudHP.Size = new System.Drawing.Size(100, 20);
            this.nudHP.TabIndex = 9;
            this.nudHP.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHP.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudSpeed
            // 
            this.nudSpeed.Location = new System.Drawing.Point(109, 142);
            this.nudSpeed.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSpeed.Name = "nudSpeed";
            this.nudSpeed.Size = new System.Drawing.Size(100, 20);
            this.nudSpeed.TabIndex = 11;
            this.nudSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSpeed.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudUpk
            // 
            this.nudUpk.Location = new System.Drawing.Point(109, 168);
            this.nudUpk.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudUpk.Name = "nudUpk";
            this.nudUpk.Size = new System.Drawing.Size(100, 20);
            this.nudUpk.TabIndex = 13;
            this.nudUpk.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudUpk.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudTrans
            // 
            this.nudTrans.Location = new System.Drawing.Point(109, 194);
            this.nudTrans.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudTrans.Name = "nudTrans";
            this.nudTrans.Size = new System.Drawing.Size(100, 20);
            this.nudTrans.TabIndex = 15;
            this.nudTrans.ValueChanged += new System.EventHandler(this.nudTrans_ValueChanged);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(25, 170);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 13);
            this.label10.TabIndex = 51;
            this.label10.Text = "Upkeep";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(18, 196);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 13);
            this.label11.TabIndex = 48;
            this.label11.Text = "Transport";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(32, 144);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 13);
            this.label12.TabIndex = 46;
            this.label12.Text = "Speed";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(48, 118);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(55, 13);
            this.label13.TabIndex = 44;
            this.label13.Text = "HP";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(23, 92);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(80, 13);
            this.label14.TabIndex = 42;
            this.label14.Text = "Defense";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(32, 66);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(71, 13);
            this.label15.TabIndex = 40;
            this.label15.Text = "Attack";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(12, 14);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(91, 13);
            this.label16.TabIndex = 38;
            this.label16.Text = "Production";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(17, 40);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(86, 13);
            this.label9.TabIndex = 53;
            this.label9.Text = "Research";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(18, 222);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 55;
            this.label1.Text = "Colony";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudDS
            // 
            this.nudDS.DecimalPlaces = 1;
            this.nudDS.Location = new System.Drawing.Point(130, 246);
            this.nudDS.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudDS.Name = "nudDS";
            this.nudDS.Size = new System.Drawing.Size(79, 20);
            this.nudDS.TabIndex = 54;
            this.nudDS.ValueChanged += new System.EventHandler(this.nudDS_ValueChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(18, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 57;
            this.label2.Text = "Death Star";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbCol
            // 
            this.cbCol.AutoSize = true;
            this.cbCol.Location = new System.Drawing.Point(109, 222);
            this.cbCol.Name = "cbCol";
            this.cbCol.Size = new System.Drawing.Size(15, 14);
            this.cbCol.TabIndex = 58;
            this.cbCol.UseVisualStyleBackColor = true;
            this.cbCol.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // btnDone
            // 
            this.btnDone.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnDone.Location = new System.Drawing.Point(134, 350);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(75, 23);
            this.btnDone.TabIndex = 59;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(18, 326);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 61;
            this.label3.Text = "Value";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(18, 300);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 63;
            this.label4.Text = "Strength";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(18, 274);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 65;
            this.label5.Text = "Total Cost";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(109, 323);
            this.txtValue.Name = "txtValue";
            this.txtValue.ReadOnly = true;
            this.txtValue.Size = new System.Drawing.Size(100, 20);
            this.txtValue.TabIndex = 66;
            // 
            // txtStr
            // 
            this.txtStr.Location = new System.Drawing.Point(109, 297);
            this.txtStr.Name = "txtStr";
            this.txtStr.ReadOnly = true;
            this.txtStr.Size = new System.Drawing.Size(100, 20);
            this.txtStr.TabIndex = 67;
            // 
            // txtCost
            // 
            this.txtCost.Location = new System.Drawing.Point(109, 271);
            this.txtCost.Name = "txtCost";
            this.txtCost.ReadOnly = true;
            this.txtCost.Size = new System.Drawing.Size(100, 20);
            this.txtCost.TabIndex = 68;
            // 
            // lblOverflow
            // 
            this.lblOverflow.ForeColor = System.Drawing.Color.Red;
            this.lblOverflow.Location = new System.Drawing.Point(12, 350);
            this.lblOverflow.Name = "lblOverflow";
            this.lblOverflow.Size = new System.Drawing.Size(116, 23);
            this.lblOverflow.TabIndex = 69;
            this.lblOverflow.Text = "Overflow";
            this.lblOverflow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbDS
            // 
            this.cbDS.AutoSize = true;
            this.cbDS.Location = new System.Drawing.Point(109, 248);
            this.cbDS.Name = "cbDS";
            this.cbDS.Size = new System.Drawing.Size(15, 14);
            this.cbDS.TabIndex = 70;
            this.cbDS.UseVisualStyleBackColor = true;
            this.cbDS.CheckedChanged += new System.EventHandler(this.cbDS_CheckedChanged);
            // 
            // CostCalculatorForm
            // 
            this.AcceptButton = this.btnDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnDone;
            this.ClientSize = new System.Drawing.Size(221, 385);
            this.Controls.Add(this.cbDS);
            this.Controls.Add(this.lblOverflow);
            this.Controls.Add(this.txtCost);
            this.Controls.Add(this.txtStr);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.cbCol);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudDS);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.nudTrans);
            this.Controls.Add(this.nudUpk);
            this.Controls.Add(this.nudSpeed);
            this.Controls.Add(this.nudHP);
            this.Controls.Add(this.nudDef);
            this.Controls.Add(this.nudAtt);
            this.Controls.Add(this.nudResearch);
            this.Controls.Add(this.nudProd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CostCalculatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            ( (System.ComponentModel.ISupportInitialize)( this.nudProd ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudResearch ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAtt ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDef ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudHP ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudSpeed ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudUpk ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudTrans ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDS ) ).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudProd;
        private System.Windows.Forms.NumericUpDown nudResearch;
        private System.Windows.Forms.NumericUpDown nudAtt;
        private System.Windows.Forms.NumericUpDown nudDef;
        private System.Windows.Forms.NumericUpDown nudHP;
        private System.Windows.Forms.NumericUpDown nudSpeed;
        private System.Windows.Forms.NumericUpDown nudUpk;
        private System.Windows.Forms.NumericUpDown nudTrans;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudDS;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbCol;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.TextBox txtStr;
        private System.Windows.Forms.TextBox txtCost;
        private System.Windows.Forms.Label lblOverflow;
        private System.Windows.Forms.CheckBox cbDS;
    }
}