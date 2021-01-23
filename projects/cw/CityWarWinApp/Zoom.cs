using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    partial class Zoom : Form
    {
        float zoom;
        Map main;

        const int timerTime = 1300;

        public Zoom(Map main, float zoom, bool? zoomIn, float delta)
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(zoomForm_MouseWheel);

            this.main = main;
            this.zoom = zoom;

            showZoom(zoomIn, delta);
        }

        private void showZoom(bool? zoomIn, float delta)
        {
            if (zoomIn.HasValue)
                delta = 260f * (zoomIn.Value ? 1f : -1f);

            float factor = 1f + Game.Random.GaussianCapped(Math.Abs(delta) * .00091f, .039f);
            if (delta >= 0)
                zoom *= factor;
            else
                zoom /= factor;

            if (zoom < 26f)
                zoom = 26f;
            else if (zoom > 390f)
                zoom = 390f;

            this.lblZoom.Text = string.Format("{0}%", ((float)(100f * zoom / Map.startZoom)).ToString("0"));

            Refresh();

            this.tmrClose.Stop();
            this.tmrClose.Start();
        }

        private void tmrClose_Tick(object sender, EventArgs e)
        {
            this.tmrClose.Enabled = false;
            main.Zoom = zoom;
            main.Refresh();
            this.Close();
        }

        private void zoomForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'z' || e.KeyChar == 'Z')
                showZoom(true, 0f);
            else if (e.KeyChar == 'x' || e.KeyChar == 'X')
                showZoom(false, 0f);
        }

        private void zoomForm_MouseWheel(object sender, MouseEventArgs e)
        {
            showZoom(null, e.Delta);
        }
    }
}