using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    public interface IGalWarAI
    {
        void PlayTurn(IEventHandler handler);

        void SetGame(Game game);
    }
}
