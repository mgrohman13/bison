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

        public readonly CostType Type;
        public readonly int Cost, Income;

        private Dictionary<string, int> units;

        internal Portal(Player owner, Tile tile, CostType type)
            : base(0, owner, tile, type.ToString() + " Portal")
        {
            if (type == CostType.Production)
                throw new Exception();

            this.Type = type;

            int mag, elm;
            Player.SplitPortalCost(Owner.Game, owner.Race, type, out mag, out elm);
            this.Cost = mag + elm;

            double income = mag / IncomeDiv;
            this.Income = Game.Random.Round(income);

            this.units = GetStartValues();

            //pay for starting amount and income rounding
            PayUpkeep(GetResourceValue() + ( this.Income - income ) * 21);
        }

        #endregion //fields and constructors

        #region overrides

        internal override void Capture(Player newOwner)
        {
            //reimburse the old owner for partially finished units
            int m, e;
            Player.SplitPortalCost(owner.Game, owner.Race, Type, out m, out e);
            double resourceValue = GetResourceValue();
            int magic = Game.Random.Round(resourceValue * m / (double)( m + e ));
            int element = Game.Random.Round(resourceValue * e / (double)( m + e ));

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
            double inc = this.GetTurnInc();
            return Game.Races[this.owner.Race].Select(unitName => Unit.CreateTempUnit(owner.Game, unitName))
                    .Where(unit => unit.CostType == this.Type).ToDictionary(unit => unit.Name,
                    unit => Game.Random.Round(Game.Random.Weighted(unit.BaseTotalCost - inc, StartAmt) + inc * StartAmt));
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
            string unit = Game.Random.SelectValue(units.Keys);

            double inc = GetTurnInc();
            int needed = Unit.CreateTempUnit(owner.Game, unit).BaseTotalCost;
            units[unit] += Game.Random.GaussianOEInt(inc, .26 / ( 1.3 + units.Count ), .091 * ( needed - units[unit] ) / ( needed + inc ));

            while (units[unit] >= needed)
            {
                units[unit] -= needed;
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
                return Cost;
            }
        }

        public double GetTurnInc()
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
    }
}
