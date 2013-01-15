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

        protected PopCarrier(int population, double soldiers)
        {
            this.Soldiers = soldiers;
            this.Population = population;
            this.movedPop = population;
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

        #region internal

        internal void ResetMoved()
        {
            this.movedPop = 0;
        }

        #endregion //internal

        #region protected

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

        protected void LosePopulation(int population)
        {
            if (population > this.Population)
                population = this.Population;

            if (population > 0)
            {
                double soldiers = GetSoldiers(population);
                Player.GoldIncome(soldiers / Consts.SoldiersForGold);
                this.Soldiers -= soldiers;

                Player.GoldIncome(population / Consts.PopulationForGoldLow);
                this.Population -= population;
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

            double soldiers = GetSoldiers(population);
            MovePop(this, destination, population, soldiers, false);

            Player.Game.PushUndoCommand(new Game.UndoCommand<PopCarrier, int, double>(
                    new Game.UndoMethod<PopCarrier, int, double>(UndoMovePop), destination, population, soldiers));
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

            source.Soldiers -= soldiers;
            destination.Soldiers += soldiers;

            source.Population -= population;
            destination.Population += population;

            if (undo)
                source.movedPop -= population;
            else
                destination.movedPop += population;
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

        public double GetSoldiers(int troops)
        {
            return GetSoldiers(this.Population, this.Soldiers, troops);
        }
        public static double GetSoldiers(int population, double soldiers, int troops)
        {
            if (population > 0)
                return ( ( soldiers * troops ) / population );
            return soldiers;
        }
        public double GetSoldierPct()
        {
            return GetSoldiers(1);
        }

        public double Soldiers
        {
            get
            {
                return this._soldiers;
            }
            protected set
            {
                checked
                {
                    this._soldiers = (float)value;
                }
            }
        }

        #endregion //public
    }
}
