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
                Player.GoldIncome(soldiers / Consts.SoldiersForGold);
                this.soldiers -= soldiers;

                Player.GoldIncome(population / Consts.PopulationForGold);
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

        public void MovePop(IEventHandler handler, int population, PopCarrier destination)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(Player);
            AssertException.Assert(destination != null);
            AssertException.Assert(population > 0);
            AssertException.Assert(population <= this.AvailablePop);
            AssertException.Assert(population <= destination.FreeSpace);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, destination.Tile));
            AssertException.Assert(this.Player == destination.Player);

            bool canUndo;
            double soldiers = MoveSoldiers(this.Population, this.soldiers, population, out canUndo);
            MovePop(this, destination, population, soldiers, false);

            if (canUndo)
                Player.Game.PushUndoCommand(new Game.UndoCommand<PopCarrier, int, double>(
                        new Game.UndoMethod<PopCarrier, int, double>(UndoMovePop), destination, population, soldiers));
            else
                Player.Game.ClearUndoStack();
        }
        internal Tile UndoMovePop(PopCarrier destination, int population, double soldiers)
        {
            TurnException.CheckTurn(Player);
            AssertException.Assert(destination != null);
            AssertException.Assert(population > 0);
            AssertException.Assert(population <= destination.Population);
            AssertException.Assert(population <= this.FreeSpace);
            AssertException.Assert(soldiers <= destination.Soldiers);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, destination.Tile));
            AssertException.Assert(this.Player == destination.Player);

            MovePop(destination, this, population, soldiers, true);

            return this.Tile;
        }
        private static void MovePop(PopCarrier source, PopCarrier destination, int population, double soldiers, bool undo)
        {
            double actual, rounded;
            GetGoldCost(population, out actual, out rounded);
            if (!undo)
            {
                AssertException.Assert(rounded < source.Player.Gold);
                actual = -actual;
                rounded = -rounded;
            }
            source.Player.AddGold(actual, rounded);

            source.soldiers -= soldiers;
            destination.soldiers += soldiers;

            source.Population -= population;
            destination.Population += population;

            if (undo)
                source.movedPop -= population;
            else
                destination.movedPop += population;
        }

        public double GetMoveSoldiers(int movePop)
        {
            return GetMoveSoldiers(this.Population, this.soldiers, movePop);
        }
        public static double GetMoveSoldiers(int population, double soldiers, int movePop)
        {
            bool canUndo;
            return GetMoveSoldiers(population, soldiers, movePop, false, out canUndo);
        }

        protected static double MoveSoldiers(int population, double soldiers, int movePop)
        {
            bool canUndo;
            return MoveSoldiers(population, soldiers, movePop, out canUndo);
        }
        private static double MoveSoldiers(int population, double soldiers, int movePop, out bool canUndo)
        {
            return GetMoveSoldiers(population, soldiers, movePop, true, out canUndo);
        }

        private static float GetMoveSoldiers(int population, double soldiers, int movePop, bool doMove, out bool canUndo)
        {
            canUndo = true;
            float moveSoldiers = 0;
            if (soldiers > Consts.FLOAT_ERROR)
            {
                if (population == movePop)
                    moveSoldiers = (float)soldiers;
                else
                    for (int mov = 1 ; mov <= movePop ; ++mov)
                    {
                        float available = (float)( soldiers - moveSoldiers );
                        float chunk = available * Consts.MoveSoldiersMult / ( Consts.MoveSoldiersMult + population - mov );
                        if (doMove)
                        {
                            canUndo = false;
                            chunk = Game.Random.GaussianCapped(chunk, Consts.SoldiersRndm, Math.Max(0, 2 * chunk - available));
                        }
                        moveSoldiers += chunk;
                    }
            }
            return moveSoldiers;
        }

        internal static double GetActualGoldCost(int population)
        {
            return population * Consts.MovePopulationGoldCost;
        }

        public static double GetGoldCost(int population)
        {
            double actual, rounded;
            GetGoldCost(population, out actual, out rounded);
            return rounded;
        }

        private static void GetGoldCost(int population, out double actual, out double rounded)
        {
            actual = GetActualGoldCost(population);
            rounded = Player.CeilGold(actual);
        }

        protected double GetSoldiers(int troops, double soldiers)
        {
            return GetSoldiers(this.Population, soldiers, troops);
        }

        public static double GetSoldiers(int population, double soldiers, int attPop)
        {
            if (population > 0)
                return ( ( soldiers * attPop ) / population );
            return soldiers;
        }

        public double GetTotalSoldierPct()
        {
            return GetSoldierPct(this.TotalSoldiers);
        }

        public double GetSoldierPct()
        {
            TurnException.CheckTurn(this.Player);

            return GetSoldierPct(this.soldiers);
        }

        protected double GetSoldierPct(double soldiers)
        {
            return GetSoldiers(this.Population, soldiers, 1);
        }

        public virtual double Soldiers
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.soldiers;
            }
        }

        public virtual double TotalSoldiers
        {
            get
            {
                return this.soldiers;
            }
        }

        #endregion //public
    }
}
