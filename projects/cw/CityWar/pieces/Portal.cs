using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public class Portal : Capturable
    {
        #region fields and constructors
        public const int AvgPortalCost = 1000;
        const double UnitCostMult = .21;
        public const double WorkPct = ( 1 - UnitCostMult );

        internal readonly int PortalCost, income;
        internal readonly CostType PortalType;
        private string[] units;
        private double[] have;
        private double upkeep;

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
            double income = mag / 66.6;
            this.income = Game.Random.Round(income);

            SetStartValues();
            upkeep += ( this.income - income ) * 21;

            tile.Add(this);
            owner.Add(this);
        }
        #endregion //fields and constructors

        #region overrides
        internal override void Capture(Player newOwner)
        {
            PayUpkeep();

            //reimburse the old owner for partially finished units
            int magic, element;
            Player.SplitPortalCost(owner.Race, PortalType, out magic, out element);
            double resourceValue = GetResourceValue();
            magic = Game.Random.Round(resourceValue * ( magic / ( (double)magic + element ) ));
            element = Game.Random.Round(resourceValue - magic);

            owner.SpendMagic(-magic);
            owner.Spend(-element, PortalType, 0);

            string oldRace = owner.Race;
            base.Capture(newOwner);

            //setup for new owner
            if (oldRace == owner.Race)
            {
                //if the new owner is the same race, he keeps the have values and pays upkeep for it
                owner.BalanceForUnit(0, resourceValue);
            }
            else
            {
                //if not the same race, reset the values as though building a new portal
                this.units = null;
                SetStartValues();
            }
        }

        private void SetStartValues()
        {
            if (this.units == null)
                this.units = new string[0];

            List<string> unitsOfType = new List<string>();
            foreach (string unitName in Game.Races[this.owner.Race])
            {
                Unit unit = Unit.CreateTempUnit(unitName);
                if (unit.costType == this.PortalType)
                    unitsOfType.Add(unitName);
            }
            string[] units = (string[])unitsOfType.ToArray();

            int i = units.Length;
            double[] have = new double[i];
            double totalStart = 0;
            while (--i > -1)
            {
                double start;
                int index;
                if (( index = Array.IndexOf(this.units, units[i]) ) > -1)
                {
                    //for loading games
                    start = this.have[index];
                }
                else
                {
                    //start with a random amount towards each unit
                    start = Game.Random.Weighted(Unit.CreateTempUnit(units[i]).BaseCost, .21f);
                    totalStart += start;
                }
                have[i] = start;
            }
            //if loading a game with a different set of units, reimburse the owner for old have values
            for (i = 0 ; i < this.units.Length ; ++i)
                if (Array.IndexOf(units, this.units[i]) < 0)
                    totalStart -= this.have[i];

            //pay for starting amount
            this.upkeep = totalStart * WorkPct;
            this.units = units;
            this.have = have;
        }

        public override bool CapableBuild(string name)
        {
            return ( name == "Wizard" );
        }

        protected override bool CanMoveChild(Tile t)
        {
            return false;
        }

        protected override bool DoMove(Tile t, out bool canUndo)
        {
            canUndo = true;
            return false;
        }

        internal override void ResetMove()
        {
            PayUpkeep();

            //pick a random unit
            int index = Game.Random.Next(units.Length);

            //add an amount based on the original cost of the portal
            have[index] += Game.Random.GaussianCapped(GetTurnInc(), .13);

            int needed = Unit.CreateTempUnit(units[index]).BaseCost;
            while (have[index] >= needed)
            {
                have[index] -= needed;
                owner.FreeUnit(units[index], this);

                upkeep += needed * UnitCostMult;
            }
        }
        internal void PayUpkeep()
        {
            owner.BalanceForUnit(0, upkeep);
            upkeep = 0;
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

        public Dictionary<string, double> getUnitValues()
        {
            Dictionary<string, double> retVal = new Dictionary<string, double>();
            for (int i = units.Length ; --i > -1 ; )
                retVal.Add(units[i], have[i]);
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
            for (int i = units.Length ; --i > -1 ; )
                total += have[i];
            return total * WorkPct;
        }
        #endregion //internal methods
    }
}
