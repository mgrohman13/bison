namespace CityWarWinApp
{
    partial class Battle
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
            this.lbAttacks = new System.Windows.Forms.ListBox();
            this.btnEnd = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDam = new System.Windows.Forms.TextBox();
            this.txtAP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTargDmg = new System.Windows.Forms.TextBox();
            this.txtArmor = new System.Windows.Forms.TextBox();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTargets = new System.Windows.Forms.TextBox();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtChance = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtRelic = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnCalc = new System.Windows.Forms.Button();
            this.btnLog = new System.Windows.Forms.Button();
            this.lbAtt = new System.Windows.Forms.ListBox();
            this.lbDef = new System.Windows.Forms.ListBox();
            this.cbAttAll = new System.Windows.Forms.CheckBox();
            this.cbDefAll = new System.Windows.Forms.CheckBox();
            this.btnHide = new System.Windows.Forms.Button();
            this.cbHidden = new System.Windows.Forms.CheckBox();
            this.txtGC = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.txtCost = new System.Windows.Forms.TextBox();
            this.txtSpecial = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.panelDefenders = new CityWarWinApp.PiecesPanel();
            this.panelAttackers = new CityWarWinApp.PiecesPanel();
            this.SuspendLayout();
            // 
            // lbAttacks
            // 
            this.lbAttacks.BackColor = System.Drawing.Color.White;
            this.lbAttacks.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAttacks.ForeColor = System.Drawing.Color.Black;
            this.lbAttacks.FormattingEnabled = true;
            this.lbAttacks.ItemHeight = 16;
            this.lbAttacks.Location = new System.Drawing.Point(261, 7);
            this.lbAttacks.Name = "lbAttacks";
            this.lbAttacks.Size = new System.Drawing.Size(254, 52);
            this.lbAttacks.TabIndex = 1;
            this.lbAttacks.SelectedIndexChanged += new System.EventHandler(this.lbAttacks_SelectedIndexChanged);
            // 
            // btnEnd
            // 
            this.btnEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEnd.BackColor = System.Drawing.Color.Silver;
            this.btnEnd.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnEnd.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnd.ForeColor = System.Drawing.Color.Black;
            this.btnEnd.Location = new System.Drawing.Point(261, 286);
            this.btnEnd.Name = "btnEnd";
            this.btnEnd.Size = new System.Drawing.Size(100, 39);
            this.btnEnd.TabIndex = 0;
            this.btnEnd.Text = "Done";
            this.btnEnd.UseVisualStyleBackColor = false;
            this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Silver;
            this.label1.Location = new System.Drawing.Point(390, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Pierce";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Silver;
            this.label2.Location = new System.Drawing.Point(322, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "Damage";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtDam
            // 
            this.txtDam.BackColor = System.Drawing.Color.Silver;
            this.txtDam.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDam.ForeColor = System.Drawing.Color.Black;
            this.txtDam.Location = new System.Drawing.Point(325, 83);
            this.txtDam.Name = "txtDam";
            this.txtDam.ReadOnly = true;
            this.txtDam.Size = new System.Drawing.Size(62, 22);
            this.txtDam.TabIndex = 5;
            this.txtDam.TabStop = false;
            // 
            // txtAP
            // 
            this.txtAP.BackColor = System.Drawing.Color.Silver;
            this.txtAP.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAP.ForeColor = System.Drawing.Color.Black;
            this.txtAP.Location = new System.Drawing.Point(393, 83);
            this.txtAP.Name = "txtAP";
            this.txtAP.ReadOnly = true;
            this.txtAP.Size = new System.Drawing.Size(51, 22);
            this.txtAP.TabIndex = 6;
            this.txtAP.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Silver;
            this.label3.Location = new System.Drawing.Point(258, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 18);
            this.label3.TabIndex = 7;
            this.label3.Text = "Target Unit";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Black;
            this.label4.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Silver;
            this.label4.Location = new System.Drawing.Point(348, 108);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "Armor";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Black;
            this.label5.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Silver;
            this.label5.Location = new System.Drawing.Point(438, 108);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 18);
            this.label5.TabIndex = 9;
            this.label5.Text = "Damage";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtTargDmg
            // 
            this.txtTargDmg.BackColor = System.Drawing.Color.Silver;
            this.txtTargDmg.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTargDmg.ForeColor = System.Drawing.Color.Black;
            this.txtTargDmg.Location = new System.Drawing.Point(441, 129);
            this.txtTargDmg.Name = "txtTargDmg";
            this.txtTargDmg.ReadOnly = true;
            this.txtTargDmg.Size = new System.Drawing.Size(62, 22);
            this.txtTargDmg.TabIndex = 10;
            this.txtTargDmg.TabStop = false;
            // 
            // txtArmor
            // 
            this.txtArmor.BackColor = System.Drawing.Color.Silver;
            this.txtArmor.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtArmor.ForeColor = System.Drawing.Color.Black;
            this.txtArmor.Location = new System.Drawing.Point(351, 129);
            this.txtArmor.Name = "txtArmor";
            this.txtArmor.ReadOnly = true;
            this.txtArmor.Size = new System.Drawing.Size(47, 22);
            this.txtArmor.TabIndex = 11;
            this.txtArmor.TabStop = false;
            // 
            // txtTarget
            // 
            this.txtTarget.BackColor = System.Drawing.Color.Silver;
            this.txtTarget.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTarget.ForeColor = System.Drawing.Color.Black;
            this.txtTarget.Location = new System.Drawing.Point(261, 129);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.ReadOnly = true;
            this.txtTarget.Size = new System.Drawing.Size(84, 22);
            this.txtTarget.TabIndex = 12;
            this.txtTarget.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Black;
            this.label6.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Silver;
            this.label6.Location = new System.Drawing.Point(258, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 18);
            this.label6.TabIndex = 13;
            this.label6.Text = "Targets";
            this.label6.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtTargets
            // 
            this.txtTargets.BackColor = System.Drawing.Color.Silver;
            this.txtTargets.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTargets.ForeColor = System.Drawing.Color.Black;
            this.txtTargets.Location = new System.Drawing.Point(261, 83);
            this.txtTargets.Name = "txtTargets";
            this.txtTargets.ReadOnly = true;
            this.txtTargets.Size = new System.Drawing.Size(59, 22);
            this.txtTargets.TabIndex = 14;
            this.txtTargets.TabStop = false;
            // 
            // txtLength
            // 
            this.txtLength.BackColor = System.Drawing.Color.Silver;
            this.txtLength.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLength.ForeColor = System.Drawing.Color.Black;
            this.txtLength.Location = new System.Drawing.Point(450, 83);
            this.txtLength.Name = "txtLength";
            this.txtLength.ReadOnly = true;
            this.txtLength.Size = new System.Drawing.Size(55, 22);
            this.txtLength.TabIndex = 17;
            this.txtLength.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Black;
            this.label7.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Silver;
            this.label7.Location = new System.Drawing.Point(447, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 18);
            this.label7.TabIndex = 16;
            this.label7.Text = "Length";
            this.label7.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtChance
            // 
            this.txtChance.BackColor = System.Drawing.Color.Silver;
            this.txtChance.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtChance.ForeColor = System.Drawing.Color.Black;
            this.txtChance.Location = new System.Drawing.Point(509, 129);
            this.txtChance.Name = "txtChance";
            this.txtChance.ReadOnly = true;
            this.txtChance.Size = new System.Drawing.Size(41, 22);
            this.txtChance.TabIndex = 19;
            this.txtChance.TabStop = false;
            this.txtChance.Text = "100%";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Black;
            this.label8.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Silver;
            this.label8.Location = new System.Drawing.Point(506, 108);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 18);
            this.label8.TabIndex = 18;
            this.label8.Text = "Kill";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtRelic
            // 
            this.txtRelic.BackColor = System.Drawing.Color.Silver;
            this.txtRelic.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRelic.ForeColor = System.Drawing.Color.Black;
            this.txtRelic.Location = new System.Drawing.Point(556, 129);
            this.txtRelic.Name = "txtRelic";
            this.txtRelic.ReadOnly = true;
            this.txtRelic.Size = new System.Drawing.Size(47, 22);
            this.txtRelic.TabIndex = 21;
            this.txtRelic.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Black;
            this.label9.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Silver;
            this.label9.Location = new System.Drawing.Point(553, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 18);
            this.label9.TabIndex = 20;
            this.label9.Text = "Score";
            this.label9.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // btnCalc
            // 
            this.btnCalc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalc.AutoSize = true;
            this.btnCalc.BackColor = System.Drawing.Color.Silver;
            this.btnCalc.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalc.ForeColor = System.Drawing.Color.Black;
            this.btnCalc.Location = new System.Drawing.Point(683, 297);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(92, 28);
            this.btnCalc.TabIndex = 31;
            this.btnCalc.Text = "Calculator";
            this.btnCalc.UseVisualStyleBackColor = false;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // btnLog
            // 
            this.btnLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLog.AutoSize = true;
            this.btnLog.BackColor = System.Drawing.Color.Silver;
            this.btnLog.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLog.ForeColor = System.Drawing.Color.Black;
            this.btnLog.Location = new System.Drawing.Point(683, 263);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(92, 28);
            this.btnLog.TabIndex = 32;
            this.btnLog.Text = "Log";
            this.btnLog.UseVisualStyleBackColor = false;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // lbAtt
            // 
            this.lbAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbAtt.BackColor = System.Drawing.Color.White;
            this.lbAtt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAtt.ForeColor = System.Drawing.Color.Black;
            this.lbAtt.FormattingEnabled = true;
            this.lbAtt.ItemHeight = 16;
            this.lbAtt.Location = new System.Drawing.Point(261, 157);
            this.lbAtt.Name = "lbAtt";
            this.lbAtt.Size = new System.Drawing.Size(254, 100);
            this.lbAtt.TabIndex = 33;
            this.lbAtt.SelectedIndexChanged += new System.EventHandler(this.lbAtt_SelectedIndexChanged);
            this.lbAtt.MouseLeave += new System.EventHandler(this.lb_MouseLeave);
            this.lbAtt.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lb_MouseMove);
            // 
            // lbDef
            // 
            this.lbDef.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDef.BackColor = System.Drawing.Color.White;
            this.lbDef.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDef.ForeColor = System.Drawing.Color.Black;
            this.lbDef.FormattingEnabled = true;
            this.lbDef.ItemHeight = 16;
            this.lbDef.Location = new System.Drawing.Point(521, 157);
            this.lbDef.Name = "lbDef";
            this.lbDef.Size = new System.Drawing.Size(254, 100);
            this.lbDef.TabIndex = 34;
            this.lbDef.SelectedIndexChanged += new System.EventHandler(this.lbDef_SelectedIndexChanged);
            this.lbDef.MouseLeave += new System.EventHandler(this.lb_MouseLeave);
            this.lbDef.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lb_MouseMove);
            // 
            // cbAttAll
            // 
            this.cbAttAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbAttAll.AutoSize = true;
            this.cbAttAll.Checked = true;
            this.cbAttAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAttAll.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cbAttAll.ForeColor = System.Drawing.Color.Silver;
            this.cbAttAll.Location = new System.Drawing.Point(261, 263);
            this.cbAttAll.Name = "cbAttAll";
            this.cbAttAll.Size = new System.Drawing.Size(77, 20);
            this.cbAttAll.TabIndex = 35;
            this.cbAttAll.Text = "Show All";
            this.cbAttAll.UseVisualStyleBackColor = true;
            this.cbAttAll.CheckedChanged += new System.EventHandler(this.cbAttAll_CheckedChanged);
            // 
            // cbDefAll
            // 
            this.cbDefAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDefAll.AutoSize = true;
            this.cbDefAll.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cbDefAll.ForeColor = System.Drawing.Color.Silver;
            this.cbDefAll.Location = new System.Drawing.Point(521, 263);
            this.cbDefAll.Name = "cbDefAll";
            this.cbDefAll.Size = new System.Drawing.Size(77, 20);
            this.cbDefAll.TabIndex = 36;
            this.cbDefAll.Text = "Show All";
            this.cbDefAll.UseVisualStyleBackColor = true;
            this.cbDefAll.CheckedChanged += new System.EventHandler(this.cbDefAll_CheckedChanged);
            // 
            // btnHide
            // 
            this.btnHide.AutoSize = true;
            this.btnHide.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnHide.BackColor = System.Drawing.Color.Silver;
            this.btnHide.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHide.ForeColor = System.Drawing.Color.Black;
            this.btnHide.Location = new System.Drawing.Point(521, 7);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(57, 28);
            this.btnHide.TabIndex = 39;
            this.btnHide.Text = "Show";
            this.btnHide.UseVisualStyleBackColor = false;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // cbHidden
            // 
            this.cbHidden.AutoSize = true;
            this.cbHidden.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cbHidden.ForeColor = System.Drawing.Color.Silver;
            this.cbHidden.Location = new System.Drawing.Point(584, 12);
            this.cbHidden.Name = "cbHidden";
            this.cbHidden.Size = new System.Drawing.Size(103, 20);
            this.cbHidden.TabIndex = 40;
            this.cbHidden.Text = "Show Hidden";
            this.cbHidden.UseVisualStyleBackColor = true;
            this.cbHidden.CheckedChanged += new System.EventHandler(this.cbHidden_CheckedChanged);
            // 
            // txtGC
            // 
            this.txtGC.BackColor = System.Drawing.Color.Silver;
            this.txtGC.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGC.ForeColor = System.Drawing.Color.Black;
            this.txtGC.Location = new System.Drawing.Point(404, 129);
            this.txtGC.Name = "txtGC";
            this.txtGC.ReadOnly = true;
            this.txtGC.Size = new System.Drawing.Size(31, 22);
            this.txtGC.TabIndex = 42;
            this.txtGC.TabStop = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.Black;
            this.label10.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Silver;
            this.label10.Location = new System.Drawing.Point(401, 108);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 18);
            this.label10.TabIndex = 41;
            this.label10.Text = "H/D";
            this.label10.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.BackColor = System.Drawing.Color.Black;
            this.lblCost.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCost.ForeColor = System.Drawing.Color.Silver;
            this.lblCost.Location = new System.Drawing.Point(571, 62);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(41, 18);
            this.lblCost.TabIndex = 43;
            this.lblCost.Text = "Cost";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.lblCost.Visible = false;
            // 
            // txtCost
            // 
            this.txtCost.BackColor = System.Drawing.Color.Silver;
            this.txtCost.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCost.ForeColor = System.Drawing.Color.Black;
            this.txtCost.Location = new System.Drawing.Point(574, 83);
            this.txtCost.Name = "txtCost";
            this.txtCost.ReadOnly = true;
            this.txtCost.Size = new System.Drawing.Size(38, 22);
            this.txtCost.TabIndex = 44;
            this.txtCost.TabStop = false;
            this.txtCost.Visible = false;
            // 
            // txtSpecial
            // 
            this.txtSpecial.BackColor = System.Drawing.Color.Silver;
            this.txtSpecial.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSpecial.ForeColor = System.Drawing.Color.Black;
            this.txtSpecial.Location = new System.Drawing.Point(511, 83);
            this.txtSpecial.Name = "txtSpecial";
            this.txtSpecial.ReadOnly = true;
            this.txtSpecial.Size = new System.Drawing.Size(57, 22);
            this.txtSpecial.TabIndex = 46;
            this.txtSpecial.TabStop = false;
            this.txtSpecial.Visible = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.Black;
            this.label11.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Silver;
            this.label11.Location = new System.Drawing.Point(508, 62);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 18);
            this.label11.TabIndex = 45;
            this.label11.Text = "Special";
            this.label11.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label11.Visible = false;
            // 
            // panelDefenders
            // 
            this.panelDefenders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDefenders.BackColor = System.Drawing.Color.White;
            this.panelDefenders.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelDefenders.Location = new System.Drawing.Point(781, 0);
            this.panelDefenders.Name = "panelDefenders";
            this.panelDefenders.Size = new System.Drawing.Size(255, 337);
            this.panelDefenders.TabIndex = 0;
            this.panelDefenders.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelDefenders_MouseDown);
            this.panelDefenders.MouseLeave += new System.EventHandler(this.panelDefenders_MouseLeave);
            this.panelDefenders.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelDefenders_MouseMove);
            this.panelDefenders.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelDefenders_MouseUp);
            // 
            // panelAttackers
            // 
            this.panelAttackers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelAttackers.BackColor = System.Drawing.Color.White;
            this.panelAttackers.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelAttackers.Location = new System.Drawing.Point(0, 0);
            this.panelAttackers.Name = "panelAttackers";
            this.panelAttackers.Size = new System.Drawing.Size(255, 337);
            this.panelAttackers.TabIndex = 0;
            this.panelAttackers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelAttackers_MouseDown);
            this.panelAttackers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelAttackers_MouseUp);
            // 
            // Battle
            // 
            this.AcceptButton = this.btnEnd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.CancelButton = this.btnEnd;
            this.ClientSize = new System.Drawing.Size(1026, 337);
            this.Controls.Add(this.txtSpecial);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtCost);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.txtGC);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cbHidden);
            this.Controls.Add(this.btnHide);
            this.Controls.Add(this.cbDefAll);
            this.Controls.Add(this.cbAttAll);
            this.Controls.Add(this.lbDef);
            this.Controls.Add(this.lbAtt);
            this.Controls.Add(this.btnLog);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.txtRelic);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtChance);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtLength);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtTargets);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtTarget);
            this.Controls.Add(this.txtArmor);
            this.Controls.Add(this.txtTargDmg);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAP);
            this.Controls.Add(this.txtDam);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.lbAttacks);
            this.Controls.Add(this.panelDefenders);
            this.Controls.Add(this.panelAttackers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Battle";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Battle_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PiecesPanel panelAttackers;
        private PiecesPanel panelDefenders;
        private System.Windows.Forms.ListBox lbAttacks;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDam;
        private System.Windows.Forms.TextBox txtAP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTargDmg;
        private System.Windows.Forms.TextBox txtArmor;
        private System.Windows.Forms.TextBox txtTarget;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTargets;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtChance;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtRelic;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.Button btnLog;
        private System.Windows.Forms.ListBox lbAtt;
        private System.Windows.Forms.ListBox lbDef;
        private System.Windows.Forms.CheckBox cbAttAll;
        private System.Windows.Forms.CheckBox cbDefAll;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.CheckBox cbHidden;
        private System.Windows.Forms.TextBox txtGC;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.TextBox txtCost;
        private System.Windows.Forms.TextBox txtSpecial;
        private System.Windows.Forms.Label label11;
    }
}