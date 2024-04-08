
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
            SuspendLayout();
            // 
            // lblMouse
            // 
            lblMouse.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblMouse.BackColor = System.Drawing.Color.White;
            lblMouse.Location = new System.Drawing.Point(0, 526);
            lblMouse.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblMouse.Name = "lblMouse";
            lblMouse.Size = new System.Drawing.Size(65, 15);
            lblMouse.TabIndex = 0;
            lblMouse.Text = "(-222,-222)";
            lblMouse.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // Map
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblMouse);
            Margin = new System.Windows.Forms.Padding(2);
            Name = "Map";
            Size = new System.Drawing.Size(420, 541);
            Load += Map_Load;
            KeyDown += Map_KeyDown;
            KeyUp += Map_KeyUp;
            MouseClick += Map_MouseClick;
            MouseMove += Map_MouseMove;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblMouse;
    }
}
