using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    class TurnException : AssertException
    {
        internal static void CheckTurn(Player player)
        {
            if (!player.IsTurn)
                throw new TurnException();
        }
    }
}
