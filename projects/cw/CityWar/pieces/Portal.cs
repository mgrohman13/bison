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
        public const double ValuePct = ( 1 - UnitCostPct );
        public const double StartAmt = .26;
        public const double IncomeDiv = 65;

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
            double income = mag / IncomeDiv;
            this.income = Game.Random.Round(income);

            SetStartValues();
            //pay for starting amount and income rounding
            PayUpkeep(GetResourceValue() + ( this.income - income ) * 21);

            owner.Add(this);
            tile.Add(this);
        }
        #endregion //fields and constructors

        #region overrides
        internal override void Capture(Player newOwner)
        {
            //reimburse the old owner for partially finished units
            int m, e;
            Player.SplitPortalCost(owner.Race, PortalType, out m, out e);
            double resourceValue = GetResourceValue();
            int magic = Game.Random.Round(resourceValue * m / (double)( m + e ));
            int element = Game.Random.Round(resourceValue * e / (double)( m + e ));

            owner.SpendMagic(-magic);
            owner.Spend(-element, PortalType, 0);

            string oldRace = owner.Race;
            base.Capture(newOwner);

            //if the new owner is the same race, he keeps the have values and pays upkeep for it
            //otherwise, reset the values as though building a new portal
            if (oldRace != owner.Race)
                SetStartValues();
            PayUpkeep(GetResourceValue());
        }

        private void SetStartValues()
        {
            double inc = this.GetTurnInc();
            this.unitsHave = Game.Races[this.owner.Race].Select(unitName => Unit.CreateTempUnit(unitName))
                    .Where(unit => unit.costType == this.PortalType).ToDictionary(unit => unit.Name,
                    unit => Game.Random.Round(Game.Random.Weighted(unit.BaseCost - inc, StartAmt) + inc * StartAmt));
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
            string unit = Game.Random.SelectValue(unitsHave.Keys);

            double inc = GetTurnInc();
            int needed = Unit.CreateTempUnit(unit).BaseCost;
            unitsHave[unit] += Game.Random.GaussianOEInt(inc, .26 / ( 1.3 + unitsHave.Count ), .091 * ( needed - unitsHave[unit] ) / ( needed + inc ));

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

        public IEnumerable<KeyValuePair<string, int>> GetUnitValues()
        {
            return Enumerable.Empty<KeyValuePair<string, int>>().Concat(unitsHave);
        }
        #endregion //public methods and properties

        #region internal methods
        internal double GetPortalValue()
        {
            return PortalCost + GetResourceValue();
        }
        internal double GetResourceValue()
        {
            return unitsHave.Values.Sum() * ValuePct;
        }
        #endregion //internal methods
    }
}
