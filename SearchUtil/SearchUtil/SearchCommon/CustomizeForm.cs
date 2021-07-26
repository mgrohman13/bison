using System;
using System.Drawing;
using System.Windows.Forms;

namespace SearchCommon
{
    public partial class CustomizeForm : Form
    {

        #region constructor and singleton

        public CustomizeForm()
        {
            InitializeComponent();
            formSize = this.Size;
        }

        static CustomizeForm custForm = new CustomizeForm();
        static Size formSize;

        static Settings settings;
        public delegate void SettingsChangedEvent();
        static SettingsChangedEvent settingsChangedEvent;

        static Random rand = new Random();

        /// <summary>
        /// Show a CustomizeForm to edit the specified Settings object.
        /// </summary>
        /// <param name="settings">The Settings object the CustomizeForm should edit.</param>
        public static void ShowCustomizeForm(Settings settings, SettingsChangedEvent settingsChangedArg)
        {
            custForm.Size = formSize;

            //wire the event
            CustomizeForm.settingsChangedEvent = settingsChangedArg;

            //set the settings and load them
            CustomizeForm.settings = settings;
            custForm.LoadSettings(settings);

            //show the dialog
            custForm.ShowDialog();
        }

        #endregion


        #region methods

        private Settings RandomSettings()
        {
            Settings result = new Settings();

            try
            {
                byte[] buffer = new byte[9];
                rand.NextBytes(buffer);
                int index = -1;

                result.AutoScrollResults = Bool();
                result.BackColor = RandomColor(buffer, ref index);
                result.DropDownItems = GaussianCappedInt(13, 30, 1);
                result.HighlightColor = RandomColor(buffer, ref index);
                result.MaxHistItems = GaussianCappedInt(130, 30, 1);
                result.NotepadDoubleclick = Bool();
                result.NotepadRightClick = Bool();
                result.ResultsPerFile = 1 + OEInt(5);
                result.Span = new TimeSpan(0, 0, GaussianCappedInt(39000, 30, 1300), 0);
                result.TextColor = RandomColor(buffer, ref index);
            }
            catch (ArgumentOutOfRangeException)
            {
                result = RandomSettings();
            }

            return result;
        }

        private Color RandomColor(byte[] buffer, ref int index)
        {
            return Color.FromArgb(buffer[++index], buffer[++index], buffer[++index]);
        }

        private bool Bool()
        {
            return ( rand.NextDouble() < .5 );
        }
        private int GaussianCappedInt(int average, double devPct, int lowerCap)
        {
            int val;
            do
            {
                val = average + (int)( Gaussian() * average * devPct );
            }
            while (val < lowerCap || val > average * 2 - lowerCap);
            return val;
        }
        private double Gaussian()
        {
            double a, b, c;
            do
            {
                a = rand.NextDouble();
                b = rand.NextDouble();
                a = a * 2 - 1;
                b = b * 2 - 1;
                c = a * a + b * b;
            } while (c >= 1);
            if (c != 0)
                c = Math.Sqrt(( -2 * Math.Log(c) ) / c);
            return a * c;
        }
        private static int OEInt(double average)
        {
            return (int)( Math.Log(1.0 - rand.NextDouble()) / Math.Log(average / ( average + 1 )) );
        }

        //select a color with the colorDialog
        private void SelectColor(Label lblColor)
        {
            this.colorDialog.Color = lblColor.BackColor;
            if (this.colorDialog.ShowDialog() == DialogResult.OK)
            {
                lblColor.BackColor = this.colorDialog.Color;
                CheckApply();
            }
        }

        //load the controls with the specified settings
        private void LoadSettings(Settings settings)
        {
            //select the right combo box entry
            if (settings.NotepadDoubleclick)
                if (settings.NotepadRightClick)
                    cbxNotepad.SelectedIndex = 2;
                else
                    cbxNotepad.SelectedIndex = 1;
            else
                if (settings.NotepadRightClick)
                    cbxNotepad.SelectedIndex = 0;
                else
                    cbxNotepad.SelectedIndex = 3;

            nudSpan.Value = (decimal)settings.Span.TotalDays;
            nudDropDownItems.Value = settings.DropDownItems;
            nudMaxHist.Value = settings.MaxHistItems;
            nudResPerFile.Value = settings.ResultsPerFile;

            chxAutoScroll.Checked = settings.AutoScrollResults;
            lblHighlightColor.BackColor = settings.HighlightColor;
            lblTextColor.BackColor = settings.TextColor;
            lblBackColor.BackColor = settings.BackColor;

            CheckApply();
        }

        //save the current control values to the settings
        private void ApplyChages()
        {
            settings.NotepadDoubleclick = cbxNotepad.SelectedIndex == 1 || cbxNotepad.SelectedIndex == 2;
            settings.NotepadRightClick = cbxNotepad.SelectedIndex == 0 || cbxNotepad.SelectedIndex == 2;

            //set the correct time
            double hourVal = ( (double)nudSpan.Value % 1.0 ) * 24.0;
            settings.Span = new TimeSpan((int)nudSpan.Value, (int)Math.Round(hourVal),
                (int)Math.Round(( hourVal % 1.0 ) * 60.0), 0);

            settings.DropDownItems = (int)nudDropDownItems.Value;
            settings.MaxHistItems = (int)nudMaxHist.Value;
            settings.ResultsPerFile = (int)nudResPerFile.Value;

            settings.AutoScrollResults = chxAutoScroll.Checked;
            settings.HighlightColor = lblHighlightColor.BackColor;
            settings.TextColor = lblTextColor.BackColor;
            settings.BackColor = lblBackColor.BackColor;

            //fire the event
            settingsChangedEvent();

            this.btnApply.Enabled = false;
        }

        //check if the apply buton should be enabled/disabled
        private void CheckApply()
        {
            //get the time specified by nudSpan
            double hourVal = ( (double)nudSpan.Value % 1.0 ) * 24.0;
            TimeSpan tempSpan = new TimeSpan((int)nudSpan.Value, (int)hourVal,
                (int)Math.Round(( hourVal % 1.0 ) * 60.0), 0);

            //check each control with its setting
            this.btnApply.Enabled = ( ( settings.ResultsPerFile != (int)nudResPerFile.Value ) ||
                ( settings.NotepadDoubleclick != ( cbxNotepad.SelectedIndex == 1 || cbxNotepad.SelectedIndex == 2 ) )
                || ( settings.NotepadRightClick != ( cbxNotepad.SelectedIndex == 0 || cbxNotepad.SelectedIndex == 2 ) )
                || ( settings.AutoScrollResults != chxAutoScroll.Checked )
                || ( settings.HighlightColor != lblHighlightColor.BackColor )
                || ( settings.TextColor != lblTextColor.BackColor )
                || ( settings.BackColor != lblBackColor.BackColor )
                || ( settings.Span != tempSpan )
                || ( nudDropDownItems.Value != settings.DropDownItems )
                || ( nudMaxHist.Value != settings.MaxHistItems ) );
        }

        #endregion


        #region form events

        private void btnOK_Click(object sender, EventArgs e)
        {
            ApplyChages();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyChages();
        }

        private void btnDefaults_Click(object sender, EventArgs e)
        {
            //load the controls with default settings
            LoadSettings(new Settings());
        }

        private void btnRandom_Click(object sender, EventArgs e)
        {
            LoadSettings(RandomSettings());
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            CheckApply();
        }

        private void cbxNotepad_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckApply();
        }

        private void chxAutoScroll_CheckedChanged(object sender, EventArgs e)
        {
            CheckApply();
        }

        private void lblColor_Click(object sender, EventArgs e)
        {
            SelectColor((Label)sender);
        }

        //private void checkbox_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter)
        //        ((CheckBox)sender).Checked = !((CheckBox)sender).Checked;
        //}

        private void CustomizeForm_KeyDown(object sender, KeyEventArgs e)
        {
            //press OK when enter is clicked
            if (e.KeyCode == Keys.Enter)
            {
                this.btnOK_Click(null, null);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        #endregion

    }
}