namespace CityWarWinApp
{
    partial class Map
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
            this.components = new System.ComponentModel.Container();
            this.timerGraphics = new System.Windows.Forms.Timer(this.components);
            this.lblDiv = new System.Windows.Forms.Label();
            this.lblMouse = new System.Windows.Forms.Label();
            this.btnEndTurn = new System.Windows.Forms.Button();
            this.lblPlayer = new System.Windows.Forms.Label();
            this.lblProd = new System.Windows.Forms.Label();
            this.lblWork = new System.Windows.Forms.Label();
            this.lblDeath = new System.Windows.Forms.Label();
            this.lblEarth = new System.Windows.Forms.Label();
            this.lblWizard = new System.Windows.Forms.Label();
            this.lblAir = new System.Windows.Forms.Label();
            this.lblNature = new System.Windows.Forms.Label();
            this.lblWater = new System.Windows.Forms.Label();
            this.btnBuildPiece = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnRest = new System.Windows.Forms.Button();
            this.btnCaptureCity = new System.Windows.Forms.Button();
            this.lblTurn = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnInfo = new System.Windows.Forms.Button();
            this.lblRelic = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.loadGame = new System.Windows.Forms.OpenFileDialog();
            this.saveGame = new System.Windows.Forms.SaveFileDialog();
            this.btnQuit = new System.Windows.Forms.Button();
            this.lblPpl = new System.Windows.Forms.Label();
            this.btnDisbandUnits = new System.Windows.Forms.Button();
            this.btnGroup = new System.Windows.Forms.Button();
            this.btnUngroup = new System.Windows.Forms.Button();
            this.chbGroup = new System.Windows.Forms.CheckBox();
            this.panelPieces = new CityWarWinApp.PiecesPanel();
            this.btnUndo = new System.Windows.Forms.Button();
            this.chbGamble = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // timerGraphics
            // 
            this.timerGraphics.Interval = 78;
            this.timerGraphics.Tick += new System.EventHandler(this.timerGraphics_Tick);
            // 
            // lblDiv
            // 
            this.lblDiv.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblDiv.BackColor = System.Drawing.Color.White;
            this.lblDiv.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblDiv.ForeColor = System.Drawing.Color.Black;
            this.lblDiv.Location = new System.Drawing.Point(545, 0);
            this.lblDiv.Name = "lblDiv";
            this.lblDiv.Size = new System.Drawing.Size(3, 600);
            this.lblDiv.TabIndex = 1;
            // 
            // lblMouse
            // 
            this.lblMouse.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblMouse.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblMouse.ForeColor = System.Drawing.Color.White;
            this.lblMouse.Location = new System.Drawing.Point(554, 210);
            this.lblMouse.Name = "lblMouse";
            this.lblMouse.Size = new System.Drawing.Size(86, 21);
            this.lblMouse.TabIndex = 2;
            this.lblMouse.Text = "0,0";
            this.lblMouse.Visible = false;
            // 
            // btnEndTurn
            // 
            this.btnEndTurn.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnEndTurn.BackColor = System.Drawing.Color.Silver;
            this.btnEndTurn.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnEndTurn.ForeColor = System.Drawing.Color.Black;
            this.btnEndTurn.Location = new System.Drawing.Point(688, 549);
            this.btnEndTurn.Name = "btnEndTurn";
            this.btnEndTurn.Size = new System.Drawing.Size(100, 39);
            this.btnEndTurn.TabIndex = 13;
            this.btnEndTurn.Text = "End Turn";
            this.btnEndTurn.UseVisualStyleBackColor = false;
            this.btnEndTurn.Click += new System.EventHandler(this.btnEndTurn_Click);
            // 
            // lblPlayer
            // 
            this.lblPlayer.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblPlayer.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblPlayer.ForeColor = System.Drawing.Color.Black;
            this.lblPlayer.Location = new System.Drawing.Point(559, 549);
            this.lblPlayer.Name = "lblPlayer";
            this.lblPlayer.Size = new System.Drawing.Size(100, 39);
            this.lblPlayer.TabIndex = 6;
            this.lblPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblProd
            // 
            this.lblProd.BackColor = System.Drawing.Color.DarkOrange;
            this.lblProd.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblProd.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblProd.ForeColor = System.Drawing.Color.Black;
            this.lblProd.Location = new System.Drawing.Point(114, 9);
            this.lblProd.Name = "lblProd";
            this.lblProd.Size = new System.Drawing.Size(45, 23);
            this.lblProd.TabIndex = 7;
            this.lblProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblProd.Click += new System.EventHandler(this.lblResource_Click);
            this.lblProd.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblProd.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblWork
            // 
            this.lblWork.BackColor = System.Drawing.Color.White;
            this.lblWork.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblWork.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblWork.ForeColor = System.Drawing.Color.Black;
            this.lblWork.Location = new System.Drawing.Point(12, 9);
            this.lblWork.Name = "lblWork";
            this.lblWork.Size = new System.Drawing.Size(45, 23);
            this.lblWork.TabIndex = 8;
            this.lblWork.Text = "2222";
            this.lblWork.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWork.Click += new System.EventHandler(this.lblResource_Click);
            this.lblWork.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblWork.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblDeath
            // 
            this.lblDeath.BackColor = System.Drawing.Color.Black;
            this.lblDeath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDeath.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblDeath.ForeColor = System.Drawing.Color.White;
            this.lblDeath.Location = new System.Drawing.Point(165, 9);
            this.lblDeath.Name = "lblDeath";
            this.lblDeath.Size = new System.Drawing.Size(45, 23);
            this.lblDeath.TabIndex = 9;
            this.lblDeath.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDeath.Click += new System.EventHandler(this.lblResource_Click);
            this.lblDeath.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblDeath.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblEarth
            // 
            this.lblEarth.BackColor = System.Drawing.Color.Gold;
            this.lblEarth.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblEarth.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblEarth.ForeColor = System.Drawing.Color.Black;
            this.lblEarth.Location = new System.Drawing.Point(216, 9);
            this.lblEarth.Name = "lblEarth";
            this.lblEarth.Size = new System.Drawing.Size(45, 23);
            this.lblEarth.TabIndex = 10;
            this.lblEarth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblEarth.Click += new System.EventHandler(this.lblResource_Click);
            this.lblEarth.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblEarth.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblWizard
            // 
            this.lblWizard.BackColor = System.Drawing.Color.DeepPink;
            this.lblWizard.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblWizard.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblWizard.ForeColor = System.Drawing.Color.Black;
            this.lblWizard.Location = new System.Drawing.Point(420, 9);
            this.lblWizard.Name = "lblWizard";
            this.lblWizard.Size = new System.Drawing.Size(45, 23);
            this.lblWizard.TabIndex = 11;
            this.lblWizard.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWizard.Click += new System.EventHandler(this.lblResource_Click);
            this.lblWizard.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblWizard.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblAir
            // 
            this.lblAir.BackColor = System.Drawing.Color.Gray;
            this.lblAir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblAir.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblAir.ForeColor = System.Drawing.Color.Black;
            this.lblAir.Location = new System.Drawing.Point(369, 9);
            this.lblAir.Name = "lblAir";
            this.lblAir.Size = new System.Drawing.Size(45, 23);
            this.lblAir.TabIndex = 12;
            this.lblAir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAir.Click += new System.EventHandler(this.lblResource_Click);
            this.lblAir.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblAir.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblNature
            // 
            this.lblNature.BackColor = System.Drawing.Color.Green;
            this.lblNature.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblNature.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblNature.ForeColor = System.Drawing.Color.Black;
            this.lblNature.Location = new System.Drawing.Point(318, 9);
            this.lblNature.Name = "lblNature";
            this.lblNature.Size = new System.Drawing.Size(45, 23);
            this.lblNature.TabIndex = 13;
            this.lblNature.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblNature.Click += new System.EventHandler(this.lblResource_Click);
            this.lblNature.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblNature.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // lblWater
            // 
            this.lblWater.BackColor = System.Drawing.Color.Blue;
            this.lblWater.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblWater.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblWater.ForeColor = System.Drawing.Color.Black;
            this.lblWater.Location = new System.Drawing.Point(267, 9);
            this.lblWater.Name = "lblWater";
            this.lblWater.Size = new System.Drawing.Size(45, 23);
            this.lblWater.TabIndex = 14;
            this.lblWater.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWater.Click += new System.EventHandler(this.lblResource_Click);
            this.lblWater.MouseEnter += new System.EventHandler(this.lblResource_MouseEnter);
            this.lblWater.MouseLeave += new System.EventHandler(this.lblResource_MouseLeave);
            // 
            // btnBuildPiece
            // 
            this.btnBuildPiece.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnBuildPiece.BackColor = System.Drawing.Color.Silver;
            this.btnBuildPiece.Font = new System.Drawing.Font("Algerian", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnBuildPiece.ForeColor = System.Drawing.Color.Black;
            this.btnBuildPiece.Location = new System.Drawing.Point(554, 44);
            this.btnBuildPiece.Name = "btnBuildPiece";
            this.btnBuildPiece.Size = new System.Drawing.Size(86, 26);
            this.btnBuildPiece.TabIndex = 1;
            this.btnBuildPiece.Text = "Build";
            this.btnBuildPiece.UseVisualStyleBackColor = false;
            this.btnBuildPiece.Visible = false;
            this.btnBuildPiece.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnNext.BackColor = System.Drawing.Color.Silver;
            this.btnNext.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnNext.ForeColor = System.Drawing.Color.Black;
            this.btnNext.Location = new System.Drawing.Point(554, 121);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(86, 23);
            this.btnNext.TabIndex = 3;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnRest
            // 
            this.btnRest.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnRest.BackColor = System.Drawing.Color.Silver;
            this.btnRest.Font = new System.Drawing.Font("Algerian", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnRest.ForeColor = System.Drawing.Color.Black;
            this.btnRest.Location = new System.Drawing.Point(554, 76);
            this.btnRest.Name = "btnRest";
            this.btnRest.Size = new System.Drawing.Size(86, 26);
            this.btnRest.TabIndex = 2;
            this.btnRest.Text = "Rest";
            this.btnRest.UseVisualStyleBackColor = false;
            this.btnRest.Click += new System.EventHandler(this.btnHeal_Click);
            // 
            // btnCaptureCity
            // 
            this.btnCaptureCity.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnCaptureCity.BackColor = System.Drawing.Color.Silver;
            this.btnCaptureCity.Font = new System.Drawing.Font("Algerian", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnCaptureCity.ForeColor = System.Drawing.Color.Black;
            this.btnCaptureCity.Location = new System.Drawing.Point(554, 12);
            this.btnCaptureCity.Name = "btnCaptureCity";
            this.btnCaptureCity.Size = new System.Drawing.Size(234, 26);
            this.btnCaptureCity.TabIndex = 0;
            this.btnCaptureCity.Text = "Capture City";
            this.btnCaptureCity.UseVisualStyleBackColor = false;
            this.btnCaptureCity.Visible = false;
            this.btnCaptureCity.Click += new System.EventHandler(this.btnBuildCity_Click);
            // 
            // lblTurn
            // 
            this.lblTurn.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblTurn.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblTurn.ForeColor = System.Drawing.Color.White;
            this.lblTurn.Location = new System.Drawing.Point(558, 379);
            this.lblTurn.Name = "lblTurn";
            this.lblTurn.Size = new System.Drawing.Size(82, 18);
            this.lblTurn.TabIndex = 20;
            this.lblTurn.Text = "1";
            this.lblTurn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.label3.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(558, 361);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 18);
            this.label3.TabIndex = 22;
            this.label3.Text = "Turn";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnInfo
            // 
            this.btnInfo.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnInfo.BackColor = System.Drawing.Color.Silver;
            this.btnInfo.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnInfo.ForeColor = System.Drawing.Color.Black;
            this.btnInfo.Location = new System.Drawing.Point(558, 400);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(82, 23);
            this.btnInfo.TabIndex = 9;
            this.btnInfo.Text = "Info";
            this.btnInfo.UseVisualStyleBackColor = false;
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            // 
            // lblRelic
            // 
            this.lblRelic.BackColor = System.Drawing.Color.Cyan;
            this.lblRelic.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblRelic.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblRelic.ForeColor = System.Drawing.Color.Black;
            this.lblRelic.Location = new System.Drawing.Point(471, 9);
            this.lblRelic.Name = "lblRelic";
            this.lblRelic.Size = new System.Drawing.Size(45, 23);
            this.lblRelic.TabIndex = 25;
            this.lblRelic.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRelic.Click += new System.EventHandler(this.lblResource_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnLoad.BackColor = System.Drawing.Color.Silver;
            this.btnLoad.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnLoad.ForeColor = System.Drawing.Color.Black;
            this.btnLoad.Location = new System.Drawing.Point(558, 491);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(82, 23);
            this.btnLoad.TabIndex = 11;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = false;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnSave.BackColor = System.Drawing.Color.Silver;
            this.btnSave.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.Location = new System.Drawing.Point(558, 462);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(82, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // loadGame
            // 
            this.loadGame.Filter = "City War Save|*.cws";
            this.loadGame.InitialDirectory = "..\\..\\..\\Saves";
            this.loadGame.Title = "Load Game";
            // 
            // saveGame
            // 
            this.saveGame.Filter = "City War Save|*.cws";
            this.saveGame.InitialDirectory = "..\\..\\..\\Saves";
            this.saveGame.Title = "Save Game";
            // 
            // btnQuit
            // 
            this.btnQuit.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnQuit.BackColor = System.Drawing.Color.Silver;
            this.btnQuit.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnQuit.ForeColor = System.Drawing.Color.Black;
            this.btnQuit.Location = new System.Drawing.Point(558, 520);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(82, 23);
            this.btnQuit.TabIndex = 12;
            this.btnQuit.Text = "Quit";
            this.btnQuit.UseVisualStyleBackColor = false;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // lblPpl
            // 
            this.lblPpl.BackColor = System.Drawing.Color.Brown;
            this.lblPpl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPpl.Font = new System.Drawing.Font("Engravers MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblPpl.ForeColor = System.Drawing.Color.Black;
            this.lblPpl.Location = new System.Drawing.Point(63, 9);
            this.lblPpl.Name = "lblPpl";
            this.lblPpl.Size = new System.Drawing.Size(45, 23);
            this.lblPpl.TabIndex = 57;
            this.lblPpl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPpl.Click += new System.EventHandler(this.lblResource_Click);
            // 
            // btnDisbandUnits
            // 
            this.btnDisbandUnits.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnDisbandUnits.BackColor = System.Drawing.Color.Silver;
            this.btnDisbandUnits.Font = new System.Drawing.Font("Algerian", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnDisbandUnits.ForeColor = System.Drawing.Color.Black;
            this.btnDisbandUnits.Location = new System.Drawing.Point(554, 263);
            this.btnDisbandUnits.Name = "btnDisbandUnits";
            this.btnDisbandUnits.Size = new System.Drawing.Size(86, 23);
            this.btnDisbandUnits.TabIndex = 7;
            this.btnDisbandUnits.Text = "Disband";
            this.btnDisbandUnits.UseVisualStyleBackColor = false;
            this.btnDisbandUnits.Click += new System.EventHandler(this.btnDisband_Click);
            // 
            // btnGroup
            // 
            this.btnGroup.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnGroup.BackColor = System.Drawing.Color.Silver;
            this.btnGroup.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnGroup.ForeColor = System.Drawing.Color.Black;
            this.btnGroup.Location = new System.Drawing.Point(554, 150);
            this.btnGroup.Name = "btnGroup";
            this.btnGroup.Size = new System.Drawing.Size(86, 23);
            this.btnGroup.TabIndex = 4;
            this.btnGroup.Text = "Group";
            this.btnGroup.UseVisualStyleBackColor = false;
            this.btnGroup.Click += new System.EventHandler(this.btnGroup_Click);
            // 
            // btnUngroup
            // 
            this.btnUngroup.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnUngroup.BackColor = System.Drawing.Color.Silver;
            this.btnUngroup.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnUngroup.ForeColor = System.Drawing.Color.Black;
            this.btnUngroup.Location = new System.Drawing.Point(554, 179);
            this.btnUngroup.Name = "btnUngroup";
            this.btnUngroup.Size = new System.Drawing.Size(86, 23);
            this.btnUngroup.TabIndex = 5;
            this.btnUngroup.Text = "Ungroup";
            this.btnUngroup.UseVisualStyleBackColor = false;
            this.btnUngroup.Click += new System.EventHandler(this.btnUngroup_Click);
            // 
            // chbGroup
            // 
            this.chbGroup.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.chbGroup.Checked = true;
            this.chbGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbGroup.Font = new System.Drawing.Font("Algerian", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.chbGroup.ForeColor = System.Drawing.Color.White;
            this.chbGroup.Location = new System.Drawing.Point(554, 208);
            this.chbGroup.Name = "chbGroup";
            this.chbGroup.Size = new System.Drawing.Size(86, 23);
            this.chbGroup.TabIndex = 6;
            this.chbGroup.Text = "Group";
            this.chbGroup.UseVisualStyleBackColor = true;
            // 
            // panelPieces
            // 
            this.panelPieces.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.panelPieces.BackColor = System.Drawing.Color.White;
            this.panelPieces.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.panelPieces.ForeColor = System.Drawing.Color.Black;
            this.panelPieces.Location = new System.Drawing.Point(646, 44);
            this.panelPieces.Name = "panelPieces";
            this.panelPieces.Size = new System.Drawing.Size(142, 499);
            this.panelPieces.TabIndex = 4;
            this.panelPieces.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelPieces_MouseClick);
            this.panelPieces.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelPieces_MouseDoubleClick);
            this.panelPieces.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelPieces_MouseDown);
            this.panelPieces.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelPieces_MouseUp);
            // 
            // btnUndo
            // 
            this.btnUndo.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnUndo.BackColor = System.Drawing.Color.Silver;
            this.btnUndo.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnUndo.ForeColor = System.Drawing.Color.Black;
            this.btnUndo.Location = new System.Drawing.Point(554, 305);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(86, 23);
            this.btnUndo.TabIndex = 8;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = false;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // chbGamble
            // 
            this.chbGamble.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.chbGamble.Checked = true;
            this.chbGamble.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbGamble.Font = new System.Drawing.Font("Algerian", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.chbGamble.ForeColor = System.Drawing.Color.White;
            this.chbGamble.Location = new System.Drawing.Point(554, 234);
            this.chbGamble.Name = "chbGamble";
            this.chbGamble.Size = new System.Drawing.Size(86, 23);
            this.chbGamble.TabIndex = 58;
            this.chbGamble.Text = "Full";
            this.chbGamble.UseVisualStyleBackColor = true;
            // 
            // Map
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.chbGamble);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.chbGroup);
            this.Controls.Add(this.btnUngroup);
            this.Controls.Add(this.btnGroup);
            this.Controls.Add(this.btnDisbandUnits);
            this.Controls.Add(this.lblPpl);
            this.Controls.Add(this.btnQuit);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lblRelic);
            this.Controls.Add(this.btnInfo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTurn);
            this.Controls.Add(this.btnRest);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnBuildPiece);
            this.Controls.Add(this.lblWater);
            this.Controls.Add(this.lblNature);
            this.Controls.Add(this.lblAir);
            this.Controls.Add(this.lblWizard);
            this.Controls.Add(this.lblEarth);
            this.Controls.Add(this.lblDeath);
            this.Controls.Add(this.lblWork);
            this.Controls.Add(this.lblProd);
            this.Controls.Add(this.lblPlayer);
            this.Controls.Add(this.btnCaptureCity);
            this.Controls.Add(this.btnEndTurn);
            this.Controls.Add(this.panelPieces);
            this.Controls.Add(this.lblMouse);
            this.Controls.Add(this.lblDiv);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Map";
            this.Text = "City War";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainMap_FormClosing);
            this.Load += new System.EventHandler(this.MainMap_Load);
            this.LocationChanged += new System.EventHandler(this.MainMap_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.MainMap_SizeChanged);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainMap_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainMap_KeyUp);
            this.MouseLeave += new System.EventHandler(this.MainMap_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainMap_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainMap_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

		private CityWarWinApp.PiecesPanel panelPieces;
		private System.Windows.Forms.Timer timerGraphics;
        private System.Windows.Forms.Label lblDiv;
        private System.Windows.Forms.Label lblMouse;
        private System.Windows.Forms.Button btnEndTurn;
        private System.Windows.Forms.Label lblPlayer;
        private System.Windows.Forms.Label lblProd;
        private System.Windows.Forms.Label lblWork;
        private System.Windows.Forms.Label lblDeath;
        private System.Windows.Forms.Label lblEarth;
        private System.Windows.Forms.Label lblWizard;
        private System.Windows.Forms.Label lblAir;
        private System.Windows.Forms.Label lblNature;
        private System.Windows.Forms.Label lblWater;
        private System.Windows.Forms.Button btnBuildPiece;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnRest;
        private System.Windows.Forms.Button btnCaptureCity;
        private System.Windows.Forms.Label lblTurn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Label lblRelic;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.SaveFileDialog saveGame;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Label lblPpl;
        public System.Windows.Forms.OpenFileDialog loadGame;
		private System.Windows.Forms.Button btnDisbandUnits;
		private System.Windows.Forms.Button btnGroup;
		private System.Windows.Forms.Button btnUngroup;
		private System.Windows.Forms.CheckBox chbGroup;
		private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.CheckBox chbGamble;
    }
}