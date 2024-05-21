
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
            mapMain = new Map();
            mapMini = new Map();
            label1 = new System.Windows.Forms.Label();
            lblEnergy = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lblMass = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lblResearch = new System.Windows.Forms.Label();
            lblEnergyInc = new System.Windows.Forms.Label();
            lblMassInc = new System.Windows.Forms.Label();
            lblResearchInc = new System.Windows.Forms.Label();
            lblResearching = new System.Windows.Forms.Label();
            infoMain = new Info();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            SuspendLayout();
            // 
            // mapMain
            // 
            mapMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mapMain.Location = new System.Drawing.Point(353, 22);
            mapMain.Margin = new System.Windows.Forms.Padding(1);
            mapMain.MouseTile = null;
            mapMain.Name = "mapMain";
            mapMain.SelTile = null;
            mapMain.Size = new System.Drawing.Size(420, 541);
            mapMain.TabIndex = 2;
            // 
            // mapMini
            // 
            mapMini.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            mapMini.Location = new System.Drawing.Point(574, 10);
            mapMini.Margin = new System.Windows.Forms.Padding(1);
            mapMini.MouseTile = null;
            mapMini.Name = "mapMini";
            mapMini.SelTile = null;
            mapMini.Size = new System.Drawing.Size(200, 200);
            mapMini.TabIndex = 3;
            mapMini.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 5);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(43, 15);
            label1.TabIndex = 4;
            label1.Text = "Energy";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblEnergy
            // 
            lblEnergy.AutoSize = true;
            lblEnergy.Location = new System.Drawing.Point(59, 5);
            lblEnergy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblEnergy.Name = "lblEnergy";
            lblEnergy.Size = new System.Drawing.Size(58, 15);
            lblEnergy.TabIndex = 5;
            lblEnergy.Text = "9999999.9";
            lblEnergy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(205, 5);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(34, 15);
            label2.TabIndex = 6;
            label2.Text = "Mass";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMass
            // 
            lblMass.AutoSize = true;
            lblMass.Location = new System.Drawing.Point(246, 5);
            lblMass.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblMass.Name = "lblMass";
            lblMass.Size = new System.Drawing.Size(52, 15);
            lblMass.TabIndex = 7;
            lblMass.Text = "999999.9";
            lblMass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(386, 5);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(54, 15);
            label3.TabIndex = 8;
            label3.Text = "Research";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResearch
            // 
            lblResearch.AutoSize = true;
            lblResearch.Location = new System.Drawing.Point(447, 5);
            lblResearch.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblResearch.Name = "lblResearch";
            lblResearch.Size = new System.Drawing.Size(52, 15);
            lblResearch.TabIndex = 9;
            lblResearch.Text = "999999.9";
            lblResearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEnergyInc
            // 
            lblEnergyInc.AutoSize = true;
            lblEnergyInc.Location = new System.Drawing.Point(130, 5);
            lblEnergyInc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblEnergyInc.Name = "lblEnergyInc";
            lblEnergyInc.Size = new System.Drawing.Size(54, 15);
            lblEnergyInc.TabIndex = 10;
            lblEnergyInc.Text = "+99999.9";
            lblEnergyInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMassInc
            // 
            lblMassInc.AutoSize = true;
            lblMassInc.Location = new System.Drawing.Point(311, 5);
            lblMassInc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblMassInc.Name = "lblMassInc";
            lblMassInc.Size = new System.Drawing.Size(54, 15);
            lblMassInc.TabIndex = 11;
            lblMassInc.Text = "+99999.9";
            lblMassInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResearchInc
            // 
            lblResearchInc.AutoSize = true;
            lblResearchInc.Location = new System.Drawing.Point(511, 5);
            lblResearchInc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblResearchInc.Name = "lblResearchInc";
            lblResearchInc.Size = new System.Drawing.Size(54, 15);
            lblResearchInc.TabIndex = 12;
            lblResearchInc.Text = "+99999.9";
            lblResearchInc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResearching
            // 
            lblResearching.AutoSize = true;
            lblResearching.Location = new System.Drawing.Point(577, 5);
            lblResearching.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblResearching.Name = "lblResearching";
            lblResearching.Size = new System.Drawing.Size(84, 15);
            lblResearching.TabIndex = 13;
            lblResearching.Text = "lblResearching";
            lblResearching.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoMain
            // 
            infoMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            infoMain.Location = new System.Drawing.Point(0, 30);
            infoMain.Margin = new System.Windows.Forms.Padding(2);
            infoMain.Name = "infoMain";
            infoMain.Selected = null;
            infoMain.Size = new System.Drawing.Size(350, 520);
            infoMain.TabIndex = 14;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressBar1.BackColor = System.Drawing.SystemColors.Control;
            progressBar1.Location = new System.Drawing.Point(458, 243);
            progressBar1.Maximum = 65000;
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(210, 99);
            progressBar1.Step = 1;
            progressBar1.TabIndex = 15;
            progressBar1.Visible = false;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(784, 561);
            Controls.Add(progressBar1);
            Controls.Add(infoMain);
            Controls.Add(lblResearching);
            Controls.Add(lblResearchInc);
            Controls.Add(lblMassInc);
            Controls.Add(lblEnergyInc);
            Controls.Add(lblResearch);
            Controls.Add(label3);
            Controls.Add(lblMass);
            Controls.Add(label2);
            Controls.Add(lblEnergy);
            Controls.Add(label1);
            Controls.Add(mapMini);
            Controls.Add(mapMain);
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(2);
            Name = "Main";
            Text = "Form1";
            FormClosing += Main_FormClosing;
            Shown += Main_Shown;
            KeyDown += Main_KeyDown;
            KeyPress += Main_KeyPress;
            KeyUp += Main_KeyUp;
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

