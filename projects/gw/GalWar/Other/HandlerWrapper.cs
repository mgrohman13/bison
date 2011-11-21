using System;
using System.Collections.Generic;
using System.Text;

namespace GalWar
{
    internal class HandlerWrapper : IEventHandler
    {
        private IEventHandler handler;

        internal HandlerWrapper(IEventHandler handler)
        {
            AssertException.Assert(handler != null);
            this.handler = handler;
        }

        Tile IEventHandler.getBuildTile(Colony colony)
        {
            try
            {
                return handler.getBuildTile(colony);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

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
                Console.WriteLine(e.ToString());

                retVal = colony.Buildable;
            }

            if (colony.CanBuild(retVal))
                return retVal;
            return colony.Player.Game.StoreProd;
        }

        int IEventHandler.MoveTroops(Colony fromColony, int total, int free, int totalPop, double soldiers)
        {
            int retVal;

            try
            {
                retVal = handler.MoveTroops(fromColony, total, free, totalPop, soldiers);

                if (retVal < 0)
                    retVal = 0;
                else if (retVal > total)
                    retVal = total;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                if (fromColony == null)
                    retVal = total;
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
                Console.WriteLine(e.ToString());

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
                Console.WriteLine(e.ToString());
            }
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int popLoss)
        {
            try
            {
                handler.OnCombat(attacker, defender, attack, defense, popLoss);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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
                Console.WriteLine(e.ToString());
            }
        }
    }
}
