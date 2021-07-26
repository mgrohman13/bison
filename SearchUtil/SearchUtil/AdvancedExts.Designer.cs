namespace SearchUtil
{
    partial class AdvancedExts
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
            this.rbOnly = new System.Windows.Forms.RadioButton();
            this.rbNot = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtExt = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.txtOnly = new System.Windows.Forms.TextBox();
            this.txtNot = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // rbOnly
            // 
            this.rbOnly.AutoSize = true;
            this.rbOnly.Location = new System.Drawing.Point(12, 12);
            this.rbOnly.Name = "rbOnly";
            this.rbOnly.Size = new System.Drawing.Size(86, 17);
            this.rbOnly.TabIndex = 0;
            this.rbOnly.TabStop = true;
            this.rbOnly.Text = "Only Search:";
            this.rbOnly.UseVisualStyleBackColor = true;
            this.rbOnly.KeyUp += new System.Windows.Forms.KeyEventHandler(this.radioButton_KeyUp);
            this.rbOnly.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // rbNot
            // 
            this.rbNot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rbNot.AutoSize = true;
            this.rbNot.Location = new System.Drawing.Point(149, 12);
            this.rbNot.Name = "rbNot";
            this.rbNot.Size = new System.Drawing.Size(99, 17);
            this.rbNot.TabIndex = 1;
            this.rbNot.TabStop = true;
            this.rbNot.Text = "Do Not Search:";
            this.rbNot.UseVisualStyleBackColor = true;
            this.rbNot.KeyUp += new System.Windows.Forms.KeyEventHandler(this.radioButton_KeyUp);
            this.rbNot.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(124, 238);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(205, 238);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // txtExt
            // 
            this.txtExt.Location = new System.Drawing.Point(12, 203);
            this.txtExt.Name = "txtExt";
            this.txtExt.Size = new System.Drawing.Size(100, 20);
            this.txtExt.TabIndex = 4;
            this.txtExt.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtExt_KeyUp);
            // 
            // btnAdd
            // 
            this.btnAdd.AutoSize = true;
            this.btnAdd.Location = new System.Drawing.Point(118, 201);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(36, 23);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.AutoSize = true;
            this.btnDelete.Location = new System.Drawing.Point(187, 201);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(93, 23);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Visible = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // txtOnly
            // 
            this.txtOnly.Location = new System.Drawing.Point(12, 35);
            this.txtOnly.Multiline = true;
            this.txtOnly.Name = "txtOnly";
            this.txtOnly.Size = new System.Drawing.Size(131, 160);
            this.txtOnly.TabIndex = 2;
            // 
            // txtNot
            // 
            this.txtNot.Location = new System.Drawing.Point(149, 35);
            this.txtNot.Multiline = true;
            this.txtNot.Name = "txtNot";
            this.txtNot.Size = new System.Drawing.Size(131, 160);
            this.txtNot.TabIndex = 3;
            // 
            // AdvancedExts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.txtNot);
            this.Controls.Add(this.txtOnly);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtExt);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.rbNot);
            this.Controls.Add(this.rbOnly);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(300, 300);
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "AdvancedExts";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Advanced Extension Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbOnly;
        private System.Windows.Forms.RadioButton rbNot;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox txtExt;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.TextBox txtOnly;
        private System.Windows.Forms.TextBox txtNot;
    }
}