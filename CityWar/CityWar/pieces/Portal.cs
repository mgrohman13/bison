using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;

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

            SplitPortalCost(owner.Race, type, out int mag, out int elm);
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
            SplitPortalCost(owner.Race, Type, out int m, out int e);
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
            return Game.Races[this.owner.Race].Select(unitName => Unit.CreateTempUnit(unitName))
                    .Where(unit => unit.CostType == this.Type).ToDictionary(unit => unit.Name,
                    unit => Game.Random.WeightedInt(unit.BaseTotalCost - UnitInc, StartAmt) + Game.Random.Round(UnitInc * StartAmt));
        }

        public override bool CapableBuild(string name)
        {
            if (name == "Wizard")
                return true;
            Unit unit = Unit.CreateTempUnit(name);
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

        private static Dictionary<string, Dictionary<CostType, int[]>> PortalCosts;

        public static void SplitPortalCost(string race, CostType costType, out int magic, out int element)
        {
            SplitPortalCosts();
            int[] retVal = PortalCosts[race][costType];
            magic = retVal[0];
            element = retVal[1];
        }

        public static Dictionary<string, Dictionary<CostType, int[]>> SplitPortalCosts()
        {
            PortalCosts ??= new();

            if (!PortalCosts.Any())
            {
                MTRandom rand = PortalRand();
                foreach (string race in rand.Iterate(Game.Races.Keys.OrderBy(r => r)))
                    PortalCosts.Add(race, GetRaceCosts(rand, race));
            }

            return PortalCosts;

            static Dictionary<CostType, int[]> GetRaceCosts(MTRandom rand, string race)
            {
                Console.WriteLine();
                Console.WriteLine(race);

                //initialize costs per resource type
                double[] totCost = new double[5];
                double[] tempTot = new double[5];
                double[] elmCost = new double[5];
                double[] popCost = new double[5];
                //need to order by name before random ordering, so changing the order of units in the Races dictionary has no effect 
                foreach (string name in rand.Iterate(Game.Races[race].OrderBy(x => x)))
                {
                    Unit unit = Unit.CreateTempUnit(name);
                    int idx = GetCTIdx(unit.CostType);
                    if (idx > -1)
                    {
                        //weight elemental cost more than population
                        totCost[idx] += rand.Gaussian(unit.BaseTotalCost + unit.BaseOtherCost / 2.1, .021);
                        ++tempTot[idx];
                        //use square root so expensive units don't outweigh cheap ones as much for calculating individual cost ratios 
                        double div = Math.Sqrt(unit.BaseTotalCost);
                        //offset by 1 to ensure non-zero values
                        elmCost[idx] += rand.Gaussian((1 + unit.BaseOtherCost) / div, .013);
                        popCost[idx] += rand.Gaussian((1 + unit.BasePplCost) / div, .0169);
                    }
                }

                Log("totCost", totCost);
                Log("tempTot", tempTot);
                Log("elmCost", elmCost);
                Log("popCost", popCost);

                //set total cost based on target average
                double avgTot = 0;
                foreach (int idx in rand.Iterate(5))
                {
                    tempTot[idx] = GetTotalPortalCost(totCost[idx], tempTot[idx]);
                    avgTot += tempTot[idx];
                }
                foreach (int idx in rand.Iterate(5))
                    totCost[idx] = tempTot[idx] * CostTotalAvg * 5.0 / avgTot;

                Log("tempTot", tempTot);
                Log("totCost", totCost);

                //set magic cost
                double[] magicAvg = new double[5];
                double[] elemMult = new double[5];
                foreach (int idx in rand.Iterate(5))
                {
                    double totalCost = totCost[idx];
                    double element = GetPortalElementCost(elmCost[idx] / (elmCost[idx] + popCost[idx]), totalCost);
                    double mag = totalCost - element;

                    magicAvg[idx] = mag;
                    elemMult[idx] = element / mag;
                }
                Log("magicAvg", magicAvg);
                Log("elemMult", elemMult);
                //we don't want portals with too similar magic costs so add some padding in between each
                double spacer = 0;
                int temp = 0;
                foreach (int idx in rand.Iterate(magicAvg.Select(avg => new Tuple<int, double>(temp++, avg)))
                    .OrderBy(tuple => tuple.Item2).Select(tuple => tuple.Item1))
                {
                    magicAvg[idx] += spacer;
                    spacer += rand.GaussianCapped(52, .13);
                }
                Log("magicAvg", magicAvg);

                //scale magic costs to target min/max  
                int[] magic = new int[5];
                double min = magicAvg.Min();
                double max = magicAvg.Max();
                double mlt = (CostMagicMax - CostMagicMin) / (max - min);
                foreach (int idx in rand.Iterate(5))
                    magic[idx] = rand.GaussianInt((magicAvg[idx] - min) * mlt + CostMagicMin, .026);

                // set elemental cost based on unit cost ratios 
                double[] elemAvg = new double[5];
                foreach (int idx in rand.Iterate(5))
                    elemAvg[idx] = magic[idx] * elemMult[idx];

                Log("elemAvg", elemAvg);

                // scale elemental cost to maintain total average cost
                int[] elemt = new int[5];
                const int costTot = 5 * CostTotalAvg;
                double elemTrg = costTot - magic.Sum();
                double elemAct = elemAvg.Sum();
                mlt = elemTrg / elemAct;
                foreach (int idx in rand.Iterate(5))
                    elemt[idx] = rand.Round(elemAvg[idx] * mlt);

                LogInt("elemt", elemt);

                //reconcile rounding errors
                int total;
                do
                {
                    total = magic.Sum() + elemt.Sum();
                    elemt[rand.Next(5)] += Math.Sign(costTot - total);
                } while (costTot != total);

                Console.WriteLine();
                Console.WriteLine(race);
                LogInt("magic", magic);
                LogInt("elemt", elemt);

                Dictionary<CostType, int[]> portalCosts = new();
                foreach (int idx in rand.Iterate(5))
                    portalCosts.Add(GetIdxCT(idx), new int[] { magic[idx], elemt[idx] });
                return portalCosts;

                static void LogInt(string desc, int[] info) => Log(desc, info.Select(i => (double)i).ToArray());
                static void Log(string desc, double[] info)
                {
                    int idx = 0;
                    Console.WriteLine(info.Select(s => $"{GetIdxCT(idx++)}:\t{s}").Aggregate(desc, (a, b) => a + Environment.NewLine + b));
                }

                //the greater the number of units and their cost, the greater the total cost of the portal
                static double GetTotalPortalCost(double totCost, double numUnits) =>
                    Math.Pow(totCost * (numUnits + 1), .39);
                //the more population the units cost, the less magic the portal costs
                static double GetPortalElementCost(double elemPct, double totalCost) =>
                    (1 - (elemPct * elemPct * .666 + .21)) * totalCost;

                static int GetCTIdx(CostType costType)
                {
                    if (costType == CostType.Production)
                        return -1;
                    int idx = (int)costType;
                    if (idx > (int)CostType.Production)
                        idx--;
                    return idx;
                }
                static CostType GetIdxCT(int idx)
                {
                    if (idx >= (int)CostType.Production)
                        idx++;
                    return (CostType)idx;
                }
            }

            static MTRandom PortalRand()
            {
                //We want to be able to use some "randomness" in portal cost calculations,
                // but it needs to be deterministic for the same set of units
                //So, create a deterministic seed based on all of the elemental units' costs
                //If something is changed about any unit, the seed will be different and "random" results will differ
                var seedData = Game.Races.Values
                    .SelectMany(v => v)
                    .Select(Unit.CreateTempUnit)
                    .Where(unit => unit.CostType != CostType.Production)
                    .OrderBy(unit => unit.Name)
                    .SelectMany(unit => new object[] {
                        unit.Race,
                        unit.Name,
                        unit.CostType,
                        unit.BaseOtherCost,
                        unit.BasePplCost,
                    });
                uint[] seed = MTRandom.GenerateSeed(seedData);

                Console.WriteLine("Portals seed:");
                Console.WriteLine(seed.Select(s => s.ToString("X")).Aggregate("", (a, b) => a + b));

                return new MTRandom(seed);
            }
        }

        #endregion //portal cost
    }
}
