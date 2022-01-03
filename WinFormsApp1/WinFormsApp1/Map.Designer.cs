
namespace WinFormsApp1
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblMouse = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblMouse
            // 
            this.lblMouse.BackColor = System.Drawing.Color.Transparent;
            this.lblMouse.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblMouse.Location = new System.Drawing.Point(0, 75);
            this.lblMouse.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMouse.Name = "lblMouse";
            this.lblMouse.Size = new System.Drawing.Size(105, 15);
            this.lblMouse.TabIndex = 0;
            this.lblMouse.Text = "lblMouse";
            this.lblMouse.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // Map
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblMouse);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Map";
            this.Size = new System.Drawing.Size(105, 90);
            this.Load += new System.EventHandler(this.Map_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Map_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Map_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Map_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Map_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMouse;
    }
}
