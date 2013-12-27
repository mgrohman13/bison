using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class Planet : SpaceObject
    {
        #region fields and constructors

        private Colony _colony;

        private short _quality;
        private float _colonizationCostMult;

        internal Planet(Tile tile)
            : base(tile)
        {
            checked
            {
                this._colony = null;

                this._quality = (short)Consts.NewPlanetQuality();
                ResetCostMult();
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
                checked
                {
                    if (( value == null ) == ( this.Colony == null ))
                        throw new Exception();

                    if (value == null && !this.Dead)
                        ResetCostMult();

                    this._colony = value;
                }
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
        private double colonizationCostMult
        {
            get
            {
                return this._colonizationCostMult;
            }
        }
        private void ResetCostMult()
        {
            checked
            {
                this._colonizationCostMult = (float)Consts.GetColonizationMult();
            }
        }

        #endregion //fields and constructors

        #region internal

        internal int DamageVictory()
        {
            int reduce = Game.Random.RangeInt(0, this.Quality);
            ReduceQuality(reduce);
            return reduce;
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

            this.Tile.Game.RemovePlanet(this);
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
                return Quality + Consts.PlanetConstValue;
            }
        }

        public double ColonizationCost
        {
            get
            {
                return Consts.GetColonizationCost(PlanetValue, colonizationCostMult);
            }
        }

        public override Player Player
        {
            get
            {
                if (Colony != null)
                    return Colony.Player;
                return null;
            }
        }

        public override string ToString()
        {
            return this.Quality.ToString();
        }

        #endregion //public
    }
}
