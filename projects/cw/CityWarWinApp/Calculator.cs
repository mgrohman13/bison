using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    public partial class Calculator : Form
    {
        private static Calculator singleton = new Calculator();

        private bool events = true;

        private List<Attack> attacks = new List<Attack>();
        private List<Defender> defenders = new List<Defender>();

        public Calculator()
        {
            InitializeComponent();
        }
        private void Calculator_Load(object sender, EventArgs e)
        {
            this.RefreshAll();
        }

        private void SetInfo(HashSet<Unit> units)
        {
            attacks.Clear();
            defenders.Clear();
            if (units != null)
                foreach (Unit unit in units)
                {
                    foreach (CityWar.Attack attack in unit.Attacks)
                        attacks.Add(new Attack(attack));
                    defenders.Add(new Defender(unit));
                }

            this.RefreshAll();
        }

        private void txtDamage_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPierce_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtHits_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtArmor_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (events)
                RefreshResult();
        }
        private void TextBoxChanged(TextBox changed, bool attack)
        {
            TextBox other = (TextBox)changed.Tag;
        }

        private void RefreshResult()
        {
            events = false;

            events = true;
        }

        public void RefreshAll()
        {
            events = false;

            RefreshAttacks();
            RefreshDefenders();

            RefreshResult();

            Refresh();

            events = true;
        }
        private void RefreshAttacks()
        {

            events = false;
            RefreshListBox(lbAttacks, attacks);
            if (lbAttacks.SelectedItems.Count == 1)
                RefreshAttack(lbAttacks.SelectedItems[0] as Attack);
            else
                RefreshAttack(null);

            events = true;
        }
        private void RefreshDefenders()
        {
            events = false;

            RefreshListBox(lbDefenders, defenders);
            RefreshDefender(lbDefenders.SelectedItem as Defender);

            events = true;
        }
        private void RefreshAttack(Attack attack)
        {
            events = false;

            if (attack == null)
            {
                this.txtDamage.Clear();
                this.txtPierce.Clear();
            }
            else
            {
                this.txtDamage.Text = attack.damage.ToString();
                this.txtPierce.Text = attack.pierce.ToString();
            }

            events = true;
        }
        private void RefreshDefender(Defender defender)
        {
            events = false;

            if (defender == null)
            {
                this.txtHits.Clear();
                this.txtArmor.Clear();
            }
            else
            {
                this.txtHits.Text = defender.hits.ToString();
                this.txtArmor.Text = defender.armor.ToString();
            }

            events = true;
        }
        private void RefreshListBox(ListBox listBox, IEnumerable objects)
        {
            events = false;

            Array selected = new object[listBox.SelectedItems.Count];
            listBox.SelectedItems.CopyTo(selected, 0);
            listBox.ClearSelected();
            listBox.Items.Clear();
            foreach (object obj in objects)
                listBox.Items.Add(obj);
            foreach (object item in selected)
                listBox.SelectedItems.Add(item);

            events = true;
        }

        public static void ShowForm(CityWar.Battle battle)
        {
            HashSet<Unit> units = null;
            if (battle != null)
            {
                units = new HashSet<Unit>();
                units.UnionWith(battle.GetAttackers());
                units.UnionWith(battle.GetDefenders());
            }

            ShowForm(units);
        }
        public static void ShowForm(IEnumerable<Unit> selected)
        {
            HashSet<Unit> units = null;
            if (selected != null)
            {
                HashSet<Tile> tiles = new HashSet<Tile>();
                foreach (Unit unit in selected)
                {
                    for (int a = 0 ; a < 6 ; ++a)
                        tiles.Add(unit.Tile.GetNeighbor(a));
                    tiles.Add(unit.Tile);
                }
                units = new HashSet<Unit>();
                foreach (Tile tile in tiles)
                    if (tile != null)
                        units.UnionWith(tile.GetAllUnits());
            }

            ShowForm(units);
        }
        private static void ShowForm(HashSet<Unit> units)
        {
            singleton.SetInfo(units);
            singleton.ShowDialog();
        }

        private class Attack
        {
            public CityWar.Attack info;
            public int damage, pierce;
            public Attack(CityWar.Attack attack)
            {
                this.info = attack;
                this.damage = attack.Damage;
                this.pierce = attack.ArmorPiercing;
            }
            public override string ToString()
            {
                if (info == null)
                    return string.Format("({0}, {1})", damage, pierce);
                else
                    return info.ToString();
            }
        }
        private class Defender
        {
            public Unit info;
            public int hits, armor;
            public Defender(Unit unit)
            {
                this.info = unit;
                this.hits = unit.Hits;
                this.armor = unit.Armor;
            }
            public override string ToString()
            {
                string retVal = "";
                if (info != null)
                    retVal = info.ToString() + " ";
                retVal += string.Format("({0}, {1})", hits, armor);
                return retVal;
            }
        }
    }
}
