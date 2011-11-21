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
			this.btnExact = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnAttack
			// 
			this.btnAttack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAttack.Location = new System.Drawing.Point(12, 218);
			this.btnAttack.Name = "btnAttack";
			this.btnAttack.Size = new System.Drawing.Size(100, 23);
			this.btnAttack.TabIndex = 29;
			this.btnAttack.Text = "Attack";
			this.btnAttack.UseVisualStyleBackColor = true;
			this.btnAttack.Click += new System.EventHandler(this.btnAttack_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(118, 218);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(100, 23);
			this.btnCancel.TabIndex = 28;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// lblAttack
			// 
			this.lblAttack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblAttack.Location = new System.Drawing.Point(12, 32);
			this.lblAttack.Name = "lblAttack";
			this.lblAttack.Size = new System.Drawing.Size(97, 23);
			this.lblAttack.TabIndex = 35;
			this.lblAttack.Text = "lblAttack";
			this.lblAttack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblAttHP
			// 
			this.lblAttHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblAttHP.Location = new System.Drawing.Point(12, 55);
			this.lblAttHP.Name = "lblAttHP";
			this.lblAttHP.Size = new System.Drawing.Size(97, 23);
			this.lblAttHP.TabIndex = 34;
			this.lblAttHP.Text = "lblAttHP";
			this.lblAttHP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblDefense
			// 
			this.lblDefense.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDefense.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDefense.Location = new System.Drawing.Point(121, 32);
			this.lblDefense.Name = "lblDefense";
			this.lblDefense.Size = new System.Drawing.Size(97, 23);
			this.lblDefense.TabIndex = 37;
			this.lblDefense.Text = "lblDefense";
			this.lblDefense.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblDefHP
			// 
			this.lblDefHP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDefHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDefHP.Location = new System.Drawing.Point(121, 55);
			this.lblDefHP.Name = "lblDefHP";
			this.lblDefHP.Size = new System.Drawing.Size(97, 23);
			this.lblDefHP.TabIndex = 36;
			this.lblDefHP.Text = "lblDefHP";
			this.lblDefHP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnSwap
			// 
			this.btnSwap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnSwap.Location = new System.Drawing.Point(65, 179);
			this.btnSwap.Name = "btnSwap";
			this.btnSwap.Size = new System.Drawing.Size(100, 23);
			this.btnSwap.TabIndex = 38;
			this.btnSwap.Text = "Swap";
			this.btnSwap.UseVisualStyleBackColor = true;
			this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
			// 
			// lblAttKill
			// 
			this.lblAttKill.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblAttKill.Location = new System.Drawing.Point(12, 124);
			this.lblAttKill.Name = "lblAttKill";
			this.lblAttKill.Size = new System.Drawing.Size(97, 23);
			this.lblAttKill.TabIndex = 40;
			this.lblAttKill.Text = "lblAttKill";
			this.lblAttKill.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblAttDmg
			// 
			this.lblAttDmg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblAttDmg.Location = new System.Drawing.Point(12, 101);
			this.lblAttDmg.Name = "lblAttDmg";
			this.lblAttDmg.Size = new System.Drawing.Size(97, 23);
			this.lblAttDmg.TabIndex = 39;
			this.lblAttDmg.Text = "lblAttDmg";
			this.lblAttDmg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblDefKill
			// 
			this.lblDefKill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDefKill.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDefKill.Location = new System.Drawing.Point(121, 124);
			this.lblDefKill.Name = "lblDefKill";
			this.lblDefKill.Size = new System.Drawing.Size(97, 23);
			this.lblDefKill.TabIndex = 42;
			this.lblDefKill.Text = "lblDefKill";
			this.lblDefKill.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblDefDmg
			// 
			this.lblDefDmg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDefDmg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDefDmg.Location = new System.Drawing.Point(121, 101);
			this.lblDefDmg.Name = "lblDefDmg";
			this.lblDefDmg.Size = new System.Drawing.Size(97, 23);
			this.lblDefDmg.TabIndex = 41;
			this.lblDefDmg.Text = "lblDefDmg";
			this.lblDefDmg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblAttPlayer
			// 
			this.lblAttPlayer.Location = new System.Drawing.Point(12, 9);
			this.lblAttPlayer.Name = "lblAttPlayer";
			this.lblAttPlayer.Size = new System.Drawing.Size(97, 23);
			this.lblAttPlayer.TabIndex = 43;
			this.lblAttPlayer.Text = "lblAttPlayer";
			this.lblAttPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblDefPlayer
			// 
			this.lblDefPlayer.Location = new System.Drawing.Point(121, 9);
			this.lblDefPlayer.Name = "lblDefPlayer";
			this.lblDefPlayer.Size = new System.Drawing.Size(97, 23);
			this.lblDefPlayer.TabIndex = 44;
			this.lblDefPlayer.Text = "lblDefPlayer";
			this.lblDefPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnExact
			// 
			this.btnExact.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnExact.Location = new System.Drawing.Point(65, 150);
			this.btnExact.Name = "btnExact";
			this.btnExact.Size = new System.Drawing.Size(100, 23);
			this.btnExact.TabIndex = 45;
			this.btnExact.Text = "Exact";
			this.btnExact.UseVisualStyleBackColor = true;
			this.btnExact.Visible = false;
			// 
			// CombatForm
			// 
			this.AcceptButton = this.btnAttack;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(230, 253);
			this.Controls.Add(this.btnExact);
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
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "CombatForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.ResumeLayout(false);

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
		private System.Windows.Forms.Button btnExact;
	}
}