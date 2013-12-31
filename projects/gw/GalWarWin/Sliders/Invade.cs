using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GalWar;
using MattUtil;

namespace GalWarWin.Sliders
{
    public class Invade : SliderController
    {
        private int attPop, attTotal, defPop, maxGold;
        private double attSoldiers, defSoldiers;
        private readonly int initial;

        public Invade(Ship ship, Colony colony)
            : this(ship.AvailablePop, ship.Population, colony.Population, ship.Soldiers, colony.Soldiers, (int)MainForm.Game.CurrentPlayer.Gold)
        {
        }

        public Invade(int attPop, int defPop, double attSoldiers, double defSoldiers)
            : this(attPop, attPop, defPop, attSoldiers, defSoldiers, (int)Math.Max(MainForm.Game.CurrentPlayer.Gold, attPop * 13) + defPop)
        {
            int gold = this.initial;
            string effcnt;
            do
                effcnt = GetEffcnt(++gold);
            while (!ValidInitial(effcnt));
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

            this.initial = TBSUtil.FindValue(delegate(int gold)
            {
                return ValidInitial(GetEffcnt(gold));
            }, 1, GetMax(), true);
        }
        private static bool ValidInitial(string effcnt)
        {
            return ( effcnt == "99%" || effcnt == ">99%" || effcnt == "100%" );
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
            if (attack > defense)
                return ( attPop - attPop + ( attack - defense ) / ( attack / attPop ) );
            else
                return ( defPop - ( defense - attack ) / ( defense / defPop ) );
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Invade Planet";
            lblSlideType.Text = "Gold";
        }

        internal override double lblExtra_Click()
        {
            int retVal;

            double defense = GetDefense();
            string prompt;
            if (GetAttack(GetValue()) > defense)
                prompt = "Enter gold per troop left:";
            else
                prompt = "Enter gold per population killed:";
            double target = InputForm.ShowForm(prompt);
            if (double.IsNaN(target) || target < 0)
                return GetValue();

            int max = GetMax();
            if (GetAttack(GetValue()) > defense)
            {
                int gold = TBSUtil.FindValue(delegate(int findGold)
                {
                    return ( GetAttack(findGold) > defense );
                }, 1, max, true);

                //we cannot use a binary search algorithm due to the initialwave logic
                retVal = gold;
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
            }
            else
            {
                max = TBSUtil.FindValue(delegate(int findGold)
                {
                    return ( GetAttack(findGold) < defense );
                }, 1, max, false);
                retVal = TBSUtil.FindValue(delegate(int gold)
                {
                    return ( gold / GetResult(gold) < target );
                }, 1, max, false);
            }
            return retVal;
        }

        internal override double lblEffcnt_Click()
        {
            double target = InputForm.ShowForm("Enter target percent chance of winning (0-100):");
            if (double.IsNaN(target) || target < 0 || target > 100)
                return GetValue();

            target /= 100;
            return TBSUtil.FindValue(delegate(int gold)
            {
                return ( GetWinPct(gold) >= target );
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
            return MainForm.FormatPctWithCheck(GetWinPct(gold));
        }

        protected override string GetExtra()
        {
            if (attPop == attTotal)
                return base.GetExtra();
            return attPop + "/" + attTotal;
        }

        private double GetWinPct(int gold)
        {
            double attack = GetAttack(gold);
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
                return VerifyAlwaysWin(chance, gold);
            }
            throw new Exception();
        }
        private double VerifyAlwaysWin(double chance, int gold)
        {
            if (chance >= 1)
            {
                double defense = GetDefense() * ( 1 + Consts.InvadeMultRandMax );
                double attack = attPop * Consts.GetInvadeStrengthBase(attPop, GetSoldiers(), gold, defense);
                //since the defender wins a tie, the attacker must win by a margin of at least one troop to actually have a 100% chance
                double margin = 1 - 1 / (double)attPop;
                if (defense > attack * margin)
                    chance = 1 - Consts.FLOAT_ERROR_ZERO;
            }
            return chance;
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
                    result = attPop * Consts.GetInvadeStrengthBase(attPop, GetSoldiers(), gold, GetDefense());
                    getAttackCache.Add(gold, result);
                }
                return result;
            }
            throw new Exception();
        }
        private double GetSoldiers()
        {
            return PopCarrier.GetSoldiers(attTotal, attSoldiers, attPop);
        }

        private double GetDefense()
        {
            return ( defPop * Consts.GetInvadeDefenseStrengthBase(defPop, defSoldiers) );
        }
    }
}
