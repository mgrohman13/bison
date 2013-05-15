namespace CityWarWinApp
{
    partial class MainMenu
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
			this.lblNewGame = new System.Windows.Forms.Label();
			this.lblLoadGame = new System.Windows.Forms.Label();
			this.lblOptions = new System.Windows.Forms.Label();
			this.lblQuit = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblNewGame
			// 
			this.lblNewGame.BackColor = System.Drawing.Color.Black;
			this.lblNewGame.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblNewGame.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblNewGame.ForeColor = System.Drawing.Color.White;
			this.lblNewGame.Location = new System.Drawing.Point(0, 0);
			this.lblNewGame.Name = "lblNewGame";
			this.lblNewGame.Size = new System.Drawing.Size(300, 60);
			this.lblNewGame.TabIndex = 0;
			this.lblNewGame.Text = "New Game";
			this.lblNewGame.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblNewGame.MouseLeave += new System.EventHandler(this.menuItem_MouseLeave);
			this.lblNewGame.Click += new System.EventHandler(this.lblNewGame_Click);
			this.lblNewGame.MouseEnter += new System.EventHandler(this.menuItem_MouseEnter);
			// 
			// lblLoadGame
			// 
			this.lblLoadGame.BackColor = System.Drawing.Color.Black;
			this.lblLoadGame.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblLoadGame.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblLoadGame.ForeColor = System.Drawing.Color.White;
			this.lblLoadGame.Location = new System.Drawing.Point(0, 60);
			this.lblLoadGame.Name = "lblLoadGame";
			this.lblLoadGame.Size = new System.Drawing.Size(300, 60);
			this.lblLoadGame.TabIndex = 1;
			this.lblLoadGame.Text = "Load Game";
			this.lblLoadGame.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblLoadGame.MouseLeave += new System.EventHandler(this.menuItem_MouseLeave);
			this.lblLoadGame.Click += new System.EventHandler(this.lblLoadGame_Click);
			this.lblLoadGame.MouseEnter += new System.EventHandler(this.menuItem_MouseEnter);
			// 
			// lblOptions
			// 
			this.lblOptions.BackColor = System.Drawing.Color.Black;
			this.lblOptions.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblOptions.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblOptions.ForeColor = System.Drawing.Color.White;
			this.lblOptions.Location = new System.Drawing.Point(0, 120);
			this.lblOptions.Name = "lblOptions";
			this.lblOptions.Size = new System.Drawing.Size(300, 60);
			this.lblOptions.TabIndex = 2;
			this.lblOptions.Text = "Options";
			this.lblOptions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblOptions.MouseLeave += new System.EventHandler(this.menuItem_MouseLeave);
			this.lblOptions.Click += new System.EventHandler(this.lblOptions_Click);
			this.lblOptions.MouseEnter += new System.EventHandler(this.menuItem_MouseEnter);
			// 
			// lblQuit
			// 
			this.lblQuit.BackColor = System.Drawing.Color.Black;
			this.lblQuit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblQuit.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblQuit.ForeColor = System.Drawing.Color.White;
			this.lblQuit.Location = new System.Drawing.Point(0, 180);
			this.lblQuit.Name = "lblQuit";
			this.lblQuit.Size = new System.Drawing.Size(300, 60);
			this.lblQuit.TabIndex = 3;
			this.lblQuit.Text = "Quit";
			this.lblQuit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblQuit.MouseLeave += new System.EventHandler(this.menuItem_MouseLeave);
			this.lblQuit.Click += new System.EventHandler(this.lblQuit_Click);
			this.lblQuit.MouseEnter += new System.EventHandler(this.menuItem_MouseEnter);
			// 
			// MainMenu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(800, 600);
			this.Controls.Add(this.lblQuit);
			this.Controls.Add(this.lblOptions);
			this.Controls.Add(this.lblLoadGame);
			this.Controls.Add(this.lblNewGame);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "MainMenu";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "City War";
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblNewGame;
        private System.Windows.Forms.Label lblLoadGame;
        private System.Windows.Forms.Label lblOptions;
        private System.Windows.Forms.Label lblQuit;

    }
}