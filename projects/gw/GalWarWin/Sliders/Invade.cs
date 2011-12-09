using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class Invade : SliderController
    {
        private readonly Ship ship;
        private readonly Colony colony;
        private readonly int initial;

        //cache results from brute-force calls to Game.FindValue that will be evaluated multiple times on a single event
        private Dictionary<int, int> goldTroopValues = new Dictionary<int, int>();
        private Dictionary<int, double> goldAttackValues = new Dictionary<int, double>();

        public Invade(Ship ship, Colony colony)
        {
            this.ship = ship;
            this.colony = colony;

            //triple-nested Game.FindValue calls...
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
            return (int)Math.Floor(ship.Player.Gold);
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
                    return ( ship.AvailablePop - attackers + ( attack - defense ) / ( attack / attackers ) );
                else
                    return ( colony.Population - ( defense - attack ) / ( defense / colony.Population ) );
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
            return troops + "/" + ship.AvailablePop;
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
                int key = attackers + gold * ship.AvailablePop;
                double result;
                if (!goldAttackValues.TryGetValue(key, out result))
                {
                    double soldiers = ship.GetPublicSoldiers(attackers);
                    double bonusGold = gold - PopCarrier.GetGoldCost(attackers);
                    int initialWave = colony.GetInitialWave(attackers, soldiers, bonusGold, GetDefense());
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
            return ( colony.Population * Consts.GetPlanetDefenseStrengthBase(colony.Population, colony.GetPublicSoldiers(colony.Population)) );
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
                    int max = Math.Min(ship.AvailablePop, (int)Math.Floor(gold / Consts.MovePopulationGoldCost));
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
                result += colony.Population;
            result += ( ship.AvailablePop + colony.Population ) * GetWinPct(attack);
            return result;
        }
    }
}
