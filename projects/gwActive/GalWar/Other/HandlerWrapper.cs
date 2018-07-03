using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    internal class HandlerWrapper : IEventHandler
    {
        private static bool callback = false, special = false;

        private IEventHandler handler;

        internal HandlerWrapper(IEventHandler handler, Game game)
            : this(handler, game, true)
        {
        }
        internal HandlerWrapper(IEventHandler handler, Game game, bool clearStack)
            : this(handler, game, clearStack, false)
        {
        }
        internal HandlerWrapper(IEventHandler handler, Game game, bool clearStack, bool special)
        {
            AssertException.Assert(handler != null);
            AssertException.Assert(!callback || ( HandlerWrapper.special && special ));

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

        void IEventHandler.getNewBuild(Colony colony, out Buildable buildable, out bool pause)
        {
            callback = true;
            special = true;

            try
            {
                handler.getNewBuild(colony, out buildable, out pause);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                buildable = colony.Buildable;
                pause = colony.PauseBuild;
            }
            finally
            {
                callback = false;
                special = false;
            }

            if (!colony.CanBuild(buildable))
                buildable = colony.Player.Game.StoreProd;
        }

        int IEventHandler.MoveTroops(Colony fromColony, int max, int totalPop, double soldiers, bool extraCost)
        {
            callback = true;

            int retVal;

            try
            {
                retVal = handler.MoveTroops(fromColony, max, totalPop, soldiers, extraCost);

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

        bool IEventHandler.Continue(Planet planet, int initPop, int initQuality, int stopPop, int stopQuality, int finalPop, int finalQuality)
        {
            callback = true;

            bool retVal;

            try
            {
                retVal = handler.Continue(planet, initPop, initQuality, stopPop, stopQuality, finalPop, finalQuality);
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
