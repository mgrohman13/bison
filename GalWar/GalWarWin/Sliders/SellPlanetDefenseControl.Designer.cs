namespace GalWarWin.Sliders
{
    partial class SellPlanetDefenseControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkProd = new System.Windows.Forms.CheckBox();
            this.lblAtt = new System.Windows.Forms.Label();
            this.lblDef = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkProd
            // 
            this.chkProd.AutoSize = true;
            this.chkProd.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkProd.Checked = true;
            this.chkProd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProd.Location = new System.Drawing.Point(0, 0);
            this.chkProd.Name = "chkProd";
            this.chkProd.Size = new System.Drawing.Size(77, 17);
            this.chkProd.TabIndex = 0;
            this.chkProd.Text = "Production";
            this.chkProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkProd.UseVisualStyleBackColor = true;
            // 
            // lblAtt
            // 
            this.lblAtt.AutoSize = true;
            this.lblAtt.Location = new System.Drawing.Point(83, 1);
            this.lblAtt.Name = "lblAtt";
            this.lblAtt.Size = new System.Drawing.Size(30, 13);
            this.lblAtt.TabIndex = 1;
            this.lblAtt.Text = "lblAtt";
            // 
            // lblDef
            // 
            this.lblDef.AutoSize = true;
            this.lblDef.Location = new System.Drawing.Point(113, 1);
            this.lblDef.Name = "lblDef";
            this.lblDef.Size = new System.Drawing.Size(34, 13);
            this.lblDef.TabIndex = 2;
            this.lblDef.Text = "lblDef";
            // 
            // SellPlanetDefenseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.lblDef);
            this.Controls.Add(this.lblAtt);
            this.Controls.Add(this.chkProd);
            this.Name = "SellPlanetDefenseControl";
            this.Size = new System.Drawing.Size(150, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkProd;
        private System.Windows.Forms.Label lblAtt;
        private System.Windows.Forms.Label lblDef;
    }
}
