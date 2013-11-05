namespace DaemonsWinApp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.btnEndTurn = new System.Windows.Forms.Button();
            this.lblPlayer = new System.Windows.Forms.Label();
            this.lblSouls = new System.Windows.Forms.Label();
            this.lblUnit1 = new System.Windows.Forms.Label();
            this.lblUnit2 = new System.Windows.Forms.Label();
            this.lblUnit3 = new System.Windows.Forms.Label();
            this.lblUnit4 = new System.Windows.Forms.Label();
            this.lblInf4 = new System.Windows.Forms.Label();
            this.lblInf3 = new System.Windows.Forms.Label();
            this.lblInf2 = new System.Windows.Forms.Label();
            this.lblInf1 = new System.Windows.Forms.Label();
            this.lblArrows = new System.Windows.Forms.Label();
            this.btnBuild = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnFight = new System.Windows.Forms.Button();
            this.lblLog = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.lblTurn = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnPlayers = new System.Windows.Forms.Button();
            this.lblAccent = new System.Windows.Forms.Label();
            this.lblMorale = new System.Windows.Forms.Label();
            ( (System.ComponentModel.ISupportInitialize)( this.pictureBox1 ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.pictureBox3 ) ).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(741, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1, 784);
            this.label1.TabIndex = 0;
            // 
            // btnEndTurn
            // 
            this.btnEndTurn.BackColor = System.Drawing.Color.Silver;
            this.btnEndTurn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnEndTurn.Location = new System.Drawing.Point(748, 686);
            this.btnEndTurn.Name = "btnEndTurn";
            this.btnEndTurn.Size = new System.Drawing.Size(169, 30);
            this.btnEndTurn.TabIndex = 1;
            this.btnEndTurn.Text = "End Turn";
            this.btnEndTurn.UseVisualStyleBackColor = false;
            this.btnEndTurn.Click += new System.EventHandler(this.btnEndTurn_Click);
            // 
            // lblPlayer
            // 
            this.lblPlayer.BackColor = System.Drawing.Color.Black;
            this.lblPlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblPlayer.Location = new System.Drawing.Point(775, 620);
            this.lblPlayer.Name = "lblPlayer";
            this.lblPlayer.Size = new System.Drawing.Size(142, 30);
            this.lblPlayer.TabIndex = 2;
            this.lblPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSouls
            // 
            this.lblSouls.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblSouls.ForeColor = System.Drawing.Color.DarkRed;
            this.lblSouls.Location = new System.Drawing.Point(877, 650);
            this.lblSouls.Name = "lblSouls";
            this.lblSouls.Size = new System.Drawing.Size(40, 30);
            this.lblSouls.TabIndex = 3;
            this.lblSouls.Text = "0666";
            this.lblSouls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblUnit1
            // 
            this.lblUnit1.BackColor = System.Drawing.Color.White;
            this.lblUnit1.Location = new System.Drawing.Point(743, 1);
            this.lblUnit1.Name = "lblUnit1";
            this.lblUnit1.Size = new System.Drawing.Size(90, 90);
            this.lblUnit1.TabIndex = 4;
            // 
            // lblUnit2
            // 
            this.lblUnit2.BackColor = System.Drawing.Color.White;
            this.lblUnit2.Location = new System.Drawing.Point(743, 91);
            this.lblUnit2.Name = "lblUnit2";
            this.lblUnit2.Size = new System.Drawing.Size(90, 90);
            this.lblUnit2.TabIndex = 5;
            // 
            // lblUnit3
            // 
            this.lblUnit3.BackColor = System.Drawing.Color.White;
            this.lblUnit3.Location = new System.Drawing.Point(743, 181);
            this.lblUnit3.Name = "lblUnit3";
            this.lblUnit3.Size = new System.Drawing.Size(90, 90);
            this.lblUnit3.TabIndex = 6;
            // 
            // lblUnit4
            // 
            this.lblUnit4.BackColor = System.Drawing.Color.White;
            this.lblUnit4.Location = new System.Drawing.Point(743, 271);
            this.lblUnit4.Name = "lblUnit4";
            this.lblUnit4.Size = new System.Drawing.Size(90, 90);
            this.lblUnit4.TabIndex = 7;
            // 
            // lblInf4
            // 
            this.lblInf4.BackColor = System.Drawing.Color.White;
            this.lblInf4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblInf4.Location = new System.Drawing.Point(832, 271);
            this.lblInf4.Name = "lblInf4";
            this.lblInf4.Size = new System.Drawing.Size(92, 90);
            this.lblInf4.TabIndex = 11;
            this.lblInf4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInf3
            // 
            this.lblInf3.BackColor = System.Drawing.Color.White;
            this.lblInf3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblInf3.Location = new System.Drawing.Point(832, 181);
            this.lblInf3.Name = "lblInf3";
            this.lblInf3.Size = new System.Drawing.Size(92, 90);
            this.lblInf3.TabIndex = 10;
            this.lblInf3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInf2
            // 
            this.lblInf2.BackColor = System.Drawing.Color.White;
            this.lblInf2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblInf2.Location = new System.Drawing.Point(832, 91);
            this.lblInf2.Name = "lblInf2";
            this.lblInf2.Size = new System.Drawing.Size(92, 90);
            this.lblInf2.TabIndex = 9;
            this.lblInf2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInf1
            // 
            this.lblInf1.BackColor = System.Drawing.Color.White;
            this.lblInf1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblInf1.Location = new System.Drawing.Point(832, 1);
            this.lblInf1.Name = "lblInf1";
            this.lblInf1.Size = new System.Drawing.Size(92, 90);
            this.lblInf1.TabIndex = 8;
            this.lblInf1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblArrows
            // 
            this.lblArrows.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblArrows.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblArrows.Location = new System.Drawing.Point(784, 650);
            this.lblArrows.Name = "lblArrows";
            this.lblArrows.Size = new System.Drawing.Size(40, 30);
            this.lblArrows.TabIndex = 12;
            this.lblArrows.Text = "1234";
            this.lblArrows.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBuild
            // 
            this.btnBuild.BackColor = System.Drawing.Color.Silver;
            this.btnBuild.Location = new System.Drawing.Point(748, 393);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(75, 23);
            this.btnBuild.TabIndex = 13;
            this.btnBuild.Text = "Build";
            this.btnBuild.UseVisualStyleBackColor = false;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.Silver;
            this.btnNext.Location = new System.Drawing.Point(748, 364);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 14;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Silver;
            this.button1.Location = new System.Drawing.Point(829, 364);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "Stay";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnFight
            // 
            this.btnFight.BackColor = System.Drawing.Color.Silver;
            this.btnFight.Location = new System.Drawing.Point(829, 393);
            this.btnFight.Name = "btnFight";
            this.btnFight.Size = new System.Drawing.Size(75, 23);
            this.btnFight.TabIndex = 16;
            this.btnFight.Text = "Fight";
            this.btnFight.UseVisualStyleBackColor = false;
            this.btnFight.Click += new System.EventHandler(this.btnFight_Click);
            // 
            // lblLog
            // 
            this.lblLog.Location = new System.Drawing.Point(748, 419);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(169, 145);
            this.lblLog.TabIndex = 17;
            this.lblLog.Text = "123456789012345678901234567\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n0\r\n1";
            this.lblLog.Click += new System.EventHandler(this.lblLog_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ( (System.Drawing.Image)( resources.GetObject("pictureBox1.Image") ) );
            this.pictureBox1.Location = new System.Drawing.Point(841, 650);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(30, 30);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ( (System.Drawing.Image)( resources.GetObject("pictureBox3.Image") ) );
            this.pictureBox3.Location = new System.Drawing.Point(748, 650);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(30, 30);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 20;
            this.pictureBox3.TabStop = false;
            // 
            // lblTurn
            // 
            this.lblTurn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblTurn.ForeColor = System.Drawing.Color.Black;
            this.lblTurn.Location = new System.Drawing.Point(748, 620);
            this.lblTurn.Name = "lblTurn";
            this.lblTurn.Size = new System.Drawing.Size(32, 30);
            this.lblTurn.TabIndex = 21;
            this.lblTurn.Text = "222";
            this.lblTurn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Silver;
            this.button2.Location = new System.Drawing.Point(829, 565);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 23;
            this.button2.Text = "Load";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Silver;
            this.button3.Location = new System.Drawing.Point(748, 565);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 22;
            this.button3.Text = "Save";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnPlayers
            // 
            this.btnPlayers.BackColor = System.Drawing.Color.Silver;
            this.btnPlayers.Location = new System.Drawing.Point(748, 592);
            this.btnPlayers.Name = "btnPlayers";
            this.btnPlayers.Size = new System.Drawing.Size(156, 23);
            this.btnPlayers.TabIndex = 24;
            this.btnPlayers.Text = "Players";
            this.btnPlayers.UseVisualStyleBackColor = false;
            this.btnPlayers.Click += new System.EventHandler(this.btnPlayers_Click);
            // 
            // lblAccent
            // 
            this.lblAccent.BackColor = System.Drawing.Color.Black;
            this.lblAccent.ForeColor = System.Drawing.Color.Black;
            this.lblAccent.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblAccent.Location = new System.Drawing.Point(744, 588);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(164, 31);
            this.lblAccent.TabIndex = 25;
            this.lblAccent.Visible = false;
            // 
            // lblMorale
            // 
            this.lblMorale.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblMorale.BackColor = System.Drawing.Color.Transparent;
            this.lblMorale.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F);
            this.lblMorale.Location = new System.Drawing.Point(888, 0);
            this.lblMorale.Name = "lblMorale";
            this.lblMorale.Size = new System.Drawing.Size(108, 20);
            this.lblMorale.TabIndex = 26;
            this.lblMorale.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(992, 773);
            this.Controls.Add(this.lblMorale);
            this.Controls.Add(this.btnPlayers);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.lblTurn);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblLog);
            this.Controls.Add(this.btnFight);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.lblArrows);
            this.Controls.Add(this.lblInf4);
            this.Controls.Add(this.lblInf3);
            this.Controls.Add(this.lblInf2);
            this.Controls.Add(this.lblInf1);
            this.Controls.Add(this.lblUnit4);
            this.Controls.Add(this.lblUnit3);
            this.Controls.Add(this.lblUnit2);
            this.Controls.Add(this.lblUnit1);
            this.Controls.Add(this.lblSouls);
            this.Controls.Add(this.lblPlayer);
            this.Controls.Add(this.btnEndTurn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblAccent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Daemons";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mainForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.mainForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mainForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mainForm_MouseUp);
            ( (System.ComponentModel.ISupportInitialize)( this.pictureBox1 ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.pictureBox3 ) ).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEndTurn;
        private System.Windows.Forms.Label lblPlayer;
        private System.Windows.Forms.Label lblSouls;
        private System.Windows.Forms.Label lblUnit1;
        private System.Windows.Forms.Label lblUnit2;
        private System.Windows.Forms.Label lblUnit3;
        private System.Windows.Forms.Label lblUnit4;
        private System.Windows.Forms.Label lblInf4;
        private System.Windows.Forms.Label lblInf3;
        private System.Windows.Forms.Label lblInf2;
        private System.Windows.Forms.Label lblInf1;
        private System.Windows.Forms.Label lblArrows;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnFight;
		private System.Windows.Forms.Label lblLog;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label lblTurn;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnPlayers;
        private System.Windows.Forms.Label lblAccent;
        private System.Windows.Forms.Label lblMorale;
    }
}

