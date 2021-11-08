namespace GalWarWin.Sliders
{
    partial class AutoRepairControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbAuto = new System.Windows.Forms.CheckBox();
            this.btnAuto = new System.Windows.Forms.Button();
            this.cbxProdRepair = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbAuto
            // 
            this.cbAuto.AutoSize = true;
            this.cbAuto.Location = new System.Drawing.Point(4, 8);
            this.cbAuto.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbAuto.Name = "cbAuto";
            this.cbAuto.Size = new System.Drawing.Size(22, 21);
            this.cbAuto.TabIndex = 0;
            this.cbAuto.UseVisualStyleBackColor = true;
            this.cbAuto.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnAuto
            // 
            this.btnAuto.Location = new System.Drawing.Point(36, 0);
            this.btnAuto.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAuto.Name = "btnAuto";
            this.btnAuto.Size = new System.Drawing.Size(112, 35);
            this.btnAuto.TabIndex = 1;
            this.btnAuto.Text = "Auto";
            this.btnAuto.UseVisualStyleBackColor = true;
            this.btnAuto.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbxProdRepair
            // 
            this.cbxProdRepair.AutoSize = true;
            this.cbxProdRepair.Location = new System.Drawing.Point(156, 8);
            this.cbxProdRepair.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbxProdRepair.Name = "cbxProdRepair";
            this.cbxProdRepair.Size = new System.Drawing.Size(131, 24);
            this.cbxProdRepair.TabIndex = 2;
            this.cbxProdRepair.Text = "Infrastructure";
            this.cbxProdRepair.UseVisualStyleBackColor = true;
            this.cbxProdRepair.CheckedChanged += new System.EventHandler(this.cbxProdRepair_CheckedChanged);
            // 
            // AutoRepairControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbxProdRepair);
            this.Controls.Add(this.btnAuto);
            this.Controls.Add(this.cbAuto);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "AutoRepairControl";
            this.Size = new System.Drawing.Size(286, 35);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbAuto;
        private System.Windows.Forms.Button btnAuto;
        private System.Windows.Forms.CheckBox cbxProdRepair;
    }
}
