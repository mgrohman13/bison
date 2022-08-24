using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace CityWar
{
    [Serializable]
    public class Portal : Capturable
    {
        #region fields and constructors

        public const int CostTotalAvg = 1000;
        private const int CostMagicMin = 520;
        private const int CostMagicMax = 910;

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
            return Math.Pow(Cost, Power) / Divisor / Math.Pow(CostTotalAvg, Power - 1);
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

        public static void SplitPortalCost(Game game, string race, CostType costType, out int magic, out int element)
        {
            int[] retVal = SplitPortalCost(game, race)[costType];
            magic = retVal[0];
            element = retVal[1];
        }

        //public static Dictionary<CostType, int[]> SplitPortalCost(Game game, string race)
        //{
        //    MTRandom rand = PortalRand(game);

        //    //initialize cost types
        //    double[] tempTot = new double[5];
        //    double[] totCost = new double[5];
        //    double[] elemCost = new double[5];
        //    double[] popCost = new double[5];

        //    //need to order by name before random ordering, so changing the order of units in the Races dictionary has no effect 
        //    foreach (string name in rand.Iterate(Game.Races[race].OrderBy(x => x)))
        //    {
        //        Unit unit = Unit.CreateTempUnit(game, name);
        //        int idx = GetCTIdx(unit.CostType);
        //        if (idx > -1)
        //        {
        //            tempTot[idx] += rand.Gaussian(unit.BaseTotalCost + unit.BaseOtherCost / 2.1, .021);
        //            ++totCost[idx];
        //            double div = Math.Sqrt(unit.BaseTotalCost);
        //            elemCost[idx] += rand.Gaussian(unit.BaseOtherCost / div, .013);
        //            popCost[idx] += rand.Gaussian(unit.BasePplCost / div, .0169);
        //        }
        //    }

        //    //set totals
        //    double avgTot = 0;
        //    foreach (int idx in rand.Iterate(5))
        //    {
        //        tempTot[idx] = GetTotalPortalCost(tempTot[idx], totCost[idx]);
        //        avgTot += tempTot[idx];
        //    }
        //    foreach (int idx in rand.Iterate(5))
        //        totCost[idx] = tempTot[idx] * CostTotalAvg * 5.0 / avgTot;

        //    //set magic costs
        //    double[] magicAvg = new double[5];
        //    double[] elemMult = new double[5];

        //    foreach (int idx in rand.Iterate(5))
        //    {
        //        double totalCost = totCost[idx];
        //        double element = GetPortalElementCost(elemCost[idx] / (elemCost[idx] + popCost[idx]), totalCost);
        //        double mag = totalCost - element;
        //        while (magicAvg.Any(avg => Math.Abs(avg - mag) < rand.Gaussian(52, .13)))
        //            mag += rand.Gaussian(13);
        //        magicAvg[idx] = mag;
        //        elemMult[idx] = element / mag;
        //    }

        //    //scale to min/max  
        //    int[] magic = new int[5];

        //    double min = magicAvg.Min();
        //    double max = magicAvg.Max();
        //    double mult = (CostMagicMax - CostMagicMin) / (max - min);
        //    foreach (int idx in rand.Iterate(5))
        //        magic[idx] = rand.GaussianInt((magicAvg[idx] - min) * mult + CostMagicMin, .026);

        //    // distribute elemental, keeping ratios 
        //    double[] elemAvg = new double[5];

        //    double elemTot = 0;
        //    foreach (int idx in rand.Iterate(5))
        //    {
        //        elemAvg[idx] = magic[idx] * elemMult[idx];
        //        elemTot += elemAvg[idx];
        //    }

        //    // scale elemental to keep average correct
        //    int[] elem = new int[5];
        //    //  TODO: rescale non min/max magic vals too to keep mag/elem ratios better?
        //    int costTot = 5 * CostTotalAvg;
        //    double elemTrg = costTot - magic.Sum();
        //    double elemAct = elemAvg.Sum();
        //    mult = elemTrg / elemAct;
        //    foreach (int idx in rand.Iterate(5))
        //        elem[idx] = rand.Round(elemAvg[idx] * mult);

        //    //reconcile total
        //    int total; do
        //    {
        //        total = magic.Sum() + elem.Sum();
        //        elem[rand.Next(5)] += Math.Sign(costTot - total);
        //    } while (costTot != total);

        //    Dictionary<CostType, int[]> portalCosts = new();
        //    foreach (int idx in rand.Iterate(5))
        //        portalCosts.Add(GetIdxCT(idx), new int[] { magic[idx], elem[idx] });
        //    return portalCosts;
        //}
        //private static MTRandom PortalRand(Game game)
        //{
        //    //We want to be able to use some "randomness" in portal cost calculations,
        //    // but it needs to be deterministic for the same set of units
        //    //So, create a deterministic seed based on all of the elemental units' costs
        //    //If something is changed about any unit, the seed will be different and "random" results will differ
        //    uint[] seed = Game.Races
        //        .SelectMany(pair => pair.Value)
        //        .Select(name => Unit.CreateTempUnit(game, name))
        //        .Where(unit => unit.CostType != CostType.Production)
        //        .OrderBy(unit => unit.Race)
        //        .ThenBy(unit => unit.Name)
        //        .SelectMany(unit => new object[] {
        //            unit.Race,
        //            unit.Name,
        //            unit.CostType,
        //            unit.BaseOtherCost,
        //            unit.BasePplCost, })
        //        .Select(obj => (uint)obj.GetHashCode())
        //        .ToArray();
        //    return new MTRandom(seed);
        //}
        //private static double GetTotalPortalCost(double totCost, double numUnits)
        //{
        //    //the greater the number of units and their cost, the greater the total cost of the portal
        //    return Math.Pow(totCost * (numUnits + 1), .39);
        //}
        //private static double GetPortalElementCost(double elemPct, double totalCost)
        //{
        //    //the more population the units cost, the less magic the portal costs
        //    return (1 - (elemPct * elemPct * .666 + .21)) * totalCost;
        //}

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
                elmInt[idx] = (int)Math.Round(elmDbl[idx] * CostTotalAvg * 5.0 / total);
                totInt += elmInt[idx];
            }
            elmInt[4] = CostTotalAvg * 5 - totInt;

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
            //the more population the units cost, the less magic the portal costs
            return (int)Math.Ceiling((1 - (elemPct * elemPct * .666 + .21)) * totalCost);
        }

        private static int GetCTIdx(CostType costType)
        {
            if (costType == CostType.Production)
                return -1;
            int idx = (int)costType;
            if (idx > (int)CostType.Production)
                idx--;
            return idx;
        }
        private static CostType GetIdxCT(int idx)
        {
            if (idx >= (int)CostType.Production)
                idx++;
            return (CostType)idx;
        }

        #endregion //portal cost
    }
}
