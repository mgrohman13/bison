namespace GalWarWin
{
	partial class ResearchForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lbxDesigns = new System.Windows.Forms.ListBox();
            this.newPlanetDefense = new GalWarWin.BuildableControl();
            this.sdObsolete = new GalWarWin.BuildableControl();
            this.shipDesignForm1 = new GalWarWin.BuildableControl();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "New Shp Design:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(430, 253);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(200, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Obsolete:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbxDesigns
            // 
            this.lbxDesigns.FormattingEnabled = true;
            this.lbxDesigns.Location = new System.Drawing.Point(200, 23);
            this.lbxDesigns.Name = "lbxDesigns";
            this.lbxDesigns.Size = new System.Drawing.Size(130, 225);
            this.lbxDesigns.TabIndex = 1;
            this.lbxDesigns.SelectedIndexChanged += new System.EventHandler(this.lbxDesigns_SelectedIndexChanged);
            // 
            // newPlanetDefense
            // 
            this.newPlanetDefense.Location = new System.Drawing.Point(330, 122);
            this.newPlanetDefense.Name = "newPlanetDefense";
            this.newPlanetDefense.Size = new System.Drawing.Size(200, 131);
            this.newPlanetDefense.TabIndex = 6;
            this.newPlanetDefense.Visible = false;
            // 
            // sdObsolete
            // 
            this.sdObsolete.Location = new System.Drawing.Point(330, 23);
            this.sdObsolete.Name = "sdObsolete";
            this.sdObsolete.Size = new System.Drawing.Size(200, 230);
            this.sdObsolete.TabIndex = 5;
            // 
            // shipDesignForm1
            // 
            this.shipDesignForm1.Location = new System.Drawing.Point(0, 23);
            this.shipDesignForm1.Name = "shipDesignForm1";
            this.shipDesignForm1.Size = new System.Drawing.Size(200, 230);
            this.shipDesignForm1.TabIndex = 4;
            // 
            // ResearchForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(530, 276);
            this.Controls.Add(this.newPlanetDefense);
            this.Controls.Add(this.sdObsolete);
            this.Controls.Add(this.lbxDesigns);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.shipDesignForm1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ResearchForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Label label2;
		public BuildableControl shipDesignForm1;
		private System.Windows.Forms.ListBox lbxDesigns;
        public BuildableControl sdObsolete;
        public BuildableControl newPlanetDefense;
	}
}