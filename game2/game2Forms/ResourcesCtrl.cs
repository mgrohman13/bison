using game2.game;
using game2.sides;

namespace game2Forms
{
    public partial class ResourcesCtrl : UserControl
    {
        public ResourcesCtrl()
        {
            InitializeComponent();
        }

        public void RefreshResources()
        {
            Player player = Program.Game.Player;

            Resources resources = player.Resources;
            this.lblbasic.Text = resources.Basic.ToString();
            this.lblAdvanced.Text = resources.Advanced.ToString();
            this.lblMobility.Text = resources.Mobility.ToString();
            this.lblSpecial.Text = resources.Special.ToString();

            Resources income = player.GetTurnEnd();
            this.lblbasicInc.Text = InfoCtrl.FormatInc(income.Basic);
            this.lblAdvancedInc.Text = InfoCtrl.FormatInc(income.Advanced);
            this.lblMobilityInc.Text = InfoCtrl.FormatInc(income.Mobility);
            this.lblSpecialInc.Text = InfoCtrl.FormatInc(income.Special);
        }
    }
}
