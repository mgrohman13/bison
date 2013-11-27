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
            this.nudWidth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nudHeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
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
            ( (System.ComponentModel.ISupportInitialize)( this.nudWidth ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudHeight ) ).BeginInit();
            this.SuspendLayout();
            // 
            // lblCancel
            // 
            this.lblCancel.BackColor = System.Drawing.Color.Black;
            this.lblCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCancel.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
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
            this.lblStart.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
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
            // nudWidth
            // 
            this.nudWidth.BackColor = System.Drawing.Color.Black;
            this.nudWidth.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.nudWidth.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nudWidth.Font = new System.Drawing.Font("Arial", 15.75F);
            this.nudWidth.ForeColor = System.Drawing.Color.LightCyan;
            this.nudWidth.Location = new System.Drawing.Point(213, 9);
            this.nudWidth.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudWidth.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudWidth.Name = "nudWidth";
            this.nudWidth.Size = new System.Drawing.Size(50, 28);
            this.nudWidth.TabIndex = 6;
            this.nudWidth.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.nudWidth.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
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
            // nudHeight
            // 
            this.nudHeight.BackColor = System.Drawing.Color.Black;
            this.nudHeight.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.nudHeight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nudHeight.Font = new System.Drawing.Font("Arial", 15.75F);
            this.nudHeight.ForeColor = System.Drawing.Color.LightCyan;
            this.nudHeight.Location = new System.Drawing.Point(289, 9);
            this.nudHeight.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudHeight.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudHeight.Name = "nudHeight";
            this.nudHeight.Size = new System.Drawing.Size(50, 28);
            this.nudHeight.TabIndex = 8;
            this.nudHeight.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.nudHeight.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label2.ForeColor = System.Drawing.Color.LightCyan;
            this.label2.Location = new System.Drawing.Point(263, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 31);
            this.label2.TabIndex = 9;
            this.label2.Text = "X";
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
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nudHeight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudWidth);
            this.Controls.Add(this.lblCancel);
            this.Controls.Add(this.lblStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NewGame";
            ( (System.ComponentModel.ISupportInitialize)( this.nudWidth ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.nudHeight ) ).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCancel;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.NumericUpDown nudWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudHeight;
        private System.Windows.Forms.Label label2;
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