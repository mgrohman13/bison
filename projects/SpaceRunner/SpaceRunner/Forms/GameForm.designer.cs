namespace SpaceRunner.Forms
{
    internal partial class GameForm
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
            this.picLife = new System.Windows.Forms.PictureBox();
            this.lblLife = new System.Windows.Forms.Label();
            this.lblFuel = new System.Windows.Forms.Label();
            this.picFuel = new System.Windows.Forms.PictureBox();
            this.lblAmmo = new System.Windows.Forms.Label();
            this.picAmmo = new System.Windows.Forms.PictureBox();
            this.lblScore = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tbTime = new System.Windows.Forms.TrackBar();
            this.tbSpeed = new System.Windows.Forms.TrackBar();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            ( (System.ComponentModel.ISupportInitialize)( this.picLife ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.picFuel ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.picAmmo ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.tbTime ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.tbSpeed ) ).BeginInit();
            this.SuspendLayout();
            // 
            // picLife
            // 
            this.picLife.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.picLife.Location = new System.Drawing.Point(6, 175);
            this.picLife.Name = "picLife";
            this.picLife.Size = new System.Drawing.Size(18, 18);
            this.picLife.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLife.TabIndex = 1;
            this.picLife.TabStop = false;
            // 
            // lblLife
            // 
            this.lblLife.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.lblLife.AutoSize = true;
            this.lblLife.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblLife.Location = new System.Drawing.Point(24, 174);
            this.lblLife.Name = "lblLife";
            this.lblLife.Size = new System.Drawing.Size(18, 19);
            this.lblLife.TabIndex = 2;
            this.lblLife.Text = "0";
            // 
            // lblFuel
            // 
            this.lblFuel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.lblFuel.AutoSize = true;
            this.lblFuel.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblFuel.Location = new System.Drawing.Point(24, 199);
            this.lblFuel.Name = "lblFuel";
            this.lblFuel.Size = new System.Drawing.Size(18, 19);
            this.lblFuel.TabIndex = 4;
            this.lblFuel.Text = "0";
            // 
            // picFuel
            // 
            this.picFuel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.picFuel.Location = new System.Drawing.Point(6, 199);
            this.picFuel.Name = "picFuel";
            this.picFuel.Size = new System.Drawing.Size(18, 18);
            this.picFuel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picFuel.TabIndex = 3;
            this.picFuel.TabStop = false;
            // 
            // lblAmmo
            // 
            this.lblAmmo.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.lblAmmo.AutoSize = true;
            this.lblAmmo.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblAmmo.Location = new System.Drawing.Point(24, 223);
            this.lblAmmo.Name = "lblAmmo";
            this.lblAmmo.Size = new System.Drawing.Size(18, 19);
            this.lblAmmo.TabIndex = 6;
            this.lblAmmo.Text = "0";
            // 
            // picAmmo
            // 
            this.picAmmo.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.picAmmo.Location = new System.Drawing.Point(6, 223);
            this.picAmmo.Name = "picAmmo";
            this.picAmmo.Size = new System.Drawing.Size(18, 18);
            this.picAmmo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picAmmo.TabIndex = 5;
            this.picAmmo.TabStop = false;
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblScore.Location = new System.Drawing.Point(0, 24);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(18, 19);
            this.lblScore.TabIndex = 7;
            this.lblScore.Text = "0";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "replay.srr";
            this.openFileDialog.Filter = "Space Runner Replay|*.srr";
            this.openFileDialog.InitialDirectory = ".";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "replay.srr";
            this.saveFileDialog.Filter = "Space Runner Replay|*.srr";
            this.saveFileDialog.InitialDirectory = ".";
            // 
            // tbTime
            // 
            this.tbTime.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.tbTime.AutoSize = false;
            this.tbTime.Location = new System.Drawing.Point(81, 1);
            this.tbTime.Name = "tbTime";
            this.tbTime.Size = new System.Drawing.Size(210, 26);
            this.tbTime.TabIndex = 8;
            this.tbTime.Visible = false;
            this.tbTime.Scroll += new System.EventHandler(this.tbTime_Scroll);
            // 
            // tbSpeed
            // 
            this.tbSpeed.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.tbSpeed.AutoSize = false;
            this.tbSpeed.LargeChange = 3;
            this.tbSpeed.Location = new System.Drawing.Point(81, 30);
            this.tbSpeed.Maximum = 39;
            this.tbSpeed.Minimum = 1;
            this.tbSpeed.Name = "tbSpeed";
            this.tbSpeed.Size = new System.Drawing.Size(210, 26);
            this.tbSpeed.TabIndex = 9;
            this.tbSpeed.TickFrequency = 3;
            this.tbSpeed.Value = 13;
            this.tbSpeed.Visible = false;
            this.tbSpeed.Scroll += new System.EventHandler(this.tbSpeed_Scroll);
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(58, 7);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(30, 13);
            this.lblTime.TabIndex = 10;
            this.lblTime.Text = "Time";
            this.lblTime.Visible = false;
            // 
            // lblSpeed
            // 
            this.lblSpeed.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(50, 36);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(38, 13);
            this.lblSpeed.TabIndex = 11;
            this.lblSpeed.Text = "Speed";
            this.lblSpeed.Visible = false;
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.tbSpeed);
            this.Controls.Add(this.tbTime);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblAmmo);
            this.Controls.Add(this.picAmmo);
            this.Controls.Add(this.lblFuel);
            this.Controls.Add(this.picFuel);
            this.Controls.Add(this.lblLife);
            this.Controls.Add(this.picLife);
            this.Name = "GameForm";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseDown);
            this.MouseLeave += new System.EventHandler(this.GameForm_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseUp);
            this.Controls.SetChildIndex(this.picLife, 0);
            this.Controls.SetChildIndex(this.lblLife, 0);
            this.Controls.SetChildIndex(this.picFuel, 0);
            this.Controls.SetChildIndex(this.lblFuel, 0);
            this.Controls.SetChildIndex(this.picAmmo, 0);
            this.Controls.SetChildIndex(this.lblAmmo, 0);
            this.Controls.SetChildIndex(this.lblScore, 0);
            this.Controls.SetChildIndex(this.tbTime, 0);
            this.Controls.SetChildIndex(this.tbSpeed, 0);
            this.Controls.SetChildIndex(this.lblTime, 0);
            this.Controls.SetChildIndex(this.lblSpeed, 0);
            ( (System.ComponentModel.ISupportInitialize)( this.picLife ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.picFuel ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.picAmmo ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.tbTime ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.tbSpeed ) ).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picLife;
        private System.Windows.Forms.Label lblLife;
        private System.Windows.Forms.Label lblFuel;
        private System.Windows.Forms.PictureBox picFuel;
        private System.Windows.Forms.Label lblAmmo;
        private System.Windows.Forms.PictureBox picAmmo;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.TrackBar tbTime;
        private System.Windows.Forms.TrackBar tbSpeed;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblSpeed;
    }
}

