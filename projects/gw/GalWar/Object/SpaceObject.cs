using System;
using System.Collections.Generic;

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

        public virtual Tile Tile
        {
            get
            {
                return this._tile;
            }
            protected set
            {
                this.Tile.SpaceObject = null;
                this._tile = value;
                this.Tile.SpaceObject = this;
            }
        }
        public abstract Player Player
        {
            get;
        }

        internal void OnDeserialization(Tile tile)
        {
            this._tile = tile;
        }
    }
}
