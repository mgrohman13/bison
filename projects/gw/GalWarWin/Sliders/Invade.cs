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

        //cache results from brute-force calls to FindValue that will be evaluated multiple times on a single event
        private Dictionary<int, int> goldTroopValues = new Dictionary<int, int>();
        private Dictionary<int, double> goldAttackValues = new Dictionary<int, double>();

        public Invade(Ship ship, Colony colony)
            : this(ship.AvailablePop, ship.Population, colony.Population, ship.TotalSoldiers, colony.TotalSoldiers, (int)MainForm.Game.CurrentPlayer.Gold)
        {
        }

        public Invade(int attPop, int attTotal, int defPop, double attSoldiers, double defSoldiers)
            : this(attPop, attTotal, defPop, attSoldiers, defSoldiers, (int)Math.Max(MainForm.Game.CurrentPlayer.Gold, attPop * 13) + defPop)
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

            //triple-nested FindValue calls...
            this.initial = MattUtil.TBSUtil.FindValue(delegate(int gold)
            {
                string effcnt = GetEffcnt(gold);
                return ( effcnt == "99%" || effcnt == "100%" );
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
            int gold = GetValue();
            return GetResult(GetAttack(gold), GetTroops(gold), GetDefense());
        }

        private double GetResult(double attack, int attackers, double defense)
        {
            if (attack > 0)
            {
                if (attack > defense)
                    return ( attPop - attackers + ( attack - defense ) / ( attack / attackers ) );
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
            int troops = GetTroops(GetValue());
            if (troops < 0)
                troops = 0;
            return troops + "/" + attPop;
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

        private double GetAttack(int gold)
        {
            return GetAttack(GetTroops(gold), gold);
        }

        private double GetAttack(int attackers, int gold)
        {
            if (attackers > 0)
            {
                //called multiple times on a single event, so cache the results
                int key = attackers + gold * attPop;
                double result;
                if (!goldAttackValues.TryGetValue(key, out result))
                {
                    double soldiers = PopCarrier.GetSoldiers(attTotal, attSoldiers, attackers);
                    double bonusGold = gold - PopCarrier.GetGoldCost(attackers);
                    int initialWave = Colony.GetInitialWave(attackers, soldiers, bonusGold, defPop, GetDefense());
                    double bonus = Consts.GetInvasionStrengthBase(attackers, soldiers, initialWave, bonusGold);
                    result = attackers * bonus;

                    goldAttackValues.Add(key, result);
                }

                return result;
            }
            return -1;
        }

        private double GetDefense()
        {
            return ( defPop * Consts.GetPlanetDefenseStrengthBase(defPop, defSoldiers) );
        }

        internal int GetTroops(int gold)
        {
            if (gold > 0)
            {
                //called multiple times on a single event, so cache the results
                int result;
                if (!goldTroopValues.TryGetValue(gold, out result))
                {
                    double defense = GetDefense();
                    int max = Math.Min(attPop, (int)( gold / Consts.MovePopulationGoldCost ));
                    result = MattUtil.TBSUtil.FindValue(delegate(int troops)
                    {
                        if (troops < max)
                        {
                            double cur = GetTroopValue(troops, gold, defense);
                            double next = GetTroopValue(troops + 1, gold, defense);
                            return ( cur > next );
                        }
                        return true;
                    }, 1, max, true);

                    goldTroopValues.Add(gold, result);
                }
                return result;
            }
            return -1;
        }

        private double GetTroopValue(int troops, int gold, double defense)
        {
            double attack = GetAttack(troops, gold);
            double result = GetResult(attack, troops, defense);
            if (attack > defense)
                result += defPop;
            result += ( attPop + defPop ) * GetWinPct(attack);
            return result;
        }
    }
}
