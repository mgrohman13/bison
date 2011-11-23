using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public abstract class PopCarrier
    {
        #region fields and constructors

        private float _soldiers;
        private ushort _population, _movedPop;

        protected PopCarrier()
        {
            this.soldiers = 0;
            this.Population = 0;
            this.movedPop = 0;
        }

        #endregion //fields and constructors

        #region abstract

        public abstract Player Player
        {
            get;
        }

        public abstract Tile Tile
        {
            get;
        }

        public abstract int MaxPop
        {
            get;
        }

        #endregion //abstract

        #region private

        internal void ResetMoved()
        {
            this.movedPop = 0;
        }

        protected void LosePopulation(int population)
        {
            if (population > this.Population)
                population = this.Population;

            if (population > 0)
            {
                double soldiers = GetSoldiers(population, this.soldiers);
                Player.AddGold(soldiers / Consts.SoldiersForGold);
                this.soldiers -= soldiers;

                Player.AddGold(population / Consts.PopulationForGold);
                this.Population -= population;
            }
        }

        #endregion //private

        #region protected

        protected double soldiers
        {
            get
            {
                return this._soldiers;
            }
            set
            {
                checked
                {
                    this._soldiers = (float)value;
                }
            }
        }

        protected int movedPop
        {
            get
            {
                return this._movedPop;
            }
            set
            {
                checked
                {
                    this._movedPop = (ushort)value;
                }
            }
        }

        #endregion //protected

        #region public

        public int Population
        {
            get
            {
                return this._population;
            }
            protected set
            {
                checked
                {
                    this._population = (ushort)value;
                }
            }
        }

        public int AvailablePop
        {
            get
            {
                return this.Population - this.movedPop;
            }
        }

        public int FreeSpace
        {
            get
            {
                return MaxPop - this.Population;
            }
        }

        public void MovePop(int population, PopCarrier destination)
        {
            TurnException.CheckTurn(Player);
            AssertException.Assert(population > 0);
            AssertException.Assert(population <= this.AvailablePop);
            AssertException.Assert(population <= destination.FreeSpace);
            AssertException.Assert(destination != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, destination.Tile));
            AssertException.Assert(this.Player == destination.Player);
            double gold = GetGoldCost(population);
            AssertException.Assert(gold <= this.Player.Gold);

            this.Player.SpendGold(gold);

            double soldiers = MoveSoldiers(this.Population, this.soldiers, population);
            this.soldiers -= soldiers;
            destination.soldiers += soldiers;

            destination.movedPop += population;
            destination.Population += population;
            this.Population -= population;
        }

        public double GetMoveSoldiers(int movePop)
        {
            return GetMoveSoldiers(this.Population, this.soldiers, movePop);
        }

        public static double GetMoveSoldiers(int population, double soldiers, int movePop)
        {
            return GetMoveSoldiers(population, soldiers, movePop, false);
        }

        protected static double MoveSoldiers(int population, double soldiers, int movePop)
        {
            return GetMoveSoldiers(population, soldiers, movePop, true);
        }

        private static float GetMoveSoldiers(int population, double soldiers, int movePop, bool doMove)
        {
            float moveSoldiers = 0;
            if (soldiers > 0)
            {
                if (population == movePop)
                    moveSoldiers = (float)soldiers;
                else
                    for (int mov = 1 ; mov <= movePop ; ++mov)
                    {
                        float available = (float)( soldiers - moveSoldiers );
                        float chunk = available * Consts.MoveSoldiersMult / ( Consts.MoveSoldiersMult + population - mov );
                        if (doMove)
                            chunk = Game.Random.GaussianCapped(chunk, Consts.SoldiersRndm, Math.Max(0, 2 * chunk - available));
                        moveSoldiers += chunk;
                    }
            }
            return moveSoldiers;
        }

        public static double GetGoldCost(int population)
        {
            return population * Consts.MovePopulationGoldCost;
        }

        public double GetPublicSoldiers(int troops)
        {
            return GetSoldiers(troops, this.DefendingSoldiers);
        }

        public double GetSoldiers(int troops)
        {
            TurnException.CheckTurn(this.Player);

            return GetSoldiers(troops, this.soldiers);
        }

        protected double GetSoldiers(int troops, double soldiers)
        {
            if (soldiers == 0 || troops == 0)
                return 0;
            if (this.Population <= 0)
                throw new Exception();
            return soldiers * troops / this.Population;
        }

        public double GetPublicSoldierPct()
        {
            return GetSoldierPct(this.DefendingSoldiers);
        }

        public double GetSoldierPct()
        {
            TurnException.CheckTurn(this.Player);

            return GetSoldierPct(this.soldiers);
        }

        protected double GetSoldierPct(double soldiers)
        {
            if (soldiers <= Consts.FLOAT_ERROR)
                return soldiers;
            if (this.Population <= 0)
                throw new Exception();
            return soldiers / this.Population;
        }

        protected virtual double DefendingSoldiers
        {
            get
            {
                return this.soldiers;
            }
        }

        #endregion //public
    }
}
