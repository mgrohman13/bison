using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    class TurnException : AssertException
    {
        internal static void CheckTurn(Player player)
        {
            if (!player.IsTurn)
                throw new TurnException();
        }
    }
}
