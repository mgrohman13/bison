
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
            this.lblResearching = new System.Windows.Forms.Label();
            this.infoMain = new WinFormsApp1.Info();
            this.SuspendLayout();
            // 
            // mapMain
            // 
            this.mapMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapMain.Location = new System.Drawing.Point(0, 22);
            this.mapMain.Margin = new System.Windows.Forms.Padding(1);
            this.mapMain.MouseTile = null;
            this.mapMain.Name = "mapMain";
            this.mapMain.SelTile = null;
            this.mapMain.Size = new System.Drawing.Size(420, 541);
            this.mapMain.TabIndex = 2;
            this.mapMain.ViewAttacks = false;
            // 
            // mapMini
            // 
            this.mapMini.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mapMini.Location = new System.Drawing.Point(574, 10);
            this.mapMini.Margin = new System.Windows.Forms.Padding(1);
            this.mapMini.MouseTile = null;
            this.mapMini.Name = "mapMini";
            this.mapMini.SelTile = null;
            this.mapMini.Size = new System.Drawing.Size(200, 200);
            this.mapMini.TabIndex = 3;
            this.mapMini.ViewAttacks = false;
            this.mapMini.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Energy";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblEnergy
            // 
            this.lblEnergy.AutoSize = true;
            this.lblEnergy.Location = new System.Drawing.Point(59, 5);
            this.lblEnergy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEnergy.Name = "lblEnergy";
            this.lblEnergy.Size = new System.Drawing.Size(58, 15);
            this.lblEnergy.TabIndex = 5;
            this.lblEnergy.Text = "9999999.9";
            this.lblEnergy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(205, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Mass";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMass
            // 
            this.lblMass.AutoSize = true;
            this.lblMass.Location = new System.Drawing.Point(246, 5);
            this.lblMass.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMass.Name = "lblMass";
            this.lblMass.Size = new System.Drawing.Size(52, 15);
            this.lblMass.TabIndex = 7;
            this.lblMass.Text = "999999.9";
            this.lblMass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(386, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Research";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResearch
            // 
            this.lblResearch.AutoSize = true;
            this.lblResearch.Location = new System.Drawing.Point(447, 5);
            this.lblResearch.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResearch.Name = "lblResearch";
            this.lblResearch.Size = new System.Drawing.Size(52, 15);
            this.lblResearch.TabIndex = 9;
            this.lblResearch.Text = "999999.9";
            this.lblResearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEnergyInc
            // 
            this.lblEnergyInc.AutoSize = true;
            this.lblEnergyInc.Location = new System.Drawing.Point(130, 5);
            this.lblEnergyInc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEnergyInc.Name = "lblEnergyInc";
            this.lblEnergyInc.Size = new System.Drawing.Size(54, 15);
            this.lblEnergyInc.TabIndex = 10;
            this.lblEnergyInc.Text = "+99999.9";
            this.lblEnergyInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMassInc
            // 
            this.lblMassInc.AutoSize = true;
            this.lblMassInc.Location = new System.Drawing.Point(311, 5);
            this.lblMassInc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMassInc.Name = "lblMassInc";
            this.lblMassInc.Size = new System.Drawing.Size(54, 15);
            this.lblMassInc.TabIndex = 11;
            this.lblMassInc.Text = "+99999.9";
            this.lblMassInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResearchInc
            // 
            this.lblResearchInc.AutoSize = true;
            this.lblResearchInc.Location = new System.Drawing.Point(511, 5);
            this.lblResearchInc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResearchInc.Name = "lblResearchInc";
            this.lblResearchInc.Size = new System.Drawing.Size(54, 15);
            this.lblResearchInc.TabIndex = 12;
            this.lblResearchInc.Text = "+99999.9";
            this.lblResearchInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResearching
            // 
            this.lblResearching.AutoSize = true;
            this.lblResearching.Location = new System.Drawing.Point(577, 5);
            this.lblResearching.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResearching.Name = "lblResearching";
            this.lblResearching.Size = new System.Drawing.Size(84, 15);
            this.lblResearching.TabIndex = 13;
            this.lblResearching.Text = "lblResearching";
            this.lblResearching.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoMain
            // 
            this.infoMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoMain.Location = new System.Drawing.Point(423, 30);
            this.infoMain.Margin = new System.Windows.Forms.Padding(2);
            this.infoMain.Name = "infoMain";
            this.infoMain.Selected = null;
            this.infoMain.Size = new System.Drawing.Size(350, 520);
            this.infoMain.TabIndex = 14;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.infoMain);
            this.Controls.Add(this.lblResearching);
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
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Main";
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Main_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Main_KeyUp);
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
        private System.Windows.Forms.Label lblResearching;
    }
}

