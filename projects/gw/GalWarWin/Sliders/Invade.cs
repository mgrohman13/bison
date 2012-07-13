using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class Invade : SliderController
    {
        private int attPop, attTotal, defPop, maxGold;
        private double attSoldiers, defSoldiers;
        private readonly int initial;

        public Invade(Ship ship, Colony colony)
            : this(ship.AvailablePop, ship.Population, colony.Population, ship.TotalSoldiers, colony.TotalSoldiers, (int)MainForm.Game.CurrentPlayer.Gold)
        {
        }

        public Invade(int attPop, int defPop, double attSoldiers, double defSoldiers)
            : this(attPop, attPop, defPop, attSoldiers, defSoldiers, (int)Math.Max(MainForm.Game.CurrentPlayer.Gold, attPop * 13) + defPop)
        {
            int gold = this.initial;
            string effcnt;
            do
                effcnt = GetEffcnt(++gold);
            while (effcnt != "100%");
            this.maxGold = Math.Max(gold, (int)MainForm.Game.CurrentPlayer.Gold);
        }

        private Invade(int attPop, int attTotal, int defPop, double attSoldiers, double defSoldiers, int maxGold)
        {
            this.maxGold = maxGold;
            this.attPop = attPop;
            this.attTotal = attTotal;
            this.defPop = defPop;
            this.attSoldiers = attSoldiers;
            this.defSoldiers = defSoldiers;

            this.initial = MattUtil.TBSUtil.FindValue(delegate(int gold)
            {
                string effcnt = GetEffcnt(gold);
                return ( effcnt == "100%" );
            }, 1, GetMax(), true);
        }

        public override double GetInitial()
        {
            return this.initial;
        }

        protected override int GetMaxInternal()
        {
            return maxGold;
        }

        protected override double GetResult()
        {
            return GetResult(GetValue());
        }

        private double GetResult(int gold)
        {
            return GetResult(GetAttack(gold), GetDefense());
        }

        private double GetResult(double attack, double defense)
        {
            if (attack > 0)
            {
                if (attack > defense)
                    return ( attPop - attPop + ( attack - defense ) / ( attack / attPop ) );
                else
                    return ( defPop - ( defense - attack ) / ( defense / defPop ) );
            }
            return 0;
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Invade Planet";
            lblSlideType.Text = "Gold";
        }

        internal override double lblExtra_Click()
        {
            double target = InputForm.ShowDialog(this.gameForm, "Enter maximum gold per troop left:");
            if (double.IsNaN(target))
                return GetValue();

            int max = GetMax();
            double defense = GetDefense();
            int gold = MattUtil.TBSUtil.FindValue(delegate(int findGold)
            {
                return ( GetAttack(findGold) > defense );
            }, 1, max, true);

            int retVal = gold;
            double last = GetResult(gold);
            while (++gold <= max)
            {
                double cur = GetResult(gold);
                if (( gold - retVal ) / ( cur - last ) < target)
                {
                    retVal = gold;
                    last = cur;
                }
            }
            return retVal;
        }

        internal override double lblEffcnt_Click()
        {
            double target = InputForm.ShowDialog(this.gameForm, "Enter target percent chance of winning (0-99):");
            if (double.IsNaN(target) || target < 0 || target > 99)
                return GetValue();

            target /= 100;
            return MattUtil.TBSUtil.FindValue(delegate(int gold)
            {
                return ( GetWinPct(GetAttack(gold)) > target );
            }, 1, GetMax(), true);
        }

        protected override string GetResultType()
        {
            if (GetAttack(GetValue()) > GetDefense())
                return "Troops Left";
            else
                return "Population Killed";
        }

        protected override string GetEffcnt()
        {
            return GetEffcnt(GetValue());
        }

        private string GetEffcnt(int gold)
        {
            return MainForm.FormatPctWithCheck(GetWinPct(GetAttack(gold)));
        }

        protected override string GetExtra()
        {
            if (attPop == attTotal)
                return base.GetExtra();
            return attPop + "/" + attTotal;
        }

        private double GetWinPct(double attack)
        {
            if (attack > 0)
            {
                double defense = GetDefense();

                bool adv = attack > defense;
                if (adv)
                {
                    double temp = attack;
                    attack = defense;
                    defense = temp;
                }

                double over = ( 1 + Consts.InvadeMultRandMax ) * attack - defense;
                double chance = 0;
                if (over > 0)
                    chance = over * over / ( attack * defense * 2 * Consts.InvadeMultRandMax * Consts.InvadeMultRandMax );

                if (adv)
                    chance = 1 - chance;
                return chance;
            }
            return 0;
        }

        private Dictionary<int, double> getAttackCache = new Dictionary<int, double>();
        private double GetAttack(int gold)
        {
            if (attPop > 0)
            {
                //this calls a brute-force algorithm and will be evaluated multiple times on a single event, so cache the results
                double result;
                if (!getAttackCache.TryGetValue(gold, out result))
                {
                    result = attPop * Consts.GetInvasionStrengthBase(attPop, PopCarrier.GetSoldiers(attTotal, attSoldiers, attPop), gold, GetDefense());
                    getAttackCache.Add(gold, result);
                }

                return result;
            }
            return -1;
        }

        private double GetDefense()
        {
            return ( defPop * Consts.GetPlanetDefenseStrengthBase(defPop, defSoldiers) );
        }
    }
}
