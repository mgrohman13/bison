using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SearchUtil
{
    public partial class AdvancedExts : Form
    {
        public AdvancedExts(string extensions)
        {
            InitializeComponent();

            this.extensions = extensions;

            Refreshextensions();
        }

        string extensions;
        public string Extensions
        {
            get
            {
                return extensions;
            }
        }

        private void Refreshextensions()
        {
            extensions = extensions.Trim();

            //check for special character pattern signifying the extensions should not be searched
            this.rbNot.Checked = ((extensions.Length > 2) && extensions[0] == '~' &&
                extensions[1] == '(' && extensions[extensions.Length - 1] == ')');
            this.rbOnly.Checked = !this.rbNot.Checked;

            this.txtOnly.Enabled = this.rbOnly.Checked;
            this.txtNot.Enabled = this.rbNot.Checked;

            //get the right textbox
            TextBox txtCur;
            if (this.rbNot.Checked)
            {
                //get rid of the special characters
                extensions = extensions.Substring(2, extensions.Length - 3);
                txtCur = txtNot;
            }
            else
                txtCur = txtOnly;

            //add the extensions to the textbox
            txtCur.Clear();
            string[] extArray = extensions.Split(';');
            foreach (string s in extArray)
                if (s.Trim().Trim('.') != "")
                    txtCur.AppendText(s.Trim().Trim('.') + "\r\n");
        }

        private void storeextensions()
        {
            //get the right textbox
            TextBox txtCur;
            if (this.rbOnly.Checked)
                txtCur = this.txtOnly;
            else
                txtCur = this.txtNot;

            if (txtCur.Text.Trim() == "")
            {
                extensions = "";
                return;
            }

            //if the extensions should not be searched, start with the special character pattern
            StringBuilder sb = new StringBuilder(this.rbNot.Checked ? "~(" : "");
            string[] txtItems = txtCur.Text.Split('\r');
            foreach (string s in txtItems)
            {
                if (s.Trim('\n').Trim().Trim('.') == "")
                    continue;

                //append the extension
                sb.Append(s.Trim('\n').Trim().Trim('.'));
                sb.Append(';');
            }
            sb.Remove(sb.Length - 1, 1);

            //if the extensions should not be searched, add the last special character
            if (this.rbNot.Checked)
                sb.Append(')');

            extensions = sb.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            storeextensions();
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.txtOnly.Enabled = this.rbOnly.Checked;
            this.txtNot.Enabled = !this.rbOnly.Checked;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (this.txtExt.Text.Trim().Trim('.') != "")
            {
                //get the right textbox
                TextBox txtCur;
                if (this.rbOnly.Checked)
                    txtCur = this.txtOnly;
                else
                    txtCur = this.txtNot;

                //add the extension
                txtCur.AppendText(this.txtExt.Text.Trim().Trim('.') + "\r\n");
            }

            this.txtExt.ResetText();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            TextBox txtCur;
            if (this.rbOnly.Checked)
                txtCur = this.txtOnly;
            else
                txtCur = this.txtNot;

            //List<object> remove = new List<object>();
            //foreach (object o in txtCur.SelectedItems)
            //    remove.Add(o);
            //foreach (object o in remove)
            //    txtCur.Items.Remove(o);
        }

        private void txtExt_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.btnAdd_Click(null, null);
        }

        private void radioButton_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //switch which radio button is checked
                this.rbOnly.Checked = !this.rbOnly.Checked;
                this.rbNot.Checked = !this.rbOnly.Checked;
            }
        }
    }
}