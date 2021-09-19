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
            this.button1 = new System.Windows.Forms.Button();
            this.panelAtt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAttNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAttHP)).BeginInit();
            this.panelResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDefHP)).BeginInit();
            this.SuspendLayout();
            // 
            // panelAtt
            // 
            this.panelAtt.Controls.Add(this.nudAttNum);
            this.panelAtt.Controls.Add(this.nudAttHP);
            this.panelAtt.Controls.Add(this.cbAtt);
            this.panelAtt.Location = new System.Drawing.Point(18, 54);
            this.panelAtt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelAtt.Name = "panelAtt";
            this.panelAtt.Size = new System.Drawing.Size(202, 40);
            this.panelAtt.TabIndex = 0;
            // 
            // nudAttNum
            // 
            this.nudAttNum.Location = new System.Drawing.Point(140, 6);
            this.nudAttNum.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.nudAttNum.Size = new System.Drawing.Size(58, 26);
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
            this.nudAttHP.Location = new System.Drawing.Point(72, 6);
            this.nudAttHP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.nudAttHP.Size = new System.Drawing.Size(58, 26);
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
            this.cbAtt.Location = new System.Drawing.Point(4, 5);
            this.cbAtt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbAtt.Name = "cbAtt";
            this.cbAtt.Size = new System.Drawing.Size(56, 28);
            this.cbAtt.TabIndex = 0;
            this.cbAtt.Text = "4";
            this.cbAtt.SelectedValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // cbDef
            // 
            this.cbDef.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDef.FormattingEnabled = true;
            this.cbDef.Items.AddRange(new object[] {
            ".5",
            "1",
            "1.5",
            "2",
            "3",
            "4"});
            this.cbDef.Location = new System.Drawing.Point(282, 58);
            this.cbDef.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbDef.Name = "cbDef";
            this.cbDef.Size = new System.Drawing.Size(56, 28);
            this.cbDef.TabIndex = 1;
            this.cbDef.Text = "2";
            this.cbDef.SelectedValueChanged += new System.EventHandler(this.valueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(18, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 35);
            this.label1.TabIndex = 4;
            this.label1.Text = "Att";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(278, 14);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 35);
            this.label2.TabIndex = 5;
            this.label2.Text = "Def";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(86, 14);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 35);
            this.label3.TabIndex = 6;
            this.label3.Text = "HP";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(153, 14);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 35);
            this.label4.TabIndex = 7;
            this.label4.Text = "Num";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(22, 100);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(126, 35);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Text = "Add Attack";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Visible = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnNewMap
            // 
            this.btnNewMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewMap.Location = new System.Drawing.Point(296, 349);
            this.btnNewMap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnNewMap.Name = "btnNewMap";
            this.btnNewMap.Size = new System.Drawing.Size(112, 35);
            this.btnNewMap.TabIndex = 9;
            this.btnNewMap.Text = "New Map";
            this.btnNewMap.UseVisualStyleBackColor = true;
            this.btnNewMap.Click += new System.EventHandler(this.btnNewMap_Click);
            // 
            // btnCalc
            // 
            this.btnCalc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalc.Location = new System.Drawing.Point(282, 100);
            this.btnCalc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(126, 35);
            this.btnCalc.TabIndex = 10;
            this.btnCalc.Text = "Calculate";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // panelResult
            // 
            this.panelResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelResult.Controls.Add(this.txtDefKill);
            this.panelResult.Controls.Add(this.txtAttKill);
            this.panelResult.Controls.Add(this.txtDefDmg);
            this.panelResult.Controls.Add(this.txtAttDmg);
            this.panelResult.Controls.Add(this.label8);
            this.panelResult.Controls.Add(this.label7);
            this.panelResult.Controls.Add(this.label6);
            this.panelResult.Controls.Add(this.label5);
            this.panelResult.Location = new System.Drawing.Point(105, 234);
            this.panelResult.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelResult.Name = "panelResult";
            this.panelResult.Size = new System.Drawing.Size(216, 106);
            this.panelResult.TabIndex = 11;
            this.panelResult.Visible = false;
            // 
            // txtDefKill
            // 
            this.txtDefKill.Location = new System.Drawing.Point(81, 74);
            this.txtDefKill.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDefKill.Name = "txtDefKill";
            this.txtDefKill.ReadOnly = true;
            this.txtDefKill.Size = new System.Drawing.Size(56, 26);
            this.txtDefKill.TabIndex = 19;
            this.txtDefKill.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAttKill
            // 
            this.txtAttKill.Location = new System.Drawing.Point(153, 74);
            this.txtAttKill.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtAttKill.Name = "txtAttKill";
            this.txtAttKill.ReadOnly = true;
            this.txtAttKill.Size = new System.Drawing.Size(56, 26);
            this.txtAttKill.TabIndex = 18;
            this.txtAttKill.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtDefDmg
            // 
            this.txtDefDmg.Location = new System.Drawing.Point(81, 38);
            this.txtDefDmg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDefDmg.Name = "txtDefDmg";
            this.txtDefDmg.ReadOnly = true;
            this.txtDefDmg.Size = new System.Drawing.Size(56, 26);
            this.txtDefDmg.TabIndex = 17;
            this.txtDefDmg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAttDmg
            // 
            this.txtAttDmg.Location = new System.Drawing.Point(153, 38);
            this.txtAttDmg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtAttDmg.Name = "txtAttDmg";
            this.txtAttDmg.ReadOnly = true;
            this.txtAttDmg.Size = new System.Drawing.Size(56, 26);
            this.txtAttDmg.TabIndex = 16;
            this.txtAttDmg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(76, 0);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 35);
            this.label8.TabIndex = 15;
            this.label8.Text = "Def";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(4, 71);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 35);
            this.label7.TabIndex = 14;
            this.label7.Text = "Death";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(4, 35);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 35);
            this.label6.TabIndex = 13;
            this.label6.Text = "Dmg";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(148, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 35);
            this.label5.TabIndex = 12;
            this.label5.Text = "Att";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudDefHP
            // 
            this.nudDefHP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDefHP.Location = new System.Drawing.Point(350, 60);
            this.nudDefHP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.nudDefHP.Size = new System.Drawing.Size(58, 26);
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
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.Location = new System.Drawing.Point(345, 14);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 35);
            this.label9.TabIndex = 12;
            this.label9.Text = "HP";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(176, 350);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 35);
            this.button1.TabIndex = 13;
            this.button1.Text = "Balance";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Calculator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 403);
            this.Controls.Add(this.button1);
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
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Calculator";
            this.Text = "Calculator";
            this.panelAtt.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudAttNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAttHP)).EndInit();
            this.panelResult.ResumeLayout(false);
            this.panelResult.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDefHP)).EndInit();
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
        private System.Windows.Forms.Button button1;
    }
}