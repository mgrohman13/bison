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
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.btnCostCalc = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnAutoRepairShips = new System.Windows.Forms.Button();
            this.btnCombat = new System.Windows.Forms.Button();
            this.btnInvasion = new System.Windows.Forms.Button();
            this.btnShowMoves = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.lblGold = new System.Windows.Forms.Label();
            this.lblPopulation = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblGoldInc = new System.Windows.Forms.Label();
            this.lblPopInc = new System.Windows.Forms.Label();
            this.lblRsrchPct = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblResearch = new System.Windows.Forms.Label();
            this.chkProduction = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chkResearch = new System.Windows.Forms.CheckBox();
            this.lblProduction = new System.Windows.Forms.Label();
            this.chkGold = new System.Windows.Forms.CheckBox();
            this.btnProduction = new System.Windows.Forms.Button();
            this.btnProdRepair = new System.Windows.Forms.Button();
            this.btnGraphs = new System.Windows.Forms.Button();
            this.lbl7Inf = new System.Windows.Forms.Label();
            this.lbl7 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGoldRepair = new System.Windows.Forms.Button();
            this.btnSaveGame = new System.Windows.Forms.Button();
            this.btnDisband = new System.Windows.Forms.Button();
            this.btnEndTurn = new System.Windows.Forms.Button();
            this.lblPlayer = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.lbl6Inf = new System.Windows.Forms.Label();
            this.lbl5Inf = new System.Windows.Forms.Label();
            this.lbl4Inf = new System.Windows.Forms.Label();
            this.lbl3Inf = new System.Windows.Forms.Label();
            this.lbl2Inf = new System.Windows.Forms.Label();
            this.lbl1Inf = new System.Windows.Forms.Label();
            this.lblBottom = new System.Windows.Forms.Label();
            this.lbl6 = new System.Windows.Forms.Label();
            this.lbl5 = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.lbl3 = new System.Windows.Forms.Label();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lblTop = new System.Windows.Forms.Label();
            this.pnlBuild = new GalWarWin.BuildableControl();
            this.btnNewGame = new System.Windows.Forms.Button();
            this.btnLoadGame = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnAutosaveView = new System.Windows.Forms.Button();
            this.tbTurns = new System.Windows.Forms.TrackBar();
            this.pnlInfo.SuspendLayout();
            this.panel1.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.tbTurns ) ).BeginInit();
            this.SuspendLayout();
            // 
            // pnlInfo
            // 
            this.pnlInfo.Controls.Add(this.btnCostCalc);
            this.pnlInfo.Controls.Add(this.btnUndo);
            this.pnlInfo.Controls.Add(this.btnAutoRepairShips);
            this.pnlInfo.Controls.Add(this.btnCombat);
            this.pnlInfo.Controls.Add(this.btnInvasion);
            this.pnlInfo.Controls.Add(this.btnShowMoves);
            this.pnlInfo.Controls.Add(this.panel1);
            this.pnlInfo.Controls.Add(this.btnProduction);
            this.pnlInfo.Controls.Add(this.btnProdRepair);
            this.pnlInfo.Controls.Add(this.btnGraphs);
            this.pnlInfo.Controls.Add(this.lbl7Inf);
            this.pnlInfo.Controls.Add(this.lbl7);
            this.pnlInfo.Controls.Add(this.btnCancel);
            this.pnlInfo.Controls.Add(this.btnGoldRepair);
            this.pnlInfo.Controls.Add(this.btnSaveGame);
            this.pnlInfo.Controls.Add(this.btnDisband);
            this.pnlInfo.Controls.Add(this.btnEndTurn);
            this.pnlInfo.Controls.Add(this.lblPlayer);
            this.pnlInfo.Controls.Add(this.lbl1);
            this.pnlInfo.Controls.Add(this.lbl6Inf);
            this.pnlInfo.Controls.Add(this.lbl5Inf);
            this.pnlInfo.Controls.Add(this.lbl4Inf);
            this.pnlInfo.Controls.Add(this.lbl3Inf);
            this.pnlInfo.Controls.Add(this.lbl2Inf);
            this.pnlInfo.Controls.Add(this.lbl1Inf);
            this.pnlInfo.Controls.Add(this.lblBottom);
            this.pnlInfo.Controls.Add(this.lbl6);
            this.pnlInfo.Controls.Add(this.lbl5);
            this.pnlInfo.Controls.Add(this.lbl4);
            this.pnlInfo.Controls.Add(this.lbl3);
            this.pnlInfo.Controls.Add(this.lbl2);
            this.pnlInfo.Controls.Add(this.lblTop);
            this.pnlInfo.Controls.Add(this.pnlBuild);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlInfo.Location = new System.Drawing.Point(1064, 0);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(200, 862);
            this.pnlInfo.TabIndex = 2;
            this.pnlInfo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseMove);
            // 
            // btnCostCalc
            // 
            this.btnCostCalc.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnCostCalc.Location = new System.Drawing.Point(100, 519);
            this.btnCostCalc.Name = "btnCostCalc";
            this.btnCostCalc.Size = new System.Drawing.Size(100, 23);
            this.btnCostCalc.TabIndex = 52;
            this.btnCostCalc.Text = "Ship Costs";
            this.btnCostCalc.UseVisualStyleBackColor = true;
            this.btnCostCalc.Click += new System.EventHandler(this.btnCostCalc_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnUndo.Location = new System.Drawing.Point(0, 461);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(100, 23);
            this.btnUndo.TabIndex = 51;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnAutoRepairShips
            // 
            this.btnAutoRepairShips.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnAutoRepairShips.Location = new System.Drawing.Point(100, 461);
            this.btnAutoRepairShips.Name = "btnAutoRepairShips";
            this.btnAutoRepairShips.Size = new System.Drawing.Size(100, 23);
            this.btnAutoRepairShips.TabIndex = 50;
            this.btnAutoRepairShips.Text = "Repair All";
            this.btnAutoRepairShips.UseVisualStyleBackColor = true;
            this.btnAutoRepairShips.Click += new System.EventHandler(this.btnAutoRepairShips_Click);
            // 
            // btnCombat
            // 
            this.btnCombat.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnCombat.Location = new System.Drawing.Point(0, 490);
            this.btnCombat.Name = "btnCombat";
            this.btnCombat.Size = new System.Drawing.Size(100, 23);
            this.btnCombat.TabIndex = 49;
            this.btnCombat.Text = "Combat";
            this.btnCombat.UseVisualStyleBackColor = true;
            this.btnCombat.Click += new System.EventHandler(this.btnCombat_Click);
            // 
            // btnInvasion
            // 
            this.btnInvasion.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnInvasion.Location = new System.Drawing.Point(100, 490);
            this.btnInvasion.Name = "btnInvasion";
            this.btnInvasion.Size = new System.Drawing.Size(100, 23);
            this.btnInvasion.TabIndex = 48;
            this.btnInvasion.Text = "Invasion";
            this.btnInvasion.UseVisualStyleBackColor = true;
            this.btnInvasion.Click += new System.EventHandler(this.btnInvasion_Click);
            // 
            // btnShowMoves
            // 
            this.btnShowMoves.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnShowMoves.Location = new System.Drawing.Point(0, 519);
            this.btnShowMoves.Name = "btnShowMoves";
            this.btnShowMoves.Size = new System.Drawing.Size(100, 23);
            this.btnShowMoves.TabIndex = 46;
            this.btnShowMoves.Text = "Enemy Moves";
            this.btnShowMoves.UseVisualStyleBackColor = true;
            this.btnShowMoves.Click += new System.EventHandler(this.btnShowMoves_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.lblGold);
            this.panel1.Controls.Add(this.lblPopulation);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.lblGoldInc);
            this.panel1.Controls.Add(this.lblPopInc);
            this.panel1.Controls.Add(this.lblRsrchPct);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.lblResearch);
            this.panel1.Controls.Add(this.chkProduction);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.chkResearch);
            this.panel1.Controls.Add(this.lblProduction);
            this.panel1.Controls.Add(this.chkGold);
            this.panel1.Location = new System.Drawing.Point(4, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(193, 92);
            this.panel1.TabIndex = 45;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(24, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(58, 23);
            this.label12.TabIndex = 25;
            this.label12.Text = "Population";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblGold
            // 
            this.lblGold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblGold.Location = new System.Drawing.Point(88, 23);
            this.lblGold.Name = "lblGold";
            this.lblGold.Size = new System.Drawing.Size(53, 23);
            this.lblGold.TabIndex = 19;
            this.lblGold.Text = "99999.9";
            this.lblGold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGold.Click += new System.EventHandler(this.lblGold_Click);
            // 
            // lblPopulation
            // 
            this.lblPopulation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblPopulation.Location = new System.Drawing.Point(88, 0);
            this.lblPopulation.Name = "lblPopulation";
            this.lblPopulation.Size = new System.Drawing.Size(42, 23);
            this.lblPopulation.TabIndex = 21;
            this.lblPopulation.Text = "99999";
            this.lblPopulation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.Location = new System.Drawing.Point(23, 24);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 20);
            this.label8.TabIndex = 26;
            this.label8.Text = "Gold";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label8.Click += new System.EventHandler(this.lblGold_Click);
            // 
            // lblGoldInc
            // 
            this.lblGoldInc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblGoldInc.Location = new System.Drawing.Point(144, 23);
            this.lblGoldInc.Name = "lblGoldInc";
            this.lblGoldInc.Size = new System.Drawing.Size(46, 23);
            this.lblGoldInc.TabIndex = 27;
            this.lblGoldInc.Text = "+999.9";
            this.lblGoldInc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGoldInc.Click += new System.EventHandler(this.lblGold_Click);
            // 
            // lblPopInc
            // 
            this.lblPopInc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblPopInc.Location = new System.Drawing.Point(144, 0);
            this.lblPopInc.Name = "lblPopInc";
            this.lblPopInc.Size = new System.Drawing.Size(35, 23);
            this.lblPopInc.TabIndex = 28;
            this.lblPopInc.Text = "+999";
            this.lblPopInc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRsrchPct
            // 
            this.lblRsrchPct.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblRsrchPct.Location = new System.Drawing.Point(88, 46);
            this.lblRsrchPct.Name = "lblRsrchPct";
            this.lblRsrchPct.Size = new System.Drawing.Size(53, 23);
            this.lblRsrchPct.TabIndex = 38;
            this.lblRsrchPct.Text = "99%";
            this.lblRsrchPct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(24, 46);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(58, 23);
            this.label9.TabIndex = 30;
            this.label9.Text = "Research";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResearch
            // 
            this.lblResearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblResearch.Location = new System.Drawing.Point(144, 46);
            this.lblResearch.Name = "lblResearch";
            this.lblResearch.Size = new System.Drawing.Size(35, 23);
            this.lblResearch.TabIndex = 31;
            this.lblResearch.Text = "+999";
            this.lblResearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkProduction
            // 
            this.chkProduction.AutoSize = true;
            this.chkProduction.Location = new System.Drawing.Point(3, 74);
            this.chkProduction.Name = "chkProduction";
            this.chkProduction.Size = new System.Drawing.Size(15, 14);
            this.chkProduction.TabIndex = 36;
            this.chkProduction.UseVisualStyleBackColor = true;
            this.chkProduction.CheckedChanged += new System.EventHandler(this.chkEmphasis_CheckedChanged);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(24, 69);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 23);
            this.label11.TabIndex = 32;
            this.label11.Text = "Production";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkResearch
            // 
            this.chkResearch.AutoSize = true;
            this.chkResearch.Location = new System.Drawing.Point(3, 51);
            this.chkResearch.Name = "chkResearch";
            this.chkResearch.Size = new System.Drawing.Size(15, 14);
            this.chkResearch.TabIndex = 35;
            this.chkResearch.UseVisualStyleBackColor = true;
            this.chkResearch.CheckedChanged += new System.EventHandler(this.chkEmphasis_CheckedChanged);
            // 
            // lblProduction
            // 
            this.lblProduction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblProduction.Location = new System.Drawing.Point(144, 69);
            this.lblProduction.Name = "lblProduction";
            this.lblProduction.Size = new System.Drawing.Size(35, 23);
            this.lblProduction.TabIndex = 33;
            this.lblProduction.Text = "+999";
            this.lblProduction.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkGold
            // 
            this.chkGold.AutoSize = true;
            this.chkGold.Location = new System.Drawing.Point(3, 28);
            this.chkGold.Name = "chkGold";
            this.chkGold.Size = new System.Drawing.Size(15, 14);
            this.chkGold.TabIndex = 34;
            this.chkGold.UseVisualStyleBackColor = true;
            this.chkGold.CheckedChanged += new System.EventHandler(this.chkEmphasis_CheckedChanged);
            // 
            // btnProduction
            // 
            this.btnProduction.Location = new System.Drawing.Point(0, 307);
            this.btnProduction.Name = "btnProduction";
            this.btnProduction.Size = new System.Drawing.Size(200, 23);
            this.btnProduction.TabIndex = 0;
            this.btnProduction.Text = "Production";
            this.btnProduction.UseVisualStyleBackColor = true;
            this.btnProduction.Click += new System.EventHandler(this.btnProduction_Click);
            // 
            // btnProdRepair
            // 
            this.btnProdRepair.Location = new System.Drawing.Point(0, 336);
            this.btnProdRepair.Name = "btnProdRepair";
            this.btnProdRepair.Size = new System.Drawing.Size(200, 23);
            this.btnProdRepair.TabIndex = 1;
            this.btnProdRepair.Text = "Repair Ship";
            this.btnProdRepair.UseVisualStyleBackColor = true;
            this.btnProdRepair.Click += new System.EventHandler(this.btnProdRepair_Click);
            // 
            // btnGraphs
            // 
            this.btnGraphs.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnGraphs.Location = new System.Drawing.Point(0, 548);
            this.btnGraphs.Name = "btnGraphs";
            this.btnGraphs.Size = new System.Drawing.Size(200, 23);
            this.btnGraphs.TabIndex = 43;
            this.btnGraphs.Text = "View Empires";
            this.btnGraphs.UseVisualStyleBackColor = true;
            this.btnGraphs.Click += new System.EventHandler(this.btnGraphs_Click);
            // 
            // lbl7Inf
            // 
            this.lbl7Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl7Inf.Location = new System.Drawing.Point(74, 281);
            this.lbl7Inf.Name = "lbl7Inf";
            this.lbl7Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl7Inf.TabIndex = 40;
            this.lbl7Inf.Text = "lblInf7";
            this.lbl7Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl7
            // 
            this.lbl7.Location = new System.Drawing.Point(8, 282);
            this.lbl7.Name = "lbl7";
            this.lbl7.Size = new System.Drawing.Size(60, 20);
            this.lbl7.TabIndex = 39;
            this.lbl7.Text = "label7";
            this.lbl7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(100, 790);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            // 
            // btnGoldRepair
            // 
            this.btnGoldRepair.Location = new System.Drawing.Point(0, 335);
            this.btnGoldRepair.Name = "btnGoldRepair";
            this.btnGoldRepair.Size = new System.Drawing.Size(200, 23);
            this.btnGoldRepair.TabIndex = 23;
            this.btnGoldRepair.Text = "Repair";
            this.btnGoldRepair.UseVisualStyleBackColor = true;
            this.btnGoldRepair.Click += new System.EventHandler(this.btnGoldRepair_Click);
            // 
            // btnSaveGame
            // 
            this.btnSaveGame.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnSaveGame.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnSaveGame.Location = new System.Drawing.Point(0, 767);
            this.btnSaveGame.Name = "btnSaveGame";
            this.btnSaveGame.Size = new System.Drawing.Size(200, 23);
            this.btnSaveGame.TabIndex = 1;
            this.btnSaveGame.Text = "Save Game";
            this.btnSaveGame.UseVisualStyleBackColor = true;
            this.btnSaveGame.Click += new System.EventHandler(this.btnSaveGame_Click);
            // 
            // btnDisband
            // 
            this.btnDisband.Location = new System.Drawing.Point(0, 364);
            this.btnDisband.Name = "btnDisband";
            this.btnDisband.Size = new System.Drawing.Size(200, 23);
            this.btnDisband.TabIndex = 21;
            this.btnDisband.Text = "Disband Ship";
            this.btnDisband.UseVisualStyleBackColor = true;
            this.btnDisband.Click += new System.EventHandler(this.btnDisband_Click);
            // 
            // btnEndTurn
            // 
            this.btnEndTurn.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnEndTurn.Location = new System.Drawing.Point(0, 839);
            this.btnEndTurn.Name = "btnEndTurn";
            this.btnEndTurn.Size = new System.Drawing.Size(200, 23);
            this.btnEndTurn.TabIndex = 0;
            this.btnEndTurn.Text = "End Turn";
            this.btnEndTurn.UseVisualStyleBackColor = true;
            this.btnEndTurn.Click += new System.EventHandler(this.btnEndTurn_Click);
            // 
            // lblPlayer
            // 
            this.lblPlayer.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.lblPlayer.Location = new System.Drawing.Point(0, 816);
            this.lblPlayer.Name = "lblPlayer";
            this.lblPlayer.Size = new System.Drawing.Size(200, 23);
            this.lblPlayer.TabIndex = 17;
            this.lblPlayer.Text = "lblPlayer";
            this.lblPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl1
            // 
            this.lbl1.Location = new System.Drawing.Point(8, 144);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(60, 20);
            this.lbl1.TabIndex = 16;
            this.lbl1.Text = "label1";
            this.lbl1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl6Inf
            // 
            this.lbl6Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl6Inf.Location = new System.Drawing.Point(74, 258);
            this.lbl6Inf.Name = "lbl6Inf";
            this.lbl6Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl6Inf.TabIndex = 15;
            this.lbl6Inf.Text = "lblInf6";
            this.lbl6Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl5Inf
            // 
            this.lbl5Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl5Inf.Location = new System.Drawing.Point(74, 235);
            this.lbl5Inf.Name = "lbl5Inf";
            this.lbl5Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl5Inf.TabIndex = 14;
            this.lbl5Inf.Text = "lblInf5";
            this.lbl5Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl5Inf.Click += new System.EventHandler(this.lbl5_Click);
            // 
            // lbl4Inf
            // 
            this.lbl4Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl4Inf.Location = new System.Drawing.Point(74, 212);
            this.lbl4Inf.Name = "lbl4Inf";
            this.lbl4Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl4Inf.TabIndex = 13;
            this.lbl4Inf.Text = "lblInf4";
            this.lbl4Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl4Inf.Click += new System.EventHandler(this.lbl4_Click);
            // 
            // lbl3Inf
            // 
            this.lbl3Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl3Inf.Location = new System.Drawing.Point(74, 189);
            this.lbl3Inf.Name = "lbl3Inf";
            this.lbl3Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl3Inf.TabIndex = 12;
            this.lbl3Inf.Text = "lblInf3";
            this.lbl3Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl2Inf
            // 
            this.lbl2Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl2Inf.Location = new System.Drawing.Point(74, 166);
            this.lbl2Inf.Name = "lbl2Inf";
            this.lbl2Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl2Inf.TabIndex = 11;
            this.lbl2Inf.Text = "lblInf2";
            this.lbl2Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl1Inf
            // 
            this.lbl1Inf.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbl1Inf.Location = new System.Drawing.Point(74, 143);
            this.lbl1Inf.Name = "lbl1Inf";
            this.lbl1Inf.Size = new System.Drawing.Size(126, 23);
            this.lbl1Inf.TabIndex = 10;
            this.lbl1Inf.Text = "lblInf1";
            this.lbl1Inf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBottom
            // 
            this.lblBottom.Location = new System.Drawing.Point(0, 304);
            this.lblBottom.Name = "lblBottom";
            this.lblBottom.Size = new System.Drawing.Size(200, 23);
            this.lblBottom.TabIndex = 9;
            this.lblBottom.Text = "lblBottom";
            this.lblBottom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl6
            // 
            this.lbl6.Location = new System.Drawing.Point(8, 259);
            this.lbl6.Name = "lbl6";
            this.lbl6.Size = new System.Drawing.Size(60, 20);
            this.lbl6.TabIndex = 7;
            this.lbl6.Text = "Experience";
            this.lbl6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl5
            // 
            this.lbl5.Location = new System.Drawing.Point(8, 236);
            this.lbl5.Name = "lbl5";
            this.lbl5.Size = new System.Drawing.Size(60, 20);
            this.lbl5.TabIndex = 6;
            this.lbl5.Text = "label5";
            this.lbl5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl5.Click += new System.EventHandler(this.lbl5_Click);
            // 
            // lbl4
            // 
            this.lbl4.Location = new System.Drawing.Point(8, 213);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(60, 20);
            this.lbl4.TabIndex = 5;
            this.lbl4.Text = "label4";
            this.lbl4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl4.Click += new System.EventHandler(this.lbl4_Click);
            // 
            // lbl3
            // 
            this.lbl3.Location = new System.Drawing.Point(8, 190);
            this.lbl3.Name = "lbl3";
            this.lbl3.Size = new System.Drawing.Size(60, 20);
            this.lbl3.TabIndex = 4;
            this.lbl3.Text = "label3";
            this.lbl3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl2
            // 
            this.lbl2.Location = new System.Drawing.Point(8, 167);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(60, 20);
            this.lbl2.TabIndex = 3;
            this.lbl2.Text = "label2";
            this.lbl2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTop
            // 
            this.lblTop.Location = new System.Drawing.Point(0, 120);
            this.lblTop.Name = "lblTop";
            this.lblTop.Size = new System.Drawing.Size(200, 23);
            this.lblTop.TabIndex = 2;
            this.lblTop.Text = "lblTop";
            this.lblTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlBuild
            // 
            this.pnlBuild.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.pnlBuild.Location = new System.Drawing.Point(0, 577);
            this.pnlBuild.Name = "pnlBuild";
            this.pnlBuild.Size = new System.Drawing.Size(200, 207);
            this.pnlBuild.TabIndex = 44;
            this.pnlBuild.Visible = false;
            // 
            // btnNewGame
            // 
            this.btnNewGame.Location = new System.Drawing.Point(12, 12);
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(75, 23);
            this.btnNewGame.TabIndex = 1;
            this.btnNewGame.Text = "New Game";
            this.btnNewGame.UseVisualStyleBackColor = true;
            this.btnNewGame.Click += new System.EventHandler(this.btnNewGame_Click);
            // 
            // btnLoadGame
            // 
            this.btnLoadGame.Location = new System.Drawing.Point(12, 41);
            this.btnLoadGame.Name = "btnLoadGame";
            this.btnLoadGame.Size = new System.Drawing.Size(75, 23);
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
            this.btnAutosaveView.Location = new System.Drawing.Point(12, 70);
            this.btnAutosaveView.Name = "btnAutosaveView";
            this.btnAutosaveView.Size = new System.Drawing.Size(75, 23);
            this.btnAutosaveView.TabIndex = 3;
            this.btnAutosaveView.Text = "History";
            this.btnAutosaveView.UseVisualStyleBackColor = true;
            this.btnAutosaveView.Click += new System.EventHandler(this.btnAutosaveView_Click);
            // 
            // tbTurns
            // 
            this.tbTurns.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbTurns.LargeChange = 1;
            this.tbTurns.Location = new System.Drawing.Point(0, 817);
            this.tbTurns.Maximum = 0;
            this.tbTurns.Name = "tbTurns";
            this.tbTurns.Size = new System.Drawing.Size(1064, 45);
            this.tbTurns.TabIndex = 4;
            this.tbTurns.Visible = false;
            this.tbTurns.Scroll += new System.EventHandler(this.tbTurns_Scroll);
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnEndTurn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnSaveGame;
            this.ClientSize = new System.Drawing.Size(1264, 862);
            this.Controls.Add(this.tbTurns);
            this.Controls.Add(this.btnAutosaveView);
            this.Controls.Add(this.btnLoadGame);
            this.Controls.Add(this.btnNewGame);
            this.Controls.Add(this.pnlInfo);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameForm_FormClosing);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDoubleClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseMove);
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.tbTurns ) ).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlInfo;
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
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblResearch;
        private System.Windows.Forms.Label lblProduction;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chkGold;
        private System.Windows.Forms.CheckBox chkResearch;
        private System.Windows.Forms.CheckBox chkProduction;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblRsrchPct;
        private System.Windows.Forms.Label lbl7Inf;
        private System.Windows.Forms.Label lbl7;
        private System.Windows.Forms.Button btnProdRepair;
        private System.Windows.Forms.Button btnNewGame;
        private System.Windows.Forms.Button btnLoadGame;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private GalWarWin.BuildableControl pnlBuild;
        private System.Windows.Forms.Button btnGraphs;
        private System.Windows.Forms.Button btnProduction;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnShowMoves;
        private System.Windows.Forms.Button btnCombat;
        private System.Windows.Forms.Button btnInvasion;
        private System.Windows.Forms.Button btnAutosaveView;
        private System.Windows.Forms.TrackBar tbTurns;
        private System.Windows.Forms.Button btnAutoRepairShips;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnCostCalc;
    }
}

