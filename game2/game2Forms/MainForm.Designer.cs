namespace game2Forms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            map = new MapCtrl();
            info = new InfoCtrl();
            runes = new RunesCtrl();
            resources = new ResourcesCtrl();
            SuspendLayout();
            // 
            // map
            // 
            map.BackColor = Color.Black;
            map.Dock = DockStyle.Fill;
            map.Location = new Point(0, 0);
            map.Name = "map";
            map.Size = new Size(1008, 729);
            map.TabIndex = 0;
            // 
            // info
            // 
            info.AutoSize = true;
            info.BackColor = Color.White;
            info.Dock = DockStyle.Left;
            info.Location = new Point(0, 0);
            info.Name = "info";
            info.Size = new Size(158, 729);
            info.TabIndex = 1;
            // 
            // runes
            // 
            runes.BackColor = Color.White;
            runes.Dock = DockStyle.Bottom;
            runes.Location = new Point(158, 579);
            runes.Name = "runes";
            runes.Size = new Size(850, 150);
            runes.TabIndex = 2;
            runes.Visible = false;
            // 
            // resources
            // 
            resources.AutoSize = true;
            resources.BackColor = Color.White;
            resources.Dock = DockStyle.Top;
            resources.Location = new Point(158, 0);
            resources.Name = "resources";
            resources.Size = new Size(850, 34);
            resources.TabIndex = 3;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1008, 729);
            Controls.Add(resources);
            Controls.Add(runes);
            Controls.Add(info);
            Controls.Add(map);
            KeyPreview = true;
            Name = "Main";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            Shown += Main_Shown;
            KeyDown += Main_KeyDown;
            KeyUp += Main_KeyUp;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MapCtrl map;
        private InfoCtrl info;
        private RunesCtrl runes;
        private ResourcesCtrl resources;
    }
}
