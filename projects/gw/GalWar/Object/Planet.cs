using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class Planet : ISpaceObject
    {
        #region fields and constructors

        private readonly Tile tile;

        private Colony _colony;

        private short _quality;

        internal Planet(Tile tile)
        {
            this.tile = tile;
            tile.SpaceObject = this;

            this._colony = null;

            this.Quality = Game.Random.OEInt(Consts.PlanetQualityOE) + Game.Random.RangeInt(Consts.PlanetQualityMin, Consts.PlanetQualityMax);
        }

        #endregion //fields and constructors

        #region internal

        internal void DamageVictory()
        {
            ReduceQuality(Game.Random.RangeInt(0, this.Quality));
        }

        internal void ReduceQuality(int damage)
        {
            this.Quality -= damage;
            if (this.Quality < 0)
                Destroy();
        }

        private void Destroy()
        {
            if (this.Dead)
                throw new Exception();

            if (this.Colony != null)
                this.Colony.Destroy();

            this.tile.Game.RemovePlanet(this);
        }

        #endregion //internal

        #region public

        public bool Dead
        {
            get
            {
                return ( this.Tile.SpaceObject != this );
            }
        }

        public double PlanetValue
        {
            get
            {
                return Quality + .65 / Consts.PopulationGrowth;
            }
        }

        public double ColonizationCost
        {
            get
            {
                return ( PlanetValue * Consts.ColonizationValueGoldCost );
            }
        }

        public Colony Colony
        {
            get
            {
                return this._colony;
            }
            internal set
            {
                if (( value == null ) == ( this._colony == null ))
                    throw new Exception();

                this._colony = value;
            }
        }

        public Tile Tile
        {
            get
            {
                return this.tile;
            }
        }

        public int Quality
        {
            get
            {
                return this._quality;
            }
            private set
            {
                checked
                {
                    this._quality = (short)value;
                }
            }
        }

        public override string ToString()
        {
            return this.Quality.ToString();
        }

        #endregion //public
    }
}