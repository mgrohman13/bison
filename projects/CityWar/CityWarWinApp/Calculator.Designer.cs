namespace CityWarWinApp
{
    partial class Calculator
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
            this.lbAttacks = new System.Windows.Forms.ListBox();
            this.lbDefenders = new System.Windows.Forms.ListBox();
            this.txtRelic = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtKill = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtHits = new System.Windows.Forms.TextBox();
            this.txtArmor = new System.Windows.Forms.TextBox();
            this.txtAverage = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPierce = new System.Windows.Forms.TextBox();
            this.txtDamage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbAttacks
            // 
            this.lbAttacks.BackColor = System.Drawing.Color.White;
            this.lbAttacks.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbAttacks.ForeColor = System.Drawing.Color.Black;
            this.lbAttacks.FormattingEnabled = true;
            this.lbAttacks.ItemHeight = 16;
            this.lbAttacks.Location = new System.Drawing.Point(219, 55);
            this.lbAttacks.Name = "lbAttacks";
            this.lbAttacks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbAttacks.Size = new System.Drawing.Size(186, 276);
            this.lbAttacks.TabIndex = 34;
            this.lbAttacks.Visible = false;
            this.lbAttacks.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            // 
            // lbDefenders
            // 
            this.lbDefenders.BackColor = System.Drawing.Color.White;
            this.lbDefenders.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbDefenders.ForeColor = System.Drawing.Color.Black;
            this.lbDefenders.FormattingEnabled = true;
            this.lbDefenders.ItemHeight = 16;
            this.lbDefenders.Location = new System.Drawing.Point(238, 12);
            this.lbDefenders.Name = "lbDefenders";
            this.lbDefenders.Size = new System.Drawing.Size(186, 276);
            this.lbDefenders.TabIndex = 35;
            this.lbDefenders.Visible = false;
            this.lbDefenders.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            // 
            // txtRelic
            // 
            this.txtRelic.BackColor = System.Drawing.Color.Silver;
            this.txtRelic.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtRelic.ForeColor = System.Drawing.Color.Black;
            this.txtRelic.Location = new System.Drawing.Point(120, 122);
            this.txtRelic.Name = "txtRelic";
            this.txtRelic.ReadOnly = true;
            this.txtRelic.Size = new System.Drawing.Size(44, 22);
            this.txtRelic.TabIndex = 53;
            this.txtRelic.TabStop = false;
            this.txtRelic.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Black;
            this.label9.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label9.ForeColor = System.Drawing.Color.Silver;
            this.label9.Location = new System.Drawing.Point(120, 101);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 18);
            this.label9.TabIndex = 52;
            this.label9.Text = "Relic";
            this.label9.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label9.Visible = false;
            // 
            // txtKill
            // 
            this.txtKill.BackColor = System.Drawing.Color.Silver;
            this.txtKill.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtKill.ForeColor = System.Drawing.Color.Black;
            this.txtKill.Location = new System.Drawing.Point(83, 122);
            this.txtKill.Name = "txtKill";
            this.txtKill.ReadOnly = true;
            this.txtKill.Size = new System.Drawing.Size(31, 22);
            this.txtKill.TabIndex = 51;
            this.txtKill.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Black;
            this.label8.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label8.ForeColor = System.Drawing.Color.Silver;
            this.label8.Location = new System.Drawing.Point(83, 101);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 18);
            this.label8.TabIndex = 50;
            this.label8.Text = "Kill";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtHits
            // 
            this.txtHits.BackColor = System.Drawing.Color.White;
            this.txtHits.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtHits.ForeColor = System.Drawing.Color.Black;
            this.txtHits.Location = new System.Drawing.Point(12, 76);
            this.txtHits.Name = "txtHits";
            this.txtHits.Size = new System.Drawing.Size(36, 22);
            this.txtHits.TabIndex = 45;
            this.txtHits.TabStop = false;
            this.txtHits.TextChanged += new System.EventHandler(this.txtHits_TextChanged);
            // 
            // txtArmor
            // 
            this.txtArmor.BackColor = System.Drawing.Color.White;
            this.txtArmor.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtArmor.ForeColor = System.Drawing.Color.Black;
            this.txtArmor.Location = new System.Drawing.Point(54, 76);
            this.txtArmor.Name = "txtArmor";
            this.txtArmor.Size = new System.Drawing.Size(50, 22);
            this.txtArmor.TabIndex = 44;
            this.txtArmor.TabStop = false;
            this.txtArmor.TextChanged += new System.EventHandler(this.txtArmor_TextChanged);
            // 
            // txtAverage
            // 
            this.txtAverage.BackColor = System.Drawing.Color.Silver;
            this.txtAverage.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtAverage.ForeColor = System.Drawing.Color.Black;
            this.txtAverage.Location = new System.Drawing.Point(12, 122);
            this.txtAverage.Name = "txtAverage";
            this.txtAverage.ReadOnly = true;
            this.txtAverage.Size = new System.Drawing.Size(66, 22);
            this.txtAverage.TabIndex = 43;
            this.txtAverage.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Black;
            this.label5.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label5.ForeColor = System.Drawing.Color.Silver;
            this.label5.Location = new System.Drawing.Point(12, 101);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 18);
            this.label5.TabIndex = 42;
            this.label5.Text = "Average";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Black;
            this.label4.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label4.ForeColor = System.Drawing.Color.Silver;
            this.label4.Location = new System.Drawing.Point(54, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 18);
            this.label4.TabIndex = 41;
            this.label4.Text = "Armor";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label3.ForeColor = System.Drawing.Color.Silver;
            this.label3.Location = new System.Drawing.Point(12, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 18);
            this.label3.TabIndex = 40;
            this.label3.Text = "Hits";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtPierce
            // 
            this.txtPierce.BackColor = System.Drawing.Color.White;
            this.txtPierce.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtPierce.ForeColor = System.Drawing.Color.Black;
            this.txtPierce.Location = new System.Drawing.Point(83, 30);
            this.txtPierce.Name = "txtPierce";
            this.txtPierce.Size = new System.Drawing.Size(54, 22);
            this.txtPierce.TabIndex = 39;
            this.txtPierce.TabStop = false;
            this.txtPierce.TextChanged += new System.EventHandler(this.txtPierce_TextChanged);
            // 
            // txtDamage
            // 
            this.txtDamage.BackColor = System.Drawing.Color.White;
            this.txtDamage.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtDamage.ForeColor = System.Drawing.Color.Black;
            this.txtDamage.Location = new System.Drawing.Point(12, 30);
            this.txtDamage.Name = "txtDamage";
            this.txtDamage.Size = new System.Drawing.Size(65, 22);
            this.txtDamage.TabIndex = 38;
            this.txtDamage.TabStop = false;
            this.txtDamage.TextChanged += new System.EventHandler(this.txtDamage_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label2.ForeColor = System.Drawing.Color.Silver;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 18);
            this.label2.TabIndex = 37;
            this.label2.Text = "Damage";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label1.ForeColor = System.Drawing.Color.Silver;
            this.label1.Location = new System.Drawing.Point(83, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 18);
            this.label1.TabIndex = 36;
            this.label1.Text = "Pierce";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // btnDone
            // 
            this.btnDone.BackColor = System.Drawing.Color.Silver;
            this.btnDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnDone.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnDone.ForeColor = System.Drawing.Color.Black;
            this.btnDone.Location = new System.Drawing.Point(184, 121);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(86, 25);
            this.btnDone.TabIndex = 54;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = false;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // Calculator
            // 
            this.AcceptButton = this.btnDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.CancelButton = this.btnDone;
            this.ClientSize = new System.Drawing.Size(284, 156);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.txtRelic);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtKill);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtHits);
            this.Controls.Add(this.txtArmor);
            this.Controls.Add(this.txtAverage);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPierce);
            this.Controls.Add(this.txtDamage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbDefenders);
            this.Controls.Add(this.lbAttacks);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Calculator";
            this.Load += new System.EventHandler(this.Calculator_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbAttacks;
        private System.Windows.Forms.ListBox lbDefenders;
        private System.Windows.Forms.TextBox txtRelic;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtKill;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtHits;
        private System.Windows.Forms.TextBox txtArmor;
        private System.Windows.Forms.TextBox txtAverage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPierce;
        private System.Windows.Forms.TextBox txtDamage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDone;
    }
}