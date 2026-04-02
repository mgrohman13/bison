namespace GalWarWin
{
	partial class GraphsForm
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
            lblPlayer = new Label();
            lblArmada = new Label();
            lblIncome = new Label();
            lblGrowth = new Label();
            lblPlanets = new Label();
            lblQuality = new Label();
            lblPopulation = new Label();
            groupBox1 = new GroupBox();
            chkSmooth = new CheckBox();
            checkBox1 = new CheckBox();
            cbxType = new ComboBox();
            btnDone = new Button();
            lblResearch = new Label();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // lblPlayer
            // 
            lblPlayer.AutoEllipsis = true;
            lblPlayer.Location = new Point(20, 17);
            lblPlayer.Margin = new Padding(5, 0, 5, 0);
            lblPlayer.Name = "lblPlayer";
            lblPlayer.Size = new Size(167, 44);
            lblPlayer.TabIndex = 1;
            lblPlayer.Text = "Player";
            lblPlayer.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblArmada
            // 
            lblArmada.AutoEllipsis = true;
            lblArmada.Location = new Point(1080, 17);
            lblArmada.Margin = new Padding(5, 0, 5, 0);
            lblArmada.Name = "lblArmada";
            lblArmada.Size = new Size(167, 44);
            lblArmada.TabIndex = 8;
            lblArmada.Text = "Armada";
            lblArmada.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblIncome
            // 
            lblIncome.AutoEllipsis = true;
            lblIncome.Location = new Point(903, 17);
            lblIncome.Margin = new Padding(5, 0, 5, 0);
            lblIncome.Name = "lblIncome";
            lblIncome.Size = new Size(167, 44);
            lblIncome.TabIndex = 7;
            lblIncome.Text = "Income";
            lblIncome.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblGrowth
            // 
            lblGrowth.AutoEllipsis = true;
            lblGrowth.Location = new Point(727, 17);
            lblGrowth.Margin = new Padding(5, 0, 5, 0);
            lblGrowth.Name = "lblGrowth";
            lblGrowth.Size = new Size(167, 44);
            lblGrowth.TabIndex = 6;
            lblGrowth.Text = "Growth";
            lblGrowth.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPlanets
            // 
            lblPlanets.AutoEllipsis = true;
            lblPlanets.Location = new Point(197, 17);
            lblPlanets.Margin = new Padding(5, 0, 5, 0);
            lblPlanets.Name = "lblPlanets";
            lblPlanets.Size = new Size(167, 44);
            lblPlanets.TabIndex = 3;
            lblPlanets.Text = "Planets";
            lblPlanets.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblQuality
            // 
            lblQuality.AutoEllipsis = true;
            lblQuality.Location = new Point(373, 17);
            lblQuality.Margin = new Padding(5, 0, 5, 0);
            lblQuality.Name = "lblQuality";
            lblQuality.Size = new Size(167, 44);
            lblQuality.TabIndex = 4;
            lblQuality.Text = "Quality";
            lblQuality.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPopulation
            // 
            lblPopulation.AutoEllipsis = true;
            lblPopulation.Location = new Point(550, 17);
            lblPopulation.Margin = new Padding(5, 0, 5, 0);
            lblPopulation.Name = "lblPopulation";
            lblPopulation.Size = new Size(167, 44);
            lblPopulation.TabIndex = 5;
            lblPopulation.Text = "Population";
            lblPopulation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(chkSmooth);
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Controls.Add(cbxType);
            groupBox1.Controls.Add(btnDone);
            groupBox1.Location = new Point(0, 67);
            groupBox1.Margin = new Padding(5, 6, 5, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(5, 6, 5, 6);
            groupBox1.Size = new Size(1653, 1323);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Graphs";
            groupBox1.Paint += groupBox1_Paint;
            // 
            // chkSmooth
            // 
            chkSmooth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkSmooth.Location = new Point(1417, 140);
            chkSmooth.Margin = new Padding(5, 6, 5, 6);
            chkSmooth.Name = "chkSmooth";
            chkSmooth.Size = new Size(217, 40);
            chkSmooth.TabIndex = 3;
            chkSmooth.Text = "Smooth Lines";
            chkSmooth.UseVisualStyleBackColor = true;
            chkSmooth.Visible = false;
            chkSmooth.CheckedChanged += checkBox_CheckedChanged;
            // 
            // checkBox1
            // 
            checkBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkBox1.Location = new Point(1417, 88);
            checkBox1.Margin = new Padding(5, 6, 5, 6);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(217, 40);
            checkBox1.TabIndex = 2;
            checkBox1.Text = "Account For Damage";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox_CheckedChanged;
            // 
            // cbxType
            // 
            cbxType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbxType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxType.FormattingEnabled = true;
            cbxType.Location = new Point(1417, 37);
            cbxType.Margin = new Padding(5, 6, 5, 6);
            cbxType.Name = "cbxType";
            cbxType.Size = new Size(214, 33);
            cbxType.TabIndex = 1;
            cbxType.SelectedIndexChanged += cbxType_SelectedIndexChanged;
            // 
            // btnDone
            // 
            btnDone.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDone.DialogResult = DialogResult.OK;
            btnDone.Location = new Point(1508, 1256);
            btnDone.Margin = new Padding(5, 6, 5, 6);
            btnDone.Name = "btnDone";
            btnDone.Size = new Size(125, 44);
            btnDone.TabIndex = 0;
            btnDone.Text = "Done";
            btnDone.UseVisualStyleBackColor = true;
            // 
            // lblResearch
            // 
            lblResearch.AutoEllipsis = true;
            lblResearch.Location = new Point(1257, 17);
            lblResearch.Margin = new Padding(5, 0, 5, 0);
            lblResearch.Name = "lblResearch";
            lblResearch.Size = new Size(167, 44);
            lblResearch.TabIndex = 9;
            lblResearch.Text = "Research";
            lblResearch.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // GraphsForm
            // 
            AcceptButton = btnDone;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnDone;
            ClientSize = new Size(1653, 1390);
            Controls.Add(lblResearch);
            Controls.Add(groupBox1);
            Controls.Add(lblPopulation);
            Controls.Add(lblQuality);
            Controls.Add(lblPlanets);
            Controls.Add(lblGrowth);
            Controls.Add(lblIncome);
            Controls.Add(lblArmada);
            Controls.Add(lblPlayer);
            Margin = new Padding(5, 6, 5, 6);
            Name = "GraphsForm";
            StartPosition = FormStartPosition.Manual;
            SizeChanged += GraphsForm_SizeChanged;
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPlayer;
		private System.Windows.Forms.Label lblArmada;
		private System.Windows.Forms.Label lblIncome;
		private System.Windows.Forms.Label lblGrowth;
		private System.Windows.Forms.Label lblPlanets;
		private System.Windows.Forms.Label lblQuality;
		private System.Windows.Forms.Label lblPopulation;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnDone;
		private System.Windows.Forms.ComboBox cbxType;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox chkSmooth;
		private System.Windows.Forms.Label lblResearch;
	}
}