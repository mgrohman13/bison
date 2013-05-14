namespace CityWarWinApp
{
	partial class Battle
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
            this.panelAttackers = new CityWarWinApp.PiecesPanel();
            this.panelDefenders = new CityWarWinApp.PiecesPanel();
            this.lbAttacks = new System.Windows.Forms.ListBox();
            this.btnEnd = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDam = new System.Windows.Forms.TextBox();
            this.txtAP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTargDmg = new System.Windows.Forms.TextBox();
            this.txtArmor = new System.Windows.Forms.TextBox();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTargets = new System.Windows.Forms.TextBox();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtChance = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtRelic = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.rbTrg = new System.Windows.Forms.RadioButton();
            this.rbRet = new System.Windows.Forms.RadioButton();
            this.label10 = new System.Windows.Forms.Label();
            this.cbSelected = new System.Windows.Forms.CheckBox();
            this.cbAttack = new System.Windows.Forms.CheckBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnCalc = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelAttackers
            // 
            this.panelAttackers.BackColor = System.Drawing.Color.White;
            this.panelAttackers.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelAttackers.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.panelAttackers.Location = new System.Drawing.Point(0, 0);
            this.panelAttackers.Name = "panelAttackers";
            this.panelAttackers.Size = new System.Drawing.Size(255, 600);
            this.panelAttackers.TabIndex = 0;
            this.panelAttackers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelAttackers_MouseDown);
            this.panelAttackers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelAttackers_MouseUp);
            // 
            // panelDefenders
            // 
            this.panelDefenders.BackColor = System.Drawing.Color.White;
            this.panelDefenders.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelDefenders.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.panelDefenders.Location = new System.Drawing.Point(645, 0);
            this.panelDefenders.Name = "panelDefenders";
            this.panelDefenders.Size = new System.Drawing.Size(255, 600);
            this.panelDefenders.TabIndex = 0;
            this.panelDefenders.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelDefenders_MouseDown);
            this.panelDefenders.MouseLeave += new System.EventHandler(this.panelDefenders_MouseLeave);
            this.panelDefenders.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelDefenders_MouseMove);
            this.panelDefenders.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelDefenders_MouseUp);
            // 
            // lbAttacks
            // 
            this.lbAttacks.BackColor = System.Drawing.Color.White;
            this.lbAttacks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbAttacks.ForeColor = System.Drawing.Color.Black;
            this.lbAttacks.FormattingEnabled = true;
            this.lbAttacks.ItemHeight = 16;
            this.lbAttacks.Location = new System.Drawing.Point(261, 12);
            this.lbAttacks.Name = "lbAttacks";
            this.lbAttacks.Size = new System.Drawing.Size(296, 52);
            this.lbAttacks.TabIndex = 1;
            this.lbAttacks.SelectedIndexChanged += new System.EventHandler(this.lbAttacks_SelectedIndexChanged);
            // 
            // btnEnd
            // 
            this.btnEnd.BackColor = System.Drawing.Color.White;
            this.btnEnd.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnEnd.ForeColor = System.Drawing.Color.Black;
            this.btnEnd.Location = new System.Drawing.Point(261, 314);
            this.btnEnd.Name = "btnEnd";
            this.btnEnd.Size = new System.Drawing.Size(130, 39);
            this.btnEnd.TabIndex = 0;
            this.btnEnd.Text = "Done";
            this.btnEnd.UseVisualStyleBackColor = false;
            this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(426, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Pierce";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(345, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "Damage";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtDam
            // 
            this.txtDam.BackColor = System.Drawing.Color.Silver;
            this.txtDam.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtDam.ForeColor = System.Drawing.Color.Black;
            this.txtDam.Location = new System.Drawing.Point(345, 88);
            this.txtDam.Name = "txtDam";
            this.txtDam.ReadOnly = true;
            this.txtDam.Size = new System.Drawing.Size(75, 22);
            this.txtDam.TabIndex = 5;
            this.txtDam.TabStop = false;
            // 
            // txtAP
            // 
            this.txtAP.BackColor = System.Drawing.Color.Silver;
            this.txtAP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtAP.ForeColor = System.Drawing.Color.Black;
            this.txtAP.Location = new System.Drawing.Point(426, 88);
            this.txtAP.Name = "txtAP";
            this.txtAP.ReadOnly = true;
            this.txtAP.Size = new System.Drawing.Size(61, 22);
            this.txtAP.TabIndex = 6;
            this.txtAP.TabStop = false;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(261, 239);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 18);
            this.label3.TabIndex = 7;
            this.label3.Text = "Target";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.White;
            this.label4.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(366, 239);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "Armor";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.White;
            this.label5.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(434, 239);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 18);
            this.label5.TabIndex = 9;
            this.label5.Text = "Damage";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtTargDmg
            // 
            this.txtTargDmg.BackColor = System.Drawing.Color.Silver;
            this.txtTargDmg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtTargDmg.ForeColor = System.Drawing.Color.Black;
            this.txtTargDmg.Location = new System.Drawing.Point(434, 260);
            this.txtTargDmg.Name = "txtTargDmg";
            this.txtTargDmg.ReadOnly = true;
            this.txtTargDmg.Size = new System.Drawing.Size(75, 22);
            this.txtTargDmg.TabIndex = 10;
            this.txtTargDmg.TabStop = false;
            // 
            // txtArmor
            // 
            this.txtArmor.BackColor = System.Drawing.Color.Silver;
            this.txtArmor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtArmor.ForeColor = System.Drawing.Color.Black;
            this.txtArmor.Location = new System.Drawing.Point(366, 260);
            this.txtArmor.Name = "txtArmor";
            this.txtArmor.ReadOnly = true;
            this.txtArmor.Size = new System.Drawing.Size(62, 22);
            this.txtArmor.TabIndex = 11;
            this.txtArmor.TabStop = false;
            // 
            // txtTarget
            // 
            this.txtTarget.BackColor = System.Drawing.Color.Silver;
            this.txtTarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtTarget.ForeColor = System.Drawing.Color.Black;
            this.txtTarget.Location = new System.Drawing.Point(261, 260);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.ReadOnly = true;
            this.txtTarget.Size = new System.Drawing.Size(99, 22);
            this.txtTarget.TabIndex = 12;
            this.txtTarget.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.White;
            this.label6.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(261, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 18);
            this.label6.TabIndex = 13;
            this.label6.Text = "Targets";
            this.label6.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtTargets
            // 
            this.txtTargets.BackColor = System.Drawing.Color.Silver;
            this.txtTargets.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtTargets.ForeColor = System.Drawing.Color.Black;
            this.txtTargets.Location = new System.Drawing.Point(261, 88);
            this.txtTargets.Name = "txtTargets";
            this.txtTargets.ReadOnly = true;
            this.txtTargets.Size = new System.Drawing.Size(78, 22);
            this.txtTargets.TabIndex = 14;
            this.txtTargets.TabStop = false;
            // 
            // txtLength
            // 
            this.txtLength.BackColor = System.Drawing.Color.Silver;
            this.txtLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtLength.ForeColor = System.Drawing.Color.Black;
            this.txtLength.Location = new System.Drawing.Point(493, 88);
            this.txtLength.Name = "txtLength";
            this.txtLength.ReadOnly = true;
            this.txtLength.Size = new System.Drawing.Size(67, 22);
            this.txtLength.TabIndex = 17;
            this.txtLength.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.White;
            this.label7.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(493, 67);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 18);
            this.label7.TabIndex = 16;
            this.label7.Text = "Length";
            this.label7.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtChance
            // 
            this.txtChance.BackColor = System.Drawing.Color.Silver;
            this.txtChance.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtChance.ForeColor = System.Drawing.Color.Black;
            this.txtChance.Location = new System.Drawing.Point(515, 260);
            this.txtChance.Name = "txtChance";
            this.txtChance.ReadOnly = true;
            this.txtChance.Size = new System.Drawing.Size(42, 22);
            this.txtChance.TabIndex = 19;
            this.txtChance.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.White;
            this.label8.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(515, 239);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 18);
            this.label8.TabIndex = 18;
            this.label8.Text = "Kill";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // txtRelic
            // 
            this.txtRelic.BackColor = System.Drawing.Color.Silver;
            this.txtRelic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.txtRelic.ForeColor = System.Drawing.Color.Black;
            this.txtRelic.Location = new System.Drawing.Point(563, 260);
            this.txtRelic.Name = "txtRelic";
            this.txtRelic.ReadOnly = true;
            this.txtRelic.Size = new System.Drawing.Size(51, 22);
            this.txtRelic.TabIndex = 21;
            this.txtRelic.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.White;
            this.label9.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.label9.ForeColor = System.Drawing.Color.Black;
            this.label9.Location = new System.Drawing.Point(563, 239);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 18);
            this.label9.TabIndex = 20;
            this.label9.Text = "Relic";
            this.label9.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // rbAll
            // 
            this.rbAll.AutoSize = true;
            this.rbAll.Checked = true;
            this.rbAll.Font = new System.Drawing.Font("Algerian", 12F);
            this.rbAll.ForeColor = System.Drawing.Color.White;
            this.rbAll.Location = new System.Drawing.Point(291, 160);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(56, 22);
            this.rbAll.TabIndex = 22;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "All";
            this.rbAll.UseVisualStyleBackColor = true;
            this.rbAll.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbTrg
            // 
            this.rbTrg.AutoSize = true;
            this.rbTrg.Font = new System.Drawing.Font("Algerian", 12F);
            this.rbTrg.ForeColor = System.Drawing.Color.White;
            this.rbTrg.Location = new System.Drawing.Point(353, 160);
            this.rbTrg.Name = "rbTrg";
            this.rbTrg.Size = new System.Drawing.Size(128, 22);
            this.rbTrg.TabIndex = 23;
            this.rbTrg.Text = "Targetable";
            this.rbTrg.UseVisualStyleBackColor = true;
            this.rbTrg.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbRet
            // 
            this.rbRet.AutoSize = true;
            this.rbRet.Font = new System.Drawing.Font("Algerian", 12F);
            this.rbRet.ForeColor = System.Drawing.Color.White;
            this.rbRet.Location = new System.Drawing.Point(487, 160);
            this.rbRet.Name = "rbRet";
            this.rbRet.Size = new System.Drawing.Size(121, 22);
            this.rbRet.TabIndex = 24;
            this.rbRet.Text = "Retalliate";
            this.rbRet.UseVisualStyleBackColor = true;
            this.rbRet.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Algerian", 12F);
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(342, 139);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(156, 18);
            this.label10.TabIndex = 25;
            this.label10.Text = "Filter Defenders:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cbSelected
            // 
            this.cbSelected.AutoSize = true;
            this.cbSelected.Font = new System.Drawing.Font("Algerian", 12F);
            this.cbSelected.ForeColor = System.Drawing.Color.White;
            this.cbSelected.Location = new System.Drawing.Point(378, 188);
            this.cbSelected.Name = "cbSelected";
            this.cbSelected.Size = new System.Drawing.Size(103, 22);
            this.cbSelected.TabIndex = 28;
            this.cbSelected.Text = "Selected";
            this.cbSelected.UseVisualStyleBackColor = true;
            this.cbSelected.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // cbAttack
            // 
            this.cbAttack.AutoSize = true;
            this.cbAttack.Font = new System.Drawing.Font("Algerian", 12F);
            this.cbAttack.ForeColor = System.Drawing.Color.White;
            this.cbAttack.Location = new System.Drawing.Point(487, 188);
            this.cbAttack.Name = "cbAttack";
            this.cbAttack.Size = new System.Drawing.Size(89, 22);
            this.cbAttack.TabIndex = 29;
            this.cbAttack.Text = "Attack";
            this.cbAttack.UseVisualStyleBackColor = true;
            this.cbAttack.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtLog.Location = new System.Drawing.Point(261, 385);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(378, 203);
            this.txtLog.TabIndex = 30;
            // 
            // btnCalc
            // 
            this.btnCalc.AutoSize = true;
            this.btnCalc.BackColor = System.Drawing.Color.White;
            this.btnCalc.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.btnCalc.ForeColor = System.Drawing.Color.Black;
            this.btnCalc.Location = new System.Drawing.Point(518, 319);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(121, 28);
            this.btnCalc.TabIndex = 31;
            this.btnCalc.Text = "Calculator";
            this.btnCalc.UseVisualStyleBackColor = false;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // Battle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.cbAttack);
            this.Controls.Add(this.cbSelected);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.rbRet);
            this.Controls.Add(this.rbTrg);
            this.Controls.Add(this.rbAll);
            this.Controls.Add(this.txtRelic);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtChance);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtLength);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtTargets);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtTarget);
            this.Controls.Add(this.txtArmor);
            this.Controls.Add(this.txtTargDmg);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAP);
            this.Controls.Add(this.txtDam);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.lbAttacks);
            this.Controls.Add(this.panelDefenders);
            this.Controls.Add(this.panelAttackers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Battle";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Battle";
            this.Load += new System.EventHandler(this.Battle_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private PiecesPanel panelAttackers;
		private PiecesPanel panelDefenders;
		private System.Windows.Forms.ListBox lbAttacks;
		private System.Windows.Forms.Button btnEnd;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtDam;
		private System.Windows.Forms.TextBox txtAP;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtTargDmg;
		private System.Windows.Forms.TextBox txtArmor;
		private System.Windows.Forms.TextBox txtTarget;
		private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTargets;
		private System.Windows.Forms.TextBox txtLength;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txtChance;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox txtRelic;
		private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.RadioButton rbTrg;
        private System.Windows.Forms.RadioButton rbRet;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox cbSelected;
        private System.Windows.Forms.CheckBox cbAttack;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnCalc;
	}
}