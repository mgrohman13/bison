namespace GalWarWin
{
    partial class CombatForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnAttack = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblAttack = new System.Windows.Forms.Label();
            this.lblAttHP = new System.Windows.Forms.Label();
            this.lblDefense = new System.Windows.Forms.Label();
            this.lblDefHP = new System.Windows.Forms.Label();
            this.btnSwap = new System.Windows.Forms.Button();
            this.lblAttKill = new System.Windows.Forms.Label();
            this.lblAttDmg = new System.Windows.Forms.Label();
            this.lblDefKill = new System.Windows.Forms.Label();
            this.lblDefDmg = new System.Windows.Forms.Label();
            this.lblAttPlayer = new System.Windows.Forms.Label();
            this.lblDefPlayer = new System.Windows.Forms.Label();
            this.btnEdit = new System.Windows.Forms.Button();
            this.nudAttack = new System.Windows.Forms.NumericUpDown();
            this.nudAttHP = new System.Windows.Forms.NumericUpDown();
            this.nudDefHP = new System.Windows.Forms.NumericUpDown();
            this.nudDefense = new System.Windows.Forms.NumericUpDown();
            this.btnLog = new System.Windows.Forms.Button();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.btnDetails = new System.Windows.Forms.Button();
            this.lblInfDef = new System.Windows.Forms.Label();
            this.lblInfAtt = new System.Windows.Forms.Label();
            this.btnChances = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudAttack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAttHP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDefHP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDefense)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAttack
            // 
            this.btnAttack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAttack.Location = new System.Drawing.Point(18, 380);
            this.btnAttack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAttack.Name = "btnAttack";
            this.btnAttack.Size = new System.Drawing.Size(150, 35);
            this.btnAttack.TabIndex = 0;
            this.btnAttack.Text = "Attack";
            this.btnAttack.UseVisualStyleBackColor = true;
            this.btnAttack.Click += new System.EventHandler(this.btnAttack_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(177, 380);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 35);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblAttack
            // 
            this.lblAttack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAttack.Location = new System.Drawing.Point(18, 49);
            this.lblAttack.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAttack.Name = "lblAttack";
            this.lblAttack.Size = new System.Drawing.Size(146, 35);
            this.lblAttack.TabIndex = 2;
            this.lblAttack.Text = "lblAttack";
            this.lblAttack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAttHP
            // 
            this.lblAttHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAttHP.Location = new System.Drawing.Point(18, 85);
            this.lblAttHP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAttHP.Name = "lblAttHP";
            this.lblAttHP.Size = new System.Drawing.Size(146, 35);
            this.lblAttHP.TabIndex = 4;
            this.lblAttHP.Text = "lblAttHP";
            this.lblAttHP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefense
            // 
            this.lblDefense.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDefense.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDefense.Location = new System.Drawing.Point(182, 49);
            this.lblDefense.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefense.Name = "lblDefense";
            this.lblDefense.Size = new System.Drawing.Size(146, 35);
            this.lblDefense.TabIndex = 7;
            this.lblDefense.Text = "lblDefense";
            this.lblDefense.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefHP
            // 
            this.lblDefHP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDefHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDefHP.Location = new System.Drawing.Point(182, 85);
            this.lblDefHP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefHP.Name = "lblDefHP";
            this.lblDefHP.Size = new System.Drawing.Size(146, 35);
            this.lblDefHP.TabIndex = 9;
            this.lblDefHP.Text = "lblDefHP";
            this.lblDefHP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSwap
            // 
            this.btnSwap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSwap.Location = new System.Drawing.Point(177, 275);
            this.btnSwap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(150, 35);
            this.btnSwap.TabIndex = 5;
            this.btnSwap.Text = "Swap";
            this.btnSwap.UseVisualStyleBackColor = true;
            this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
            // 
            // lblAttKill
            // 
            this.lblAttKill.AutoEllipsis = true;
            this.lblAttKill.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAttKill.Location = new System.Drawing.Point(18, 191);
            this.lblAttKill.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAttKill.Name = "lblAttKill";
            this.lblAttKill.Size = new System.Drawing.Size(146, 35);
            this.lblAttKill.TabIndex = 14;
            this.lblAttKill.Text = "lblAttKill";
            this.lblAttKill.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAttDmg
            // 
            this.lblAttDmg.AutoEllipsis = true;
            this.lblAttDmg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAttDmg.Location = new System.Drawing.Point(18, 155);
            this.lblAttDmg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAttDmg.Name = "lblAttDmg";
            this.lblAttDmg.Size = new System.Drawing.Size(146, 35);
            this.lblAttDmg.TabIndex = 12;
            this.lblAttDmg.Text = "lblAttDmg";
            this.lblAttDmg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefKill
            // 
            this.lblDefKill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDefKill.AutoEllipsis = true;
            this.lblDefKill.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDefKill.Location = new System.Drawing.Point(182, 191);
            this.lblDefKill.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefKill.Name = "lblDefKill";
            this.lblDefKill.Size = new System.Drawing.Size(146, 35);
            this.lblDefKill.TabIndex = 15;
            this.lblDefKill.Text = "lblDefKill";
            this.lblDefKill.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefDmg
            // 
            this.lblDefDmg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDefDmg.AutoEllipsis = true;
            this.lblDefDmg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDefDmg.Location = new System.Drawing.Point(182, 155);
            this.lblDefDmg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefDmg.Name = "lblDefDmg";
            this.lblDefDmg.Size = new System.Drawing.Size(146, 35);
            this.lblDefDmg.TabIndex = 13;
            this.lblDefDmg.Text = "lblDefDmg";
            this.lblDefDmg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAttPlayer
            // 
            this.lblAttPlayer.AutoEllipsis = true;
            this.lblAttPlayer.Location = new System.Drawing.Point(18, 14);
            this.lblAttPlayer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAttPlayer.Name = "lblAttPlayer";
            this.lblAttPlayer.Size = new System.Drawing.Size(146, 35);
            this.lblAttPlayer.TabIndex = 10;
            this.lblAttPlayer.Text = "lblAttPlayer";
            this.lblAttPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefPlayer
            // 
            this.lblDefPlayer.AutoEllipsis = true;
            this.lblDefPlayer.Location = new System.Drawing.Point(182, 14);
            this.lblDefPlayer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefPlayer.Name = "lblDefPlayer";
            this.lblDefPlayer.Size = new System.Drawing.Size(146, 35);
            this.lblDefPlayer.TabIndex = 11;
            this.lblDefPlayer.Text = "lblDefPlayer";
            this.lblDefPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Location = new System.Drawing.Point(18, 275);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(150, 35);
            this.btnEdit.TabIndex = 4;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // nudAttack
            // 
            this.nudAttack.Location = new System.Drawing.Point(18, 49);
            this.nudAttack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudAttack.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudAttack.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttack.Name = "nudAttack";
            this.nudAttack.Size = new System.Drawing.Size(146, 26);
            this.nudAttack.TabIndex = 8;
            this.nudAttack.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudAttack.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttack.Visible = false;
            this.nudAttack.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudAttHP
            // 
            this.nudAttHP.Location = new System.Drawing.Point(18, 85);
            this.nudAttHP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudAttHP.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudAttHP.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttHP.Name = "nudAttHP";
            this.nudAttHP.Size = new System.Drawing.Size(146, 26);
            this.nudAttHP.TabIndex = 9;
            this.nudAttHP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudAttHP.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAttHP.Visible = false;
            this.nudAttHP.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudDefHP
            // 
            this.nudDefHP.Location = new System.Drawing.Point(182, 85);
            this.nudDefHP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudDefHP.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudDefHP.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDefHP.Name = "nudDefHP";
            this.nudDefHP.Size = new System.Drawing.Size(146, 26);
            this.nudDefHP.TabIndex = 11;
            this.nudDefHP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudDefHP.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDefHP.Visible = false;
            this.nudDefHP.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // nudDefense
            // 
            this.nudDefense.Location = new System.Drawing.Point(182, 49);
            this.nudDefense.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudDefense.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudDefense.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDefense.Name = "nudDefense";
            this.nudDefense.Size = new System.Drawing.Size(146, 26);
            this.nudDefense.TabIndex = 10;
            this.nudDefense.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudDefense.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDefense.Visible = false;
            this.nudDefense.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // btnLog
            // 
            this.btnLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLog.AutoSize = true;
            this.btnLog.Location = new System.Drawing.Point(90, 320);
            this.btnLog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(111, 35);
            this.btnLog.TabIndex = 6;
            this.btnLog.Text = "Combat Log";
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // chkLog
            // 
            this.chkLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLog.AutoSize = true;
            this.chkLog.Location = new System.Drawing.Point(210, 328);
            this.chkLog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(113, 24);
            this.chkLog.TabIndex = 7;
            this.chkLog.Text = "Auto Show";
            this.chkLog.UseVisualStyleBackColor = true;
            // 
            // btnDetails
            // 
            this.btnDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDetails.AutoSize = true;
            this.btnDetails.Location = new System.Drawing.Point(96, 231);
            this.btnDetails.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(111, 35);
            this.btnDetails.TabIndex = 2;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Visible = false;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // lblInfDef
            // 
            this.lblInfDef.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfDef.AutoEllipsis = true;
            this.lblInfDef.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfDef.Location = new System.Drawing.Point(182, 120);
            this.lblInfDef.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfDef.Name = "lblInfDef";
            this.lblInfDef.Size = new System.Drawing.Size(146, 35);
            this.lblInfDef.TabIndex = 17;
            this.lblInfDef.Text = "lblInfDef";
            this.lblInfDef.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInfAtt
            // 
            this.lblInfAtt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfAtt.AutoEllipsis = true;
            this.lblInfAtt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfAtt.Location = new System.Drawing.Point(18, 120);
            this.lblInfAtt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfAtt.Name = "lblInfAtt";
            this.lblInfAtt.Size = new System.Drawing.Size(146, 35);
            this.lblInfAtt.TabIndex = 18;
            this.lblInfAtt.Text = "lblInfAtt";
            this.lblInfAtt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnChances
            // 
            this.btnChances.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChances.AutoSize = true;
            this.btnChances.Location = new System.Drawing.Point(216, 231);
            this.btnChances.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnChances.Name = "btnChances";
            this.btnChances.Size = new System.Drawing.Size(111, 35);
            this.btnChances.TabIndex = 3;
            this.btnChances.Text = "Chances";
            this.btnChances.UseVisualStyleBackColor = true;
            this.btnChances.Click += new System.EventHandler(this.btnChances_Click);
            // 
            // CombatForm
            // 
            this.AcceptButton = this.btnAttack;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(345, 434);
            this.Controls.Add(this.btnChances);
            this.Controls.Add(this.lblInfAtt);
            this.Controls.Add(this.lblInfDef);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.chkLog);
            this.Controls.Add(this.btnLog);
            this.Controls.Add(this.nudDefense);
            this.Controls.Add(this.nudDefHP);
            this.Controls.Add(this.nudAttHP);
            this.Controls.Add(this.nudAttack);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.lblDefPlayer);
            this.Controls.Add(this.lblAttPlayer);
            this.Controls.Add(this.lblDefKill);
            this.Controls.Add(this.lblDefDmg);
            this.Controls.Add(this.lblAttKill);
            this.Controls.Add(this.lblAttDmg);
            this.Controls.Add(this.btnSwap);
            this.Controls.Add(this.lblDefense);
            this.Controls.Add(this.lblDefHP);
            this.Controls.Add(this.lblAttack);
            this.Controls.Add(this.lblAttHP);
            this.Controls.Add(this.btnAttack);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "CombatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CombatForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudAttack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAttHP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDefHP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDefense)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAttack;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblAttack;
        private System.Windows.Forms.Label lblAttHP;
        private System.Windows.Forms.Label lblDefense;
        private System.Windows.Forms.Label lblDefHP;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.Label lblAttKill;
        private System.Windows.Forms.Label lblAttDmg;
        private System.Windows.Forms.Label lblDefKill;
        private System.Windows.Forms.Label lblDefDmg;
        private System.Windows.Forms.Label lblAttPlayer;
        private System.Windows.Forms.Label lblDefPlayer;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.NumericUpDown nudAttack;
        private System.Windows.Forms.NumericUpDown nudAttHP;
        private System.Windows.Forms.NumericUpDown nudDefHP;
        private System.Windows.Forms.NumericUpDown nudDefense;
        private System.Windows.Forms.Button btnLog;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Label lblInfDef;
        private System.Windows.Forms.Label lblInfAtt;
        private System.Windows.Forms.Button btnChances;
    }
}