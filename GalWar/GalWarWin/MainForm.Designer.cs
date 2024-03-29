namespace GalWarWin
{
    partial class MainForm
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
            if (disposing)
            {
                if (font != null)
                    font.Dispose();
                if (components != null)
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
            this.pnlHUD = new System.Windows.Forms.Panel();
            this.lblNext = new System.Windows.Forms.Label();
            this.lblPrev = new System.Windows.Forms.Label();
            this.btnColonies = new System.Windows.Forms.Button();
            this.btnShips = new System.Windows.Forms.Button();
            this.lblLoc = new System.Windows.Forms.Label();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblTop = new System.Windows.Forms.Label();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lbl3 = new System.Windows.Forms.Label();
            this.lbl7Inf = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.lbl7 = new System.Windows.Forms.Label();
            this.lbl5 = new System.Windows.Forms.Label();
            this.lbl6 = new System.Windows.Forms.Label();
            this.lbl1Inf = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.lbl2Inf = new System.Windows.Forms.Label();
            this.lbl6Inf = new System.Windows.Forms.Label();
            this.lbl3Inf = new System.Windows.Forms.Label();
            this.lbl5Inf = new System.Windows.Forms.Label();
            this.lbl4Inf = new System.Windows.Forms.Label();
            this.pnlEconomy = new System.Windows.Forms.Panel();
            this.lblProdTot = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblGold = new System.Windows.Forms.Label();
            this.lblPopulation = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblGoldInc = new System.Windows.Forms.Label();
            this.lblPopInc = new System.Windows.Forms.Label();
            this.lblPlayerResearch = new System.Windows.Forms.Label();
            this.lblResearch = new System.Windows.Forms.Label();
            this.chkProduction = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chkResearch = new System.Windows.Forms.CheckBox();
            this.lblProduction = new System.Windows.Forms.Label();
            this.chkGold = new System.Windows.Forms.CheckBox();
            this.lblRsrchPct = new System.Windows.Forms.Label();
            this.btnProduction = new System.Windows.Forms.Button();
            this.btnGoldRepair = new System.Windows.Forms.Button();
            this.btnDisband = new System.Windows.Forms.Button();
            this.lblBottom = new System.Windows.Forms.Label();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnAutoRepairShips = new System.Windows.Forms.Button();
            this.btnCombat = new System.Windows.Forms.Button();
            this.btnInvasion = new System.Windows.Forms.Button();
            this.btnShowMoves = new System.Windows.Forms.Button();
            this.btnCostCalc = new System.Windows.Forms.Button();
            this.btnGraphs = new System.Windows.Forms.Button();
            this.btnSaveGame = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnEndTurn = new System.Windows.Forms.Button();
            this.lblPlayer = new System.Windows.Forms.Label();
            this.btnNewGame = new System.Windows.Forms.Button();
            this.btnLoadGame = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnAutosaveView = new System.Windows.Forms.Button();
            this.tbTurns = new System.Windows.Forms.TrackBar();
            this.pnlBuild = new GalWarWin.BuildableControl();
            this.pnlHUD.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlEconomy.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbTurns)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHUD
            // 
            this.pnlHUD.BackColor = System.Drawing.Color.White;
            this.pnlHUD.Controls.Add(this.lblNext);
            this.pnlHUD.Controls.Add(this.lblPrev);
            this.pnlHUD.Controls.Add(this.pnlBuild);
            this.pnlHUD.Controls.Add(this.btnColonies);
            this.pnlHUD.Controls.Add(this.btnShips);
            this.pnlHUD.Controls.Add(this.lblLoc);
            this.pnlHUD.Controls.Add(this.pnlInfo);
            this.pnlHUD.Controls.Add(this.pnlEconomy);
            this.pnlHUD.Controls.Add(this.btnProduction);
            this.pnlHUD.Controls.Add(this.btnGoldRepair);
            this.pnlHUD.Controls.Add(this.btnDisband);
            this.pnlHUD.Controls.Add(this.lblBottom);
            this.pnlHUD.Controls.Add(this.btnUndo);
            this.pnlHUD.Controls.Add(this.btnAutoRepairShips);
            this.pnlHUD.Controls.Add(this.btnCombat);
            this.pnlHUD.Controls.Add(this.btnInvasion);
            this.pnlHUD.Controls.Add(this.btnShowMoves);
            this.pnlHUD.Controls.Add(this.btnCostCalc);
            this.pnlHUD.Controls.Add(this.btnGraphs);
            this.pnlHUD.Controls.Add(this.btnSaveGame);
            this.pnlHUD.Controls.Add(this.btnCancel);
            this.pnlHUD.Controls.Add(this.btnEndTurn);
            this.pnlHUD.Controls.Add(this.lblPlayer);
            this.pnlHUD.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlHUD.Location = new System.Drawing.Point(1101, 0);
            this.pnlHUD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlHUD.Name = "pnlHUD";
            this.pnlHUD.Size = new System.Drawing.Size(300, 1095);
            this.pnlHUD.TabIndex = 2;
            this.pnlHUD.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            // 
            // lblNext
            // 
            this.lblNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblNext.AutoEllipsis = true;
            this.lblNext.Location = new System.Drawing.Point(262, 1025);
            this.lblNext.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNext.Name = "lblNext";
            this.lblNext.Size = new System.Drawing.Size(38, 35);
            this.lblNext.TabIndex = 59;
            this.lblNext.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPrev
            // 
            this.lblPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPrev.AutoEllipsis = true;
            this.lblPrev.Location = new System.Drawing.Point(0, 1025);
            this.lblPrev.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPrev.Name = "lblPrev";
            this.lblPrev.Size = new System.Drawing.Size(38, 35);
            this.lblPrev.TabIndex = 58;
            this.lblPrev.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnColonies
            // 
            this.btnColonies.Location = new System.Drawing.Point(0, 728);
            this.btnColonies.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnColonies.Name = "btnColonies";
            this.btnColonies.Size = new System.Drawing.Size(150, 35);
            this.btnColonies.TabIndex = 56;
            this.btnColonies.Text = "Colonies";
            this.btnColonies.UseVisualStyleBackColor = true;
            this.btnColonies.Click += new System.EventHandler(this.btnColonies_Click);
            // 
            // btnShips
            // 
            this.btnShips.Location = new System.Drawing.Point(150, 728);
            this.btnShips.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnShips.Name = "btnShips";
            this.btnShips.Size = new System.Drawing.Size(150, 35);
            this.btnShips.TabIndex = 57;
            this.btnShips.Text = "Ships";
            this.btnShips.UseVisualStyleBackColor = true;
            this.btnShips.Click += new System.EventHandler(this.btnShips_Click);
            // 
            // lblLoc
            // 
            this.lblLoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLoc.Location = new System.Drawing.Point(0, 940);
            this.lblLoc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLoc.Name = "lblLoc";
            this.lblLoc.Size = new System.Drawing.Size(150, 35);
            this.lblLoc.TabIndex = 55;
            this.lblLoc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLoc.Click += new System.EventHandler(this.lblLoc_Click);
            // 
            // pnlInfo
            // 
            this.pnlInfo.AutoSize = true;
            this.pnlInfo.Controls.Add(this.lblTop);
            this.pnlInfo.Controls.Add(this.lbl2);
            this.pnlInfo.Controls.Add(this.lbl3);
            this.pnlInfo.Controls.Add(this.lbl7Inf);
            this.pnlInfo.Controls.Add(this.lbl4);
            this.pnlInfo.Controls.Add(this.lbl7);
            this.pnlInfo.Controls.Add(this.lbl5);
            this.pnlInfo.Controls.Add(this.lbl6);
            this.pnlInfo.Controls.Add(this.lbl1Inf);
            this.pnlInfo.Controls.Add(this.lbl1);
            this.pnlInfo.Controls.Add(this.lbl2Inf);
            this.pnlInfo.Controls.Add(this.lbl6Inf);
            this.pnlInfo.Controls.Add(this.lbl3Inf);
            this.pnlInfo.Controls.Add(this.lbl5Inf);
            this.pnlInfo.Controls.Add(this.lbl4Inf);
            this.pnlInfo.Location = new System.Drawing.Point(0, 169);
            this.pnlInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(304, 283);
            this.pnlInfo.TabIndex = 54;
            this.pnlInfo.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lblTop
            // 
            this.lblTop.AutoEllipsis = true;
            this.lblTop.Location = new System.Drawing.Point(0, 0);
            this.lblTop.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTop.Name = "lblTop";
            this.lblTop.Size = new System.Drawing.Size(300, 35);
            this.lblTop.TabIndex = 2;
            this.lblTop.Text = "lblTop";
            this.lblTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTop.Click += new System.EventHandler(this.lblTop_Click);
            this.lblTop.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl2
            // 
            this.lbl2.AutoEllipsis = true;
            this.lbl2.Location = new System.Drawing.Point(12, 72);
            this.lbl2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(90, 31);
            this.lbl2.TabIndex = 3;
            this.lbl2.Text = "label2";
            this.lbl2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl3
            // 
            this.lbl3.AutoEllipsis = true;
            this.lbl3.Location = new System.Drawing.Point(12, 108);
            this.lbl3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl3.Name = "lbl3";
            this.lbl3.Size = new System.Drawing.Size(90, 31);
            this.lbl3.TabIndex = 4;
            this.lbl3.Text = "label3";
            this.lbl3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl3.Click += new System.EventHandler(this.lbl3_Click);
            this.lbl3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl7Inf
            // 
            this.lbl7Inf.AutoEllipsis = true;
            this.lbl7Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl7Inf.Location = new System.Drawing.Point(111, 248);
            this.lbl7Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl7Inf.Name = "lbl7Inf";
            this.lbl7Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl7Inf.TabIndex = 40;
            this.lbl7Inf.Text = "lblInf7";
            this.lbl7Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl7Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl4
            // 
            this.lbl4.AutoEllipsis = true;
            this.lbl4.Location = new System.Drawing.Point(12, 143);
            this.lbl4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(90, 31);
            this.lbl4.TabIndex = 5;
            this.lbl4.Text = "label4";
            this.lbl4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl4.Click += new System.EventHandler(this.lbl4_Click);
            this.lbl4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl7
            // 
            this.lbl7.AutoEllipsis = true;
            this.lbl7.Location = new System.Drawing.Point(12, 249);
            this.lbl7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl7.Name = "lbl7";
            this.lbl7.Size = new System.Drawing.Size(90, 31);
            this.lbl7.TabIndex = 39;
            this.lbl7.Text = "label7";
            this.lbl7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl7.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl5
            // 
            this.lbl5.AutoEllipsis = true;
            this.lbl5.Location = new System.Drawing.Point(12, 178);
            this.lbl5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl5.Name = "lbl5";
            this.lbl5.Size = new System.Drawing.Size(90, 31);
            this.lbl5.TabIndex = 6;
            this.lbl5.Text = "label5";
            this.lbl5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl5.Click += new System.EventHandler(this.lbl5_Click);
            this.lbl5.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl6
            // 
            this.lbl6.AutoEllipsis = true;
            this.lbl6.Location = new System.Drawing.Point(12, 214);
            this.lbl6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl6.Name = "lbl6";
            this.lbl6.Size = new System.Drawing.Size(90, 31);
            this.lbl6.TabIndex = 7;
            this.lbl6.Text = "Experience";
            this.lbl6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl6.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl1Inf
            // 
            this.lbl1Inf.AutoEllipsis = true;
            this.lbl1Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl1Inf.Location = new System.Drawing.Point(111, 35);
            this.lbl1Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl1Inf.Name = "lbl1Inf";
            this.lbl1Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl1Inf.TabIndex = 10;
            this.lbl1Inf.Text = "lblInf1";
            this.lbl1Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl1Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl1
            // 
            this.lbl1.AutoEllipsis = true;
            this.lbl1.Location = new System.Drawing.Point(12, 37);
            this.lbl1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(90, 31);
            this.lbl1.TabIndex = 16;
            this.lbl1.Text = "label1";
            this.lbl1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl2Inf
            // 
            this.lbl2Inf.AutoEllipsis = true;
            this.lbl2Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl2Inf.Location = new System.Drawing.Point(111, 71);
            this.lbl2Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl2Inf.Name = "lbl2Inf";
            this.lbl2Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl2Inf.TabIndex = 11;
            this.lbl2Inf.Text = "lblInf2";
            this.lbl2Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl2Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl6Inf
            // 
            this.lbl6Inf.AutoEllipsis = true;
            this.lbl6Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl6Inf.Location = new System.Drawing.Point(111, 212);
            this.lbl6Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl6Inf.Name = "lbl6Inf";
            this.lbl6Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl6Inf.TabIndex = 15;
            this.lbl6Inf.Text = "lblInf6";
            this.lbl6Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl6Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl3Inf
            // 
            this.lbl3Inf.AutoEllipsis = true;
            this.lbl3Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl3Inf.Location = new System.Drawing.Point(111, 106);
            this.lbl3Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl3Inf.Name = "lbl3Inf";
            this.lbl3Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl3Inf.TabIndex = 12;
            this.lbl3Inf.Text = "lblInf3";
            this.lbl3Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl3Inf.Click += new System.EventHandler(this.lbl3_Click);
            this.lbl3Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl5Inf
            // 
            this.lbl5Inf.AutoEllipsis = true;
            this.lbl5Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl5Inf.Location = new System.Drawing.Point(111, 177);
            this.lbl5Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl5Inf.Name = "lbl5Inf";
            this.lbl5Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl5Inf.TabIndex = 14;
            this.lbl5Inf.Text = "lblInf5";
            this.lbl5Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl5Inf.Click += new System.EventHandler(this.lbl5_Click);
            this.lbl5Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // lbl4Inf
            // 
            this.lbl4Inf.AutoEllipsis = true;
            this.lbl4Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl4Inf.Location = new System.Drawing.Point(111, 142);
            this.lbl4Inf.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl4Inf.Name = "lbl4Inf";
            this.lbl4Inf.Size = new System.Drawing.Size(189, 35);
            this.lbl4Inf.TabIndex = 13;
            this.lbl4Inf.Text = "lblInf4";
            this.lbl4Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl4Inf.Click += new System.EventHandler(this.lbl4_Click);
            this.lbl4Inf.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlInfo_MouseClick);
            // 
            // pnlEconomy
            // 
            this.pnlEconomy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlEconomy.AutoSize = true;
            this.pnlEconomy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlEconomy.Controls.Add(this.lblProdTot);
            this.pnlEconomy.Controls.Add(this.label12);
            this.pnlEconomy.Controls.Add(this.lblGold);
            this.pnlEconomy.Controls.Add(this.lblPopulation);
            this.pnlEconomy.Controls.Add(this.label8);
            this.pnlEconomy.Controls.Add(this.lblGoldInc);
            this.pnlEconomy.Controls.Add(this.lblPopInc);
            this.pnlEconomy.Controls.Add(this.lblPlayerResearch);
            this.pnlEconomy.Controls.Add(this.lblResearch);
            this.pnlEconomy.Controls.Add(this.chkProduction);
            this.pnlEconomy.Controls.Add(this.label11);
            this.pnlEconomy.Controls.Add(this.chkResearch);
            this.pnlEconomy.Controls.Add(this.lblProduction);
            this.pnlEconomy.Controls.Add(this.chkGold);
            this.pnlEconomy.Controls.Add(this.lblRsrchPct);
            this.pnlEconomy.Location = new System.Drawing.Point(7, 18);
            this.pnlEconomy.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlEconomy.Name = "pnlEconomy";
            this.pnlEconomy.Size = new System.Drawing.Size(289, 141);
            this.pnlEconomy.TabIndex = 45;
            // 
            // lblProdTot
            // 
            this.lblProdTot.AutoEllipsis = true;
            this.lblProdTot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProdTot.Location = new System.Drawing.Point(132, 106);
            this.lblProdTot.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProdTot.Name = "lblProdTot";
            this.lblProdTot.Size = new System.Drawing.Size(63, 35);
            this.lblProdTot.TabIndex = 39;
            this.lblProdTot.Text = "99999";
            this.lblProdTot.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoEllipsis = true;
            this.label12.Location = new System.Drawing.Point(36, 0);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 35);
            this.label12.TabIndex = 25;
            this.label12.Text = "Population";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblGold
            // 
            this.lblGold.AutoEllipsis = true;
            this.lblGold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGold.Location = new System.Drawing.Point(132, 35);
            this.lblGold.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGold.Name = "lblGold";
            this.lblGold.Size = new System.Drawing.Size(80, 35);
            this.lblGold.TabIndex = 19;
            this.lblGold.Text = "99999.9";
            this.lblGold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGold.Click += new System.EventHandler(this.lblGold_Click);
            // 
            // lblPopulation
            // 
            this.lblPopulation.AutoEllipsis = true;
            this.lblPopulation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPopulation.Location = new System.Drawing.Point(132, 0);
            this.lblPopulation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPopulation.Name = "lblPopulation";
            this.lblPopulation.Size = new System.Drawing.Size(63, 35);
            this.lblPopulation.TabIndex = 21;
            this.lblPopulation.Text = "99999";
            this.lblPopulation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.Location = new System.Drawing.Point(34, 37);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 30);
            this.label8.TabIndex = 26;
            this.label8.Text = "Gold";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label8.Click += new System.EventHandler(this.lblGold_Click);
            // 
            // lblGoldInc
            // 
            this.lblGoldInc.AutoEllipsis = true;
            this.lblGoldInc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGoldInc.Location = new System.Drawing.Point(216, 35);
            this.lblGoldInc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGoldInc.Name = "lblGoldInc";
            this.lblGoldInc.Size = new System.Drawing.Size(69, 35);
            this.lblGoldInc.TabIndex = 27;
            this.lblGoldInc.Text = "+999.9";
            this.lblGoldInc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGoldInc.Click += new System.EventHandler(this.lblGold_Click);
            // 
            // lblPopInc
            // 
            this.lblPopInc.AutoEllipsis = true;
            this.lblPopInc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPopInc.Location = new System.Drawing.Point(216, 0);
            this.lblPopInc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPopInc.Name = "lblPopInc";
            this.lblPopInc.Size = new System.Drawing.Size(52, 35);
            this.lblPopInc.TabIndex = 28;
            this.lblPopInc.Text = "+999";
            this.lblPopInc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPlayerResearch
            // 
            this.lblPlayerResearch.AutoEllipsis = true;
            this.lblPlayerResearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPlayerResearch.Location = new System.Drawing.Point(36, 71);
            this.lblPlayerResearch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPlayerResearch.Name = "lblPlayerResearch";
            this.lblPlayerResearch.Size = new System.Drawing.Size(86, 34);
            this.lblPlayerResearch.TabIndex = 30;
            this.lblPlayerResearch.Text = "Research";
            this.lblPlayerResearch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPlayerResearch.Click += new System.EventHandler(this.lblResearch_Click);
            // 
            // lblResearch
            // 
            this.lblResearch.AutoEllipsis = true;
            this.lblResearch.BackColor = System.Drawing.Color.Transparent;
            this.lblResearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResearch.Location = new System.Drawing.Point(216, 71);
            this.lblResearch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResearch.Name = "lblResearch";
            this.lblResearch.Size = new System.Drawing.Size(52, 35);
            this.lblResearch.TabIndex = 31;
            this.lblResearch.Text = "+999";
            this.lblResearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblResearch.Click += new System.EventHandler(this.lblResearch_Click);
            // 
            // chkProduction
            // 
            this.chkProduction.AutoSize = true;
            this.chkProduction.Location = new System.Drawing.Point(4, 114);
            this.chkProduction.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkProduction.Name = "chkProduction";
            this.chkProduction.Size = new System.Drawing.Size(22, 21);
            this.chkProduction.TabIndex = 36;
            this.chkProduction.UseVisualStyleBackColor = true;
            this.chkProduction.CheckedChanged += new System.EventHandler(this.chkEmphasis_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoEllipsis = true;
            this.label11.Location = new System.Drawing.Point(36, 106);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(87, 35);
            this.label11.TabIndex = 32;
            this.label11.Text = "Production";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkResearch
            // 
            this.chkResearch.AutoSize = true;
            this.chkResearch.Location = new System.Drawing.Point(4, 78);
            this.chkResearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkResearch.Name = "chkResearch";
            this.chkResearch.Size = new System.Drawing.Size(22, 21);
            this.chkResearch.TabIndex = 35;
            this.chkResearch.UseVisualStyleBackColor = true;
            this.chkResearch.CheckedChanged += new System.EventHandler(this.chkEmphasis_CheckedChanged);
            // 
            // lblProduction
            // 
            this.lblProduction.AutoEllipsis = true;
            this.lblProduction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProduction.Location = new System.Drawing.Point(216, 106);
            this.lblProduction.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProduction.Name = "lblProduction";
            this.lblProduction.Size = new System.Drawing.Size(52, 35);
            this.lblProduction.TabIndex = 33;
            this.lblProduction.Text = "+999";
            this.lblProduction.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkGold
            // 
            this.chkGold.AutoSize = true;
            this.chkGold.Location = new System.Drawing.Point(4, 43);
            this.chkGold.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkGold.Name = "chkGold";
            this.chkGold.Size = new System.Drawing.Size(22, 21);
            this.chkGold.TabIndex = 34;
            this.chkGold.UseVisualStyleBackColor = true;
            this.chkGold.CheckedChanged += new System.EventHandler(this.chkEmphasis_CheckedChanged);
            // 
            // lblRsrchPct
            // 
            this.lblRsrchPct.AutoEllipsis = true;
            this.lblRsrchPct.BackColor = System.Drawing.Color.Transparent;
            this.lblRsrchPct.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRsrchPct.Location = new System.Drawing.Point(132, 71);
            this.lblRsrchPct.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRsrchPct.Name = "lblRsrchPct";
            this.lblRsrchPct.Size = new System.Drawing.Size(80, 35);
            this.lblRsrchPct.TabIndex = 40;
            this.lblRsrchPct.Text = "99%";
            this.lblRsrchPct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnProduction
            // 
            this.btnProduction.Location = new System.Drawing.Point(0, 462);
            this.btnProduction.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnProduction.Name = "btnProduction";
            this.btnProduction.Size = new System.Drawing.Size(300, 35);
            this.btnProduction.TabIndex = 0;
            this.btnProduction.Text = "Production";
            this.btnProduction.UseVisualStyleBackColor = true;
            this.btnProduction.Click += new System.EventHandler(this.btnProduction_Click);
            // 
            // btnGoldRepair
            // 
            this.btnGoldRepair.Location = new System.Drawing.Point(0, 505);
            this.btnGoldRepair.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnGoldRepair.Name = "btnGoldRepair";
            this.btnGoldRepair.Size = new System.Drawing.Size(300, 35);
            this.btnGoldRepair.TabIndex = 23;
            this.btnGoldRepair.Text = "Repair";
            this.btnGoldRepair.UseVisualStyleBackColor = true;
            this.btnGoldRepair.Click += new System.EventHandler(this.btnGoldRepair_Click);
            // 
            // btnDisband
            // 
            this.btnDisband.Location = new System.Drawing.Point(0, 549);
            this.btnDisband.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDisband.Name = "btnDisband";
            this.btnDisband.Size = new System.Drawing.Size(300, 35);
            this.btnDisband.TabIndex = 21;
            this.btnDisband.Text = "Disband Ship";
            this.btnDisband.UseVisualStyleBackColor = true;
            this.btnDisband.Click += new System.EventHandler(this.btnDisband_Click);
            // 
            // lblBottom
            // 
            this.lblBottom.Location = new System.Drawing.Point(0, 457);
            this.lblBottom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBottom.Name = "lblBottom";
            this.lblBottom.Size = new System.Drawing.Size(300, 35);
            this.lblBottom.TabIndex = 9;
            this.lblBottom.Text = "lblBottom";
            this.lblBottom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnUndo
            // 
            this.btnUndo.Location = new System.Drawing.Point(0, 594);
            this.btnUndo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(150, 35);
            this.btnUndo.TabIndex = 51;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnAutoRepairShips
            // 
            this.btnAutoRepairShips.Location = new System.Drawing.Point(150, 594);
            this.btnAutoRepairShips.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAutoRepairShips.Name = "btnAutoRepairShips";
            this.btnAutoRepairShips.Size = new System.Drawing.Size(150, 35);
            this.btnAutoRepairShips.TabIndex = 50;
            this.btnAutoRepairShips.Text = "Repair All";
            this.btnAutoRepairShips.UseVisualStyleBackColor = true;
            this.btnAutoRepairShips.Click += new System.EventHandler(this.btnAutoRepairShips_Click);
            // 
            // btnCombat
            // 
            this.btnCombat.Location = new System.Drawing.Point(0, 638);
            this.btnCombat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCombat.Name = "btnCombat";
            this.btnCombat.Size = new System.Drawing.Size(150, 35);
            this.btnCombat.TabIndex = 49;
            this.btnCombat.Text = "Combat";
            this.btnCombat.UseVisualStyleBackColor = true;
            this.btnCombat.Click += new System.EventHandler(this.btnCombat_Click);
            // 
            // btnInvasion
            // 
            this.btnInvasion.Location = new System.Drawing.Point(150, 638);
            this.btnInvasion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnInvasion.Name = "btnInvasion";
            this.btnInvasion.Size = new System.Drawing.Size(150, 35);
            this.btnInvasion.TabIndex = 48;
            this.btnInvasion.Text = "Invasion";
            this.btnInvasion.UseVisualStyleBackColor = true;
            this.btnInvasion.Click += new System.EventHandler(this.btnInvasion_Click);
            // 
            // btnShowMoves
            // 
            this.btnShowMoves.Location = new System.Drawing.Point(0, 683);
            this.btnShowMoves.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnShowMoves.Name = "btnShowMoves";
            this.btnShowMoves.Size = new System.Drawing.Size(150, 35);
            this.btnShowMoves.TabIndex = 46;
            this.btnShowMoves.Text = "Enemy Moves";
            this.btnShowMoves.UseVisualStyleBackColor = true;
            this.btnShowMoves.Click += new System.EventHandler(this.btnShowMoves_Click);
            // 
            // btnCostCalc
            // 
            this.btnCostCalc.Location = new System.Drawing.Point(150, 683);
            this.btnCostCalc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCostCalc.Name = "btnCostCalc";
            this.btnCostCalc.Size = new System.Drawing.Size(150, 35);
            this.btnCostCalc.TabIndex = 52;
            this.btnCostCalc.Text = "Ship Costs";
            this.btnCostCalc.UseVisualStyleBackColor = true;
            this.btnCostCalc.Click += new System.EventHandler(this.btnCostCalc_Click);
            // 
            // btnGraphs
            // 
            this.btnGraphs.Location = new System.Drawing.Point(0, 772);
            this.btnGraphs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnGraphs.Name = "btnGraphs";
            this.btnGraphs.Size = new System.Drawing.Size(300, 35);
            this.btnGraphs.TabIndex = 43;
            this.btnGraphs.Text = "View Empires";
            this.btnGraphs.UseVisualStyleBackColor = true;
            this.btnGraphs.Click += new System.EventHandler(this.btnGraphs_Click);
            // 
            // btnSaveGame
            // 
            this.btnSaveGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveGame.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnSaveGame.Location = new System.Drawing.Point(0, 985);
            this.btnSaveGame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSaveGame.Name = "btnSaveGame";
            this.btnSaveGame.Size = new System.Drawing.Size(300, 35);
            this.btnSaveGame.TabIndex = 1;
            this.btnSaveGame.Text = "Save Game";
            this.btnSaveGame.UseVisualStyleBackColor = true;
            this.btnSaveGame.Click += new System.EventHandler(this.btnSaveGame_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(150, 940);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            // 
            // btnEndTurn
            // 
            this.btnEndTurn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEndTurn.Location = new System.Drawing.Point(0, 1060);
            this.btnEndTurn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnEndTurn.Name = "btnEndTurn";
            this.btnEndTurn.Size = new System.Drawing.Size(300, 35);
            this.btnEndTurn.TabIndex = 0;
            this.btnEndTurn.Text = "End Turn";
            this.btnEndTurn.UseVisualStyleBackColor = true;
            this.btnEndTurn.Click += new System.EventHandler(this.btnEndTurn_Click);
            // 
            // lblPlayer
            // 
            this.lblPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPlayer.AutoEllipsis = true;
            this.lblPlayer.Location = new System.Drawing.Point(38, 1025);
            this.lblPlayer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPlayer.Name = "lblPlayer";
            this.lblPlayer.Size = new System.Drawing.Size(225, 35);
            this.lblPlayer.TabIndex = 17;
            this.lblPlayer.Text = "lblPlayer";
            this.lblPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNewGame
            // 
            this.btnNewGame.Location = new System.Drawing.Point(18, 18);
            this.btnNewGame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(112, 35);
            this.btnNewGame.TabIndex = 1;
            this.btnNewGame.Text = "New Game";
            this.btnNewGame.UseVisualStyleBackColor = true;
            this.btnNewGame.Click += new System.EventHandler(this.btnNewGame_Click);
            // 
            // btnLoadGame
            // 
            this.btnLoadGame.Location = new System.Drawing.Point(18, 63);
            this.btnLoadGame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLoadGame.Name = "btnLoadGame";
            this.btnLoadGame.Size = new System.Drawing.Size(112, 35);
            this.btnLoadGame.TabIndex = 0;
            this.btnLoadGame.Text = "Load Game";
            this.btnLoadGame.UseVisualStyleBackColor = true;
            this.btnLoadGame.Click += new System.EventHandler(this.btnLoadGame_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Gal War Save|*.gws";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Gal War Save|*.gws";
            // 
            // btnAutosaveView
            // 
            this.btnAutosaveView.Location = new System.Drawing.Point(18, 108);
            this.btnAutosaveView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAutosaveView.Name = "btnAutosaveView";
            this.btnAutosaveView.Size = new System.Drawing.Size(112, 35);
            this.btnAutosaveView.TabIndex = 3;
            this.btnAutosaveView.Text = "History";
            this.btnAutosaveView.UseVisualStyleBackColor = true;
            this.btnAutosaveView.Click += new System.EventHandler(this.btnAutosaveView_Click);
            // 
            // tbTurns
            // 
            this.tbTurns.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbTurns.LargeChange = 1;
            this.tbTurns.Location = new System.Drawing.Point(0, 1026);
            this.tbTurns.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbTurns.Maximum = 0;
            this.tbTurns.Name = "tbTurns";
            this.tbTurns.Size = new System.Drawing.Size(1101, 69);
            this.tbTurns.TabIndex = 4;
            this.tbTurns.Visible = false;
            this.tbTurns.Scroll += new System.EventHandler(this.tbTurns_Scroll);
            this.tbTurns.MouseEnter += new System.EventHandler(this.tbTurns_MouseEnter);
            this.tbTurns.MouseLeave += new System.EventHandler(this.tbTurns_MouseLeave);
            // 
            // pnlBuild
            // 
            this.pnlBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlBuild.Location = new System.Drawing.Point(0, 577);
            this.pnlBuild.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.pnlBuild.Name = "pnlBuild";
            this.pnlBuild.Size = new System.Drawing.Size(300, 354);
            this.pnlBuild.TabIndex = 53;
            this.pnlBuild.Visible = false;
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnEndTurn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnSaveGame;
            this.ClientSize = new System.Drawing.Size(1401, 1095);
            this.Controls.Add(this.tbTurns);
            this.Controls.Add(this.btnAutosaveView);
            this.Controls.Add(this.btnLoadGame);
            this.Controls.Add(this.btnNewGame);
            this.Controls.Add(this.pnlHUD);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseLeave += new System.EventHandler(this.MainForm_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseUp);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.pnlHUD.ResumeLayout(false);
            this.pnlHUD.PerformLayout();
            this.pnlInfo.ResumeLayout(false);
            this.pnlEconomy.ResumeLayout(false);
            this.pnlEconomy.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbTurns)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlHUD;
        private System.Windows.Forms.Label lbl1Inf;
        private System.Windows.Forms.Label lblBottom;
        private System.Windows.Forms.Label lbl6;
        private System.Windows.Forms.Label lbl5;
        private System.Windows.Forms.Label lbl4;
        private System.Windows.Forms.Label lbl3;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Label lblTop;
        private System.Windows.Forms.Label lbl6Inf;
        private System.Windows.Forms.Label lbl5Inf;
        private System.Windows.Forms.Label lbl4Inf;
        private System.Windows.Forms.Label lbl3Inf;
        private System.Windows.Forms.Label lbl2Inf;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lblPlayer;
        private System.Windows.Forms.Button btnEndTurn;
        private System.Windows.Forms.Label lblGold;
        private System.Windows.Forms.Label lblPopulation;
        private System.Windows.Forms.Button btnDisband;
        private System.Windows.Forms.Button btnSaveGame;
        private System.Windows.Forms.Button btnGoldRepair;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblGoldInc;
        private System.Windows.Forms.Label lblPopInc;
        private System.Windows.Forms.Label lblPlayerResearch;
        private System.Windows.Forms.Label lblResearch;
        private System.Windows.Forms.Label lblProduction;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chkGold;
        private System.Windows.Forms.CheckBox chkResearch;
        private System.Windows.Forms.CheckBox chkProduction;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbl7Inf;
        private System.Windows.Forms.Label lbl7;
        //private System.Windows.Forms.Button btnProdRepair;
        private System.Windows.Forms.Button btnNewGame;
        private System.Windows.Forms.Button btnLoadGame;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnGraphs;
        private System.Windows.Forms.Button btnProduction;
        private System.Windows.Forms.Panel pnlEconomy;
        private System.Windows.Forms.Button btnShowMoves;
        private System.Windows.Forms.Button btnCombat;
        private System.Windows.Forms.Button btnInvasion;
        private System.Windows.Forms.Button btnAutosaveView;
        private System.Windows.Forms.TrackBar tbTurns;
        private System.Windows.Forms.Button btnAutoRepairShips;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnCostCalc;
        private BuildableControl pnlBuild;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Label lblLoc;
        private System.Windows.Forms.Label lblProdTot;
        private System.Windows.Forms.Button btnColonies;
        private System.Windows.Forms.Button btnShips;
        private System.Windows.Forms.Label lblNext;
        private System.Windows.Forms.Label lblPrev;
        private System.Windows.Forms.Label lblRsrchPct;
    }
}

