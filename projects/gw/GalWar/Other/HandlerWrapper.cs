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

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender)
        {
            bool retVal;

            try
            {
                retVal = handler.ConfirmCombat(attacker, defender);
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

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense)
        {
            try
            {
                handler.OnCombat(attacker, defender, attack, defense);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void IEventHandler.OnLevel(Ship ship, double pct, int last, int needed)
        {
            try
            {
                handler.OnLevel(ship, pct, last, needed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void IEventHandler.OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage)
        {
            try
            {
                handler.OnBombard(ship, planet, freeDmg, colonyDamage, planetDamage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void IEventHandler.OnInvade(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense)
        {
            try
            {
                handler.OnInvade(ship, colony, attackers, attSoldiers, gold, attack, defense);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

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
    }
}
