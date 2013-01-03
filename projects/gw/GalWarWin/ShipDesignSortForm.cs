using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class ShipDesignSortForm : Form
    {
        private static ShipDesignSortForm form = new ShipDesignSortForm();

        public ShipDesignSortForm()
        {
            InitializeComponent();
        }

        internal static bool ShowForm()
        {
            return ( form.ShowDialog() == DialogResult.OK );
        }

        internal static double GetValue(ShipDesign shipDesign)
        {
            bool str = ( form.cbAttack.Checked || form.cbDefense.Checked );
            bool tr = ( form.cbTransport.Checked || form.cbColony.Checked );
            bool ds = ( form.cbDS.Checked );

            double value;

            if (( str ^ tr ^ ds ) ? ( str && tr && ds ) : ( str || tr || ds ))
            {
                int att = form.cbAttack.Checked ? shipDesign.Att : 0;
                int def = form.cbDefense.Checked ? shipDesign.Def : 0;
                int hp = str ? shipDesign.HP : 0;
                int speed = form.cbSpeed.Checked ? shipDesign.Speed : 2;
                int trans = form.cbTransport.Checked ? shipDesign.Trans : 0;
                bool colony = form.cbColony.Checked ? shipDesign.Colony : false;
                double bombardDamage = ds ? shipDesign.BombardDamage : 0;
                if (!form.cbSpeed.Checked)
                {
                    trans *= Game.Random.Round(shipDesign.Speed / 2.0);
                    bombardDamage *= shipDesign.Speed / 2.0;
                }
                value = ShipDesign.GetValue(att, def, hp, speed, trans, colony, bombardDamage, shipDesign.Research);
            }
            else if (str)
            {
                int att = form.cbAttack.Checked ? shipDesign.Att : 0;
                int def = form.cbDefense.Checked ? shipDesign.Def : 0;
                int speed = form.cbSpeed.Checked ? shipDesign.Speed : 2;
                value = ShipDesign.GetStrength(att, def, shipDesign.HP, speed);
            }
            else if (tr)
            {
                int speed = form.cbSpeed.Checked ? shipDesign.Speed : 1;
                int trans = form.cbTransport.Checked ? shipDesign.Trans * speed : 0;
                if (form.cbColony.Checked && shipDesign.Colony)
                    trans += Game.Random.Round(30.0 * ( speed + 2.1 ));
                value = trans;
            }
            else if (ds)
            {
                int speed = form.cbSpeed.Checked ? shipDesign.Speed : 1;
                value = shipDesign.BombardDamage * speed;
            }
            else
            {
                value = form.cbSpeed.Checked ? shipDesign.Speed : 1;
            }

            double div = 1;
            if (form.cbCost.Checked && form.cbUpkeep.Checked)
                div = ( shipDesign.Cost + shipDesign.Upkeep * shipDesign.GetUpkeepPayoff(MainForm.Game.MapSize) );
            else if (form.cbCost.Checked)
                div = shipDesign.Cost;
            else if (form.cbUpkeep.Checked)
                div = shipDesign.Upkeep + shipDesign.GetUpkeepPayoff(MainForm.Game.MapSize) * Consts.FLOAT_ERROR;

            return value / div;
        }
    }
}
