using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    public class GalWarAI : IGalWarAI
    {
        private Game game;

        private IState state;

        public void SetGame(Game game)
        {
            this.game = game;
        }

        public void PlayTurn(IEventHandler handler)
        {
            if (state == null)
            {
                this.state = Transition();
            }
            else
            {
                if (state.TransitionOK())
                    this.state = Transition();
                else
                    this.state = EmergencyTransition();
            }

            TacticalOverrides(handler);

            state.PlayTurn(handler);
        }

        private void TacticalOverrides(IEventHandler handler)
        {

        }

        private IState EmergencyTransition()
        {
            IState state = this.state;



            return state;
        }

        private IState Transition()
        {
            IState state = EmergencyTransition();
            if (state != null)
                return state;
            state = this.state;



            return state;
        }
    }
}
