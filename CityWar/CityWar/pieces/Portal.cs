using System;
using System.Collections.Generic;
using System.Linq;

namespace CityWar
{
    [Serializable]
    public class Portal : Capturable
    {
        #region fields and constructors

        public const int AvgPortalCost = 1000;
        public const double UnitCostPct = .21;
        public const double ValuePct = (1 - UnitCostPct);
        public const double StartAmt = .26;
        public const double IncomeDiv = 65;

        public readonly CostType Type;
        public readonly int Cost, Income, UnitInc;

        private Dictionary<string, int> units;

        internal Portal(Player owner, Tile tile, CostType type)
            : base(0, owner, tile, type.ToString() + " Portal")
        {
            if (type == CostType.Production)
                throw new Exception();

            this.Type = type;

            SplitPortalCost(Owner.Game, owner.Race, type, out int mag, out int elm);
            this.Cost = mag + elm;

            double income = mag / IncomeDiv;
            this.Income = Game.Random.GaussianCappedInt(income, .039, 1);
            double unitInc = GetUnitInc() + (income - this.Income) / ValuePct;
            this.UnitInc = Game.Random.GaussianCappedInt(Math.Max(unitInc, 1), .065, 1);

            this.units = GetStartValues();

            //pay for starting unit amounts and income randomness
            PayUpkeep(GetResourceValue() + (this.UnitInc - unitInc) * 21 * ValuePct);
        }

        #endregion //fields and constructors

        #region overrides

        internal override void Capture(Player newOwner)
        {
            //reimburse the old owner for partially finished units
            SplitPortalCost(owner.Game, owner.Race, Type, out int m, out int e);
            double resourceValue = GetResourceValue();
            int magic = Game.Random.Round(resourceValue * m / (double)(m + e));
            int element = Game.Random.Round(resourceValue * e / (double)(m + e));

            owner.SpendMagic(-magic);
            owner.Spend(-element, Type, 0);

            string oldRace = owner.Race;
            base.Capture(newOwner);

            //if the new owner is the same race, he keeps the have values and pays upkeep for it
            //otherwise, reset the values as though building a new portal
            if (oldRace != owner.Race)
                this.units = GetStartValues();
            PayUpkeep(GetResourceValue());
        }

        private Dictionary<string, int> GetStartValues()
        {
            return Game.Races[this.owner.Race].Select(unitName => Unit.CreateTempUnit(owner.Game, unitName))
                    .Where(unit => unit.CostType == this.Type).ToDictionary(unit => unit.Name,
                    unit => Game.Random.WeightedInt(unit.BaseTotalCost - UnitInc, StartAmt) + Game.Random.Round(UnitInc * StartAmt));
        }

        public override bool CapableBuild(string name)
        {
            if (name == "Wizard")
                return true;
            Unit unit = Unit.CreateTempUnit(owner.Game, name);
            if (!RaceCheck(unit))
                return false;
            return (this.Type == unit.CostType);
        }

        protected override bool CanMoveChild(Tile t)
        {
            return false;
        }

        protected override bool DoMove(Tile t, bool gamble, out bool canUndo)
        {
            canUndo = true;
            return false;
        }

        internal override void ResetMove()
        {
            string unit = Game.Random.SelectValue(units.Keys);

            int needed = owner.Game.GetUnitNeeds(unit);

            if (!EarnedIncome)
                units[unit] += Game.Random.GaussianOEInt(UnitInc, .26 / (1.3 + units.Count), .091 * (needed - units[unit]) / (double)(needed + UnitInc));

            while (units[unit] >= needed)
            {
                units[unit] -= needed;
                owner.FreeUnit(unit, this);

                PayUpkeep(needed * UnitCostPct);
            }

            base.ResetMove();
        }
        internal void PayUpkeep(double upkeep)
        {
            owner.BalanceForUnit(0, upkeep);
        }

        internal override double Heal()
        {
            return -1;
        }
        internal override void UndoHeal(double healInfo)
        {
            throw new Exception();
        }

        #endregion //overrides

        #region public methods and properties

        private double GetUnitInc()
        {
            const double Power = 1.3, Divisor = 16.9;
            return Math.Pow(Cost, Power) / Divisor / Math.Pow(AvgPortalCost, Power - 1);
        }

        public IEnumerable<KeyValuePair<string, int>> GetUnitValues()
        {
            return Enumerable.Empty<KeyValuePair<string, int>>().Concat(units);
        }

        #endregion //public methods and properties

        #region internal methods

        internal double GetPortalValue()
        {
            return Cost + GetResourceValue();
        }
        internal double GetResourceValue()
        {
            return units.Values.Sum() * ValuePct;
        }

        #endregion //internal methods

        #region portal cost

        public static int TotalPortalCost(Game game, string race, CostType costType)
        {
            int[] cost = SplitPortalCost(game, race)[costType];
            return cost[0] + cost[1];
        }
        public static void SplitPortalCost(Game game, string race, CostType costType, out int magic, out int element)
        {
            int[] retVal = SplitPortalCost(game, race)[costType];
            magic = retVal[0];
            element = retVal[1];
        }
        public static Dictionary<CostType, int[]> SplitPortalCost(Game game, string race)
        {
            Dictionary<CostType, int[]> portalCosts = new();

            double[] elmDbl = new double[5];
            int[] elmInt = new int[5];
            double[] other = new double[5];
            double[] ppl = new double[5];

            foreach (string name in Game.Races[race])
            {
                Unit unit = Unit.CreateTempUnit(game, name);
                int idx = GetCTIdx(unit.CostType);
                if (idx > -1)
                {
                    elmDbl[idx] += unit.BaseTotalCost + unit.BaseOtherCost / 2.1;
                    ++elmInt[idx];

                    double div = Math.Sqrt(unit.BaseTotalCost);
                    other[idx] += unit.BaseOtherCost / div;
                    ppl[idx] += unit.BasePplCost / div;
                }
            }

            double total = 0;
            for (int idx = 0; idx < 5; ++idx)
            {
                elmDbl[idx] = GetTotalPortalCost(elmDbl[idx], elmInt[idx]);
                total += elmDbl[idx];
            }

            int totInt = 0;
            for (int idx = 0; idx < 4; ++idx)
            {
                elmInt[idx] = (int)Math.Round(elmDbl[idx] * AvgPortalCost * 5.0 / total);
                totInt += elmInt[idx];
            }
            elmInt[4] = AvgPortalCost * 5 - totInt;

            for (int idx = 0; idx < 5; ++idx)
            {
                int totalCost = elmInt[idx];
                int element = GetPortalElementCost(other[idx] / (other[idx] + ppl[idx]), totalCost);
                int magic = totalCost - element;
                portalCosts.Add(GetIdxCT(idx), new int[] { magic, element });
            }

            return portalCosts;
        }
        private static double GetTotalPortalCost(double totCost, int numUnits)
        {
            //the greater the number of units and their cost, the greater the total cost of the portal
            return Math.Pow(totCost * (numUnits + 1), .39);
        }
        public static int GetPortalElementCost(double elemPct, double totalCost)
        {
            ////the more population the units cost, the less magic the portal costs
            return (int)Math.Ceiling((1 - (elemPct * elemPct * .666 + .21)) * totalCost);

            ////the more population the units cost, the more magic the portal costs
            //elemPct *= elemPct;
            //if (elemPct <= .26)
            //    return 1;
            //elemPct = (0.65 * (elemPct - 0.26));
            //return (int)Math.Ceiling(elemPct * totalCost);
        }
        private static int GetCTIdx(CostType costType)
        {
            return costType switch
            {
                CostType.Air => 0,
                CostType.Earth => 1,
                CostType.Nature => 2,
                CostType.Water => 3,
                CostType.Death => 4,
                _ => -1,
            };
        }
        private static CostType GetIdxCT(int idx)
        {
            return idx switch
            {
                0 => CostType.Air,
                1 => CostType.Earth,
                2 => CostType.Nature,
                3 => CostType.Water,
                4 => CostType.Death,
                _ => throw new Exception(),
            };
        }

        #endregion //portal cost
    }
}
