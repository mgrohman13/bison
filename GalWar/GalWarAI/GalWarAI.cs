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
        internal int Research = Game.Random.Round(Consts.StartResearch);
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
            foreach (Ship s in LoopShips(false))
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
            }
        }

        private void TurnEnd()
        {
            this.NewDesign = null;

            newShipMovement.Clear();
            foreach (Ship s in LoopShips(false))
                newShipMovement[s] = s.Tile;

            int research;
            double population, production, gold;
            game.CurrentPlayer.GetTurnIncome(out population, out research, out production, out gold);
            this.Research += research;
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
            if (game.GetPlayers().Count == 2)
            {
                Player enemy = game.GetPlayers().First<Player>(delegate(Player p)
                {
                    return ( !p.IsTurn );
                });

                //personality		
                int quality;
                int enemyQuality;
                GetQuality(out quality, out enemyQuality);
                Tuple<Player, int> victory = GetTurnsToVictory();
                if (victory.Item1.IsTurn)
                {
                    //TODO: determine if reasearch victory is potenitially quicker than killing
                }
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
            if (DoDefend())
                return new Defend(game, this, humanHandler);
            Player enemy;
            if (DoTotalWar(out enemy))
                return new TotalWar(enemy, game, this, humanHandler);

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
            //TODO: check actual popluation trend over time
            GetPopNeededMaxTurns(out turns, out enemyTurns);
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

            ////prevent victory
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
            foreach (Planet p in LoopPlanets(true))
            {
                TileInfluence inf = TileInfluence.GetInfluence(game, this, p.Tile);
                int qualRank = inf.GetInfluence(TileInfluence.InfluenceType.Quality).GetRank(game.CurrentPlayer),
                    popRank = inf.GetInfluence(TileInfluence.InfluenceType.Population).GetRank(game.CurrentPlayer);
                if (( qualRank == 0 || popRank == 0 ) && qualRank < 2 && popRank < 2)
                    ColonizePlanet.Add(p);
            }
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
        private Tuple<Player, int> cacheTurnsToVictory = null;
        private Dictionary<Graphs.GraphType, Tuple<float[, ,], Dictionary<Player, int>>> cacheGraph = null;
        private void ClearCache()
        {
            cacheThreat = null;
            cacheTurnsToVictory = null;
            cacheGraph = null;

            TileInfluence.ClearCache();
        }

        internal void OnResearch(ShipDesign newDesign)
        {
            this.NewDesign = newDesign;

            if (this.Research < newDesign.Research)
                this.Research = newDesign.Research;
            //TODO: bring down if significatly greater?
        }

        internal Dictionary<Colony, Dictionary<Player, double>> GetThreat()
        {
            if (cacheThreat == null)
            {
                cacheThreat = new Dictionary<Colony, Dictionary<Player, double>>();
                foreach (Colony c in LoopColonies(true))
                {
                    var playerThreat = new Dictionary<Player, double>();
                    foreach (Player p in LoopPlayers(false))
                    {
                        //personality	

                        double moveStrThreat = 1;
                        double moveInvThreat = 1;
                        double buildStr = ( c.GetTotalIncome() / 3 * 3.9 + Math.Sqrt(game.CurrentPlayer.GetPopulation()) * 1.3 ) * GetStrPerProd();
                        foreach (Tuple<Ship, double> pair in GetShipsWithin(c.Tile, 5.2))
                            if (pair.Item1.Player == p)
                            {
                                Ship s = pair.Item1;
                                Tuple<Tile, int> move = ShipMovement[s];
                                double value = ( Tile.GetDistance(move.Item1, c.Tile) - Tile.GetDistance(s.Tile, c.Tile) )
                                        / s.MaxSpeed / ( move.Item2 * move.Item2 + 1 ) / pair.Item2;
                                if (value > 0)
                                {
                                    moveStrThreat += value * s.GetStrength();
                                    moveInvThreat += value * ( s.BombardDamage * s.MaxSpeed * 3.9 + s.Population );
                                }
                            }
                        moveStrThreat /= buildStr;
                        double moveThreat = Math.Pow(moveStrThreat, .78) * Math.Pow(moveInvThreat, 2.6);

                        TileInfluence inf = TileInfluence.GetInfluence(game, this, c.Tile);
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
            var ret = new Dictionary<Colony, double>();
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

        internal Tuple<Player, int> GetTurnsToVictory()
        {
            if (cacheTurnsToVictory == null)
            {
                //TODO: does not take into account impending invasions, colonization, currently transporting troops...

                //TODO: take into account turn order
                Dictionary<Player, List<Tuple<int, double>>> colonies = new Dictionary<Player, List<Tuple<int, double>>>();
                Dictionary<Player, double> totalPop = new Dictionary<Player, double>();
                foreach (Colony c in LoopColonies())
                {
                    List<Tuple<int, double>> playerColonies;
                    if (!colonies.TryGetValue(c.Player, out playerColonies))
                    {
                        playerColonies = new List<Tuple<int, double>>();
                        colonies[c.Player] = playerColonies;
                    }
                    playerColonies.Add(new Tuple<int, double>(c.Planet.Quality, c.Population));

                    double pop;
                    totalPop.TryGetValue(c.Player, out pop);
                    totalPop[c.Player] = pop + c.Population;
                }

                Dictionary<Player, double> currentReseach = game.GetResearch();
                double mult = this.Research / currentReseach[game.CurrentPlayer];
                Dictionary<Player, double> research = new Dictionary<Player, double>();
                foreach (Player p in LoopPlayers())
                    research[p] = currentReseach[p] * mult;

                //personality		
                //PlanetCreationRate
                int maxTurns = (int)Math.Round(1.3 / 104);
                for (int a = 0 ; a <= maxTurns ; ++a)
                {
                    Player top = null;
                    double maxR = -1, secondR = -1;
                    foreach (Player p in LoopPlayers())
                    {
                        double playerPop = totalPop[p];

                        //check victory
                        double res = research[p];
                        if (res > maxR)
                        {
                            top = p;
                            secondR = maxR;
                            maxR = res;
                        }
                        else if (res > secondR)
                        {
                            secondR = res;
                        }

                        //inc research
                        research[p] = res + playerPop * Consts.Income / 3;

                        //grow pop
                        List<Tuple<int, double>> playerColonies = colonies[p];
                        for (int b = 0 ; b < playerColonies.Count ; ++b)
                        {
                            Tuple<int, double> colony = playerColonies[b];
                            double growth = Consts.GetPopulationGrowth(colony.Item2, colony.Item1);
                            playerColonies[b] = new Tuple<int, double>(colony.Item1, colony.Item2 + growth);
                            playerPop += growth;
                        }

                        totalPop[p] = playerPop;
                    }

                    if (maxR > secondR * Consts.ResearchVictoryMult)
                    {
                        cacheTurnsToVictory = new Tuple<Player, int>(top, a);
                        return cacheTurnsToVictory;
                    }
                }

                cacheTurnsToVictory = new Tuple<Player, int>(null, int.MaxValue);
            }

            return cacheTurnsToVictory;
        }

        private Tuple<float[, ,], Dictionary<Player, int>> GetGraph(Graphs.GraphType type)
        {
            if (cacheGraph == null)
                cacheGraph = new Dictionary<Graphs.GraphType, Tuple<float[, ,], Dictionary<Player, int>>>();

            Tuple<float[, ,], Dictionary<Player, int>> retVal;
            if (!cacheGraph.TryGetValue(type, out retVal))
            {
                Dictionary<int, Player> playerIndexes;
                float[, ,] graph = game.Graphs.Get(type, out playerIndexes);
                var reverse = new Dictionary<Player, int>();
                foreach (KeyValuePair<int, Player> pair in playerIndexes)
                    reverse[pair.Value] = pair.Key;
                retVal = new Tuple<float[, ,], Dictionary<Player, int>>(graph, reverse);
                cacheGraph[type] = retVal;
            }
            return retVal;
        }

        internal void GetPopNeededMaxTurns(out double turns, out double enemyTurns)
        {
            //TODO: take into account turn order
            turns = 0;
            enemyTurns = 0;
            foreach (Colony c in LoopColonies())
                if (c.Player.IsTurn)
                    turns = Math.Max(turns, GetAvgTurnsToFill(c));
                else
                    enemyTurns = Math.Max(turns, GetAvgTurnsToFill(c));
        }

        internal double GetInvadeDist(Colony c, double max)
        {
            return GetInvadeDist(c, max, null);
        }
        internal double GetInvadeDist(Colony c, double max, Player enemy)
        {
            double turns = max;
            foreach (Ship s in ( enemy == null ? LoopShipsNotPlayer(c.Player) : LoopShips(enemy) ))
                if (( s.Population > 0 || s.DeathStar ))
                    turns = Math.Min(turns, GetShipDistance(s, c.Tile));
            return turns;
        }

        internal IEnumerable<Tuple<Ship, double>> GetShipsWithin(Tile t, double turns)
        {
            foreach (Ship s in LoopShips())
            {
                double dist = GetShipDistance(s, t);
                if (dist < turns * Consts.FLOAT_ERROR_ONE)
                    yield return new Tuple<Ship, double>(s, dist);
            }
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
            quality = GetQuality(game.CurrentPlayer);
            enemyQuality = 0;
            foreach (Player p in LoopPlayers(false))
                enemyQuality = Math.Max(enemyQuality, GetQuality(p));
        }
        internal int GetQuality(Player p)
        {
            int tot = 0;
            foreach (Colony c in LoopColonies(p))
                tot += c.Planet.Quality;
            return tot;
        }

        internal int GetPopulation(Player p)
        {
            int tot = 0;
            foreach (Colony c in LoopColonies(p))
                tot += c.Population;
            return tot;
        }

        internal double GetStrPerProd()
        {
            double strPerProd = 0;
            //foreach (ShipDesign design in LoopDesigns())
            //    strPerProd = Math.Max(strPerProd, design.GetStrength() / ( design.Cost + design.Upkeep * design.GetUpkeepPayoff(game.MapSize) ));
            return strPerProd;
        }

        internal IEnumerable<Player> LoopPlayers()
        {
            return game.GetPlayers();
        }
        internal IEnumerable<Player> LoopPlayers(bool friendly)
        {
            if (friendly)
                yield return game.CurrentPlayer;
            else
                foreach (Player p in LoopNotPlayer(game.CurrentPlayer))
                    yield return p;
        }
        private IEnumerable<Player> LoopNotPlayer(Player player)
        {
            foreach (Player p in LoopPlayers())
                if (p != player)
                    yield return p;
        }

        internal IEnumerable<ShipDesign> LoopDesigns()
        {
            return game.CurrentPlayer.GetShipDesigns();
        }

        internal IEnumerable<Ship> LoopShips()
        {
            return LoopShips((bool?)null);
        }
        internal IEnumerable<Ship> LoopShips(bool? friendly)
        {
            foreach (Player p in ( friendly.HasValue ? LoopPlayers(friendly.Value) : LoopPlayers() ))
                foreach (Ship s in LoopShips(p))
                    yield return s;
        }
        internal IEnumerable<Ship> LoopShips(Player p)
        {
            return p.GetShips();
        }
        private IEnumerable<Ship> LoopShipsNotPlayer(Player player)
        {
            foreach (Player p in LoopNotPlayer(player))
                foreach (Ship s in LoopShips(p))
                    yield return s;
        }

        internal IEnumerable<Colony> LoopColonies()
        {
            return LoopColonies((bool?)null);
        }
        internal IEnumerable<Colony> LoopColonies(bool? friendly)
        {
            foreach (Player p in ( friendly.HasValue ? LoopPlayers(friendly.Value) : LoopPlayers() ))
                foreach (Colony c in LoopColonies(p))
                    yield return c;
        }
        internal IEnumerable<Colony> LoopColonies(Player p)
        {
            return p.GetColonies();
        }

        internal IEnumerable<Planet> LoopPlanets()
        {
            return game.GetPlanets();
        }
        internal IEnumerable<Planet> LoopPlanets(bool uncolonized)
        {
            if (uncolonized)
            {
                foreach (Planet p in LoopPlanets())
                    if (p.Colony == null)
                        yield return p;
            }
            else
            {
                foreach (Colony c in LoopColonies())
                    yield return c.Planet;
            }
        }

        #endregion //Information

        //All actual commands to the game should be sent through here and ClearCache() probably called after each one
        #region Commands



        #endregion //Commands
    }
}
