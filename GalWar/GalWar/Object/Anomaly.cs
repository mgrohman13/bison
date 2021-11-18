using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class Anomaly : SpaceObject
    {
        #region Explore

        private float _planetChance;

        [NonSerialized]
        private double _value = double.NaN;
        [NonSerialized]
        private bool _usedValue = false;

        internal Anomaly(Tile tile)
            : base(tile)
        {
            checked
            {
                this._planetChance = Game.Random.GaussianOE(1f, .39f, .26f);
            }
        }

        internal double planetChance
        {
            get
            {
                return this._planetChance;
            }
            set
            {
                checked
                {
                    this._planetChance = (float)value;
                }
            }
        }

        private double value
        {
            get
            {
                this.usedValue = true;
                return this._value;
            }
            set
            {
                checked
                {
                    this._value = value;
                }
            }
        }
        private bool usedValue
        {
            get
            {
                return this._usedValue;
            }
            set
            {
                checked
                {
                    this._usedValue = value;
                }
            }
        }

        public override Player Player
        {
            get
            {
                return null;
            }
        }

        internal void Explore(IEventHandler handler, Ship ship)
        {
            this.value = GenerateValue();
            this.Tile.SpaceObject = null;

            Planet planet = Tile.Game.CreateAnomalyPlanet(handler, this);
            if (planet != null)
            {
                ship.Player.GoldIncome(ConsolationValue());
                return;
            }

            Dictionary<ExploreType, int> options = new Dictionary<ExploreType, int>();
            options.Add(GlobalEvent, 1);//high
            options.Add(LostColony, 2);//med
            options.Add(Death, 3);//always
            options.Add(Production, 6);//always
            options.Add(SalvageShip, 13);//always
            options.Add(Pickup, 15);//low
            options.Add(Valuables, 16);//always
            options.Add(Experience, 17);//always
            options.Add(Wormhole, 21);//high

            while (true)
            {
                this.usedValue = false;

                ExploreType ExploreType = Game.Random.SelectValue(options);
                if (ExploreType(handler, ship))
                {
                    if (!this.usedValue)
                        throw new Exception();

                    return;
                }
                else
                {
                    if (Game.Random.Bool())
                        options.Remove(ExploreType);
                }
            }
        }
        private delegate bool ExploreType(IEventHandler handler, Ship ship);

        private double GenerateValue()
        {
            const double soldierMult = Consts.ProductionForSoldiers / Math.PI;
            double quality = 0, pop = 0, armada = 0;
            foreach (Planet planet in Tile.Game.GetPlanets())
            {
                Colony colony = planet.Colony;
                if (colony == null)
                {
                    quality += planet.PlanetValue / 3.9;
                }
                else
                {
                    quality += planet.PlanetValue;
                    pop += colony.Population;
                    armada += colony.PDCostAvgResearch / 2.6 + colony.totalProd / 1.69 + colony.Soldiers * soldierMult;
                }
            }
            foreach (Player player in Tile.Game.GetPlayers())
            {
                foreach (Ship ship in player.GetShips())
                {
                    pop += ship.Population / 1.3;
                    armada += ship.GetCostAvgResearch() + ship.Soldiers * soldierMult / 1.3;
                }
                armada += player.TotalGold / 2.1;
            }

            double value = (armada / 52.0 + Consts.Income / 2.6 * (5 * pop + 2 * quality) / 7.0) / (double)Tile.Game.GetPlayers().Count;
            value = Consts.LimitMin(value, 13);
            return Game.Random.GaussianOE(value, .26, .3, GenerateConsolationValue(value));
        }
        private static double GenerateConsolationValue(double value)
        {
            double avg = (Math.Pow(value + 1, .78) - 1) * .52;
            return Game.Random.GaussianCapped(avg, .21, avg > 1 ? 1 : 0);
        }

        private double ConsolationValue()
        {
            return GenerateConsolationValue(this.value);
        }

        private static int GetDesignResearch(double designResearch)
        {
            double dev = 3.9 / Math.Sqrt(designResearch);
            return Game.Random.GaussianOEInt(designResearch, dev, dev * .26);
        }
        private double GetAvgDesignResearch(Player player, double value)
        {
            return (1 * ((1 * player.ResearchDisplay + 3 * player.Research) / 4.0)
                    + 1 * (player.GetLastResearched() + value) + 2 * (Tile.Game.AvgResearch)) / 4.0;
        }
        private void CompensateDesign(Player player, ShipDesign design, double designResearch, double expectedShips)
        {
            expectedShips *= (1 + Consts.RepairCostMult * .39);
            double gold = (design.GetTotCost() - design.GetTotCost(Game.Random.Round(designResearch))) * expectedShips;
            double consolation = ConsolationValue();
            if (gold > consolation)
                gold = consolation + Math.Pow(gold - consolation + 1, .91) - 1;
            if (gold > value)
                gold = value + Math.Pow(gold - consolation + 1, .13) - 1;
            player.GoldIncome(gold);
        }

        private Tile GetRandomTile(Ship anomShip, int move)
        {
            var anomalies = Enumerable.Empty<Tile>();
            if (Game.Random.Bool())
                anomalies = anomalies.Union(Tile.Game.GetSpaceObjects().OfType<Anomaly>().Select(a => a.Tile));
            if (Game.Random.Bool())
                anomalies = anomalies.Union(Tile.Game.GetWormholes().SelectMany(w => w.Tiles));

            if (anomalies.Any())
                return MoveTile(Game.Random.SelectValue(anomalies), 2.1, anomShip, move);
            return Tile.Game.GetRandomTile();
        }

        private Dictionary<Player, int> GetPlayerProximity()
        {
            Dictionary<Player, int> retVal = new Dictionary<Player, int>();
            foreach (Player player in Tile.Game.GetPlayers())
            {
                double avgDist = 0, totDist = 0;
                foreach (Colony colony in player.GetColonies())
                {
                    double weight = GetColonyWeight(colony);
                    double distance = Tile.GetDistance(this.Tile, colony.Tile);
                    avgDist += weight * distance * distance;
                    totDist += weight;
                }
                avgDist /= totDist;

                retVal.Add(player, Game.Random.Round(byte.MaxValue / avgDist));
            }
            if (retVal.Values.Sum() == 0)
                return GetPlayerProximity();
            return retVal;
        }
        private Dictionary<Colony, int> GetPlayerColonyWeights(Player player, out double total)
        {
            total = 0;
            var colonies = new Dictionary<Colony, int>();
            foreach (Colony colony in player.GetColonies())
            {
                int weight = Game.Random.Round(GetColonyWeight(colony) * byte.MaxValue / (double)Tile.GetDistance(this.Tile, colony.Tile));
                colonies.Add(colony, weight);
                total += weight;
            }
            return colonies;
        }

        private static double GetColonyWeight(Colony colony)
        {
            return (colony.Planet.PlanetValue / 1.3 + colony.Population);
        }

        internal static Dictionary<SpaceObject, HashSet<SpaceObject>> GetAttInv(Game game)
        {
            return GetAttInv(game, null, null);
        }
        internal static bool GetAttInv(Tile newColony, Player colonyPlayer)
        {
            return GetAttInv(newColony.Game, newColony, colonyPlayer).Single().Value.Any();
        }
        private static Dictionary<SpaceObject, HashSet<SpaceObject>> GetAttInv(Game game, Tile newColony, Player colonyPlayer)
        {
            var retVal = new Dictionary<SpaceObject, HashSet<SpaceObject>>();

            if (newColony == null)
            {
                foreach (SpaceObject spaceObject in game.GetSpaceObjects())
                {
                    var attackers = new HashSet<SpaceObject>();
                    if (spaceObject.Player != null)
                    {
                        bool inv = (spaceObject is Planet);
                        if (!inv && !(spaceObject is Ship))
                            throw new Exception();

                        //check for attacks from Planet Defenses
                        if (!inv)
                            foreach (Tile neighbor in Tile.GetNeighbors(spaceObject.Tile))
                            {
                                Planet planet = neighbor.SpaceObject as Planet;
                                if (planet != null)
                                {
                                    if (planet.Player != null && spaceObject.Player != planet.Player)
                                        attackers.Add(planet);
                                    break;
                                }
                            }

                        attackers.UnionWith(GetAttackers(game, spaceObject.Tile, spaceObject.Player, inv));
                    }
                    retVal.Add(spaceObject, attackers);
                }
            }
            else
            {
                retVal.Add(game.GetSpaceObjects().First(), GetAttackers(game, newColony, colonyPlayer, true));
            }

            return retVal;
        }
        private static HashSet<SpaceObject> GetAttackers(Game game, Tile target, Player trgPlayer, bool inv)
        {
            var attackers = new HashSet<GalWar.SpaceObject>();
            foreach (Player attPlayer in game.GetPlayers())
                if (trgPlayer != attPlayer)
                    foreach (Ship attShip in attPlayer.GetShips())
                        if (!inv || attShip.MaxPop > 0 || attShip.DeathStar)
                        {
                            List<Tile> path = Tile.PathFind(attShip.Tile, target, attShip.Player, false, attShip.CurSpeed + (inv ? 2 : 1));
                            if (path != null)
                            {
                                int diff = attShip.CurSpeed - path.Count;
                                bool canAttack = false;
                                if (inv)
                                {
                                    if (diff > -3)
                                        if (attShip.Population > 0 || attShip.DeathStar)
                                        {
                                            canAttack = true;
                                        }
                                        else if (attShip.MaxPop > 0)
                                        {
                                            //check if the ship could conceivably pick up some population
                                            foreach (PopCarrier popCarrier in attShip.Player.GetShips().Cast<PopCarrier>().Concat(attShip.Player.GetColonies()))
                                                if (popCarrier.AvailablePop > 0)
                                                {
                                                    int speed = attShip.CurSpeed;
                                                    if (popCarrier is Ship)
                                                        speed += ((Ship)popCarrier).CurSpeed;
                                                    if (speed > Tile.GetDistance(attShip.Tile, popCarrier.Tile) - 2)
                                                    {
                                                        canAttack = true;
                                                        break;
                                                    }
                                                }
                                        }
                                }
                                else if (diff > -2)
                                {
                                    canAttack = true;
                                }
                                if (canAttack)
                                    attackers.Add(attShip);
                            }
                        }
            return attackers;
        }

        public enum AnomalyType
        {
            AskProductionOrDefense,
            AskResearchOrGold,
            AskTerraform,
            Apocalypse,
            Death,
            Experience,
            Gold,
            Heal,
            LostColony,
            NewPlanet,
            PickupPopulation,
            PickupSoldiers,
            PlanetDefenses,
            PopulationGrowth,
            Production,
            SalvageShip,
            Soldiers,
            SoldiersAndDefenses,
            Terraform,
            Wormhole,
        }

        #endregion //Explore

        #region LostColony

        private bool LostColony(IEventHandler handler, Ship anomShip)
        {
            if (!Tile.Game.CheckPlanetDistance(this.Tile))
                return false;

            Dictionary<Player, int> playerProximity = GetPlayerProximity();

            if (Game.Random.Bool() && Colony(handler, anomShip.Player, playerProximity, anomShip))
                return true;

            while (playerProximity.Values.Sum() > 0)
                if (Colony(handler, Game.Random.SelectValue(playerProximity), playerProximity, anomShip))
                    return true;

            return false;
        }
        private bool Colony(IEventHandler handler, Player player, Dictionary<Player, int> playerProximity, Ship anomShip)
        {
            playerProximity.Remove(player);

            if (GetAttInv(this.Tile, player))
                return false;

            var colonyDesigns = player.GetDesigns().Where(design => design.Colony);
            if (!colonyDesigns.Any())
                return false;
            int distance = player.GetColonies().Min(colony => Tile.GetDistance(this.Tile, colony.Tile));
            double cost = colonyDesigns.Min(design => (design.Upkeep * distance / (double)design.Speed
                    + design.Cost - design.GetColonizationValue(Tile.Game)));

            double amount = this.value - cost;
            double mult = Consts.GetColonizationMult(Tile.Game) * Consts.AnomalyQualityCostMult;
            if (amount < Consts.GetColonizationCost(Consts.PlanetConstValue, mult))
                return false;

            handler.Explore(AnomalyType.LostColony, player);

            if (player != anomShip.Player)
                anomShip.Player.GoldIncome(ConsolationValue());

            Planet planet = Tile.Game.CreatePlanet(this.Tile);
            planet.ReduceQuality(planet.Quality -
                    MattUtil.TBSUtil.FindValue(value => (amount > Consts.GetColonizationCost(Consts.PlanetConstValue + value, mult)),
                    0, Consts.NewPlanetQuality(), false));
            player.NewColony(player == anomShip.Player ? handler : null, planet, 0, 0, amount - Consts.GetColonizationCost(planet.PlanetValue, mult));

            return true;
        }

        #endregion //LostColony

        #region GlobalEvent

        private bool GlobalEvent(IEventHandler handler, Ship anomShip)
        {
            HashSet<Planet> planets = Tile.Game.GetPlanets();
            double quality = 0, pop = 0, colonies = 0;
            foreach (Planet planet in planets)
            {
                quality += planet.Quality;
                if (planet.Colony != null)
                {
                    pop += planet.Colony.Population;
                    ++colonies;
                }
            }

            if (Game.Random.Bool())
            {
                int addQuality = Consts.TerraformPlanetQuality();
                double cost;
                Planet planet = GetTerraformPlanet(anomShip.Player, addQuality, out cost);
                if (planet != null)
                {
                    double terraformAmt = addQuality;
                    double apocalypseAmt = quality / 2.0;
                    if (Game.Random.Bool(terraformAmt / (terraformAmt + apocalypseAmt)))
                        return DamageVictory(handler, anomShip);
                    return Terraform(handler, planet, addQuality, cost);
                }
                return false;
            }
            else
            {
                foreach (Player player in Tile.Game.GetPlayers())
                    foreach (Ship ship in player.GetShips())
                        pop += ship.Population;
                double forExplorer = this.value * Consts.PopulationForGoldHigh;
                double diePct = Game.Random.GaussianCapped(.3, .169, .13);

                double addAmt = forExplorer + (forExplorer * (2 *
                        ((2 * quality + 1 * pop) / 3.0 / Consts.AverageQuality)
                        + 4 * colonies + 1 * planets.Count) / 7.0);
                double killAmt = diePct * pop;

                if (Game.Random.Bool(addAmt / (addAmt + killAmt)))
                    return KillPop(handler, diePct, anomShip);
                return AddPop(handler, addAmt, forExplorer, anomShip);
            }
        }

        private bool DamageVictory(IEventHandler handler, Ship anomShip)
        {
            handler.Explore(AnomalyType.Apocalypse);

            Dictionary<Player, double> addGold = new Dictionary<Player, double>();
            addGold[anomShip.Player] = ConsolationValue();
            foreach (Planet planet in Tile.Game.GetPlanets())
            {
                double gold = double.NaN, mult = double.NaN, before = double.NaN;
                if (planet.Colony != null)
                {
                    gold = Math.Sqrt((planet.Colony.Population + Consts.PlanetConstValue) / (planet.Quality + Consts.PlanetConstValue)) * Consts.AnomalyQualityCostMult;

                    mult = Consts.GetColonizationMult(Tile.Game) * Consts.AnomalyQualityCostMult;
                    before = Consts.GetColonizationCost(planet.PlanetValue, mult);
                }

                planet.DamageVictory();

                if (planet.Colony != null)
                {
                    gold *= before - Consts.GetColonizationCost(planet.PlanetValue, mult);

                    double amt;
                    addGold.TryGetValue(planet.Player, out amt);
                    addGold[planet.Player] = amt + gold;
                }
            }

            double min = addGold.Values.Min();
            foreach (var pair in addGold)
                pair.Key.AddGold(pair.Value - min, true);

            return true;
        }

        private static bool Terraform(IEventHandler handler, Planet planet, int addQuality, double cost)
        {
            Colony colony = planet.Colony;
            if (colony == null || handler.Explore(AnomalyType.AskTerraform, colony, addQuality, cost))
            {
                if (colony != null)
                    colony.Player.SpendGold(cost);
                else
                    handler.Explore(AnomalyType.Terraform, planet, addQuality);
                planet.ReduceQuality(-addQuality);
            }
            return true;
        }
        private Planet GetTerraformPlanet(Player player, int addQuality, out double cost)
        {
            Colony colony;
            double total;
            Dictionary<Colony, int> colonies = GetPlayerColonyWeights(player, out total);
            do
            {
                colony = Game.Random.SelectValue(colonies);
                colonies.Remove(colony);
                double mult = Consts.GetColonizationMult(Tile.Game) * Consts.AnomalyQualityCostMult;
                double before = Consts.GetColonizationCost(colony.Planet.PlanetValue, mult);
                double after = Consts.GetColonizationCost(colony.Planet.PlanetValue + addQuality, mult);
                cost = Player.RoundGold(after - before - this.value, true);
            } while (!player.HasGold(cost) && colonies.Any());

            Planet retVal;
            if (!player.HasGold(cost))
            {
                retVal = null;
                cost = double.NaN;
                Dictionary<Planet, int> uncolonized = Tile.Game.GetPlanets().Where(planet => planet.Colony == null).ToDictionary(planet => planet,
                        planet => Game.Random.Round(planet.PlanetValue * byte.MaxValue / (double)Tile.GetDistance(this.Tile, planet.Tile)));
                if (uncolonized.Any())
                    retVal = Game.Random.SelectValue(uncolonized);
            }
            else
            {
                retVal = colony.Planet;
            }
            return retVal;
        }

        private bool KillPop(IEventHandler handler, double diePct, Ship anomShip)
        {
            handler.Explore(AnomalyType.Apocalypse);

            anomShip.Player.AddGold(ConsolationValue(), true);

            foreach (Player player in Tile.Game.GetPlayers())
            {
                foreach (Colony colony in player.GetColonies())
                    colony.LosePopulation(GetPopLoss(diePct, colony), true);
                foreach (Ship ship in player.GetShips())
                    ship.LosePopulation(GetPopLoss(diePct, ship), true);
            }

            return true;
        }
        private static int GetPopLoss(double diePct, PopCarrier popCarrier)
        {
            return GetPopChange(popCarrier.Population * diePct, popCarrier.Population * .52);
        }

        private bool AddPop(IEventHandler handler, double addAmt, double forExplorer, Ship anomShip)
        {
            handler.Explore(AnomalyType.PopulationGrowth);

            foreach (Player player in Tile.Game.GetPlayers())
            {
                double amt = (addAmt - forExplorer) / (double)Tile.Game.GetPlayers().Count;
                if (anomShip.Player == player)
                    amt += forExplorer;

                double total;
                Dictionary<Colony, int> colonies = GetPlayerColonyWeights(player, out total);
                amt /= total;

                foreach (var pair in colonies)
                    pair.Key.LosePopulation(-GetPopChange(amt * pair.Value, 1.04 * pair.Key.Population));
            }

            return true;
        }

        private static int GetPopChange(double amt, double upperCap)
        {
            int lowerCap = Game.Random.Round(2 * amt - upperCap);
            while (amt < lowerCap)
            {
                lowerCap = Game.Random.RangeInt(0, lowerCap);
            }
            if (lowerCap < 1)
                lowerCap = ((amt > 1) ? 1 : 0);
            return Game.Random.GaussianCappedInt(amt, .039, lowerCap);
        }

        #endregion //GlobalEvent

        #region Death

        private bool Death(IEventHandler handler, Ship ship)
        {
            int damage = ((ship.MaxHP < 2 || Game.Random.Bool()) ? (ship.MaxHP) : (1 + Game.Random.WeightedInt(ship.MaxHP - 2, .26)));

            handler.Explore(AnomalyType.Death, -damage);

            if (damage < ship.HP)
            {
                int initPop = ship.Population;
                double rawExp = 0, valueExp = 0;

                ship.Damage(damage, ref rawExp, ref valueExp);

                ship.Player.GoldIncome(ConsolationValue() + ship.GetDisbandValue(damage) - (ship.GetValueExpForRawExp(rawExp) + valueExp) / Consts.ExpForGold);

                ship.AddExperience(rawExp, valueExp, initPop);
                ship.LevelUp(handler);
            }
            else
            {
                ship.Player.AddGold(this.value + ship.DisbandValue, true);

                ship.Destroy(true, true);
            }

            return true;
        }

        #endregion //Death

        #region Production

        private bool Production(IEventHandler handler, Ship anomShip)
        {
            double total;
            Dictionary<Colony, int> colonies = GetPlayerColonyWeights(anomShip.Player, out total);

            Colony single = null;
            if (colonies.Count == 1 || Game.Random.Bool())
                single = Game.Random.SelectValue(colonies);

            double value = this.value;
            int production = Game.Random.Round(value / 1.3);

            Func<Colony, bool> AllowProd = colony => (colony.CurBuild is BuildShip || colony.CurBuild is StoreProd);

            bool notify = true;
            AnomalyType type;
            switch (Game.Random.Next(13))
            {
                case 0:
                    type = AnomalyType.Soldiers;
                    break;
                case 1:
                case 2:
                case 3:
                    type = AnomalyType.PlanetDefenses;
                    break;
                case 4:
                    type = AnomalyType.Production;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    if (single != null)
                    {
                        type = AnomalyType.SoldiersAndDefenses;
                        if (AllowProd(single))
                        {
                            value *= 1.3;
                            double showProd = single.CurBuild.GetAddProduction(production, false);
                            type = handler.Explore(AnomalyType.AskProductionOrDefense, single, showProd, value) ? AnomalyType.Production : AnomalyType.SoldiersAndDefenses;
                            notify = false;
                        }
                        else
                            ;
                    }
                    else
                    {
                        type = Game.Random.Bool() ? AnomalyType.Production : AnomalyType.SoldiersAndDefenses;
                    }
                    break;
                case 9:
                case 10:
                case 11:
                case 12:
                    type = AnomalyType.SoldiersAndDefenses;
                    break;
                default:
                    throw new Exception();
            }

            if (type == AnomalyType.Production)
            {
                if (single == null ? !colonies.Keys.Any(AllowProd) : !AllowProd(single))
                    type = AnomalyType.SoldiersAndDefenses;
                else
                    value = production;
            }
            if (type == AnomalyType.Soldiers || type == AnomalyType.SoldiersAndDefenses)
            {
                if (single == null ? colonies.Keys.All(colony => colony.Population == 0) : single.Population == 0)
                    type = AnomalyType.PlanetDefenses;
            }

            if (single == null)
            {
                if (notify)
                    handler.Explore(type);
                value /= total;
                foreach (var pair in colonies)
                    PlanetDefenseRemoteSoldiersProduction(type, pair.Key, value * pair.Value);
            }
            else
            {
                if (notify)
                {
                    double show = value;
                    if (type == AnomalyType.Production)
                        show = single.CurBuild.GetAddProduction(show, false);
                    handler.Explore(type, single, show);
                }
                PlanetDefenseRemoteSoldiersProduction(type, single, value);
            }

            return true;
        }

        private static void PlanetDefenseRemoteSoldiersProduction(AnomalyType type, Colony colony, double value)
        {
            switch (type)
            {
                case AnomalyType.SoldiersAndDefenses:
                    colony.BuildSoldiersAndDefenses(value);
                    break;
                case AnomalyType.Soldiers:
                    colony.BuildSoldiers(value);
                    break;
                case AnomalyType.PlanetDefenses:
                    colony.BuildPlanetDefense(value);
                    break;
                case AnomalyType.Production:
                    colony.AddProduction(value);
                    break;
                default:
                    throw new Exception();
            }
        }

        #endregion //Production

        #region Wormhole

        private bool Wormhole(IEventHandler handler, Ship anomShip)
        {
            bool any = false;

            int move = anomShip.LoseMove();

            if (Game.Random.Bool())
                any |= PushShip(handler, anomShip, move);
            if (Game.Random.Bool())
                any |= CreateWormhole(handler, anomShip, move);
            if (Game.Random.Bool())
                any |= PullIn(handler, anomShip, move);
            if (Game.Random.Bool())
                any |= CreateAnomalies(handler, anomShip, move);

            if (any)
            {
                handler.Explore(AnomalyType.Wormhole);

                anomShip.Player.GoldIncome(ConsolationValue());
            }
            else
            {
                anomShip.UndoLoseMove(move);
            }

            return any;
        }

        private bool PushShip(IEventHandler handler, Ship anomShip, int move)
        {
            Tile tile = GetRandomTile(anomShip, move);
            while (tile.SpaceObject is Anomaly)
                tile = MoveTile(tile, .91, anomShip, move);
            if (tile.SpaceObject == null)
            {
                Dictionary<SpaceObject, HashSet<SpaceObject>> before = GetAttInv(Tile.Game);
                Tile oldTile = anomShip.Tile;
                anomShip.Teleport(tile);

                if (ValidateChange(before, anomShip))
                    return true;
                else
                    anomShip.Teleport(oldTile);
            }
            return false;
        }

        private bool PullIn(IEventHandler handler, Ship anomShip, int move)
        {
            var objects = new Dictionary<SpaceObject, int>();

            double shipValue = Tile.Game.GetSpaceObjects().OfType<Ship>().Average(ship => ship.GetValue());
            var planets = Tile.Game.GetPlanets();
            foreach (SpaceObject spaceObject in Game.Random.Iterate(Tile.Game.GetSpaceObjects()))
                if (spaceObject != anomShip)
                {
                    Ship ship = spaceObject as Ship;
                    Planet planet = spaceObject as Planet;
                    if (ship != null)
                    {
                        AddPullChance(objects, ship, Math.Pow(ship.GetValue() / shipValue, .65));
                    }
                    else if (planet != null)
                    {
                        if (planets.All(p2 => planet == p2 || Tile.GetDistance(this.Tile, p2.Tile) > Consts.PlanetDistance))
                        {
                            double mult = Consts.AverageQuality + planet.PlanetValue;
                            if (planet.Colony != null)
                                mult += Consts.AverageQuality / 3.9 + planet.Quality / 1.69 + planet.Colony.Population / 1.3;
                            mult /= Consts.AverageQuality + Consts.PlanetConstValue;
                            if (planet.Colony != null)
                                mult = Math.Pow(mult, 1.3);
                            AddPullChance(objects, planet, 2.1 * mult);
                        }
                    }
                    else if (spaceObject is Anomaly)
                    {
                        int amt = Game.Random.OEInt(1 / (double)Tile.Game.GetSpaceObjects().Count());
                        if (amt > 0)
                            objects.Add(spaceObject, amt);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

            while (objects.Any())
            {
                SpaceObject pull = Game.Random.SelectValue(objects);
                objects.Remove(pull);

                Dictionary<SpaceObject, HashSet<SpaceObject>> before = GetAttInv(Tile.Game);
                Tile oldTile = pull.Tile;
                pull.Teleport(this.Tile);

                if (ValidateChange(before, anomShip))
                    return true;

                pull.Teleport(oldTile);

                while (Game.Random.Bool() && objects.Any())
                    objects.Remove(Game.Random.SelectValue(objects.Keys));
            }

            return false;
        }
        private void AddPullChance(Dictionary<SpaceObject, int> objects, SpaceObject spaceObj, double distMult)
        {
            if (!objects.Any())
                distMult += 1.3;
            double avg = Tile.GetDistance(this.Tile, spaceObj.Tile) * distMult * .39;
            avg = (Math.Sqrt(Tile.Game.MapSize) * 6.5 + 13) / (avg * avg + 13);
            if (avg > 1)
                avg = Math.Sqrt(avg);
            else
                avg *= avg;
            int amt = Game.Random.OEInt(avg / Math.Sqrt((double)Tile.Game.GetSpaceObjects().Count()));
            if (amt > 0)
                objects.Add(spaceObj, amt);
        }

        private bool CreateAnomalies(IEventHandler handler, Ship anomShip, int move)
        {
            bool retVal = false;
            int create = Game.Random.OEInt(1.69);
            Tile tile = this.Tile;
            for (int a = 0; a < create; ++a)
            {
                tile = MoveTile(tile, 5.2, anomShip, move);
                if (Tile.Game.CreateAnomaly(tile) != null)
                    retVal = true;
            }
            return retVal;
        }

        private Tile MoveTile(Tile tile, double avg, Ship anomShip, int move)
        {
            avg *= Math.Sqrt(.39 + Math.Sqrt(Tile.Game.MapSize) / 130.0 + (move + .5 + anomShip.MaxSpeed) / 16.9);
            return Tile.Game.GetRandomTile(tile, avg);
        }

        private bool CreateWormhole(IEventHandler handler, Ship anomShip, int move)
        {
            return Tile.Game.CreateWormhole(handler, this.Tile, GetRandomTile(anomShip, move), anomShip);
        }

        internal static bool ValidateChange(Dictionary<SpaceObject, HashSet<SpaceObject>> beforeAttInv, Ship anomShip)
        {
            Dictionary<SpaceObject, HashSet<SpaceObject>> afterAttInv = GetAttInv(anomShip.Tile.Game);
            foreach (SpaceObject spaceObject in anomShip.Tile.Game.GetSpaceObjects())
                if (spaceObject != anomShip)
                {
                    HashSet<SpaceObject> before = beforeAttInv[spaceObject];
                    HashSet<SpaceObject> after = afterAttInv[spaceObject];
                    if (spaceObject is Ship)
                    {
                        if (spaceObject.Player != anomShip.Player && after.Except(before).Any())
                            return false;
                    }
                    else
                    {
                        int count = after.Union(before).Count();
                        if (after.Count() != count || before.Count() != count)
                        {
                            if (!(spaceObject is Planet))
                                throw new Exception();
                            return false;
                        }
                    }
                }
            return true;
        }

        #endregion //Wormhole

        #region Experience

        private bool Experience(IEventHandler handler, Ship ship)
        {
            handler.Explore(AnomalyType.Experience, ship);

            bool funky = Game.Random.Bool();
            if (funky)
            {
                if (CanInvade(ship))
                    funky = false;
                else
                    ;
            }
            ship.AddAnomalyExperience(handler, this.value, funky, Game.Random.Bool());

            return true;
        }

        #endregion //Experience

        #region SalvageShip

        private bool SalvageShip(IEventHandler handler, Ship anomShip)
        {
            Player player = anomShip.Player;
            if (Game.Random.Bool())
                player = Game.Random.SelectValue(GetPlayerProximity());

            handler.Explore(AnomalyType.SalvageShip, player);

            double min = GenerateValue(), max = GenerateValue();
            if (min > max)
            {
                double temp = min;
                min = max;
                max = temp;
            }

            double designResearch = GetAvgDesignResearch(player, this.value);
            ShipDesign design = new ShipDesign(player, GetDesignResearch(designResearch), min, max);

            CompensateDesign(player, design, designResearch, 1);
            Ship newShip = player.NewShip(handler, this.Tile, design);
            player.GoldIncome(this.value - design.Cost);
            if (newShip.Player == anomShip.Player)
                newShip.LoseMove();

            if (anomShip.Player != player)
            {
                Ship s1 = newShip, s2 = anomShip;
                if (Game.Random.Bool())
                {
                    s1 = anomShip;
                    s2 = newShip;
                }

                double pct = AttackShip(handler, s1, s2);
                pct += AttackShip(handler, s2, s1);

                pct = 2 - pct;
                if (anomShip.Dead)
                    pct += 2;
                double gold = ConsolationValue();
                gold += (this.value - gold) * pct / 4.0;

                if (anomShip.Dead)
                    anomShip.Player.AddGold(gold, true);
                else
                    anomShip.Player.GoldIncome(gold);
            }

            return true;
        }

        private static double AttackShip(IEventHandler handler, Ship att, Ship def)
        {
            if (!att.Dead && !def.Dead && Game.Random.Bool() && (Game.Random.Bool() || handler.ConfirmCombat(att, def)))
                return att.AttackAnomalyShip(handler, def);
            return 1;
        }

        #endregion //SalvageShip

        #region Pickup

        private bool Pickup(IEventHandler handler, Ship ship)
        {
            int pop = Game.Random.Round(this.value * Consts.PopulationForGoldHigh);
            int hp = Game.Random.Round(this.value / ship.GetProdForHP(1));
            double soldiers = this.value / Consts.ProductionForSoldiers;

            Func<int, int, bool> DoChance = (add, max) =>
            {
                return (Game.Random.Range(Math.Sqrt(add), add) <= max);
            };
            bool canPop = (pop > 0 && ship.FreeSpace > 0 && (pop <= ship.FreeSpace || DoChance(pop, ship.FreeSpace)));
            bool canHeal = (hp > 0 && ship.HP < ship.MaxHP && (ship.HP + hp <= ship.MaxHP || DoChance(hp, ship.MaxHP - ship.HP)));
            bool canSoldiers = (ship.Population > 0 && soldiers / (double)ship.Population > .01);

            if ((canPop || canSoldiers) && CanInvade(ship))
            {
                if (canPop)
                    ;
                else
                    ;
                if (canSoldiers)
                    ;
                else
                    ;
                canPop = false;
                canSoldiers = false;
            }
            else
            {
                if (canPop)
                    ;
                else
                    ;
                if (canSoldiers)
                    ;
                else
                    ;
                if (CanInvade(ship))
                    ;
                else
                    ;
            }

            if (canSoldiers)
            {
                double soldierChance = ship.GetSoldierPct() / 1.69;
                soldierChance = 2.1 / (2.1 + soldiers / (double)ship.Population + soldierChance * soldierChance);
                if (canPop)
                    soldierChance /= 2.1;
                if (canHeal)
                    soldierChance /= 2.1;

                if (Game.Random.Bool(soldierChance))
                {
                    handler.Explore(AnomalyType.PickupSoldiers, soldiers);

                    ship.AddSoldiers(soldiers);
                    return true;
                }
                else
                    ;
            }

            if (canPop && canHeal)
            {
                canPop = Game.Random.Bool();
                canHeal = !canPop;
            }

            if (canPop)
            {
                pop = Math.Min(pop, ship.FreeSpace);

                handler.Explore(AnomalyType.PickupPopulation, pop);

                ship.AddPopulation(pop);
                ship.Player.GoldIncome(this.value - pop / Consts.PopulationForGoldHigh);
                ship.LoseMove();
                return true;
            }
            if (canHeal)
            {
                int show = Math.Min(hp, ship.MaxHP - ship.HP);
                handler.Explore(AnomalyType.Heal, show);

                double prod = this.value;
                double result = ship.ProductionRepair(ref prod, ship.GetProdForHP(hp), true);
                if (result != (int)result)
                    ;
                if (result != hp)
                    ;
                if (result != show)
                    ;
                ship.Player.GoldIncome(prod);
                return true;
            }

            return false;
        }

        private bool CanInvade(Ship ship)
        {
            foreach (Planet planet in Tile.Game.GetPlanets())
                if (planet.Colony != null && planet.Player != ship.Player && (Tile.GetDistance(planet.Tile, ship.Tile) - 1 <= ship.CurSpeed))
                    return true;
            return false;
        }

        #endregion //Population

        #region Valuables

        private bool Valuables(IEventHandler handler, Ship anomShip)
        {
            double value = Player.RoundGold(this.value, true);

            double div = 1.3;
            bool research, notify = true;
            switch (Game.Random.Next(13))
            {
                case 1:
                case 2:
                case 3:
                    div *= 1.69;
                    research = handler.Explore(AnomalyType.AskResearchOrGold, value);
                    notify = false;
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                    research = true;
                    break;
                default:
                    research = false;
                    break;
            }

            if (research)
            {
                value = this.value / div;
                double designResearch = GetAvgDesignResearch(anomShip.Player, value);
                ShipDesign design = anomShip.Player.FreeResearch(handler, Game.Random.Round(value), GetDesignResearch(designResearch));
                CompensateDesign(anomShip.Player, design, designResearch, .65 + 1.3 * value / design.GetTotCost());
            }
            else
            {
                if (notify)
                    handler.Explore(AnomalyType.Gold, value);
                anomShip.Player.AddGold(value);
            }

            return true;
        }

        #endregion //Valuables
    }
}
