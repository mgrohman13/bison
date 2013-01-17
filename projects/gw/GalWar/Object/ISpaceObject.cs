using System;
using System.Collections.Generic;

namespace GalWar
{
    public interface ISpaceObject
    {
        Tile Tile
        {
            get;
        }
        Player Player
        {
            get;
        }
    }
}
