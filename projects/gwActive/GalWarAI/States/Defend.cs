using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    class Defend : BaseState
    {
        public Defend(Game game, GalWarAI ai, IEventHandler humanHandler)
            : base(game, ai, humanHandler)
        {
        }

        protected override void GetDeafultEconomy(out bool gold, out bool research, out bool production)
        {
            //TODO: may also need gold instead?
            gold = false;
            research = false;
            production = true;
        }
    }
}
