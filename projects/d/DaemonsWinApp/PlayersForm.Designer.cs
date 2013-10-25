namespace DaemonsWinApp
{
    partial class PlayersForm
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
            this.lbxPlayer = new System.Windows.Forms.ListBox();
            this.lbxSoul = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbxStr = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lbxProd = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbxPlayer
            // 
            this.lbxPlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lbxPlayer.FormattingEnabled = true;
            this.lbxPlayer.ItemHeight = 16;
            this.lbxPlayer.Location = new System.Drawing.Point(12, 35);
            this.lbxPlayer.Name = "lbxPlayer";
            this.lbxPlayer.Size = new System.Drawing.Size(120, 116);
            this.lbxPlayer.TabIndex = 0;
            this.lbxPlayer.SelectedIndexChanged += new System.EventHandler(this.lbx_SelectedIndexChanged);
            // 
            // lbxSoul
            // 
            this.lbxSoul.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbxSoul.FormattingEnabled = true;
            this.lbxSoul.ItemHeight = 16;
            this.lbxSoul.Location = new System.Drawing.Point(138, 35);
            this.lbxSoul.Name = "lbxSoul";
            this.lbxSoul.Size = new System.Drawing.Size(120, 116);
            this.lbxSoul.TabIndex = 1;
            this.lbxSoul.SelectedIndexChanged += new System.EventHandler(this.lbx_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "PLAYER";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(135, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "SOULS";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(261, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 23);
            this.label3.TabIndex = 5;
            this.label3.Text = "STRENGTH";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lbxStr
            // 
            this.lbxStr.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbxStr.FormattingEnabled = true;
            this.lbxStr.ItemHeight = 16;
            this.lbxStr.Location = new System.Drawing.Point(264, 35);
            this.lbxStr.Name = "lbxStr";
            this.lbxStr.Size = new System.Drawing.Size(120, 116);
            this.lbxStr.TabIndex = 4;
            this.lbxStr.SelectedIndexChanged += new System.EventHandler(this.lbx_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(387, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 23);
            this.label4.TabIndex = 7;
            this.label4.Text = "PRODUCTION";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lbxProd
            // 
            this.lbxProd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lbxProd.FormattingEnabled = true;
            this.lbxProd.ItemHeight = 16;
            this.lbxProd.Location = new System.Drawing.Point(390, 35);
            this.lbxProd.Name = "lbxProd";
            this.lbxProd.Size = new System.Drawing.Size(120, 116);
            this.lbxProd.TabIndex = 6;
            this.lbxProd.SelectedIndexChanged += new System.EventHandler(this.lbx_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.button1.Location = new System.Drawing.Point(435, 157);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Done";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // PlayersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(682, 262);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbxProd);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbxStr);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbxSoul);
            this.Controls.Add(this.lbxPlayer);
            this.Name = "PlayersForm";
            this.Text = "PlayersForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbxPlayer;
        private System.Windows.Forms.ListBox lbxSoul;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lbxStr;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lbxProd;
        private System.Windows.Forms.Button button1;
    }
}