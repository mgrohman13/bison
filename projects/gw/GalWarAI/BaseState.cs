using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    internal abstract class BaseState : IState
    {
        protected Game game;

        public BaseState(Game game)
        {
            this.game = game;
        }

        public void PlayTurn(IEventHandler handler)
        {
        }

        public bool TransitionOK()
        {
            return false;
        }
    }
}
