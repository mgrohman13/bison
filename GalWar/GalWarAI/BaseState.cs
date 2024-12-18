﻿using System;
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

        Tile IEventHandler.GetBuildTile(Colony colony, ShipDesign design)
        {
            //TODO: ?
            HashSet<Tile> options = Tile.GetNeighbors(colony.Tile);
            foreach (Tile t in options.ToArray())
                if (t.SpaceObject != null)
                    options.Remove(t);
            return Game.Random.SelectValue(options);
        }

        Buildable IEventHandler.GetNewBuild(Colony colony, double production, bool floor)
        {
            //all buildable changes are handled explicitly
            Buildable buildable = colony.CurBuild;
            //pause = colony.PauseBuild;
            return buildable;
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int totalPop, double soldiers, bool extraCost)
        {
            if (fromColony == null)
                //TODO: make sure you will not immediately be invaded
                return 0;
            else
                //TODO: check if we need to evacuate and can safely
                return max;
        }

        bool IEventHandler.Continue(Planet planet, int initPop, int initQuality, int stopPop, int stopQuality, int finalPop, int finalQuality)
        {
            //TODO
            return false;
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender)
        {
            //TODO
            return true;
        }

        bool IEventHandler.Explore(Anomaly.AnomalyType anomalyType, params object[] info)
        {
            //TODO
            return true;
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete)
        {
            ai.OnResearch(newDesign);
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense)
        {
            //do nothing
        }

        void IEventHandler.OnLevel(Ship ship, double pct, int last, int needed)
        {
            //do nothing
        }

        void IEventHandler.OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage)
        {
            //do nothing
        }

        void IEventHandler.OnInvade(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense)
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
