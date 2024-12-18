﻿using ClassLibrary1;
using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Trade : Form
    {
        private readonly static Trade form = new();

        public Trade()
        {
            InitializeComponent();
        }

        public static bool ShowTrade()
        {
            form.pnlBurn.Visible = Program.Game.Player.CanBurnMass();
            form.pnlFabricate.Visible = Program.Game.Player.CanFabricateMass();
            form.pnlScrap.Visible = Program.Game.Player.CanScrapResearch();

            form.nudBurn.Value = 0;
            form.nudBurn.Maximum = Math.Max(0, Program.Game.Player.Mass / Consts.BurnMassPerEnergy);
            form.nudFabricate.Value = 0;
            form.nudFabricate.Maximum = Math.Max(0, Program.Game.Player.Energy / Consts.EnergyPerFabricateMass);
            form.nudScrap.Value = 0;
            form.nudScrap.Increment = Consts.MassForScrapResearch;
            form.nudScrap.Maximum = Program.Game.Player.Research.GetProgress(Program.Game.Player.Research.Researching) * Consts.MassForScrapResearch;

            if (form.ShowDialog() == DialogResult.OK)
            {
                Program.Game.Player.Trade((int)form.nudBurn.Value, (int)form.nudFabricate.Value, GetResearch());
                return true;
            }
            return false;
        }


        private void NUD_ValueChanged(object sender, EventArgs e)
        {
            SetLext(lblBurn, form.nudBurn.Value * Consts.BurnMassPerEnergy);
            SetLext(lblFabricate, form.nudFabricate.Value * Consts.EnergyPerFabricateMass);
            SetLext(lblScrap, GetResearch());
        }
        private static int GetResearch() => (int)Math.Ceiling(form.nudScrap.Value / Consts.MassForScrapResearch);
        private static void SetLext(Label label, decimal value)
        {
            label.Text = ((int)(-value)).ToString();
        }
    }
}
