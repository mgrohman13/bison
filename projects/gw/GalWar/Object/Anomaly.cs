using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class Anomaly : SpaceObject
    {
        #region Explore

        [NonSerialized]
        private double _value;
        [NonSerialized]
        private bool _usedValue;

        internal Anomaly(Tile tile)
            : base(tile)
        {
            checked
            {
                this._usedValue = false;
                this._value = double.NaN;
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

            Planet planet = Tile.Game.CreateAnomalyPlanet(handler, this.Tile);
            if (planet != null)
            {
                ship.Player.GoldIncome(ConsolationValue());
                return;
            }

            Dictionary<ExploreType, int> options = new Dictionary<ExploreType, int>();
            options.Add(GlobalEvent, 1);//med
            options.Add(LostColony, 2);//med
            options.Add(Death, 3);//always
            options.Add(Production, 6);//high
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
                    options.Remove(ExploreType);
                }
            }
        }
        private delegate bool ExploreType(IEventHandler handler, Ship ship);

        private double GenerateValue()
        {
            double quality = 0, pop = 0, armada = 0;
            foreach (Planet planet in Tile.Game.GetPlanets())
            {
                if (planet.Colony == null)
                {
                    quality += planet.Quality / 2.6;
                }
                else
                {
                    quality += planet.Quality;
                    pop += planet.Colony.Population;
                    armada += planet.Colony.PlanetDefenseCostAvgResearch / 1.69 + planet.Colony.production / 2.1;
                }
            }
            foreach (Player player in Tile.Game.GetPlayers())
            {
                foreach (Ship ship in player.GetShips())
                {
                    pop += ship.Population / 1.3;
                    armada += ship.GetCostAvgResearch();
                }
                armada += player.TotalGold / 3.0;
            }

            double value = ( armada / 52.0 + Consts.Income / 2.6 * ( 5 * pop + 2 * quality ) / 7.0 ) / (double)Tile.Game.GetPlayers().Count;
            return Game.Random.GaussianOE(value, .26, .3, GenerateConsolationValue(value));
        }
        private static double GenerateConsolationValue(double value)
        {
            double avg = ( Math.Pow(value + 1, .78) - 1 ) * .52;
            return Game.Random.GaussianCapped(avg, .21, avg > 1 ? 1 : 0);
        }

        private double ConsolationValue()
        {
            return GenerateConsolationValue(this.value);
        }

        private int GetDesignResearch(Player player)
        {
            double avg = ( 1 * ( ( 1 * player.ResearchGuess + 2 * player.ResearchDisplay + 4 * player.Research ) / 7.0 )
                    + 2 * ( player.LastResearched + this.value ) + 4 * ( Tile.Game.AvgResearch ) ) / 7.0;
            return Game.Random.GaussianOEInt(avg, .13, .013);
        }

        private Tile GetRandomTile(Ship anomShip)
        {
            if (Game.Random.Bool())
                foreach (SpaceObject spaceObject in Game.Random.Iterate(Tile.Game.GetSpaceObjects()))
                    if (spaceObject is Anomaly)
                        return MoveTile(spaceObject.Tile, 2.1, anomShip);
            return Tile.Game.GetRandomTile();
        }
        private bool AnomalousTile(Tile tile)
        {
            return ( tile.SpaceObject is Anomaly || tile.Teleporter != null );
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

                retVal.Add(player, Game.Random.Round(ushort.MaxValue / avgDist));
            }
            return retVal;
        }
        private Dictionary<Colony, int> GetPlayerColonyWeights(Player player, out double total)
        {
            total = 0;
            var colonies = new Dictionary<Colony, int>();
            foreach (Colony colony in player.GetColonies())
            {
                int weight = Game.Random.Round(GetColonyWeight(colony) * ushort.MaxValue / (double)Tile.GetDistance(this.Tile, colony.Tile));
                colonies.Add(colony, weight);
                total += weight;
            }
            return colonies;
        }

        private static double GetColonyWeight(Colony colony)
        {
            return ( colony.Planet.PlanetValue / 1.3 + colony.Population );
        }

        internal static HashSet<SpaceObject> GetAttInv(Tile target, bool inv)
        {
            return GetAttInv(target, inv, null);
        }

        private static void GetAttInvPlayers(Tile target, Ship anomShip, out Player oneInv, out bool twoInv, out Player oneAtt, out bool twoAtt)
        {
            GetAttInvPlayers(GetAttInv(target, true, anomShip), out oneInv, out twoInv);
            GetAttInvPlayers(GetAttInv(target, false, anomShip), out oneAtt, out twoAtt);
        }
        private static void GetAttInvPlayers(HashSet<SpaceObject> spaceObjs, out Player one, out bool two)
        {
            one = null;
            two = false;
            foreach (SpaceObject spaceObj in spaceObjs)
                if (one == null)
                {
                    one = spaceObj.Player;
                }
                else if (one != spaceObj.Player)
                {
                    one = null;
                    two = true;
                    return;
                }
        }

        private static HashSet<SpaceObject> GetAttInv(Tile target, bool inv, Ship anomShip)
        {
            HashSet<SpaceObject> retVal = new HashSet<SpaceObject>();

            //check for attacks from Planet Defenses
            if (!inv)
                foreach (Tile neighbors in Tile.GetNeighbors(target))
                {
                    Planet planet = neighbors.SpaceObject as Planet;
                    if (planet != null)
                    {
                        if (planet.Colony != null)
                            retVal.Add(planet);
                        break;
                    }
                }

            foreach (Player player in target.Game.GetPlayers())
                foreach (Ship attShip in player.GetShips())
                    if (attShip.Tile != target && CanAttack(attShip, attShip.Tile, target, inv, anomShip))
                        retVal.Add(attShip);

            return retVal;
        }
        private static bool CanAttack(Ship attShip, Tile attShipTile, Tile target, bool inv, Ship anomShip)
        {
            int diff = GetSpeed(attShip, anomShip) - Tile.GetDistance(target, attShipTile);
            if (inv)
            {
                if (diff > -2)
                    if (attShip.Population > 0 || attShip.DeathStar)
                    {
                        return true;
                    }
                    else if (attShip.FreeSpace > 0)
                    {
                        //check if the ship could conceivably pick up some population
                        foreach (Colony colony in attShip.Player.GetColonies())
                            if (colony.AvailablePop > 0 && GetSpeed(attShip, anomShip) > Tile.GetDistance(attShipTile, colony.Tile) - 2)
                                return true;
                        foreach (Ship friendlyShip in attShip.Player.GetShips())
                            if (friendlyShip.AvailablePop > 0 && GetSpeed(attShip, anomShip) + GetSpeed(friendlyShip, anomShip)
                                    > Tile.GetDistance(attShipTile, friendlyShip.Tile) - 2)
                                return true;
                    }
            }
            else if (diff > -1)
            {
                return true;
            }
            return false;
        }
        private static int GetSpeed(Ship speedShip, Ship anomShip)
        {
            return ( anomShip == speedShip ? 0 : speedShip.CurSpeed );
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
            SoldiersAndDefense,
            Wormhole,
        }

        #endregion //Explore

        #region LostColony

        private bool LostColony(IEventHandler handler, Ship anomShip)
        {
            if (!Tile.Game.CheckPlanetDistance(this.Tile))
                return false;

            Player oneInv, oneAtt;
            bool twoInv, twoAtt;
            GetAttInvPlayers(this.Tile, null, out oneInv, out twoInv, out oneAtt, out twoAtt);
            if (twoInv)
                return false;

            if (oneInv == null)
            {
                Dictionary<Player, int> playerProximity = GetPlayerProximity();

                if (Game.Random.Bool() && Colony(handler, anomShip.Player, playerProximity, anomShip))
                    return true;

                while (playerProximity.Count > 0)
                    if (Colony(handler, Game.Random.SelectValue(playerProximity), playerProximity, anomShip))
                        return true;

                return false;
            }
            else
            {
                return Colony(handler, oneInv, anomShip);
            }
        }
        private bool Colony(IEventHandler handler, Player player, Dictionary<Player, int> playerProximity, Ship anomShip)
        {
            if (Colony(handler, player, anomShip))
                return true;
            playerProximity.Remove(player);
            return false;
        }
        private bool Colony(IEventHandler handler, Player player, Ship anomShip)
        {
            int distance = int.MaxValue;
            foreach (Colony colony in player.GetColonies())
                distance = Math.Min(distance, Tile.GetDistance(this.Tile, colony.Tile));
            double cost = int.MaxValue;
            foreach (ShipDesign design in player.GetDesigns())
                if (design.Colony)
                    cost = Math.Min(cost, design.Upkeep * distance / (double)design.Speed
                            + design.Cost - design.GetColonizationValue(Tile.Game.MapSize, player.LastResearched));

            double amount = this.value - cost;
            double mult = Consts.GetColonizationMult() * Consts.AnomalyQualityCostMult;
            if (amount < Consts.GetColonizationCost(Consts.PlanetConstValue, mult))
                return false;

            handler.Explore(AnomalyType.LostColony, player);

            if (player != anomShip.Player)
                anomShip.Player.GoldIncome(ConsolationValue());

            Planet planet = Tile.Game.CreatePlanet(this.Tile);
            planet.ReduceQuality(planet.Quality -
                    MattUtil.TBSUtil.FindValue(value => ( amount > Consts.GetColonizationCost(Consts.PlanetConstValue + value, mult) ),
                    0, Consts.NewPlanetQuality(), false));
            int production = Game.Random.Round(amount - Consts.GetColonizationCost(Consts.PlanetConstValue + planet.Quality, mult));
            player.NewColony(player == anomShip.Player ? handler : null, planet, 0, 0, production);

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
                foreach (var pair in GetTerraformColonies(anomShip))
                    if (pair.Value.Item2 > 0 && CanTerraform(this.value + GetExpectCost(pair.Key, Consts.TerraformPlanetQuality()) * Consts.GetColonizationMult(), anomShip))
                    {
                        double terraformAmt = Consts.TerraformQualityMult * Consts.AverageQuality;
                        double apocalypseAmt = quality / 2.0;

                        if (Game.Random.Bool(terraformAmt / ( terraformAmt + apocalypseAmt )))
                            return DamageVictory(handler, anomShip);
                        return Terraform(handler, anomShip);
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

                double addAmt = forExplorer + ( forExplorer * ( 2 *
                        ( ( 2 * quality + 1 * pop ) / 3.0 / Consts.AverageQuality )
                        + 4 * colonies + 1 * planets.Count ) / 7.0 );
                double killAmt = diePct * pop;

                if (Game.Random.Bool(addAmt / ( addAmt + killAmt )))
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
                    gold = Math.Sqrt(( planet.Colony.Population + Consts.PlanetConstValue ) / ( planet.Quality + Consts.PlanetConstValue )) * Consts.AnomalyQualityCostMult;

                    mult = Consts.GetColonizationMult();
                    before = Consts.GetColonizationCost(planet.Quality, mult);
                }

                planet.DamageVictory();

                if (planet.Colony != null)
                {
                    gold *= before - Consts.GetColonizationCost(planet.Quality, mult);

                    double amt;
                    addGold.TryGetValue(planet.Colony.Player, out amt);
                    addGold[planet.Colony.Player] = amt + gold;
                }
            }

            double min = double.MaxValue;
            foreach (double gold in addGold.Values)
                min = Math.Min(min, gold);
            foreach (var pair in addGold)
                pair.Key.AddGold(pair.Value - min, true);

            return true;
        }

        private bool Terraform(IEventHandler handler, Ship anomShip)
        {
            double[] colonyChances;
            Dictionary<Colony, int> colonies = GetTerraformColonies(anomShip, out colonyChances);
            if (colonies.Count == 0)
                return false;

            while (colonies.Count > 0)
            {
                Colony trgColony = Game.Random.SelectValue(colonies);

                int addQuality = Consts.TerraformPlanetQuality();
                double expectCost = GetExpectCost(trgColony, addQuality);
                double actualCost = Player.RoundGold(this.value + expectCost * Consts.GetColonizationMult(), true);

                bool canTerraform = CanTerraform(actualCost, anomShip);
                canTerraform &= handler.Explore(AnomalyType.AskTerraform, trgColony, addQuality, actualCost, expectCost, colonyChances, canTerraform);
                if (canTerraform)
                {
                    trgColony.Planet.ReduceQuality(-addQuality);
                    trgColony.Player.AddGold(actualCost);
                    return true;
                }
                else
                {
                    colonies.Remove(trgColony);
                    foreach (var pair in colonies.ToArray())
                    {
                        int amt = GetTerraformChance(.52 * pair.Value);
                        if (amt > 0)
                            colonies[pair.Key] = amt;
                        else
                            colonies.Remove(pair.Key);
                    }
                }
            }

            return true;
        }
        private Dictionary<Colony, int> GetTerraformColonies(Ship anomShip, out double[] colonyChances)
        {
            Dictionary<Colony, Tuple<double, int>> terraformColonies = GetTerraformColonies(anomShip);

            colonyChances = new double[terraformColonies.Count];
            Dictionary<Colony, int> retVal = new Dictionary<Colony, int>(terraformColonies.Count);

            int idx = -1;
            foreach (var pair in terraformColonies)
            {
                colonyChances[++idx] = pair.Value.Item1;
                if (pair.Value.Item2 > 0)
                    retVal.Add(pair.Key, pair.Value.Item2);
            }

            return retVal;
        }
        private Dictionary<Colony, Tuple<double, int>> GetTerraformColonies(Ship anomShip)
        {
            var colonies = new Dictionary<Colony, Tuple<double, int>>();

            foreach (Colony colony in anomShip.Player.GetColonies())
            {
                double raw = .169 * Math.Sqrt(Tile.Game.MapSize) / (double)Tile.GetDistance(colony.Tile, this.Tile);
                colonies.Add(colony, new Tuple<double, int>(raw, GetTerraformChance(raw)));
            }

            return colonies;
        }
        private static double GetExpectCost(Colony trgColony, double addQuality)
        {
            double before = Consts.GetColonizationCost(trgColony.Planet.Quality, Consts.AnomalyQualityCostMult);
            double after = Consts.GetColonizationCost(trgColony.Planet.Quality + addQuality, Consts.AnomalyQualityCostMult);
            return before - after;
        }
        private static bool CanTerraform(double cost, Ship anomShip)
        {
            return ( -cost < anomShip.Player.Gold );
        }
        private static int GetTerraformChance(double amt)
        {
            return Game.Random.GaussianOEInt(amt, .26, .13);
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
                double amt = ( addAmt - forExplorer ) / (double)Tile.Game.GetPlayers().Count;
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
            int lowerCap = 0;
            if (amt > GetLowerCap(amt, upperCap))
            {
                do
                {
                    lowerCap = GetLowerCap(amt, Game.Random.DoubleHalf(upperCap));
                } while (amt < lowerCap);
            }
            if (lowerCap < 1)
                lowerCap = ( ( amt > 1 ) ? 1 : 0 );
            return Game.Random.GaussianCappedInt(amt, .039, lowerCap);
        }
        private static int GetLowerCap(double average, double upperCap)
        {
            return Game.Random.Round(2 * average - upperCap);
        }

        #endregion //GlobalEvent

        #region Death

        private bool Death(IEventHandler handler, Ship ship)
        {
            int damage = ( ( ship.MaxHP < 2 || Game.Random.Bool() ) ? ( ship.MaxHP ) : ( 1 + Game.Random.WeightedInt(ship.MaxHP - 2, .26) ) );

            handler.Explore(AnomalyType.Death, -damage);

            if (damage < ship.HP)
            {
                double rawExp = 0, valueExp = 0;

                ship.Damage(damage, ref rawExp, ref valueExp);

                ship.Player.GoldIncome(ConsolationValue() + ship.GetDisbandValue(damage) - ship.GetValueExpForRawExp(rawExp) - valueExp);

                ship.AddExperience(rawExp, valueExp);
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
            Dictionary<Colony, int> dict = GetPlayerColonyWeights(anomShip.Player, out total);
            IEnumerable<KeyValuePair<Colony, int>> colonies = dict;

            Colony single = null;
            if (dict.Count == 1 || Game.Random.Bool())
                single = Game.Random.SelectValue(dict);

            double value = this.value;
            int production = Game.Random.Round(value / 1.3);

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
                type = AnomalyType.Production;
                if (single != null && !( single.Buildable is PlanetDefense ))
                {
                    type = AnomalyType.SoldiersAndDefense;
                    if (single.Buildable != null)
                    {
                        type = handler.Explore(AnomalyType.AskProductionOrDefense, single, production) ? AnomalyType.Production : AnomalyType.SoldiersAndDefense;
                        notify = false;
                    }
                }
                break;
            case 9:
            case 10:
            case 11:
            case 12:
                type = AnomalyType.SoldiersAndDefense;
                break;
            default:
                throw new Exception();
            }

            if (type == AnomalyType.Production)
            {
                if (single == null)
                {
                    colonies = colonies.Where(pair => ( pair.Key.Buildable != null ));
                    if (colonies.Count() == 0)
                        return false;
                }
                else
                {
                    if (single.Buildable == null)
                        return false;
                }

                value = production;
            }

            if (single == null)
            {
                handler.Explore(type);
                value /= total;
                foreach (var pair in colonies)
                    PlanetDefenseRemoteSoldiersProduction(type, pair.Key, value * pair.Value);
            }
            else
            {
                if (notify)
                    handler.Explore(type, single, value);
                PlanetDefenseRemoteSoldiersProduction(type, single, value);
            }

            return true;
        }

        private void PlanetDefenseRemoteSoldiersProduction(AnomalyType type, Colony colony, double value)
        {
            switch (type)
            {
            case AnomalyType.SoldiersAndDefense:
                colony.BuildPlanetDefense(value, true);
                break;
            case AnomalyType.Soldiers:
                colony.BuildSoldiers(value);
                break;
            case AnomalyType.PlanetDefenses:
                colony.BuildAttAndDef(value);
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

            if (Game.Random.Bool())
                any |= PullIn(handler, !any, anomShip);
            if (Game.Random.Bool())
                any |= PushShip(handler, !any, anomShip);
            if (Game.Random.Bool())
                any |= CreateAnomalies(handler, !any, anomShip);

            if (any)
                anomShip.LoseMove();
            else
                any = CreateTeleporter(handler, anomShip);

            if (any)
                anomShip.Player.GoldIncome(ConsolationValue());

            return any;
        }

        private bool PushShip(IEventHandler handler, bool notify, Ship anomShip)
        {
            Tile tile = GetRandomTile(anomShip);
            while (tile.SpaceObject is Anomaly)
                tile = MoveTile(tile, .91, anomShip);
            if (tile.SpaceObject == null)
            {
                Player oneInv, oneAtt;
                bool twoInv, twoAtt;
                GetAttInvPlayers(tile, anomShip, out oneInv, out twoInv, out oneAtt, out twoAtt);
                if (!twoAtt && ( oneAtt == null || oneAtt == anomShip.Player ))
                {
                    if (notify)
                        handler.Explore(Anomaly.AnomalyType.Wormhole);

                    anomShip.Teleport(tile);
                    return true;
                }
            }
            return false;
        }

        private bool PullIn(IEventHandler handler, bool notify, Ship anomShip)
        {
            Player oneInv, oneAtt;
            bool twoInv, twoAtt;
            GetAttInvPlayers(this.Tile, anomShip, out oneInv, out twoInv, out oneAtt, out twoAtt);

            Dictionary<SpaceObject, int> objects = new Dictionary<SpaceObject, int>();

            double planetMult = ( twoInv || twoAtt ? 1.69 : ( ( oneInv != null ) || ( oneAtt != null ) ? 1.3 : 1.04 ) );
            HashSet<Planet> planets = Tile.Game.GetPlanets();
            foreach (SpaceObject spaceObject in Game.Random.Iterate(Tile.Game.GetSpaceObjects()))
                if (spaceObject != anomShip)
                {
                    Ship ship = spaceObject as Ship;
                    Planet planet = spaceObject as Planet;
                    if (ship != null)
                    {
                        if (!twoAtt || ship.Player == anomShip.Player)
                            AddPullChance(objects, ship.Player == anomShip.Player ? null : oneAtt, ship, .39, anomShip);
                    }
                    else if (planet != null)
                    {
                        foreach (Planet p2 in planets)
                            if (planet != p2 && Tile.GetDistance(this.Tile, p2.Tile) <= Consts.PlanetDistance)
                                goto next;

                        bool colony = ( planet.Colony != null );
                        if (!twoInv || !colony)
                            AddPullChance(objects, oneInv, planet, ( colony ? 1.3 : .91 ) * planetMult *
                                    Math.Sqrt(( Consts.AverageQuality + planet.Quality ) / Consts.AverageQuality), anomShip);
                    }
                    else if (spaceObject is Anomaly)
                    {
                        AddPullChance(objects, null, spaceObject, .78, null);
                    }
                    else
                    {
                        throw new Exception();
                    }
next:
                    ;
                }

            if (objects.Count > 0)
            {
                if (notify)
                    handler.Explore(Anomaly.AnomalyType.Wormhole);

                SpaceObject teleport = Game.Random.SelectValue(objects);
                Ship ship = teleport as Ship;
                if (ship != null)
                {
                    ship.Teleport(this.Tile);
                }
                else if (teleport is Anomaly)
                {
                    teleport.Tile.SpaceObject = null;
                    if (Tile.Game.CreateAnomaly(this.Tile) == null)
                        throw new Exception();
                }
                else
                {
                    ( (Planet)teleport ).Teleport(this.Tile);
                }
                return true;
            }

            return false;
        }
        private void AddPullChance(Dictionary<SpaceObject, int> objects, Player can, SpaceObject spaceObj, double distMult, Ship anomShip)
        {
            if (spaceObj != anomShip && ( can == null || spaceObj.Player == null || spaceObj.Player == can ))
            {
                if (objects.Count == 0)
                    distMult *= 2.6;
                double avg = Tile.GetDistance(this.Tile, spaceObj.Tile) * distMult;
                avg = ( Tile.Game.MapSize / 6.5 + 1.3 ) / ( avg * avg + 9.1 );
                if (avg > 1)
                    avg = Math.Sqrt(avg);
                else
                    avg *= avg;
                int amt = Game.Random.OEInt(avg / (double)Tile.Game.GetSpaceObjects().Count);
                if (amt > 0 && !( spaceObj is Ship && CanAttackAny((Ship)spaceObj, this.Tile, anomShip) ))
                    objects.Add(spaceObj, amt);
            }
        }
        private static bool CanAttackAny(Ship attShip, Tile attShipTile, Ship anomShip)
        {
            foreach (Player player in attShipTile.Game.GetPlayers())
                if (player != attShip.Player && player != anomShip.Player)
                {
                    foreach (Colony colony in player.GetColonies())
                        if (CanAttack(attShip, attShipTile, colony.Tile, true, anomShip))
                            return true;
                    foreach (Ship trgShip in player.GetShips())
                        if (CanAttack(attShip, attShipTile, trgShip.Tile, false, anomShip))
                            return true;
                }
            return false;
        }

        private bool CreateAnomalies(IEventHandler handler, bool notify, Ship anomShip)
        {
            bool retVal = false;
            int create = Game.Random.OEInt(1.69);
            for (int a = 0 ; a < create ; ++a)
            {
                Tile tile = MoveTile(this.Tile, 5.2, anomShip);
                if (Tile.Game.CreateAnomaly(tile) != null)
                {
                    if (notify)
                        handler.Explore(Anomaly.AnomalyType.Wormhole);
                    notify = false;

                    retVal = true;
                }
            }
            return retVal;
        }

        private Tile MoveTile(Tile tile, double avg, Ship anomShip)
        {
            avg *= Math.Sqrt(.39 + Math.Sqrt(Tile.Game.MapSize) / 130.0 + ( anomShip.CurSpeed + anomShip.MaxSpeed ) / 16.9);
            return Tile.Game.GetRandomTile(tile, avg);
        }

        private bool CreateTeleporter(IEventHandler handler, Ship anomShip)
        {
            return Tile.Game.CreateTeleporter(handler, this.Tile, GetRandomTile(anomShip));
        }

        #endregion //Wormhole

        #region Experience

        private bool Experience(IEventHandler handler, Ship ship)
        {
            handler.Explore(AnomalyType.Experience, ship);

            ship.AddAnomalyExperience(handler, this.value, Game.Random.Bool(), Game.Random.Bool());

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

            double min = this.value, max = GenerateValue();
            if (min > max)
            {
                double temp = min;
                min = max;
                max = temp;
            }

            ShipDesign design = new ShipDesign(player, GetDesignResearch(player), min, max);
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
                gold += ( this.value - gold ) * pct / 4.0;

                if (anomShip.Dead)
                    anomShip.Player.AddGold(gold, true);
                else
                    anomShip.Player.GoldIncome(gold);
            }

            return true;
        }

        private static double AttackShip(IEventHandler handler, Ship att, Ship def)
        {
            if (!att.Dead && !def.Dead && Game.Random.Bool() && ( Game.Random.Bool() || handler.ConfirmCombat(att, def) ))
                return att.AttackAnomalyShip(handler, def);
            return 1;
        }

        #endregion //SalvageShip

        #region Pickup

        private bool Pickup(IEventHandler handler, Ship ship)
        {
            int pop = Game.Random.Round(this.value * Consts.PopulationForGoldHigh);
            double soldiers = this.value / Consts.ExpForSoldiers;

            bool canPop = ( pop > 0 && pop <= ship.FreeSpace );
            bool canHeal = ( ship.HP < ship.MaxHP && ship.GetProdForHP(1) < this.value
                    && ship.GetProdForHP(ship.MaxHP - ship.HP + 1) > this.value );
            if (ship.Population > 0 && soldiers / ship.Population > .01)
            {
                double soldierChance = ship.GetSoldierPct() / 1.69;
                soldierChance = 2.1 / ( 2.1 + soldiers / (double)ship.Population + soldierChance * soldierChance );
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
            }

            if (canPop && canHeal)
            {
                canPop = Game.Random.Bool();
                canHeal = !canPop;
            }

            if (canPop)
            {
                handler.Explore(AnomalyType.PickupPopulation, pop);

                ship.AddPopulation(pop);
                return true;
            }
            else if (canHeal)
            {
                int hp = Game.Random.Round(ship.GetHPForProd(this.value, false));
                handler.Explore(AnomalyType.Heal, hp);

                double prod = ship.GetProdForHP(hp), gold = this.value - prod;
                if (ship.ProductionRepair(ref prod, ref gold, true, false) != hp)
                    throw new Exception();
                ship.Player.GoldIncome(prod + gold);
                return true;
            }

            return false;
        }

        #endregion //Population

        #region Valuables

        private bool Valuables(IEventHandler handler, Ship anomShip)
        {
            double value = Player.RoundGold(this.value, true);

            bool research, notify = true;
            switch (Game.Random.Next(13))
            {
            case 1:
            case 2:
            case 3:
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
                anomShip.Player.FreeResearch(handler, Game.Random.Round(this.value / 1.3), GetDesignResearch(anomShip.Player));
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
