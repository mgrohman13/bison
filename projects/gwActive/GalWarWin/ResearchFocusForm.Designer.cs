namespace GalWarWin
{
    partial class ResearchFocusForm
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbxUpgrade = new System.Windows.Forms.ListBox();
            this.cbColony = new System.Windows.Forms.CheckBox();
            this.cbUpgrade = new System.Windows.Forms.CheckBox();
            this.cbUpkeep = new System.Windows.Forms.CheckBox();
            this.cbCost = new System.Windows.Forms.CheckBox();
            this.cbSpeed = new System.Windows.Forms.CheckBox();
            this.cbDefense = new System.Windows.Forms.CheckBox();
            this.cbAttack = new System.Windows.Forms.CheckBox();
            this.cbDS = new System.Windows.Forms.CheckBox();
            this.cbTransport = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.sdUpgrade = new GalWarWin.BuildableControl();
            this.lblChance = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(330, 267);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lbxUpgrade
            // 
            this.lbxUpgrade.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.lbxUpgrade.Enabled = false;
            this.lbxUpgrade.FormattingEnabled = true;
            this.lbxUpgrade.Location = new System.Drawing.Point(100, 25);
            this.lbxUpgrade.Name = "lbxUpgrade";
            this.lbxUpgrade.Size = new System.Drawing.Size(130, 277);
            this.lbxUpgrade.TabIndex = 1;
            this.lbxUpgrade.SelectedIndexChanged += new System.EventHandler(this.lbxUpgrade_SelectedIndexChanged);
            this.lbxUpgrade.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbxUpgrade_MouseDoubleClick);
            // 
            // cbColony
            // 
            this.cbColony.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbColony.AutoSize = true;
            this.cbColony.Location = new System.Drawing.Point(12, 60);
            this.cbColony.Name = "cbColony";
            this.cbColony.Size = new System.Drawing.Size(82, 17);
            this.cbColony.TabIndex = 7;
            this.cbColony.Text = "Colony Ship";
            this.cbColony.UseVisualStyleBackColor = true;
            this.cbColony.CheckedChanged += new System.EventHandler(this.cbColony_CheckedChanged);
            // 
            // cbUpgrade
            // 
            this.cbUpgrade.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbUpgrade.AutoSize = true;
            this.cbUpgrade.Location = new System.Drawing.Point(12, 273);
            this.cbUpgrade.Name = "cbUpgrade";
            this.cbUpgrade.Size = new System.Drawing.Size(67, 17);
            this.cbUpgrade.TabIndex = 8;
            this.cbUpgrade.Text = "Upgrade";
            this.cbUpgrade.UseVisualStyleBackColor = true;
            this.cbUpgrade.CheckedChanged += new System.EventHandler(this.cbUpgrade_CheckedChanged);
            // 
            // cbUpkeep
            // 
            this.cbUpkeep.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbUpkeep.AutoSize = true;
            this.cbUpkeep.Location = new System.Drawing.Point(12, 237);
            this.cbUpkeep.Name = "cbUpkeep";
            this.cbUpkeep.Size = new System.Drawing.Size(64, 17);
            this.cbUpkeep.TabIndex = 9;
            this.cbUpkeep.Text = "Upkeep";
            this.cbUpkeep.UseVisualStyleBackColor = true;
            this.cbUpkeep.CheckedChanged += new System.EventHandler(this.cbUpkeep_CheckedChanged);
            // 
            // cbCost
            // 
            this.cbCost.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbCost.AutoSize = true;
            this.cbCost.Location = new System.Drawing.Point(12, 214);
            this.cbCost.Name = "cbCost";
            this.cbCost.Size = new System.Drawing.Size(67, 17);
            this.cbCost.TabIndex = 10;
            this.cbCost.Text = "Powerful";
            this.cbCost.UseVisualStyleBackColor = true;
            this.cbCost.CheckedChanged += new System.EventHandler(this.cbCost_CheckedChanged);
            // 
            // cbSpeed
            // 
            this.cbSpeed.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbSpeed.AutoSize = true;
            this.cbSpeed.Location = new System.Drawing.Point(12, 178);
            this.cbSpeed.Name = "cbSpeed";
            this.cbSpeed.Size = new System.Drawing.Size(46, 17);
            this.cbSpeed.TabIndex = 11;
            this.cbSpeed.Text = "Fast";
            this.cbSpeed.UseVisualStyleBackColor = true;
            this.cbSpeed.CheckedChanged += new System.EventHandler(this.cbSpeed_CheckedChanged);
            // 
            // cbDefense
            // 
            this.cbDefense.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbDefense.AutoSize = true;
            this.cbDefense.Location = new System.Drawing.Point(12, 142);
            this.cbDefense.Name = "cbDefense";
            this.cbDefense.Size = new System.Drawing.Size(66, 17);
            this.cbDefense.TabIndex = 12;
            this.cbDefense.Text = "Defense";
            this.cbDefense.UseVisualStyleBackColor = true;
            this.cbDefense.CheckedChanged += new System.EventHandler(this.cbDefense_CheckedChanged);
            // 
            // cbAttack
            // 
            this.cbAttack.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbAttack.AutoSize = true;
            this.cbAttack.Location = new System.Drawing.Point(12, 119);
            this.cbAttack.Name = "cbAttack";
            this.cbAttack.Size = new System.Drawing.Size(57, 17);
            this.cbAttack.TabIndex = 13;
            this.cbAttack.Text = "Attack";
            this.cbAttack.UseVisualStyleBackColor = true;
            this.cbAttack.CheckedChanged += new System.EventHandler(this.cbAttack_CheckedChanged);
            // 
            // cbDS
            // 
            this.cbDS.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbDS.AutoSize = true;
            this.cbDS.Location = new System.Drawing.Point(12, 83);
            this.cbDS.Name = "cbDS";
            this.cbDS.Size = new System.Drawing.Size(77, 17);
            this.cbDS.TabIndex = 14;
            this.cbDS.Text = "Death Star";
            this.cbDS.UseVisualStyleBackColor = true;
            this.cbDS.CheckedChanged += new System.EventHandler(this.cbDS_CheckedChanged);
            // 
            // cbTransport
            // 
            this.cbTransport.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.cbTransport.AutoSize = true;
            this.cbTransport.Location = new System.Drawing.Point(12, 37);
            this.cbTransport.Name = "cbTransport";
            this.cbTransport.Size = new System.Drawing.Size(71, 17);
            this.cbTransport.TabIndex = 15;
            this.cbTransport.Text = "Transport";
            this.cbTransport.UseVisualStyleBackColor = true;
            this.cbTransport.CheckedChanged += new System.EventHandler(this.cbTransport_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(236, 267);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 23);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // sdUpgrade
            // 
            this.sdUpgrade.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.sdUpgrade.Enabled = false;
            this.sdUpgrade.Location = new System.Drawing.Point(230, 25);
            this.sdUpgrade.Name = "sdUpgrade";
            this.sdUpgrade.Size = new System.Drawing.Size(200, 230);
            this.sdUpgrade.TabIndex = 5;
            // 
            // lblChance
            // 
            this.lblChance.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChance.Location = new System.Drawing.Point(0, 0);
            this.lblChance.Name = "lblChance";
            this.lblChance.Size = new System.Drawing.Size(430, 25);
            this.lblChance.TabIndex = 17;
            this.lblChance.Text = "lblChance";
            this.lblChance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ResearchFocusForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(430, 302);
            this.Controls.Add(this.lblChance);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbTransport);
            this.Controls.Add(this.cbDS);
            this.Controls.Add(this.cbAttack);
            this.Controls.Add(this.cbDefense);
            this.Controls.Add(this.cbSpeed);
            this.Controls.Add(this.cbCost);
            this.Controls.Add(this.cbUpkeep);
            this.Controls.Add(this.cbUpgrade);
            this.Controls.Add(this.cbColony);
            this.Controls.Add(this.sdUpgrade);
            this.Controls.Add(this.lbxUpgrade);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ResearchFocusForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ListBox lbxUpgrade;
        public BuildableControl sdUpgrade;
        private System.Windows.Forms.CheckBox cbColony;
        private System.Windows.Forms.CheckBox cbUpgrade;
        private System.Windows.Forms.CheckBox cbUpkeep;
        private System.Windows.Forms.CheckBox cbCost;
        private System.Windows.Forms.CheckBox cbSpeed;
        private System.Windows.Forms.CheckBox cbDefense;
        private System.Windows.Forms.CheckBox cbAttack;
        private System.Windows.Forms.CheckBox cbDS;
        private System.Windows.Forms.CheckBox cbTransport;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblChance;
	}
}