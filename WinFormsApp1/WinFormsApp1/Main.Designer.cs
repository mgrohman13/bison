
namespace WinFormsApp1
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.infoMain = new WinFormsApp1.Info();
            this.mapMain = new WinFormsApp1.Map();
            this.mapMini = new WinFormsApp1.Map();
            this.SuspendLayout();
            // 
            // infoMain
            // 
            this.infoMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoMain.Location = new System.Drawing.Point(1398, 306);
            this.infoMain.Name = "infoMain";
            this.infoMain.Size = new System.Drawing.Size(500, 718);
            this.infoMain.TabIndex = 1;
            // 
            // mapMain
            // 
            this.mapMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapMain.Location = new System.Drawing.Point(0, 0);
            this.mapMain.Name = "mapMain";
            this.mapMain.SelTile = null;
            this.mapMain.Size = new System.Drawing.Size(1392, 1024);
            this.mapMain.TabIndex = 2;
            // 
            // mapMini
            // 
            this.mapMini.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mapMini.Location = new System.Drawing.Point(1398, 0);
            this.mapMini.Name = "mapMini";
            this.mapMini.SelTile = null;
            this.mapMini.Size = new System.Drawing.Size(500, 300);
            this.mapMini.TabIndex = 3;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1898, 1024);
            this.Controls.Add(this.mapMini);
            this.Controls.Add(this.mapMain);
            this.Controls.Add(this.infoMain);
            this.KeyPreview = true;
            this.Name = "Main";
            this.Text = "Form1";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Main_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion

        private Info infoMain;
        private Map mapMain;
        private Map mapMini;
    }
}

