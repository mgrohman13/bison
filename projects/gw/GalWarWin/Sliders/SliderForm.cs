using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public partial class SliderForm : Form
    {
        private static SliderForm form = new SliderForm();

        private SliderController controller;
        private int baseHeight;

        private Control custom;
        private MouseEventHandler customMouseEventHandler;

        private int mouseWheelValue = -1;

        private SliderForm()
        {
            InitializeComponent();

            //MouseWheel event does not show up in the designer...
            trackBar.MouseWheel += new MouseEventHandler(trackBar_MouseWheel);
            foreach (Control control in this.Controls)
                if (control != this.trackBar)
                    control.MouseWheel += new MouseEventHandler(control_MouseWheel);

            baseHeight = this.Height;
        }

        private void SetController(SliderController controller)
        {
            this.controller = controller;

            controller.DoSetText += new EventHandler(this.SetText);

            this.Controls.Remove(custom);
            custom = controller.GetCustomControl();
            if (custom == null)
            {
                this.Height = baseHeight;
            }
            else
            {
                this.Controls.Add(custom);
                this.Height = baseHeight + custom.Height + 6;
                custom.Location = new Point(( this.ClientSize.Width - custom.Width ) / 2, this.btnOK.Location.Y - custom.Height - 3);
                customMouseEventHandler = new MouseEventHandler(control_MouseWheel);
                custom.MouseWheel += customMouseEventHandler;
            }

            controller.SetGetValueDelegate(GetValue);

            int min = controller.GetMin(), max = controller.GetMax();

            this.trackBar.Minimum = min;
            this.trackBar.Maximum = max;

            SetValue(controller.GetInitial());
            SetText();

            this.trackBar.Visible = ( max > min );

            form.txtAmt.SelectAll();
        }

        private int GetValue()
        {
            if (controller.GetMax() > 0)
                return this.trackBar.Value;
            return 0;
        }

        private void trackBar_MouseWheel(object sender, MouseEventArgs e)
        {
            //this event happens before the value actually changes, so store off the desired result value
            //this way we ignore the SystemInformation.MouseWheelScrollLines handling and always scroll by 1
            mouseWheelValue = GetNewValue(e);
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (mouseWheelValue > -1)
            {
                //after the value is changed, if it was caused by the built-in mousewheel handling of the trackbar, we overwrite the value manually
                SetValue(mouseWheelValue);
                mouseWheelValue = -1;
            }
            else
            {
                SetText();
            }
        }

        private void control_MouseWheel(object sender, MouseEventArgs e)
        {
            SetValue(GetNewValue(e));
        }

        private int GetNewValue(MouseEventArgs e)
        {
            int value = GetValue();
            if (e.Delta < 0)
                --value;
            else if (e.Delta > 0)
                ++value;
            return value;
        }

        private void txtAmt_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtAmt.Text, out value))
                SetValue(value);
            else
                SetText();
        }

        private void SetValue(double v)
        {
            int value = Game.Random.Round(v);
            if (value < trackBar.Minimum)
                value = trackBar.Minimum;
            if (value > trackBar.Maximum)
                value = trackBar.Maximum;
            this.trackBar.Value = value;
        }

        private void SetText()
        {
            SetText(null, null);
        }
        private void SetText(object sender, EventArgs eventArgs)
        {
            controller.SetText(txtAmt, lblExtra, lblTitle, lblSlideType, lblAmt, lblResultType, lblEffcnt);
        }

        public static int ShowForm(SliderController controller)
        {
            form.SetController(controller);

            //SetController may adjust the size so call SetLocation afterwards
            MainForm.GameForm.SetLocation(form);

            if (form.ShowDialog() == DialogResult.OK)
                return form.GetValue();
            return -1;
        }

        private void lblExtra_Click(object sender, EventArgs e)
        {
            SetValue(controller.lblExtra_Click());
        }

        private void lblEffcnt_Click(object sender, EventArgs e)
        {
            SetValue(controller.lblEffcnt_Click());
        }

        private void SliderForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (custom != null)
                custom.MouseWheel -= customMouseEventHandler;
        }

        private void SliderForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!this.txtAmt.Focused)
            {
                this.txtAmt.Text = e.KeyChar.ToString();
                this.txtAmt.Focus();
            }
        }
    }
}
