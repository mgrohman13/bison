
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblEnergy = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblMass = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblResearch = new System.Windows.Forms.Label();
            this.lblEnergyInc = new System.Windows.Forms.Label();
            this.lblMassInc = new System.Windows.Forms.Label();
            this.lblResearchInc = new System.Windows.Forms.Label();
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
            this.mapMain.Location = new System.Drawing.Point(0, 37);
            this.mapMain.Name = "mapMain";
            this.mapMain.SelTile = null;
            this.mapMain.Size = new System.Drawing.Size(1392, 987);
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
            this.mapMini.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Energy";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblEnergy
            // 
            this.lblEnergy.AutoSize = true;
            this.lblEnergy.Location = new System.Drawing.Point(84, 9);
            this.lblEnergy.Name = "lblEnergy";
            this.lblEnergy.Size = new System.Drawing.Size(96, 25);
            this.lblEnergy.TabIndex = 5;
            this.lblEnergy.Text = "9999999.9";
            this.lblEnergy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(293, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "Mass";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMass
            // 
            this.lblMass.AutoSize = true;
            this.lblMass.Location = new System.Drawing.Point(352, 9);
            this.lblMass.Name = "lblMass";
            this.lblMass.Size = new System.Drawing.Size(86, 25);
            this.lblMass.TabIndex = 7;
            this.lblMass.Text = "999999.9";
            this.lblMass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(551, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "Research";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResearch
            // 
            this.lblResearch.AutoSize = true;
            this.lblResearch.Location = new System.Drawing.Point(638, 9);
            this.lblResearch.Name = "lblResearch";
            this.lblResearch.Size = new System.Drawing.Size(86, 25);
            this.lblResearch.TabIndex = 9;
            this.lblResearch.Text = "999999.9";
            this.lblResearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEnergyInc
            // 
            this.lblEnergyInc.AutoSize = true;
            this.lblEnergyInc.Location = new System.Drawing.Point(186, 9);
            this.lblEnergyInc.Name = "lblEnergyInc";
            this.lblEnergyInc.Size = new System.Drawing.Size(88, 25);
            this.lblEnergyInc.TabIndex = 10;
            this.lblEnergyInc.Text = "+99999.9";
            this.lblEnergyInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMassInc
            // 
            this.lblMassInc.AutoSize = true;
            this.lblMassInc.Location = new System.Drawing.Point(444, 9);
            this.lblMassInc.Name = "lblMassInc";
            this.lblMassInc.Size = new System.Drawing.Size(88, 25);
            this.lblMassInc.TabIndex = 11;
            this.lblMassInc.Text = "+99999.9";
            this.lblMassInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResearchInc
            // 
            this.lblResearchInc.AutoSize = true;
            this.lblResearchInc.Location = new System.Drawing.Point(730, 9);
            this.lblResearchInc.Name = "lblResearchInc";
            this.lblResearchInc.Size = new System.Drawing.Size(88, 25);
            this.lblResearchInc.TabIndex = 12;
            this.lblResearchInc.Text = "+99999.9";
            this.lblResearchInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1898, 1024);
            this.Controls.Add(this.lblResearchInc);
            this.Controls.Add(this.lblMassInc);
            this.Controls.Add(this.lblEnergyInc);
            this.Controls.Add(this.lblResearch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblMass);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblEnergy);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mapMini);
            this.Controls.Add(this.mapMain);
            this.Controls.Add(this.infoMain);
            this.KeyPreview = true;
            this.Name = "Main";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Info infoMain;
        private Map mapMain;
        private Map mapMini;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblEnergy;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblMass;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblResearch;
        private System.Windows.Forms.Label lblEnergyInc;
        private System.Windows.Forms.Label lblMassInc;
        private System.Windows.Forms.Label lblResearchInc;
    }
}

