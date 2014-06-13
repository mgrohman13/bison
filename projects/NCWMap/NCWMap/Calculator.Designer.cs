namespace NCWMap
{
    partial class Calculator
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
            this.panelAtt = new System.Windows.Forms.Panel();
            this.nudAttNum = new System.Windows.Forms.NumericUpDown();
            this.nudAttHP = new System.Windows.Forms.NumericUpDown();
            this.cbAtt = new System.Windows.Forms.ComboBox();
            this.cbDef = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnNewMap = new System.Windows.Forms.Button();
            this.btnCalc = new System.Windows.Forms.Button();
            this.panelResult = new System.Windows.Forms.Panel();
            this.txtDefKill = new System.Windows.Forms.TextBox();
            this.txtAttKill = new System.Windows.Forms.TextBox();
            this.txtDefDmg = new System.Windows.Forms.TextBox();
            this.txtAttDmg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.nudDefHP = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.panelAtt.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAttNum ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAttHP ) ).BeginInit();
            this.panelResult.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDefHP ) ).BeginInit();
            this.SuspendLayout();
            // 
            // panelAtt
            // 
            this.panelAtt.Controls.Add(this.nudAttNum);
            this.panelAtt.Controls.Add(this.nudAttHP);
            this.panelAtt.Controls.Add(this.cbAtt);
            this.panelAtt.Location = new System.Drawing.Point(12, 35);
            this.panelAtt.Name = "panelAtt";
            this.panelAtt.Size = new System.Drawing.Size(135, 26);
            this.panelAtt.TabIndex = 0;
            // 
            // nudAttNum
            // 
            this.nudAttNum.Location = new System.Drawing.Point(93, 4);
            this.nudAttNum.Maximum = new decimal(new int[] {
            13,
            0,
            0,
            0});
            this.nudAttNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttNum.Name = "nudAttNum";
            this.nudAttNum.Size = new System.Drawing.Size(39, 20);
            this.nudAttNum.TabIndex = 3;
            this.nudAttNum.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttNum.Visible = false;
            this.nudAttNum.ValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // nudAttHP
            // 
            this.nudAttHP.Location = new System.Drawing.Point(48, 4);
            this.nudAttHP.Maximum = new decimal(new int[] {
            36,
            0,
            0,
            0});
            this.nudAttHP.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttHP.Name = "nudAttHP";
            this.nudAttHP.Size = new System.Drawing.Size(39, 20);
            this.nudAttHP.TabIndex = 2;
            this.nudAttHP.Value = new decimal(new int[] {
            36,
            0,
            0,
            0});
            this.nudAttHP.ValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // cbAtt
            // 
            this.cbAtt.FormattingEnabled = true;
            this.cbAtt.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cbAtt.Location = new System.Drawing.Point(3, 3);
            this.cbAtt.Name = "cbAtt";
            this.cbAtt.Size = new System.Drawing.Size(39, 21);
            this.cbAtt.TabIndex = 0;
            this.cbAtt.Text = "4";
            this.cbAtt.SelectedValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // cbDef
            // 
            this.cbDef.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.cbDef.FormattingEnabled = true;
            this.cbDef.Items.AddRange(new object[] {
            ".5",
            "1",
            "1.5",
            "2",
            "3",
            "4"});
            this.cbDef.Location = new System.Drawing.Point(188, 38);
            this.cbDef.Name = "cbDef";
            this.cbDef.Size = new System.Drawing.Size(39, 21);
            this.cbDef.TabIndex = 1;
            this.cbDef.Text = "2";
            this.cbDef.SelectedValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Att";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.label2.Location = new System.Drawing.Point(185, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Def";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(57, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "HP";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(102, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 23);
            this.label4.TabIndex = 7;
            this.label4.Text = "Num";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(15, 65);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(84, 23);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Text = "Add Attack";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Visible = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnNewMap
            // 
            this.btnNewMap.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnNewMap.Location = new System.Drawing.Point(197, 227);
            this.btnNewMap.Name = "btnNewMap";
            this.btnNewMap.Size = new System.Drawing.Size(75, 23);
            this.btnNewMap.TabIndex = 9;
            this.btnNewMap.Text = "New Map";
            this.btnNewMap.UseVisualStyleBackColor = true;
            this.btnNewMap.Click += new System.EventHandler(this.btnNewMap_Click);
            // 
            // btnCalc
            // 
            this.btnCalc.Location = new System.Drawing.Point(188, 65);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(84, 23);
            this.btnCalc.TabIndex = 10;
            this.btnCalc.Text = "Calculate";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // panelResult
            // 
            this.panelResult.Controls.Add(this.txtDefKill);
            this.panelResult.Controls.Add(this.txtAttKill);
            this.panelResult.Controls.Add(this.txtDefDmg);
            this.panelResult.Controls.Add(this.txtAttDmg);
            this.panelResult.Controls.Add(this.label8);
            this.panelResult.Controls.Add(this.label7);
            this.panelResult.Controls.Add(this.label6);
            this.panelResult.Controls.Add(this.label5);
            this.panelResult.Location = new System.Drawing.Point(70, 94);
            this.panelResult.Name = "panelResult";
            this.panelResult.Size = new System.Drawing.Size(144, 69);
            this.panelResult.TabIndex = 11;
            this.panelResult.Visible = false;
            // 
            // txtDefKill
            // 
            this.txtDefKill.Location = new System.Drawing.Point(54, 48);
            this.txtDefKill.Name = "txtDefKill";
            this.txtDefKill.ReadOnly = true;
            this.txtDefKill.Size = new System.Drawing.Size(39, 20);
            this.txtDefKill.TabIndex = 19;
            this.txtDefKill.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAttKill
            // 
            this.txtAttKill.Location = new System.Drawing.Point(102, 48);
            this.txtAttKill.Name = "txtAttKill";
            this.txtAttKill.ReadOnly = true;
            this.txtAttKill.Size = new System.Drawing.Size(39, 20);
            this.txtAttKill.TabIndex = 18;
            this.txtAttKill.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtDefDmg
            // 
            this.txtDefDmg.Location = new System.Drawing.Point(54, 25);
            this.txtDefDmg.Name = "txtDefDmg";
            this.txtDefDmg.ReadOnly = true;
            this.txtDefDmg.Size = new System.Drawing.Size(39, 20);
            this.txtDefDmg.TabIndex = 17;
            this.txtDefDmg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAttDmg
            // 
            this.txtAttDmg.Location = new System.Drawing.Point(102, 25);
            this.txtAttDmg.Name = "txtAttDmg";
            this.txtAttDmg.ReadOnly = true;
            this.txtAttDmg.Size = new System.Drawing.Size(39, 20);
            this.txtAttDmg.TabIndex = 16;
            this.txtAttDmg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(51, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 23);
            this.label8.TabIndex = 15;
            this.label8.Text = "Def";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(3, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 23);
            this.label7.TabIndex = 14;
            this.label7.Text = "Death";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(3, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 23);
            this.label6.TabIndex = 13;
            this.label6.Text = "Dmg";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(99, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 23);
            this.label5.TabIndex = 12;
            this.label5.Text = "Att";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudDefHP
            // 
            this.nudDefHP.Location = new System.Drawing.Point(233, 39);
            this.nudDefHP.Maximum = new decimal(new int[] {
            36,
            0,
            0,
            0});
            this.nudDefHP.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDefHP.Name = "nudDefHP";
            this.nudDefHP.Size = new System.Drawing.Size(39, 20);
            this.nudDefHP.TabIndex = 4;
            this.nudDefHP.Value = new decimal(new int[] {
            36,
            0,
            0,
            0});
            this.nudDefHP.ValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // label9
            // 
            this.label9.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.label9.Location = new System.Drawing.Point(230, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 23);
            this.label9.TabIndex = 12;
            this.label9.Text = "HP";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Calculator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.nudDefHP);
            this.Controls.Add(this.panelResult);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.btnNewMap);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbDef);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelAtt);
            this.Name = "Calculator";
            this.Text = "Calculator";
            this.panelAtt.ResumeLayout(false);
            ( (System.ComponentModel.ISupportInitialize)( this.nudAttNum ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudAttHP ) ).EndInit();
            this.panelResult.ResumeLayout(false);
            this.panelResult.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.nudDefHP ) ).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAtt;
        private System.Windows.Forms.NumericUpDown nudAttNum;
        private System.Windows.Forms.NumericUpDown nudAttHP;
        private System.Windows.Forms.ComboBox cbDef;
        private System.Windows.Forms.ComboBox cbAtt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnNewMap;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.Panel panelResult;
        private System.Windows.Forms.TextBox txtAttDmg;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDefKill;
        private System.Windows.Forms.TextBox txtAttKill;
        private System.Windows.Forms.TextBox txtDefDmg;
        private System.Windows.Forms.NumericUpDown nudDefHP;
        private System.Windows.Forms.Label label9;
    }
}