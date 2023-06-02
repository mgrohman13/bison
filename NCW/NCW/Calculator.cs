using System;
using System.Windows.Forms;

namespace NCWMap
{
    public partial class Calculator : Form
    {
        //private List<Control>[] attacks;

        public Calculator()
        {
            InitializeComponent();
            Calculate();
            //attacks = new[] { cbAtt,nuda };
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //Panel newPanel = new Panel();
            //SetControlProperties(panelAtt, newPanel);
            //newPanel.Location = new Point(panelAtt.Location.X, panelAtt.Location.Y + 30);
            //Controls.Add(newPanel);
        }
        //public static void SetControlProperties(Control from, Control to)
        //{
        //    to.Location = from.Location;
        //    to.Size = from.Size;

        //    Panel toPanel = to as Panel;
        //    ComboBox toBox = to as ComboBox;
        //    NumericUpDown toNud = to as NumericUpDown;
        //    if (toPanel != null)
        //    {
        //        to.Location = new Point(to.Location.X, to.Location.Y + 30);
        //        foreach (Control child in from.Controls)
        //        {
        //            Control newChild;
        //              if (child is ComboBox)
        //                newChild = new ComboBox();
        //            else if (child is NumericUpDown)
        //                newChild = new NumericUpDown();
        //            else
        //                throw new Exception();
        //            SetControlProperties(child, newChild);
        //        }
        //    }
        //    else if (toBox != null)
        //    {
        //        ComboBox fromBox = from as ComboBox;
        //        object[] items = new object[fromBox.Items.Count];
        //        fromBox.Items.CopyTo(items, 0);
        //        toBox.Items.AddRange(items);
        //        toBox.SelectedIndex = fromBox.SelectedIndex;
        //    }
        //    else if (toNud != null)
        //    {
        //        NumericUpDown fromNud = from as NumericUpDown;
        //        toNud.Value = fromNud.Value;
        //        toNud.Minimum = fromNud.Minimum;
        //        toNud.Maximum = fromNud.Maximum;
        //    }
        //            else
        //                throw new Exception();
        //}

        private void valueChanged(object sender, EventArgs e)
        {
            panelResult.Visible = false;
            Calculate();
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            Calculate();
        }
        private void Calculate()
        {
            int att = int.Parse((string)cbAtt.SelectedItem);
            double rawDef = double.Parse((string)cbDef.SelectedItem);
            int def = (int)rawDef;
            bool divDef = false;
            if (rawDef != def)
            {
                def = (int)( rawDef * 2.0 );
                divDef = true;
                if (def != rawDef * 2.0)
                    throw new Exception();
            }
            int attHP = (int)nudAttHP.Value;
            int defHP = (int)nudDefHP.Value;

            BattleResult result = new BattleResult(att, def, divDef, attHP, defHP);

            txtAttDmg.Text = GetDmg(result.AttDmg);
            txtAttKill.Text = GetKill(result.AttKill);
            txtDefDmg.Text = GetDmg(result.DefDmg);
            txtDefKill.Text = GetKill(result.DefKill);

            panelResult.Visible = true;
        }
        private string GetDmg(double dmg)
        {
            return dmg.ToString("0.000");
        }
        private string GetKill(double kill)
        {
            return kill.ToString("0.0%");
        }

        private void btnNewMap_Click(object sender, EventArgs e)
        {
            new NewMap().ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Balance().ShowDialog();
        }
    }
}
