namespace CityWarWinApp
{
    partial class NewGame
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
            this.lblCancel = new System.Windows.Forms.Label();
            this.lblStart = new System.Windows.Forms.Label();
            this.nudSize = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lbxPlayers = new System.Windows.Forms.ListBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.lblColor = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblEdit = new System.Windows.Forms.Label();
            this.lblAddNew = new System.Windows.Forms.Label();
            this.lblDelete = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbxRace = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudSize)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCancel
            // 
            this.lblCancel.BackColor = System.Drawing.Color.Black;
            this.lblCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCancel.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCancel.ForeColor = System.Drawing.Color.White;
            this.lblCancel.Location = new System.Drawing.Point(62, 439);
            this.lblCancel.Name = "lblCancel";
            this.lblCancel.Size = new System.Drawing.Size(300, 60);
            this.lblCancel.TabIndex = 7;
            this.lblCancel.Text = "Cancel";
            this.lblCancel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCancel.Click += new System.EventHandler(this.lblCancel_Click);
            this.lblCancel.MouseEnter += new System.EventHandler(this.lblCancel_MouseEnter);
            this.lblCancel.MouseLeave += new System.EventHandler(this.lblCancel_MouseLeave);
            // 
            // lblStart
            // 
            this.lblStart.BackColor = System.Drawing.Color.Black;
            this.lblStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblStart.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStart.ForeColor = System.Drawing.Color.White;
            this.lblStart.Location = new System.Drawing.Point(62, 379);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(300, 60);
            this.lblStart.TabIndex = 6;
            this.lblStart.Text = "Start";
            this.lblStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStart.Click += new System.EventHandler(this.lblStart_Click);
            this.lblStart.MouseEnter += new System.EventHandler(this.lblStart_MouseEnter);
            this.lblStart.MouseLeave += new System.EventHandler(this.lblStart_MouseLeave);
            // 
            // nudSize
            // 
            this.nudSize.BackColor = System.Drawing.Color.Black;
            this.nudSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.nudSize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nudSize.DecimalPlaces = 1;
            this.nudSize.Font = new System.Drawing.Font("Arial", 15.75F);
            this.nudSize.ForeColor = System.Drawing.Color.LightCyan;
            this.nudSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudSize.Location = new System.Drawing.Point(213, 9);
            this.nudSize.Maximum = new decimal(new int[] {
            26 ,
            0,
            0,
            0});
            this.nudSize.Minimum = new decimal(new int[] {
            65,
            0,
            0,
            65536});
            this.nudSize.Name = "nudSize";
            this.nudSize.Size = new System.Drawing.Size(65, 28);
            this.nudSize.TabIndex = 6;
            this.nudSize.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.nudSize.Value = new decimal(new int[] {
            117,
            0,
            0,
            65536});
            this.nudSize.ValueChanged += new System.EventHandler(this.nudSize_ValueChanged);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label1.ForeColor = System.Drawing.Color.LightCyan;
            this.label1.Location = new System.Drawing.Point(95, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 31);
            this.label1.TabIndex = 7;
            this.label1.Text = "Map Size:";
            // 
            // lblSize
            // 
            this.lblSize.BackColor = System.Drawing.Color.Black;
            this.lblSize.Font = new System.Drawing.Font("Arial", 15.75F);
            this.lblSize.ForeColor = System.Drawing.Color.LightCyan;
            this.lblSize.Location = new System.Drawing.Point(284, 9);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(128, 31);
            this.lblSize.TabIndex = 9;
            this.lblSize.Text = "X";
            // 
            // lbxPlayers
            // 
            this.lbxPlayers.BackColor = System.Drawing.Color.Black;
            this.lbxPlayers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbxPlayers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbxPlayers.Font = new System.Drawing.Font("Arial", 15.75F);
            this.lbxPlayers.ForeColor = System.Drawing.Color.White;
            this.lbxPlayers.FormattingEnabled = true;
            this.lbxPlayers.ItemHeight = 26;
            this.lbxPlayers.Location = new System.Drawing.Point(71, 139);
            this.lbxPlayers.Name = "lbxPlayers";
            this.lbxPlayers.Size = new System.Drawing.Size(130, 160);
            this.lbxPlayers.TabIndex = 0;
            this.lbxPlayers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbxPlayers_DrawItem);
            this.lbxPlayers.SelectedIndexChanged += new System.EventHandler(this.lbxPlayers_SelectedIndexChanged);
            // 
            // lblColor
            // 
            this.lblColor.BackColor = System.Drawing.Color.White;
            this.lblColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblColor.ForeColor = System.Drawing.Color.Black;
            this.lblColor.Location = new System.Drawing.Point(290, 177);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(30, 30);
            this.lblColor.TabIndex = 1;
            this.lblColor.Click += new System.EventHandler(this.lblColor_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label3.ForeColor = System.Drawing.Color.LightCyan;
            this.label3.Location = new System.Drawing.Point(207, 177);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 30);
            this.label3.TabIndex = 13;
            this.label3.Text = "Color";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtName
            // 
            this.txtName.BackColor = System.Drawing.Color.Black;
            this.txtName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtName.Font = new System.Drawing.Font("Arial", 15.75F);
            this.txtName.ForeColor = System.Drawing.Color.White;
            this.txtName.Location = new System.Drawing.Point(207, 139);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(130, 32);
            this.txtName.TabIndex = 2;
            // 
            // lblEdit
            // 
            this.lblEdit.BackColor = System.Drawing.Color.Black;
            this.lblEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblEdit.Font = new System.Drawing.Font("Arial", 15.75F);
            this.lblEdit.ForeColor = System.Drawing.Color.White;
            this.lblEdit.Location = new System.Drawing.Point(207, 243);
            this.lblEdit.Name = "lblEdit";
            this.lblEdit.Size = new System.Drawing.Size(130, 30);
            this.lblEdit.TabIndex = 3;
            this.lblEdit.Text = "Edit";
            this.lblEdit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblEdit.Click += new System.EventHandler(this.lblEdit_Click);
            // 
            // lblAddNew
            // 
            this.lblAddNew.BackColor = System.Drawing.Color.Black;
            this.lblAddNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAddNew.Font = new System.Drawing.Font("Arial", 15.75F);
            this.lblAddNew.ForeColor = System.Drawing.Color.White;
            this.lblAddNew.Location = new System.Drawing.Point(207, 302);
            this.lblAddNew.Name = "lblAddNew";
            this.lblAddNew.Size = new System.Drawing.Size(130, 30);
            this.lblAddNew.TabIndex = 5;
            this.lblAddNew.Text = "Add New";
            this.lblAddNew.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAddNew.Click += new System.EventHandler(this.lblAddNew_Click);
            // 
            // lblDelete
            // 
            this.lblDelete.BackColor = System.Drawing.Color.Black;
            this.lblDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblDelete.Font = new System.Drawing.Font("Arial", 15.75F);
            this.lblDelete.ForeColor = System.Drawing.Color.White;
            this.lblDelete.Location = new System.Drawing.Point(71, 302);
            this.lblDelete.Name = "lblDelete";
            this.lblDelete.Size = new System.Drawing.Size(130, 30);
            this.lblDelete.TabIndex = 4;
            this.lblDelete.Text = "Delete";
            this.lblDelete.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDelete.Click += new System.EventHandler(this.lblDelete_Click);
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Black;
            this.label5.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label5.ForeColor = System.Drawing.Color.LightCyan;
            this.label5.Location = new System.Drawing.Point(71, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(268, 31);
            this.label5.TabIndex = 19;
            this.label5.Text = "Players:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Black;
            this.label4.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label4.ForeColor = System.Drawing.Color.LightCyan;
            this.label4.Location = new System.Drawing.Point(207, 207);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 30);
            this.label4.TabIndex = 21;
            this.label4.Text = "Race";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbxRace
            // 
            this.cbxRace.FormattingEnabled = true;
            this.cbxRace.Location = new System.Drawing.Point(289, 210);
            this.cbxRace.Name = "cbxRace";
            this.cbxRace.Size = new System.Drawing.Size(73, 21);
            this.cbxRace.TabIndex = 22;
            // 
            // NewGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(424, 508);
            this.Controls.Add(this.cbxRace);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblDelete);
            this.Controls.Add(this.lblAddNew);
            this.Controls.Add(this.lblEdit);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.lbxPlayers);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudSize);
            this.Controls.Add(this.lblCancel);
            this.Controls.Add(this.lblStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NewGame";
            ((System.ComponentModel.ISupportInitialize)(this.nudSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCancel;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.NumericUpDown nudSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.ListBox lbxPlayers;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblEdit;
        private System.Windows.Forms.Label lblAddNew;
        private System.Windows.Forms.Label lblDelete;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbxRace;
    }
}