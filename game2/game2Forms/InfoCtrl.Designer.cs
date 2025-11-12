namespace game2Forms
{
    partial class InfoCtrl
    {
        ///// <summary> 
        ///// Required designer variable.
        ///// </summary>
        //private System.ComponentModel.IContainer components;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            tableLayoutPanel = new TableLayoutPanel();
            lblTemplate = new Label();
            pbTemplate = new PictureBox();
            lblHeader = new Label();
            ((System.ComponentModel.ISupportInitialize)pbTemplate).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel.BackColor = Color.White;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel.Location = new Point(21, 65);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel.Size = new Size(0, 0);
            tableLayoutPanel.TabIndex = 0;
            // 
            // lblTemplate
            // 
            lblTemplate.Anchor = AnchorStyles.Left;
            lblTemplate.AutoEllipsis = true;
            lblTemplate.AutoSize = true;
            lblTemplate.Font = new Font("Cascadia Code", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTemplate.Location = new Point(34, 89);
            lblTemplate.Name = "lblTemplate";
            lblTemplate.Size = new Size(96, 18);
            lblTemplate.TabIndex = 0;
            lblTemplate.Text = "lblTemplate";
            lblTemplate.TextAlign = ContentAlignment.MiddleLeft;
            lblTemplate.Visible = false;
            lblTemplate.TextChanged += ChildLabel_TextChanged;
            // 
            // pbTemplate
            // 
            pbTemplate.Anchor = AnchorStyles.Left;
            pbTemplate.Location = new Point(3, 89);
            pbTemplate.Name = "pbTemplate";
            pbTemplate.Size = new Size(18, 18);
            pbTemplate.SizeMode = PictureBoxSizeMode.StretchImage;
            pbTemplate.TabIndex = 4;
            pbTemplate.TabStop = false;
            pbTemplate.Visible = false;
            // 
            // lblHeader
            // 
            lblHeader.AutoSize = true;
            lblHeader.Font = new Font("Cascadia Code", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHeader.Location = new Point(0, 25);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(80, 18);
            lblHeader.TabIndex = 4;
            lblHeader.Text = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            // 
            // InfoCtrl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            BackColor = SystemColors.Control;
            Controls.Add(lblTemplate);
            Controls.Add(lblHeader);
            Controls.Add(pbTemplate);
            Controls.Add(tableLayoutPanel);
            Name = "InfoCtrl";
            Size = new Size(200, 400);
            ((System.ComponentModel.ISupportInitialize)pbTemplate).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private Label lblTemplate;
        private Label lblHeader;
        private PictureBox pbTemplate;
    }
}