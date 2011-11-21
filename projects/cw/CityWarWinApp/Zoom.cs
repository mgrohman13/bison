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

        public Zoom(Map main, float zoom, bool zoomIn)
        {
            InitializeComponent();

            this.main = main;
            this.zoom = zoom;

            showZoom(zoomIn);
        }

        private void showZoom(bool zoomIn)
        {
            float factor = 1f + Game.Random.GaussianCapped(.13f, .039f);
            if (zoomIn)
                zoom *= factor;
            else
                zoom /= factor;

            if (zoom < 30f)
                zoom = 30f;
            else if (zoom > 300f)
                zoom = 300f;

            this.lblZoom.Text = string.Format("{0}%", ( (float)( 100f * zoom / Map.startZoom ) ).ToString("0"));

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
                showZoom(true);
            else if (e.KeyChar == 'x' || e.KeyChar == 'X')
                showZoom(false);
        }
    }
}