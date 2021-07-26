using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public abstract class SliderController
    {
        protected GetValueDelegate GetValue;

        public delegate int GetValueDelegate();

        public event EventHandler DoSetText;

        protected void Refresh()
        {
            if (DoSetText != null)
                DoSetText(this, new EventArgs());
        }

        public void SetGetValueDelegate(GetValueDelegate GetValue)
        {
            this.GetValue = GetValue;
        }

        public virtual Control GetCustomControl()
        {
            return null;
        }

        public abstract double GetInitial();

        public virtual int GetMin()
        {
            return 1;
        }

        public int GetMax()
        {
            return Math.Max(0, GetMaxInternal());
        }

        protected abstract int GetMaxInternal();

        protected abstract double GetResult();

        protected abstract void SetText(Label lblTitle, Label lblSlideType);

        protected virtual string GetResultType()
        {
            return "Gold";
        }

        protected virtual string GetEffcnt()
        {
            int value = GetValue();
            if (value == 0)
                return string.Empty;
            else
                return ( (double)( GetResult() / (double)value ) ).ToString("0.00");
        }

        protected virtual string GetExtra()
        {
            return string.Empty;
        }

        public void SetText(TextBox txtAmt, Label lblExtra, Label lblTitle, Label lblSlideType, Label lblAmt, Label lblResultType, Label lblEffcnt)
        {
            int value = GetValue();
            txtAmt.Text = value.ToString();
            lblExtra.Text = GetExtra();

            SetText(lblTitle, lblSlideType);

            double result = GetResult();
            if (double.IsNaN(result))
            {
                lblAmt.Text = string.Empty;
                lblResultType.Text = string.Empty;
                lblEffcnt.Text = string.Empty;
            }
            else
            {
                lblAmt.Text = MainForm.FormatDouble(result);
                lblResultType.Text = GetResultType();
                lblEffcnt.Text = GetEffcnt();
            }
        }

        internal virtual double lblExtra_Click()
        {
            return GetValue();
        }

        internal virtual double lblEffcnt_Click()
        {
            return GetValue();
        }
    }
}
