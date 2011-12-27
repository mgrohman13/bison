namespace GalWarWin.Sliders
{
    partial class SliderForm
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
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtAmt = new System.Windows.Forms.TextBox();
            this.lblAmt = new System.Windows.Forms.Label();
            this.lblEffcnt = new System.Windows.Forms.Label();
            this.lblResultType = new System.Windows.Forms.Label();
            this.btnCanel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblExtra = new System.Windows.Forms.Label();
            this.lblSlideType = new System.Windows.Forms.Label();
            ( (System.ComponentModel.ISupportInitialize)( this.trackBar ) ).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar
            // 
            this.trackBar.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.trackBar.LargeChange = 1;
            this.trackBar.Location = new System.Drawing.Point(0, 23);
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(250, 45);
            this.trackBar.TabIndex = 1;
            this.trackBar.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 23);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "lblTitle";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtAmt
            // 
            this.txtAmt.Location = new System.Drawing.Point(0, 65);
            this.txtAmt.Name = "txtAmt";
            this.txtAmt.Size = new System.Drawing.Size(100, 20);
            this.txtAmt.TabIndex = 2;
            this.txtAmt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtAmt.TextChanged += new System.EventHandler(this.txtAmt_TextChanged);
            // 
            // lblAmt
            // 
            this.lblAmt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblAmt.Location = new System.Drawing.Point(0, 85);
            this.lblAmt.Name = "lblAmt";
            this.lblAmt.Size = new System.Drawing.Size(100, 23);
            this.lblAmt.TabIndex = 5;
            this.lblAmt.Text = "lblAmt";
            this.lblAmt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEffcnt
            // 
            this.lblEffcnt.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblEffcnt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            this.lblEffcnt.Location = new System.Drawing.Point(138, 85);
            this.lblEffcnt.Name = "lblEffcnt";
            this.lblEffcnt.Size = new System.Drawing.Size(100, 23);
            this.lblEffcnt.TabIndex = 7;
            this.lblEffcnt.Text = "lblEffcnt";
            this.lblEffcnt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblResultType
            // 
            this.lblResultType.AutoSize = true;
            this.lblResultType.Location = new System.Drawing.Point(106, 90);
            this.lblResultType.Name = "lblResultType";
            this.lblResultType.Size = new System.Drawing.Size(71, 13);
            this.lblResultType.TabIndex = 6;
            this.lblResultType.Text = "lblResultType";
            // 
            // btnCanel
            // 
            this.btnCanel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnCanel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCanel.Location = new System.Drawing.Point(150, 108);
            this.btnCanel.Name = "btnCanel";
            this.btnCanel.Size = new System.Drawing.Size(100, 23);
            this.btnCanel.TabIndex = 9;
            this.btnCanel.Text = "Cancel";
            this.btnCanel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(50, 108);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // lblExtra
            // 
            this.lblExtra.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.lblExtra.Location = new System.Drawing.Point(138, 63);
            this.lblExtra.Name = "lblExtra";
            this.lblExtra.Size = new System.Drawing.Size(100, 23);
            this.lblExtra.TabIndex = 4;
            this.lblExtra.Text = "lblExtra";
            this.lblExtra.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSlideType
            // 
            this.lblSlideType.AutoSize = true;
            this.lblSlideType.Location = new System.Drawing.Point(106, 68);
            this.lblSlideType.Name = "lblSlideType";
            this.lblSlideType.Size = new System.Drawing.Size(40, 13);
            this.lblSlideType.TabIndex = 3;
            this.lblSlideType.Text = "Troops";
            // 
            // SliderForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCanel;
            this.ClientSize = new System.Drawing.Size(250, 131);
            this.Controls.Add(this.lblSlideType);
            this.Controls.Add(this.lblExtra);
            this.Controls.Add(this.btnCanel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblResultType);
            this.Controls.Add(this.lblEffcnt);
            this.Controls.Add(this.lblAmt);
            this.Controls.Add(this.txtAmt);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.trackBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SliderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SliderForm_FormClosed);
            ( (System.ComponentModel.ISupportInitialize)( this.trackBar ) ).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtAmt;
        private System.Windows.Forms.Label lblAmt;
        private System.Windows.Forms.Label lblEffcnt;
        private System.Windows.Forms.Label lblResultType;
        public System.Windows.Forms.Button btnCanel;
        public System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblExtra;
        private System.Windows.Forms.Label lblSlideType;
    }
}