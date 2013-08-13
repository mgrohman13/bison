using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public abstract class SpaceObject
    {
        [NonSerialized]
        private Tile _tile;

        protected SpaceObject(Tile tile)
        {
            checked
            {
                if (this is Colony)
                {
                    if (tile != null)
                        throw new Exception();
                }
                else
                {
                    tile.SpaceObject = this;
                }

                this._tile = tile;
            }
        }

        public abstract Player Player
        {
            get;
        }

        public virtual Tile Tile
        {
            get
            {
                return this._tile;
            }
            protected set
            {
                checked
                {
                    Tile.SpaceObject = null;
                    this._tile = null;
                    OnDeserialization(value);
                }
            }
        }

        internal void OnDeserialization(Tile value)
        {
            checked
            {
                if (Tile != null || this is Colony)
                    throw new Exception();

                this._tile = value;
                Tile.SpaceObject = this;
            }
        }
    }
}
