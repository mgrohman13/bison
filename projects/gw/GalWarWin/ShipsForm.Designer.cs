namespace GalWarWin
{
    partial class ShipsForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Att = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Def = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Spd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Upk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Pop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Other = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ( (System.ComponentModel.ISupportInitialize)( this.dataGridView1 ) ).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
             this.Type,
            this.Att,
            this.Def,
            this.HP,
            this.Spd,
            this.Upk,
            this.Exp,
            this.Pop,
            this.Other});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(657, 262);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_ColumnHeaderMouseClick);
            // 
            // Type
            // 
            this.Type.DataPropertyName = "Type";
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.Width = 56;
            // 
            // Att
            // 
            this.Att.DataPropertyName = "Att";
            this.Att.HeaderText = "Att";
            this.Att.Name = "Att";
            this.Att.ReadOnly = true;
            this.Att.Width = 45;
            // 
            // Def
            // 
            this.Def.DataPropertyName = "Def";
            this.Def.HeaderText = "Def";
            this.Def.Name = "Def";
            this.Def.ReadOnly = true;
            this.Def.Width = 49;
            // 
            // HP
            // 
            this.HP.DataPropertyName = "HP";
            this.HP.HeaderText = "HP";
            this.HP.Name = "HP";
            this.HP.ReadOnly = true;
            this.HP.Width = 47;
            // 
            // Spd
            // 
            this.Spd.DataPropertyName = "Spd";
            this.Spd.HeaderText = "Spd";
            this.Spd.Name = "Spd";
            this.Spd.ReadOnly = true;
            this.Spd.Width = 51;
            // 
            // Upk
            // 
            this.Upk.DataPropertyName = "Upk";
            this.Upk.HeaderText = "Upk";
            this.Upk.Name = "Upk";
            this.Upk.ReadOnly = true;
            this.Upk.Width = 52;
            // 
            // Exp
            // 
            this.Exp.DataPropertyName = "Exp";
            this.Exp.HeaderText = "Exp";
            this.Exp.Name = "Exp";
            this.Exp.ReadOnly = true;
            this.Exp.Width = 50;
            // 
            // Pop
            // 
            this.Pop.DataPropertyName = "Pop";
            this.Pop.HeaderText = "Pop";
            this.Pop.Name = "Pop";
            this.Pop.ReadOnly = true;
            this.Pop.Width = 51;
            // 
            // Other
            // 
            this.Other.DataPropertyName = "Other";
            this.Other.HeaderText = "";
            this.Other.Name = "Other";
            this.Other.ReadOnly = true;
            this.Other.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Other.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Other.Width = 19;
            // 
            // ShipsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(657, 262);
            this.Controls.Add(this.dataGridView1);
            this.Name = "ShipsForm";
            ( (System.ComponentModel.ISupportInitialize)( this.dataGridView1 ) ).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Att;
        private System.Windows.Forms.DataGridViewTextBoxColumn Def;
        private System.Windows.Forms.DataGridViewTextBoxColumn HP;
        private System.Windows.Forms.DataGridViewTextBoxColumn Spd;
        private System.Windows.Forms.DataGridViewTextBoxColumn Upk;
        private System.Windows.Forms.DataGridViewTextBoxColumn Exp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pop;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Other;
    }
}