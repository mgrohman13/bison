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
        private GalWarAI ai;

        public BaseState(Game game, GalWarAI ai)
        {
            this.game = game;
            this.ai = ai;
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
