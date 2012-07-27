using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    internal abstract class BaseState : IState, IEventHandler
    {
        protected Game game;
        private GalWarAI ai;
        private IEventHandler humanHandler;

        public BaseState(Game game, GalWarAI ai, IEventHandler humanHandler)
        {
            this.game = game;
            this.ai = ai;
            this.humanHandler = humanHandler;
        }

        public void PlayTurn()
        {
            FindGoals();
            ai.Execute();
        }

        //TODO: abstract
        public virtual bool TransitionOK()
        {
            //TODO: check if we can transition out
            return true;
        }
        //TODO: abstract
        protected void FindGoals()
        {
            //TODO: set goals in this.ai

            //TODO: Wait: store prod
        }
        protected abstract void GetDeafultEconomy(out bool gold, out bool research, out bool production);

        #region IEventHandler Members

        Tile IEventHandler.getBuildTile(Colony colony)
        {
            //TODO: ?
            HashSet<Tile> options = Tile.GetNeighbors(colony.Tile);
            foreach (Tile t in options.ToArray())
                if (t.SpaceObject != null)
                    options.Remove(t);
            return options.ToArray()[Game.Random.Next(options.Count)];
        }

        Buildable IEventHandler.getNewBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            //all buildable changes are handled explicitly
            return colony.Buildable;
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int free, int totalPop, double soldiers)
        {
            if (fromColony == null)
                //TODO: make sure you will not immediately be invaded
                return 0;
            else
                //TODO: check if we need to evacuate and can safely
                return max;
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender)
        {
            //TODO
            return true;
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            ai.OnResearch(newDesign);
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int popLoss)
        {
            //do nothing
        }

        void IEventHandler.OnLevel(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp)
        {
            //do nothing
        }

        void IEventHandler.Event()
        {
            //propogate events back to the UI
            humanHandler.Event();
        }

        #endregion
    }
}
