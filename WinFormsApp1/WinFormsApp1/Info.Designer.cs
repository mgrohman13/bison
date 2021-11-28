
namespace WinFormsApp1
{
    partial class Info
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnEndTurn = new System.Windows.Forms.Button();
            this.lbl1 = new System.Windows.Forms.Label();
            this.lblInf1 = new System.Windows.Forms.Label();
            this.lblInf2 = new System.Windows.Forms.Label();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lblInf3 = new System.Windows.Forms.Label();
            this.lbl3 = new System.Windows.Forms.Label();
            this.dgvAttacks = new System.Windows.Forms.DataGridView();
            this.lblInf4 = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.lblTurn = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttacks)).BeginInit();
            this.SuspendLayout();
            // 
            // btnEndTurn
            // 
            this.btnEndTurn.Location = new System.Drawing.Point(0, 0);
            this.btnEndTurn.Name = "btnEndTurn";
            this.btnEndTurn.Size = new System.Drawing.Size(112, 34);
            this.btnEndTurn.TabIndex = 0;
            this.btnEndTurn.Text = "End Turn";
            this.btnEndTurn.UseVisualStyleBackColor = true;
            this.btnEndTurn.Click += new System.EventHandler(this.Button1_Click);
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(7, 39);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(41, 25);
            this.lbl1.TabIndex = 1;
            this.lbl1.Text = "lbl1";
            // 
            // lblInf1
            // 
            this.lblInf1.AutoSize = true;
            this.lblInf1.Location = new System.Drawing.Point(72, 39);
            this.lblInf1.Name = "lblInf1";
            this.lblInf1.Size = new System.Drawing.Size(62, 25);
            this.lblInf1.TabIndex = 2;
            this.lblInf1.Text = "lblInf1";
            // 
            // lblInf2
            // 
            this.lblInf2.AutoSize = true;
            this.lblInf2.Location = new System.Drawing.Point(72, 64);
            this.lblInf2.Name = "lblInf2";
            this.lblInf2.Size = new System.Drawing.Size(62, 25);
            this.lblInf2.TabIndex = 4;
            this.lblInf2.Text = "lblInf2";
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(7, 64);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(41, 25);
            this.lbl2.TabIndex = 3;
            this.lbl2.Text = "lbl2";
            // 
            // lblInf3
            // 
            this.lblInf3.AutoSize = true;
            this.lblInf3.Location = new System.Drawing.Point(72, 89);
            this.lblInf3.Name = "lblInf3";
            this.lblInf3.Size = new System.Drawing.Size(62, 25);
            this.lblInf3.TabIndex = 6;
            this.lblInf3.Text = "lblInf3";
            // 
            // lbl3
            // 
            this.lbl3.AutoSize = true;
            this.lbl3.Location = new System.Drawing.Point(7, 89);
            this.lbl3.Name = "lbl3";
            this.lbl3.Size = new System.Drawing.Size(41, 25);
            this.lbl3.TabIndex = 5;
            this.lbl3.Text = "lbl3";
            // 
            // dgvAttacks
            // 
            this.dgvAttacks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAttacks.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvAttacks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttacks.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvAttacks.Location = new System.Drawing.Point(0, 375);
            this.dgvAttacks.Name = "dgvAttacks";
            this.dgvAttacks.RowHeadersVisible = false;
            this.dgvAttacks.RowHeadersWidth = 62;
            this.dgvAttacks.RowTemplate.Height = 33;
            this.dgvAttacks.Size = new System.Drawing.Size(300, 225);
            this.dgvAttacks.TabIndex = 9;
            // 
            // lblInf4
            // 
            this.lblInf4.AutoSize = true;
            this.lblInf4.Location = new System.Drawing.Point(72, 114);
            this.lblInf4.Name = "lblInf4";
            this.lblInf4.Size = new System.Drawing.Size(62, 25);
            this.lblInf4.TabIndex = 11;
            this.lblInf4.Text = "lblInf4";
            // 
            // lbl4
            // 
            this.lbl4.AutoSize = true;
            this.lbl4.Location = new System.Drawing.Point(7, 114);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(41, 25);
            this.lbl4.TabIndex = 10;
            this.lbl4.Text = "lbl4";
            // 
            // lblTurn
            // 
            this.lblTurn.Location = new System.Drawing.Point(118, 0);
            this.lblTurn.Name = "lblTurn";
            this.lblTurn.Size = new System.Drawing.Size(182, 34);
            this.lblTurn.TabIndex = 12;
            this.lblTurn.Text = "lblTurn";
            this.lblTurn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblTurn);
            this.Controls.Add(this.lblInf4);
            this.Controls.Add(this.lbl4);
            this.Controls.Add(this.dgvAttacks);
            this.Controls.Add(this.lblInf3);
            this.Controls.Add(this.lbl3);
            this.Controls.Add(this.lblInf2);
            this.Controls.Add(this.lbl2);
            this.Controls.Add(this.lblInf1);
            this.Controls.Add(this.lbl1);
            this.Controls.Add(this.btnEndTurn);
            this.Name = "Info";
            this.Size = new System.Drawing.Size(300, 600);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttacks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lblInf1;
        private System.Windows.Forms.Label lblInf2;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Label lblInf3;
        private System.Windows.Forms.Label lbl3;
        private System.Windows.Forms.DataGridView dgvAttacks;
        private System.Windows.Forms.Button btnEndTurn;
        private System.Windows.Forms.Label lblInf4;
        private System.Windows.Forms.Label lbl4;
        private System.Windows.Forms.Label lblTurn;
    }
}
