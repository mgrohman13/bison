namespace DaemonsWinApp
{
    partial class InfoForm
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
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.pnlMove = new System.Windows.Forms.Panel();
            this.chbAll = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.pnlMove.SuspendLayout();
            this.SuspendLayout();
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(268, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(16, 262);
            this.vScrollBar1.TabIndex = 0;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // pnlMove
            // 
            this.pnlMove.Controls.Add(this.chbAll);
            this.pnlMove.Controls.Add(this.btnOk);
            this.pnlMove.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlMove.Location = new System.Drawing.Point(0, 224);
            this.pnlMove.Name = "pnlMove";
            this.pnlMove.Size = new System.Drawing.Size(268, 38);
            this.pnlMove.TabIndex = 3;
            this.pnlMove.Visible = false;
            // 
            // chbAll
            // 
            this.chbAll.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.chbAll.AutoSize = true;
            this.chbAll.Location = new System.Drawing.Point(198, 7);
            this.chbAll.Name = "chbAll";
            this.chbAll.Size = new System.Drawing.Size(67, 17);
            this.chbAll.TabIndex = 5;
            this.chbAll.Text = "Show All";
            this.chbAll.UseVisualStyleBackColor = true;
            this.chbAll.CheckedChanged += new System.EventHandler(this.chbAll_CheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.BackColor = System.Drawing.Color.Silver;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(101, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = false;
            // 
            // InfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.pnlMove);
            this.Controls.Add(this.vScrollBar1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InfoForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "infoForm";
            this.SizeChanged += new System.EventHandler(this.infoForm_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.infoForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.infoForm_KeyUp);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.infoForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.infoForm_MouseUp);
            this.pnlMove.ResumeLayout(false);
            this.pnlMove.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.VScrollBar vScrollBar1;
		private System.Windows.Forms.Panel pnlMove;
		private System.Windows.Forms.CheckBox chbAll;
		private System.Windows.Forms.Button btnOk;
    }
}