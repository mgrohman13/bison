using System;
using System.Collections.Generic;

namespace GalWar
{
    internal class HandlerWrapper : IEventHandler
    {
        private static bool callback = false;
        private IEventHandler handler;

        internal HandlerWrapper(IEventHandler handler, Game game)
            : this(handler, game, true)
        {
        }
        internal HandlerWrapper(IEventHandler handler, Game game, bool clearStack)
        {
            AssertException.Assert(handler != null);
            AssertException.Assert(!callback);

            this.handler = handler;
            ( (IEventHandler)this ).Event();

            if (clearStack)
                game.ClearUndoStack();
        }

        Tile IEventHandler.getBuildTile(Colony colony)
        {
            callback = true;

            try
            {
                return handler.getBuildTile(colony);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return null;
            }
            finally
            {
                callback = false;
            }
        }

        Buildable IEventHandler.getNewBuild(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            callback = true;

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
            finally
            {
                callback = false;
            }

            if (colony.CanBuild(retVal))
                return retVal;
            return colony.Player.Game.StoreProd;
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int free, int totalPop, double soldiers)
        {
            callback = true;

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
            finally
            {
                callback = false;
            }

            return retVal;
        }

        bool IEventHandler.Continue()
        {
            callback = true;

            bool retVal;

            try
            {
                retVal = handler.Continue();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                retVal = true;
            }
            finally
            {
                callback = false;
            }

            return retVal;
        }

        bool IEventHandler.ConfirmCombat(Combatant attacker, Combatant defender)
        {
            callback = true;

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
            finally
            {
                callback = false;
            }

            return retVal;
        }

        bool IEventHandler.Explore(Anomaly.AnomalyType anomalyType, params object[] info)
        {
            callback = true;

            bool retVal;

            try
            {
                retVal = handler.Explore(anomalyType, info);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                retVal = true;
            }
            finally
            {
                callback = false;
            }

            return retVal;
        }

        void IEventHandler.OnResearch(ShipDesign newDesign, HashSet<ShipDesign> obsolete)
        {
            callback = true;

            try
            {
                handler.OnResearch(newDesign, obsolete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                callback = false;
            }
        }

        void IEventHandler.OnCombat(Combatant attacker, Combatant defender, int attack, int defense)
        {
            callback = true;

            try
            {
                handler.OnCombat(attacker, defender, attack, defense);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                callback = false;
            }
        }

        void IEventHandler.OnLevel(Ship ship, double pct, int last, int needed)
        {
            callback = true;

            try
            {
                handler.OnLevel(ship, pct, last, needed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                callback = false;
            }
        }

        void IEventHandler.OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage)
        {
            callback = true;

            try
            {
                handler.OnBombard(ship, planet, freeDmg, colonyDamage, planetDamage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                callback = false;
            }
        }

        void IEventHandler.OnInvade(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense)
        {
            callback = true;

            try
            {
                handler.OnInvade(ship, colony, attackers, attSoldiers, gold, attack, defense);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                callback = false;
            }
        }

        void IEventHandler.Event()
        {
            callback = true;

            try
            {
                handler.Event();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                callback = false;
            }
        }
    }
}
