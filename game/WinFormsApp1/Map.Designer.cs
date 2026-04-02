
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
            lblMouse = new System.Windows.Forms.Label();
            lblMissile = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lblMouse
            // 
            lblMouse.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblMouse.BackColor = System.Drawing.Color.White;
            lblMouse.Location = new System.Drawing.Point(0, 877);
            lblMouse.Name = "lblMouse";
            lblMouse.Size = new System.Drawing.Size(109, 25);
            lblMouse.TabIndex = 0;
            lblMouse.Text = "(-2222,-2222)";
            lblMouse.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblMissile
            // 
            lblMissile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblMissile.AutoSize = true;
            lblMissile.BackColor = System.Drawing.Color.White;
            lblMissile.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblMissile.Location = new System.Drawing.Point(3, 0);
            lblMissile.Name = "lblMissile";
            lblMissile.Size = new System.Drawing.Size(63, 33);
            lblMissile.TabIndex = 1;
            lblMissile.Text = "999";
            lblMissile.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            lblMissile.Visible = false;
            lblMissile.MouseClick += lblMissile_MouseClick;
            // 
            // Map
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblMissile);
            Controls.Add(lblMouse);
            Name = "Map";
            Size = new System.Drawing.Size(600, 902);
            Load += Map_Load;
            KeyDown += Map_KeyDown;
            KeyUp += Map_KeyUp;
            MouseClick += Map_MouseClick;
            MouseMove += Map_MouseMove;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblMouse;
        private System.Windows.Forms.Label lblMissile;
    }
}
