using System;
using System.Collections.Generic;

namespace GalWar
{
    internal class HandlerWrapper : IEventHandler
    {
        private IEventHandler handler;

        internal HandlerWrapper(IEventHandler handler, Game game)
            : this(handler, game, true)
        {
        }
        internal HandlerWrapper(IEventHandler handler, Game game, bool clearStack)
        {
            AssertException.Assert(handler != null);
            this.handler = handler;
            this.Event();

            if (clearStack)
                game.ClearUndoStack();
        }

        Tile IEventHandler.getBuildTile(Colony colony)
        {
            try
            {
                return handler.getBuildTile(colony);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return null;
            }
        }

        Buildable IEventHandler.getNewBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            Buildable retVal;

            try
            {
                retVal = handler.getNewBuild(colony, accountForIncome, switchLoss, additionalLosses);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                retVal = colony.Buildable;
            }

            if (colony.CanBuild(retVal))
                return retVal;
            return colony.Player.Game.StoreProd;
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int free, int totalPop, double soldiers)
        {
            int retVal;

            try
            {
                retVal = handler.MoveTroops(fromColony, max, free, totalPop, soldiers);

                if (retVal < 0)
                    retVal = 0;
                else if (retVal > max)
                    retVal = max;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (fromColony == null)
                    retVal = max;
                else
                    retVal = 0;
            }

            return retVal;
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender, int freeDmg)
        {
            bool retVal;

            try
            {
                retVal = handler.ConfirmCombat(attacker, defender, freeDmg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                retVal = true;
            }

            return retVal;
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            try
            {
                handler.OnResearch(newDesign, obsolete, oldDefense, newDefense);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int startHP, int popLoss)
        {
            try
            {
                handler.OnCombat(attacker, defender, attack, defense, startHP, popLoss);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void IEventHandler.OnLevel(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp)
        {
            try
            {
                handler.OnLevel(ship, expType, pct, needExp, lastExp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void IEventHandler.OnBombard(Ship ship, Planet planet, Colony colony, int freeDmg, int colonyDamage, int planetDamage, int startExp)
        {
            try
            {
                handler.OnBombard(ship, planet, colony, freeDmg, colonyDamage, planetDamage, startExp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region IEventHandler Members


        public void Event()
        {
            try
            {
                handler.Event();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}
