using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public class Portal : Capturable
    {
        #region fields and constructors
        public const int AvgPortalCost = 1000;
        public const double UnitCostPct = .21;
        public const double ValuePct = ( 1 - UnitCostPct );
        public const double StartAmt = .26;

        internal readonly int PortalCost, income;
        internal readonly CostType PortalType;

        private Dictionary<string, int> unitsHave;

        internal Portal(Player owner, Tile tile, CostType type)
            : base(0, owner, tile)
        {
            if (type == CostType.Production)
                throw new Exception();

            this.name = type.ToString() + " Portal";

            this.PortalType = type;
            int mag, elm;
            Player.SplitPortalCost(owner.Race, type, out mag, out elm);
            this.PortalCost = mag + elm;
            double income = mag / 65.0;
            this.income = Game.Random.Round(income);

            SetStartValues();
            PayUpkeep(( this.income - income ) * 21);

            tile.Add(this);
            owner.Add(this);
        }
        #endregion //fields and constructors

        #region overrides
        internal override void Capture(Player newOwner)
        {
            //reimburse the old owner for partially finished units
            int magic, element;
            Player.SplitPortalCost(owner.Race, PortalType, out magic, out element);
            double resourceValue = GetResourceValue();
            magic = Game.Random.Round(resourceValue * magic / ( (double)magic + element ));
            element = Game.Random.Round(resourceValue * element / ( (double)magic + element ));

            owner.SpendMagic(-magic);
            owner.Spend(-element, PortalType, 0);

            string oldRace = owner.Race;
            base.Capture(newOwner);

            //setup for new owner
            if (oldRace == owner.Race)
            {
                //if the new owner is the same race, he keeps the have values and pays upkeep for it
                PayUpkeep(resourceValue);
            }
            else
            {
                //if not the same race, reset the values as though building a new portal
                SetStartValues();
            }
        }

        private void SetStartValues()
        {
            this.unitsHave = new Dictionary<string, int>();

            Dictionary<string, int> unitCosts = new Dictionary<string, int>();
            foreach (string unitName in Game.Races[this.owner.Race])
            {
                Unit unit = Unit.CreateTempUnit(unitName);
                if (unit.costType == this.PortalType)
                    unitCosts.Add(unitName, unit.BaseCost);
            }

            double totalStart = 0;
            foreach (var pair in unitCosts)
            {
                string unit = pair.Key;
                double baseCost = pair.Value;

                double inc = GetTurnInc();
                int start = Game.Random.Round(Game.Random.Weighted(baseCost - inc, StartAmt) + inc * StartAmt);

                totalStart += start;
                unitsHave.Add(unit, start);
            }

            //pay for starting amount
            PayUpkeep(totalStart * ValuePct);
        }

        public override bool CapableBuild(string name)
        {
            return ( name == "Wizard" );
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
            string unit = null;
            foreach (string name in Game.Random.Iterate(unitsHave.Keys))
            {
                unit = name;
                break;
            }

            double inc = GetTurnInc();
            int needed = Unit.CreateTempUnit(unit).BaseCost;
            unitsHave[unit] += Game.Random.GaussianOEInt(inc, .13 / unitsHave.Count, .065 * ( needed - unitsHave[unit] ) / ( needed + inc ));

            while (unitsHave[unit] >= needed)
            {
                unitsHave[unit] -= needed;
                owner.FreeUnit(unit, this);

                PayUpkeep(needed * UnitCostPct);
            }
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
        public int TotalCost
        {
            get
            {
                return PortalCost;
            }
        }
        public int Income
        {
            get
            {
                return income;
            }
        }

        public double GetTurnInc()
        {
            const double Power = 1.3, Divisor = 16.9;
            return Math.Pow(PortalCost, Power) / Divisor / Math.Pow(AvgPortalCost, Power - 1);
        }

        public Dictionary<string, int> GetUnitValues()
        {
            Dictionary<string, int> retVal = new Dictionary<string, int>();
            foreach (var pair in unitsHave)
                retVal.Add(pair.Key, pair.Value);
            return retVal;
        }
        #endregion //public methods and properties

        #region internal methods
        internal double GetPortalValue()
        {
            return PortalCost + GetResourceValue();
        }
        internal double GetResourceValue()
        {
            double total = 0;
            foreach (int value in unitsHave.Values)
                total += value;
            return total * ValuePct;
        }
        #endregion //internal methods
    }
}
