namespace CityWarWinApp
{
    partial class ChangeTerrain
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnMountain = new System.Windows.Forms.Button();
            this.btnWater = new System.Windows.Forms.Button();
            this.btnForest = new System.Windows.Forms.Button();
            this.btnPlains = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Silver;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnCancel.ForeColor = System.Drawing.Color.Black;
            this.btnCancel.Location = new System.Drawing.Point(52, 70);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnMountain
            // 
            this.btnMountain.BackColor = System.Drawing.Color.Gold;
            this.btnMountain.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnMountain.ForeColor = System.Drawing.Color.Black;
            this.btnMountain.Location = new System.Drawing.Point(12, 12);
            this.btnMountain.Name = "btnMountain";
            this.btnMountain.Size = new System.Drawing.Size(90, 23);
            this.btnMountain.TabIndex = 0;
            this.btnMountain.Text = "Mountain";
            this.btnMountain.UseVisualStyleBackColor = false;
            this.btnMountain.Click += new System.EventHandler(this.btnMountain_Click);
            // 
            // btnWater
            // 
            this.btnWater.BackColor = System.Drawing.Color.Blue;
            this.btnWater.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnWater.ForeColor = System.Drawing.Color.Black;
            this.btnWater.Location = new System.Drawing.Point(108, 12);
            this.btnWater.Name = "btnWater";
            this.btnWater.Size = new System.Drawing.Size(90, 23);
            this.btnWater.TabIndex = 1;
            this.btnWater.Text = "Water";
            this.btnWater.UseVisualStyleBackColor = false;
            this.btnWater.Click += new System.EventHandler(this.btnWater_Click);
            // 
            // btnForest
            // 
            this.btnForest.BackColor = System.Drawing.Color.Green;
            this.btnForest.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnForest.ForeColor = System.Drawing.Color.Black;
            this.btnForest.Location = new System.Drawing.Point(108, 41);
            this.btnForest.Name = "btnForest";
            this.btnForest.Size = new System.Drawing.Size(90, 23);
            this.btnForest.TabIndex = 3;
            this.btnForest.Text = "Forest";
            this.btnForest.UseVisualStyleBackColor = false;
            this.btnForest.Click += new System.EventHandler(this.btnForest_Click);
            // 
            // btnPlains
            // 
            this.btnPlains.BackColor = System.Drawing.Color.Gray;
            this.btnPlains.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnPlains.ForeColor = System.Drawing.Color.Black;
            this.btnPlains.Location = new System.Drawing.Point(12, 41);
            this.btnPlains.Name = "btnPlains";
            this.btnPlains.Size = new System.Drawing.Size(90, 23);
            this.btnPlains.TabIndex = 2;
            this.btnPlains.Text = "Plains";
            this.btnPlains.UseVisualStyleBackColor = false;
            this.btnPlains.Click += new System.EventHandler(this.btnPlains_Click);
            // 
            // ChangeTerrain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(288, 105);
            this.Controls.Add(this.btnPlains);
            this.Controls.Add(this.btnForest);
            this.Controls.Add(this.btnWater);
            this.Controls.Add(this.btnMountain);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ChangeTerrain";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Change Terrain";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnMountain;
        private System.Windows.Forms.Button btnWater;
        private System.Windows.Forms.Button btnForest;
        private System.Windows.Forms.Button btnPlains;
    }
}