using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    class Blitz : BaseState
    {
        public Blitz(Game game, GalWarAI ai, IEventHandler humanHandler)
            : base(game, ai, humanHandler)
        {
        }

        protected override void GetDeafultEconomy(out bool gold, out bool research, out bool production)
        {
            //TODO: gold?
            gold = true;
            research = false;
            production = false;
        }
    }
}
