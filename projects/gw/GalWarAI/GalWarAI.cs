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
            TacticalOverrides(handler);

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

            state.PlayTurn(handler);
        }

        private IState EmergencyTransition()
        {
            foreach (Planet p in game.GetPlanets())
                if (p.Colony == null)
                {
                    TileInfluence inf = new TileInfluence(game, p.Tile);
                    return new Colonize(game, this);
                }

            return this.state;
        }

        private IState Transition()
        {
            IState state = EmergencyTransition();
            if (state != null)
                return state;
            state = this.state;



            return state;
        }

        private void TacticalOverrides(IEventHandler handler)
        {
            foreach (Planet p in game.GetPlanets())
                if (p.Colony == null)
                {
                    TileInfluence inf = new TileInfluence(game, p.Tile);
                    if (inf.GetInfluence(TileInfluence.InfluenceType.Prod).GetRank(game.CurrentPlayer) == 0)
                        ColonizePlanet(p);
                }
        }

        private void ColonizePlanet(Planet p)
        {
        }
    }
}
