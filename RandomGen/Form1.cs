using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace RandomGen
{
    class randStuff : Form
    {

        //TODO:
        //seeding/ticker
        //ShiftVal
        //float/double
        //Iterate (shuffle)
        //bits/bytes
        //SelectValue


        #region Windows Form Designer generated code

        Container components = null;
        GroupBox groupBox1;
        GroupBox groupBox2;
        GroupBox groupBox3;
        Label label1;
        Label label2;
        Label label3;
        Label label4;
        Label label5;
        Label label6;
        Label label7;
        Label label8;
        TextBox rangeMin;
        TextBox rangeMax;
        TextBox rangeRes;
        TextBox oeAvg;
        TextBox oeRes;
        TextBox rndRes;
        TextBox rndNum;
        TextBox rndDiv;
        CheckBox rangeInt;
        CheckBox oeInt;
        Button btnRange;
        Button btnOE;
        Button btnRnd;
        GroupBox groupBox4;
        Label label9;
        TextBox boolChance;
        Button btnBool;
        TextBox boolRes;
        Label label11;
        CheckBox boolInv;
        GroupBox groupBox5;
        CheckBox gauInt;
        Label label10;
        Label label12;
        TextBox gauAvg;
        Button btnGau;
        TextBox gauDev;
        TextBox gauRes;
        Label label13;
        Label label14;
        TextBox gauCap;
        GroupBox groupBox6;
        CheckBox wghInt;
        Label label15;
        Label label16;
        TextBox wghMax;
        Button btnWgh;
        TextBox wghWgh;
        TextBox wghRes;
        Label label17;
        Label label18;
        TextBox rndStep;

        #endregion // Windows Form Designer generated code

        static MattUtil.MTRandom random;

        randStuff()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (components != null)
                    components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rangeMin = new System.Windows.Forms.TextBox();
            this.rangeMax = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.rangeRes = new System.Windows.Forms.TextBox();
            this.btnRange = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rangeInt = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.oeInt = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnOE = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.oeAvg = new System.Windows.Forms.TextBox();
            this.oeRes = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label18 = new System.Windows.Forms.Label();
            this.rndStep = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnRnd = new System.Windows.Forms.Button();
            this.rndDiv = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rndRes = new System.Windows.Forms.TextBox();
            this.rndNum = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.boolInv = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.boolChance = new System.Windows.Forms.TextBox();
            this.btnBool = new System.Windows.Forms.Button();
            this.boolRes = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.gauCap = new System.Windows.Forms.TextBox();
            this.gauInt = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.gauAvg = new System.Windows.Forms.TextBox();
            this.btnGau = new System.Windows.Forms.Button();
            this.gauDev = new System.Windows.Forms.TextBox();
            this.gauRes = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.wghInt = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.wghMax = new System.Windows.Forms.TextBox();
            this.btnWgh = new System.Windows.Forms.Button();
            this.wghWgh = new System.Windows.Forms.TextBox();
            this.wghRes = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Minimum";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Maximum";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // rangeMin
            // 
            this.rangeMin.Location = new System.Drawing.Point(63, 19);
            this.rangeMin.Name = "rangeMin";
            this.rangeMin.Size = new System.Drawing.Size(100, 20);
            this.rangeMin.TabIndex = 2;
            this.rangeMin.Text = "1";
            // 
            // rangeMax
            // 
            this.rangeMax.Location = new System.Drawing.Point(63, 45);
            this.rangeMax.Name = "rangeMax";
            this.rangeMax.Size = new System.Drawing.Size(100, 20);
            this.rangeMax.TabIndex = 3;
            this.rangeMax.Text = "6";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Result";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // rangeRes
            // 
            this.rangeRes.Location = new System.Drawing.Point(63, 100);
            this.rangeRes.Name = "rangeRes";
            this.rangeRes.ReadOnly = true;
            this.rangeRes.Size = new System.Drawing.Size(100, 20);
            this.rangeRes.TabIndex = 5;
            this.rangeRes.TabStop = false;
            // 
            // btnRange
            // 
            this.btnRange.Location = new System.Drawing.Point(63, 71);
            this.btnRange.Name = "btnRange";
            this.btnRange.Size = new System.Drawing.Size(75, 23);
            this.btnRange.TabIndex = 0;
            this.btnRange.Text = "OK";
            this.btnRange.Click += new System.EventHandler(this.btnRange_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rangeInt);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.rangeMin);
            this.groupBox1.Controls.Add(this.btnRange);
            this.groupBox1.Controls.Add(this.rangeMax);
            this.groupBox1.Controls.Add(this.rangeRes);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(169, 169);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Range";
            // 
            // rangeInt
            // 
            this.rangeInt.AutoSize = true;
            this.rangeInt.Checked = true;
            this.rangeInt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rangeInt.Location = new System.Drawing.Point(20, 75);
            this.rangeInt.Name = "rangeInt";
            this.rangeInt.Size = new System.Drawing.Size(37, 17);
            this.rangeInt.TabIndex = 7;
            this.rangeInt.Text = "int";
            this.rangeInt.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.oeInt);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.btnOE);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.oeAvg);
            this.groupBox2.Controls.Add(this.oeRes);
            this.groupBox2.Location = new System.Drawing.Point(187, 187);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(169, 169);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Exponential";
            // 
            // oeInt
            // 
            this.oeInt.AutoSize = true;
            this.oeInt.Checked = true;
            this.oeInt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.oeInt.Location = new System.Drawing.Point(6, 49);
            this.oeInt.Name = "oeInt";
            this.oeInt.Size = new System.Drawing.Size(74, 17);
            this.oeInt.TabIndex = 8;
            this.oeInt.Text = "Geometric";
            this.oeInt.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Average";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // btnOE
            // 
            this.btnOE.Location = new System.Drawing.Point(86, 45);
            this.btnOE.Name = "btnOE";
            this.btnOE.Size = new System.Drawing.Size(75, 23);
            this.btnOE.TabIndex = 7;
            this.btnOE.Text = "OK";
            this.btnOE.Click += new System.EventHandler(this.btnOE_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Result";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // oeAvg
            // 
            this.oeAvg.Location = new System.Drawing.Point(63, 19);
            this.oeAvg.Name = "oeAvg";
            this.oeAvg.Size = new System.Drawing.Size(100, 20);
            this.oeAvg.TabIndex = 9;
            this.oeAvg.Text = "13";
            // 
            // oeRes
            // 
            this.oeRes.Location = new System.Drawing.Point(63, 74);
            this.oeRes.Name = "oeRes";
            this.oeRes.ReadOnly = true;
            this.oeRes.Size = new System.Drawing.Size(100, 20);
            this.oeRes.TabIndex = 11;
            this.oeRes.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.rndStep);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.btnRnd);
            this.groupBox3.Controls.Add(this.rndDiv);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.rndRes);
            this.groupBox3.Controls.Add(this.rndNum);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(362, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(169, 169);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Round";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(18, 74);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(39, 13);
            this.label18.TabIndex = 18;
            this.label18.Text = "Round";
            this.label18.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // rndStep
            // 
            this.rndStep.Location = new System.Drawing.Point(63, 71);
            this.rndStep.Name = "rndStep";
            this.rndStep.Size = new System.Drawing.Size(100, 20);
            this.rndStep.TabIndex = 19;
            this.rndStep.Text = "3";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Divisor";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // btnRnd
            // 
            this.btnRnd.Location = new System.Drawing.Point(63, 97);
            this.btnRnd.Name = "btnRnd";
            this.btnRnd.Size = new System.Drawing.Size(75, 23);
            this.btnRnd.TabIndex = 12;
            this.btnRnd.Text = "OK";
            this.btnRnd.Click += new System.EventHandler(this.btnRnd_Click);
            // 
            // rndDiv
            // 
            this.rndDiv.Location = new System.Drawing.Point(63, 45);
            this.rndDiv.Name = "rndDiv";
            this.rndDiv.Size = new System.Drawing.Size(100, 20);
            this.rndDiv.TabIndex = 9;
            this.rndDiv.Text = "6";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Number";
            this.label6.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // rndRes
            // 
            this.rndRes.Location = new System.Drawing.Point(63, 126);
            this.rndRes.Name = "rndRes";
            this.rndRes.ReadOnly = true;
            this.rndRes.Size = new System.Drawing.Size(100, 20);
            this.rndRes.TabIndex = 17;
            this.rndRes.TabStop = false;
            // 
            // rndNum
            // 
            this.rndNum.Location = new System.Drawing.Point(63, 19);
            this.rndNum.Name = "rndNum";
            this.rndNum.Size = new System.Drawing.Size(100, 20);
            this.rndNum.TabIndex = 15;
            this.rndNum.Text = "26";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 129);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Result";
            this.label7.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.boolInv);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.boolChance);
            this.groupBox4.Controls.Add(this.btnBool);
            this.groupBox4.Controls.Add(this.boolRes);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Location = new System.Drawing.Point(362, 187);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(169, 169);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Boolean";
            // 
            // boolInv
            // 
            this.boolInv.AutoSize = true;
            this.boolInv.Checked = true;
            this.boolInv.CheckState = System.Windows.Forms.CheckState.Checked;
            this.boolInv.Location = new System.Drawing.Point(15, 49);
            this.boolInv.Name = "boolInv";
            this.boolInv.Size = new System.Drawing.Size(42, 17);
            this.boolInv.TabIndex = 8;
            this.boolInv.Text = "1/x";
            this.boolInv.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Chance";
            this.label9.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // boolChance
            // 
            this.boolChance.Location = new System.Drawing.Point(63, 19);
            this.boolChance.Name = "boolChance";
            this.boolChance.Size = new System.Drawing.Size(100, 20);
            this.boolChance.TabIndex = 2;
            this.boolChance.Text = "2";
            // 
            // btnBool
            // 
            this.btnBool.Location = new System.Drawing.Point(63, 45);
            this.btnBool.Name = "btnBool";
            this.btnBool.Size = new System.Drawing.Size(75, 23);
            this.btnBool.TabIndex = 0;
            this.btnBool.Text = "OK";
            this.btnBool.Click += new System.EventHandler(this.btnBool_Click);
            // 
            // boolRes
            // 
            this.boolRes.Location = new System.Drawing.Point(63, 74);
            this.boolRes.Name = "boolRes";
            this.boolRes.ReadOnly = true;
            this.boolRes.Size = new System.Drawing.Size(100, 20);
            this.boolRes.TabIndex = 5;
            this.boolRes.TabStop = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(20, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(37, 13);
            this.label11.TabIndex = 4;
            this.label11.Text = "Result";
            this.label11.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.gauCap);
            this.groupBox5.Controls.Add(this.gauInt);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(this.gauAvg);
            this.groupBox5.Controls.Add(this.btnGau);
            this.groupBox5.Controls.Add(this.gauDev);
            this.groupBox5.Controls.Add(this.gauRes);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Location = new System.Drawing.Point(12, 187);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(169, 169);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Gaussian";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(31, 74);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(26, 13);
            this.label14.TabIndex = 8;
            this.label14.Text = "Cap";
            this.label14.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // gauCap
            // 
            this.gauCap.Location = new System.Drawing.Point(63, 71);
            this.gauCap.Name = "gauCap";
            this.gauCap.Size = new System.Drawing.Size(100, 20);
            this.gauCap.TabIndex = 9;
            this.gauCap.Text = "1";
            // 
            // gauInt
            // 
            this.gauInt.AutoSize = true;
            this.gauInt.Checked = true;
            this.gauInt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.gauInt.Location = new System.Drawing.Point(20, 101);
            this.gauInt.Name = "gauInt";
            this.gauInt.Size = new System.Drawing.Size(37, 17);
            this.gauInt.TabIndex = 7;
            this.gauInt.Text = "int";
            this.gauInt.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 22);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Average";
            this.label10.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 48);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(46, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "Std Dev";
            this.label12.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // gauAvg
            // 
            this.gauAvg.Location = new System.Drawing.Point(63, 19);
            this.gauAvg.Name = "gauAvg";
            this.gauAvg.Size = new System.Drawing.Size(100, 20);
            this.gauAvg.TabIndex = 2;
            this.gauAvg.Text = "5";
            // 
            // btnGau
            // 
            this.btnGau.Location = new System.Drawing.Point(63, 97);
            this.btnGau.Name = "btnGau";
            this.btnGau.Size = new System.Drawing.Size(75, 23);
            this.btnGau.TabIndex = 0;
            this.btnGau.Text = "OK";
            this.btnGau.Click += new System.EventHandler(this.btnGau_Click);
            // 
            // gauDev
            // 
            this.gauDev.Location = new System.Drawing.Point(63, 45);
            this.gauDev.Name = "gauDev";
            this.gauDev.Size = new System.Drawing.Size(100, 20);
            this.gauDev.TabIndex = 3;
            this.gauDev.Text = "1.5";
            // 
            // gauRes
            // 
            this.gauRes.Location = new System.Drawing.Point(63, 126);
            this.gauRes.Name = "gauRes";
            this.gauRes.ReadOnly = true;
            this.gauRes.Size = new System.Drawing.Size(100, 20);
            this.gauRes.TabIndex = 5;
            this.gauRes.TabStop = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(20, 129);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(37, 13);
            this.label13.TabIndex = 4;
            this.label13.Text = "Result";
            this.label13.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.wghInt);
            this.groupBox6.Controls.Add(this.label15);
            this.groupBox6.Controls.Add(this.label16);
            this.groupBox6.Controls.Add(this.wghMax);
            this.groupBox6.Controls.Add(this.btnWgh);
            this.groupBox6.Controls.Add(this.wghWgh);
            this.groupBox6.Controls.Add(this.wghRes);
            this.groupBox6.Controls.Add(this.label17);
            this.groupBox6.Location = new System.Drawing.Point(187, 12);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(169, 169);
            this.groupBox6.TabIndex = 8;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Weighted";
            // 
            // wghInt
            // 
            this.wghInt.AutoSize = true;
            this.wghInt.Checked = true;
            this.wghInt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wghInt.Location = new System.Drawing.Point(20, 75);
            this.wghInt.Name = "wghInt";
            this.wghInt.Size = new System.Drawing.Size(37, 17);
            this.wghInt.TabIndex = 7;
            this.wghInt.Text = "int";
            this.wghInt.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(16, 48);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(41, 13);
            this.label15.TabIndex = 6;
            this.label15.Text = "Weight";
            this.label15.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 22);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(51, 13);
            this.label16.TabIndex = 1;
            this.label16.Text = "Maximum";
            this.label16.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // wghMax
            // 
            this.wghMax.Location = new System.Drawing.Point(63, 19);
            this.wghMax.Name = "wghMax";
            this.wghMax.Size = new System.Drawing.Size(100, 20);
            this.wghMax.TabIndex = 2;
            this.wghMax.Text = "10";
            // 
            // btnWgh
            // 
            this.btnWgh.Location = new System.Drawing.Point(63, 71);
            this.btnWgh.Name = "btnWgh";
            this.btnWgh.Size = new System.Drawing.Size(75, 23);
            this.btnWgh.TabIndex = 0;
            this.btnWgh.Text = "OK";
            this.btnWgh.Click += new System.EventHandler(this.btnWgh_Click);
            // 
            // wghWgh
            // 
            this.wghWgh.Location = new System.Drawing.Point(63, 45);
            this.wghWgh.Name = "wghWgh";
            this.wghWgh.Size = new System.Drawing.Size(100, 20);
            this.wghWgh.TabIndex = 3;
            this.wghWgh.Text = ".3";
            // 
            // wghRes
            // 
            this.wghRes.Location = new System.Drawing.Point(63, 100);
            this.wghRes.Name = "wghRes";
            this.wghRes.ReadOnly = true;
            this.wghRes.Size = new System.Drawing.Size(100, 20);
            this.wghRes.TabIndex = 5;
            this.wghRes.TabStop = false;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(20, 103);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(37, 13);
            this.label17.TabIndex = 4;
            this.label17.Text = "Result";
            this.label17.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // randStuff
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(543, 368);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Name = "randStuff";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Random Stuff";
            this.Shown += new System.EventHandler(this.randStuff_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion // Windows Form Designer generated code

        [STAThread]
        static void Main()
        {
            random = new MattUtil.MTRandom();
            random.StartTick();

            Application.Run(new randStuff());

            random.Dispose();
        }

        const int sleepTime = 260;

        void btnRange_Click(object sender, EventArgs e)
        {
            DoRand(rangeRes, delegate(object obj)
            {
                if (this.rangeInt.Checked)
                {
                    int min = int.Parse(this.rangeMin.Text),
                        max = int.Parse(this.rangeMax.Text);
                    this.rangeRes.Text = random.RangeInt(min, max).ToString();
                }
                else
                {
                    double min = double.Parse(this.rangeMin.Text),
                       max = double.Parse(this.rangeMax.Text);
                    this.rangeRes.Text = random.Range(min, max).ToString();
                }
            });
        }

        void btnOE_Click(object sender, EventArgs e)
        {
            DoRand(oeRes, delegate(object obj)
            {
                double avg = double.Parse(this.oeAvg.Text);
                if (this.oeInt.Checked)
                    this.oeRes.Text = random.OEInt(avg).ToString();
                else
                    this.oeRes.Text = random.OE(avg).ToString();
            });
        }

        void btnRnd_Click(object sender, EventArgs e)
        {
            DoRand(rndRes, delegate(object obj)
            {
                double num = double.Parse(this.rndNum.Text);
                double div = double.Parse(this.rndDiv.Text);
                double step = double.Parse(this.rndStep.Text);
                this.rndRes.Text = ( random.Round(num / div / step) * step ).ToString();
            });
        }

        void btnBool_Click(object sender, EventArgs e)
        {
            DoRand(boolRes, delegate(object obj)
            {
                double chance = double.Parse(this.boolChance.Text);
                if (!boolInv.Checked)
                    chance = 1 / chance;
                int cInt = (int)chance;
                if (cInt == 2)
                    this.boolRes.Text = random.Bool().ToString();
                else if (cInt == chance)
                    this.boolRes.Text = ( random.RangeInt(1, cInt) == 1 ).ToString();
                else
                    this.boolRes.Text = random.Bool(1 / chance).ToString();
            });
        }

        void btnGau_Click(object sender, EventArgs e)
        {
            DoRand(gauRes, delegate(object obj)
            {
                double avg = double.Parse(this.gauAvg.Text);
                double dev = double.Parse(this.gauDev.Text);
                int resI = int.MinValue;
                double resD = double.NaN;
                if (this.gauCap.Text.Length > 0)
                {
                    string[] split = this.gauCap.Text.Split('(', ')');
                    double lowCap;
                    bool zero = ( avg == 0 );
                    if (zero)
                        ++avg;
                    if (gauInt.Checked)
                    {
                        int cap;
                        try
                        {
                            cap = int.Parse(split[0]);
                        }
                        catch
                        {
                            cap = int.Parse(split[1]);
                            cap = random.Round(2 * avg - cap);
                        }
                        lowCap = cap;
                        if (zero)
                            ++cap;
                        resI = random.GaussianCappedInt(avg, dev / avg, cap);
                    }
                    else
                    {
                        double cap;
                        try
                        {
                            cap = double.Parse(split[0]);
                        }
                        catch
                        {
                            cap = double.Parse(split[1]);
                            cap = 2 * avg - cap;
                        }
                        lowCap = cap;
                        if (zero)
                            ++cap;
                        resD = random.GaussianCapped(avg, dev / avg, cap);
                    }
                    if (zero)
                    {
                        --resI;
                        --resD;
                    }
                    gauCap.Text = string.Format("{0} ({1})", lowCap, 2 * avg - lowCap);
                }
                else
                {
                    resD = avg + random.Gaussian(dev);
                    if (gauInt.Checked)
                        resI = random.Round(resD);
                }
                if (gauInt.Checked)
                    this.gauRes.Text = resI.ToString();
                else
                    this.gauRes.Text = resD.ToString();
            });
        }

        void btnWgh_Click(object sender, EventArgs e)
        {
            DoRand(wghRes, delegate(object obj)
            {
                double weight = double.Parse(this.wghWgh.Text);
                if (wghInt.Checked)
                {
                    int max = int.Parse(this.wghMax.Text);
                    this.wghRes.Text = random.WeightedInt(max, weight).ToString();
                }
                else
                {
                    double max = double.Parse(this.wghMax.Text);
                    this.wghRes.Text = random.Weighted(max, weight).ToString();
                }
            });
        }

        void DoRand(TextBox result, WaitCallback Callback)
        {
            try
            {
                result.Clear();
                this.Refresh();
                Thread.Sleep(sleepTime);
                Callback(null);
            }
            catch
            {
                result.Text = "ERROR";
            }
        }

        private void randStuff_Shown(object sender, EventArgs e)
        {
            this.Refresh();
            foreach (int i in random.Iterate(6))
            {
                switch (i)
                {
                case 0:
                    btnRange_Click(null, null);
                    break;
                case 1:
                    btnOE_Click(null, null);
                    break;
                case 2:
                    btnRnd_Click(null, null);
                    break;
                case 3:
                    btnBool_Click(null, null);
                    break;
                case 4:
                    btnGau_Click(null, null);
                    break;
                case 5:
                    btnWgh_Click(null, null);
                    break;
                }
            }
        }
    }
}
