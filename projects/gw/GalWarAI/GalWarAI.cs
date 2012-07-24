using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    public class GalWarAI : IGalWarAI
    {
        #region Control

        private Game game;
        private IState state;

        //info
        internal Dictionary<Ship, Tuple<Tile, int>> ShipMovement = new Dictionary<Ship, Tuple<Tile, int>>();
        private Dictionary<Ship, Tile> newShipMovement = new Dictionary<Ship, Tile>();
        internal ShipDesign NewDesign = null;

        //goals
        internal List<Planet> ColonizePlanet = new List<Planet>();
        internal Dictionary<Colony, Buildable> ColonyProduction = new Dictionary<Colony, Buildable>();

        public void SetGame(Game game)
        {
            this.game = game;
        }

        public void PlayTurn(IEventHandler humanHandler)
        {
            TurnStart();

            GlobalPriorities();
            TransitionState(humanHandler);
            state.PlayTurn();

            TurnEnd();
            ClearCache();
        }

        private void TurnStart()
        {
            foreach (Ship s in ShipMovement.Keys.ToArray())
                if (s.Dead)
                    ShipMovement.Remove(s);
            LoopShips(false, delegate(Ship s)
            {
                Tuple<Tile, int> value;
                Tile t;
                if (newShipMovement.TryGetValue(s, out t))
                    if (t == s.Tile)
                        value = new Tuple<Tile, int>(( value = ShipMovement[s] ).Item1, value.Item2 + 1);
                    else
                        value = new Tuple<Tile, int>(t, 0);
                else
                    value = new Tuple<Tile, int>(s.Tile, 0);
                ShipMovement[s] = value;
            });
        }

        private void TurnEnd()
        {
            this.NewDesign = null;

            newShipMovement.Clear();
            LoopShips(false, delegate(Ship s)
            {
                newShipMovement[s] = s.Tile;
            });
        }

        private void TransitionState(IEventHandler humanHandler)
        {
            IState emergencyTransition = GetEmergencyState(humanHandler);
            if (emergencyTransition != null)
                state = emergencyTransition;
            else if (state == null || state.TransitionOK())
                state = GetState(humanHandler);
        }

        private IState GetEmergencyState(IEventHandler humanHandler)
        {
            //finish game
            if (game.GetPlayers().Length == 2)
            {
                Player enemy = game.GetPlayers().First<Player>(delegate(Player p)
                {
                    return ( !p.IsTurn );
                });

                int quality;
                int enemyQuality;
                GetQuality(out quality, out enemyQuality);
                //TODO: determine if reasearch victory is potenitially quicker than killing
                if (quality > enemyQuality * Consts.ResearchVictoryMult)
                {
                    if (GetThreatLevel() > 130)
                        return new Defend(game, this, humanHandler);
                    else
                        return new Wait(game, this, humanHandler);
                }

                //kill last
                return new TotalWar(enemy, game, this, humanHandler);
            }

            //immediate threat
            //personality		
            if (GetThreatLevel() > 666)
                return new Defend(game, this, humanHandler);

            //return null if no emergency
            return null;
        }

        private IState GetState(IEventHandler humanHandler)
        {
            if (DoWait())
                return new Wait(game, this, humanHandler);
            if (DoBlitz())
                return new Blitz(game, this, humanHandler);
            Player enemy;
            if (DoTotalWar(out enemy))
                return new TotalWar(enemy, game, this, humanHandler);
            if (DoDefend())
                return new Defend(game, this, humanHandler);

            //default state - this method should never return null
            if (this.state == null)
                return new Wait(game, this, humanHandler);
            return this.state;
        }

        private bool DoWait()
        {
            int quality;
            int enemyQuality;
            GetQuality(out quality, out enemyQuality);
            //achieve victory
            if (quality > enemyQuality * Consts.ResearchVictoryMult)
                return true;

            double turns;
            double enemyTurns;
            GetPopShort(out turns, out enemyTurns);
            //need population
            if (turns > enemyTurns)
                return true;

            ////need research
            //TODO: based on current research value, research income, and total quality
            //game.GetResearch();

            ////other wars
            //TODO: based on ?, and only if reasonably balanced fight

            return false;
        }

        private bool DoBlitz()
        {
            ////easy/close target
            //TODO: based on ?

            return false;
        }

        private bool DoTotalWar(out Player enemy)
        {
            ////current str greater
            //TODO: based on ?

            ////fall behind long-term
            //TODO: based on ?

            ////assist vs too strong
            //TODO: based on ?

            ////prevent victory
            //TODO: based on ?

            enemy = null;
            return false;
        }

        private bool DoDefend()
        {
            ////potenital threat
            //TODO: based on ?

            return false;
        }

        private void GlobalPriorities()
        {
            //note - Anything that relies on significant randomness (ship attacks, invasions, etc.)
            //  should actually be executed here so that the result can be used when determining goals.
            //  Otherwise, a goal should just be set.

            //TODO: tactics - execute these until there are no more
            // save planet	
            //      quality
            //      pop
            // invade	    
            //      take quality
            //      kill quality
            //      kill pop
            // save ship	
            //      retreat
            //      disband
            // kill ship	
            //      kill
            //      chase
            //      corner

            //TODO: check military viability
            //TODO: check current colony ship locations/direction
            LoopPlanets(true, delegate(Planet p)
            {
                TileInfluence inf = new TileInfluence(game, p.Tile);
                int qualRank = inf.GetInfluence(TileInfluence.InfluenceType.Quality).GetRank(game.CurrentPlayer),
                    popRank = inf.GetInfluence(TileInfluence.InfluenceType.Population).GetRank(game.CurrentPlayer);
                if (( qualRank == 0 || popRank == 0 ) && qualRank < 2 && popRank < 2)
                    ColonizePlanet.Add(p);
            });
        }

        internal void Execute()
        {
            //TODO: resolve main goals
            //this.colonizePlanets
            ColonizePlanet.RemoveAll(delegate(Planet p)
            {
                //TODO: check viability?
                return ( p.Colony != null );
            });

            //TODO: Question: How do you free up gold needed in tactics BEFORE resolve economy and finalize?

            //TODO: resolve economy
            //  -resolve the desired buildable for each colony (determining if more prod is needed or has excess)
            //  -set global economy emphasis (compare short term econ considerations with this.state.GetDeafultEconomy)
            //  -set buildable (using this.colonyProduction and this.newDesign) for each colony, determine any excess prod

            //TODO: finalize
            //  buy prod / gold repais
            //      -determine priorities, necessary gold, ideal gold (validate ideal against income)
            //      -sell prod from colonize with excess if needed (weighing ideal/income)
        }

        #endregion //Control

        //TODO: smart caching - optimize tradeoff between lazily initializing and minimizing looping
        #region Information

        [NonSerialized]
        private Dictionary<Colony, Dictionary<Player, double>> cacheThreat = null;
        private void ClearCache()
        {
            cacheThreat = null;
        }

        internal void OnResearch(ShipDesign newDesign)
        {
            this.NewDesign = newDesign;
        }

        internal Dictionary<Colony, Dictionary<Player, double>> GetThreat()
        {
            if (cacheThreat == null)
            {
                cacheThreat = new Dictionary<Colony, Dictionary<Player, double>>();
                foreach (Colony c in game.CurrentPlayer.GetColonies())
                {
                    Dictionary<Player, double> playerThreat = new Dictionary<Player, double>();
                    foreach (Player p in game.GetPlayers())
                        if (!p.IsTurn)
                        {
                            //personality	

                            double moveStrThreat = 1;
                            double moveInvThreat = 1;
                            double buildStr = ( c.GetTotalIncome() / 3 * 3.9 + Math.Sqrt(game.CurrentPlayer.GetPopulation()) * 1.3 ) * GetStrPerProd();
                            Dictionary<Ship, double> shipsWithin = GetShipsWithin(c.Tile, 5.2);
                            foreach (KeyValuePair<Ship, double> pair in shipsWithin)
                                if (pair.Key.Player == p)
                                {
                                    Ship s = pair.Key;
                                    Tuple<Tile, int> move = ShipMovement[s];
                                    double value = ( Tile.GetDistance(move.Item1, c.Tile) - Tile.GetDistance(s.Tile, c.Tile) )
                                            / s.MaxSpeed / ( move.Item2 * move.Item2 + 1 ) / pair.Value;
                                    if (value > 0)
                                    {
                                        moveStrThreat += value * s.GetStrength();
                                        moveInvThreat += value * ( s.BombardDamage * s.MaxSpeed * 3.9 + s.Population );
                                    }
                                }
                            moveStrThreat /= buildStr;
                            double moveThreat = Math.Pow(moveStrThreat, .78) * Math.Pow(moveInvThreat, 2.6);

                            TileInfluence inf = new TileInfluence(game, c.Planet.Tile);
                            TileInfluence.Influence armada = inf.GetInfluence(TileInfluence.InfluenceType.Armada);
                            double strThreat = Math.Pow(( armada.GetValue(p) + 1 ) / ( armada.GetValue(game.CurrentPlayer) + buildStr / 3.9 + 1 ), 1.3);

                            double invDistThreat = ( 7.8 + .52 ) / ( GetInvadeDist(c, 7.8, p) + .52 );
                            double invInfThreat = 1;
                            invInfThreat += inf.GetInfluence(TileInfluence.InfluenceType.Transport).GetValue(p);
                            invInfThreat += inf.GetInfluence(TileInfluence.InfluenceType.DeathStar).GetValue(p) * 3.9;
                            double invThreat = Math.Pow(invDistThreat, 2.1) * Math.Pow(invInfThreat, .39);

                            playerThreat[p] = moveThreat * strThreat * invThreat;
                        }
                    cacheThreat[c] = playerThreat;
                }
            }
            return cacheThreat;
        }
        internal Dictionary<Player, double> GetThreat(Colony c)
        {
            return GetThreat()[c];
        }
        internal Dictionary<Colony, double> GetThreat(Player p)
        {
            Dictionary<Colony, double> ret = new Dictionary<Colony, double>();
            foreach (KeyValuePair<Colony, Dictionary<Player, double>> pair in GetThreat())
                ret[pair.Key] = pair.Value[p];
            return ret;
        }
        internal double GetThreatLevel()
        {
            return CombineThreat(EnumThreatLevel());
        }
        private IEnumerable<double> EnumThreatLevel()
        {
            foreach (Dictionary<Player, double> val in GetThreat().Values)
                foreach (double d in val.Values)
                    yield return d;
        }
        internal double GetThreatLevel(Colony c)
        {
            return CombineThreat(GetThreat()[c].Values);
        }
        internal double GetThreatLevel(Player p)
        {
            return CombineThreat(EnumThreatLevel(p));
        }
        private IEnumerable<double> EnumThreatLevel(Player p)
        {
            foreach (Dictionary<Player, double> val in GetThreat().Values)
                yield return val[p];
        }
        internal double GetThreatLevel(Colony c, Player p)
        {
            return GetThreat()[c][p];
        }
        private double CombineThreat(IEnumerable<double> threats)
        {
            double threat = 1, count = 0;
            foreach (double d in threats)
            {
                threat *= d;
                ++count;
            }
            if (count > 1)
                threat = Math.Pow(threat, 1 / Math.Sqrt(count));
            return threat;
        }

        internal void GetPopShort(out double turns, out double enemyTurns)
        {
            //TODO: check actual popluation trend over time
            turns = 0;
            enemyTurns = 0;
            foreach (Planet p in game.GetPlanets())
                if (p.Colony != null)
                    if (p.Colony.Player.IsTurn)
                        turns = Math.Max(turns, GetAvgTurnsToFill(p.Colony));
                    else
                        enemyTurns = Math.Max(turns, GetAvgTurnsToFill(p.Colony));
        }

        internal double GetInvadeDist(Colony c, double max)
        {
            return GetInvadeDist(c, max, null);
        }

        internal double GetInvadeDist(Colony c, double max, Player p2)
        {
            double turns = max;
            foreach (Player p in game.GetPlayers())
                if (p2 == null ? !p.IsTurn : p == p2)
                    foreach (Ship s in p.GetShips())
                        if (s.Population > 0 || s.DeathStar)
                            turns = Math.Min(turns, GetShipDistance(s, c.Tile));
            return turns;
        }

        internal Dictionary<Ship, double> GetShipsWithin(Tile t, double turns)
        {
            Dictionary<Ship, double> retVal = new Dictionary<Ship, double>();
            foreach (Player p in game.GetPlayers())
                foreach (Ship s in p.GetShips())
                {
                    double dist = GetShipDistance(s, t);
                    if (dist < turns + Consts.FLOAT_ERROR)
                        retVal.Add(s, dist);
                }
            return retVal;
        }

        internal double GetShipDistance(Ship s, Tile t)
        {
            return ( Tile.GetDistance(s.Tile, t) - 1 ) / s.MaxSpeed;
        }

        internal double GetAvgTurnsToFill(Colony c)
        {
            double turns = 0;
            double pop = c.Population;
            int quality = c.Planet.Quality;
            if (pop < quality)
                while (true)
                {
                    double growth = Consts.GetPopulationGrowth(pop, quality);
                    if (growth + pop < quality)
                    {
                        ++turns;
                    }
                    else
                    {
                        turns += ( quality - pop ) / growth;
                        break;
                    }
                    pop += growth;
                }
            return turns;
        }

        internal void GetQuality(out int quality, out int enemyQuality)
        {
            quality = 0;
            enemyQuality = 0;
            foreach (Player p in game.GetPlayers())
            {
                int tot = 0;
                foreach (Colony c in p.GetColonies())
                    tot += c.Planet.Quality;
                if (p.IsTurn)
                    quality = tot;
                else
                    enemyQuality = Math.Max(enemyQuality, tot);
            }
        }

        internal double GetStrPerProd()
        {
            double strPerProd = 0;
            foreach (ShipDesign design in game.CurrentPlayer.GetShipDesigns())
                strPerProd = Math.Max(strPerProd, design.GetStrength() / ( design.Cost + design.Upkeep * design.GetUpkeepPayoff(game.MapSize) ));
            return strPerProd;
        }

        internal void LoopShips(Action<Ship> Action)
        {
            LoopShips(null, Action);
        }
        internal void LoopShips(bool friendly, Action<Ship> Action)
        {
            LoopShips(delegate(Ship s)
            {
                return ( friendly == s.Player.IsTurn );
            }, Action);
        }
        internal void LoopShips(Predicate<Ship> Filter, Action<Ship> Action)
        {
            foreach (Player p in game.GetPlayers())
                foreach (Ship s in p.GetShips())
                    if (Filter == null || Filter(s))
                        Action(s);
        }

        internal void LoopPlanets(Action<Planet> Action)
        {
            LoopPlanets(null, Action);
        }
        internal void LoopPlanets(bool uncolonized, Action<Planet> Action)
        {
            LoopPlanets(delegate(Planet p)
            {
                return ( uncolonized == ( p.Colony == null ) );
            }, Action);
        }
        internal void LoopPlanets(Predicate<Planet> Filter, Action<Planet> Action)
        {
            foreach (Planet p in game.GetPlanets())
                if (Filter == null || Filter(p))
                    Action(p);
        }

        #endregion //Information

        //All actual command to the game should be sent through here and ClearCache() probably called after each one
        #region Commands



        #endregion //Commands
    }
}
