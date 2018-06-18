using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public abstract class PopCarrier : SpaceObject
    {
        #region fields and constructors

        private ushort _movedPop, _population;
        private float _soldiers;

        protected PopCarrier(Tile tile, int population, double soldiers)
            : base(tile)
        {
            checked
            {
                this._movedPop = (ushort)population;
                this._population = (ushort)population;
                this._soldiers = (float)soldiers;
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

        #endregion //fields and constructors

        #region abstract

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

        internal void LosePopulation(int population)
        {
            LosePopulation(population, false);
        }
        internal void LosePopulation(int population, bool addGold)
        {
            if (population > this.Population)
                population = this.Population;

            double gold = 0;

            if (population > 0)
            {
                double soldiers = GetSoldiers(population);
                gold += ( soldiers / Consts.SoldiersForGold );
                this.Soldiers -= soldiers;
            }

            gold += ( population / Consts.PopulationForGoldLow );
            this.Population -= population;

            if (addGold)
                this.Player.AddGold(gold, true);
            else
                this.Player.GoldIncome(gold);
        }

        #endregion //protected

        #region public

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

            double soldiers = GetMoveSoldiers(this.Population, this.Soldiers, population);
            MovePop(this, destination, population, soldiers, false);

            Player.Game.PushUndoCommand(new Game.UndoCommand<PopCarrier, int, double>(
                    new Game.UndoMethod<PopCarrier, int, double>(UndoMovePop), destination, population, soldiers));
        }
        private Tile UndoMovePop(PopCarrier destination, int population, double soldiers)
        {
            TurnException.CheckTurn(Player);
            AssertException.Assert(destination != null);
            AssertException.Assert(population > 0);
            AssertException.Assert(population <= destination.Population);
            AssertException.Assert(population <= this.FreeSpace);
            AssertException.Assert(soldiers > -Consts.FLOAT_ERROR_ZERO);
            AssertException.Assert(soldiers < destination.Soldiers * Consts.FLOAT_ERROR_ONE);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, destination.Tile));
            AssertException.Assert(this.Player == destination.Player);

            MovePop(destination, this, population, soldiers, true);

            return this.Tile;
        }
        private void MovePop(PopCarrier source, PopCarrier destination, int population, double soldiers, bool undo)
        {
            double goldCost = GetGoldCost(population, soldiers);
            if (!undo)
                goldCost = -goldCost;
            source.Player.GoldIncome(goldCost);

            source.Soldiers -= soldiers;
            destination.Soldiers += soldiers;

            source.Population -= population;
            destination.Population += population;

            if (undo)
                source.movedPop -= population;
            else
                destination.movedPop += population;
        }

        public double GetMoveSoldiers(int movePop)
        {
            return GetMoveSoldiers(this.Population, this.Soldiers, movePop);
        }
        public static double GetMoveSoldiers(int population, double soldiers, int movePop)
        {
            double moveSoldiers = 0;
            if (soldiers > Consts.FLOAT_ERROR_ZERO)
                if (population == movePop)
                    moveSoldiers = soldiers;
                else
                    for (int mov = 1 ; mov <= movePop ; ++mov)
                        moveSoldiers += ( soldiers - moveSoldiers ) * Consts.MoveSoldiersMult / ( Consts.MoveSoldiersMult + population - mov );
            return moveSoldiers;
        }

        protected double GetGoldCost(int population, double soldiers)
        {
            return Consts.GetMovePopCost(Player.Game.MapSize, population, soldiers);
        }

        public double GetSoldiers(int troops)
        {
            return GetSoldiers(this.Population, this.Soldiers, troops);
        }
        public static double GetSoldiers(int population, double soldiers, int troops)
        {
            if (population > 0)
                return ( ( soldiers * troops ) / (double)population );
            return soldiers;
        }
        public double GetSoldierPct()
        {
            return GetSoldiers(1);
        }

        #endregion //public
    }
}
