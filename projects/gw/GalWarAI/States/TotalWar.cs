using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    class TotalWar : BaseState
    {
        internal readonly Player Enemy;

        public TotalWar(Player enemy, Game game, GalWarAI ai, IEventHandler humanHandler)
            : base(game, ai, humanHandler)
        {
            this.Enemy = enemy;
        }

        protected override void GetDeafultEconomy(out bool gold, out bool research, out bool production)
        {
            //TODO: determine when gold vs. prod
            gold = true;
            research = false;
            production = true;
        }
    }
}
