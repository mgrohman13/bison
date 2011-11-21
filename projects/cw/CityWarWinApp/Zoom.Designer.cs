namespace CityWarWinApp
{
    partial class Zoom
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
			this.lblZoom = new System.Windows.Forms.Label();
			this.tmrClose = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// lblZoom
			// 
			this.lblZoom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblZoom.Font = new System.Drawing.Font("Algerian", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblZoom.ForeColor = System.Drawing.Color.White;
			this.lblZoom.Location = new System.Drawing.Point(0, 0);
			this.lblZoom.Name = "lblZoom";
			this.lblZoom.Size = new System.Drawing.Size(130, 60);
			this.lblZoom.TabIndex = 0;
			this.lblZoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tmrClose
			// 
			this.tmrClose.Enabled = true;
			this.tmrClose.Interval = 1300;
			this.tmrClose.Tick += new System.EventHandler(this.tmrClose_Tick);
			// 
			// Zoom
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(130, 60);
			this.Controls.Add(this.lblZoom);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "Zoom";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Zoom";
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.zoomForm_KeyPress);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.Timer tmrClose;
    }
}