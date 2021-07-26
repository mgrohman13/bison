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

        private float _prodMult, _soldierInc;

        internal Planet(Tile tile)
            : base(tile)
        {
            checked
            {
                this._colony = null;

                this._quality = (short)Consts.NewPlanetQuality();
                ResetCostMult();

                this._prodMult = 1f;
                this._soldierInc = 0f;
                SetPlanetValues(1);
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
                    if (value >= 0 && !this.Dead)
                        SetPlanetValues(Math.Abs(this.Quality - value) / ( Math.Max(Quality, value) + Consts.PlanetConstValue ));

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
        internal double prodMult
        {
            get
            {
                return this._prodMult;
            }
        }
        internal double soldierInc
        {
            get
            {
                return this._soldierInc;
            }
        }

        private void ResetCostMult()
        {
            checked
            {
                this._colonizationCostMult = (float)Consts.GetColonizationMult(Tile.Game);
            }
        }
        private void SetPlanetValues(double variation)
        {
            if (Game.Random.Bool(Math.Pow(variation, 1.3)))
                checked
                {
                    this._prodMult = Game.Random.GaussianCapped(1f, .169f, .52f);
                    this._soldierInc = Game.Random.Weighted(.091f, .13f);
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
                return ( this.Tile == null || this.Tile.SpaceObject != this );
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
