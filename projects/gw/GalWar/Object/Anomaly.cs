using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class Anomaly : ISpaceObject
    {
        #region Explore

        private readonly Tile _tile;

        [NonSerialized]
        private double _value;
        [NonSerialized]
        private bool _usedValue;

        internal Anomaly(Tile tile)
        {
            checked
            {
                this._tile = tile;
                tile.SpaceObject = this;

                this._usedValue = false;
                this._value = double.NaN;
            }
        }

        public Tile Tile
        {
            get
            {
                return this._tile;
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

        public Player Player
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
            options.Add(ExploreType.GlobalEvent, 2);
            options.Add(ExploreType.Death, 3);
            options.Add(ExploreType.LostColony, 4);
            options.Add(ExploreType.Production, 5);
            options.Add(ExploreType.SalvageShip, 9);
            options.Add(ExploreType.Wormhole, 10);
            options.Add(ExploreType.Experience, 11);
            options.Add(ExploreType.Population, 12);
            options.Add(ExploreType.Valuables, 13);

            while (true)
            {
                this.usedValue = false;

                ExploreType type = Game.Random.SelectValue(options);
                if (Explore(handler, type, ship))
                {
                    if (!this.usedValue)
                        throw new Exception();

                    return;
                }
                else
                {
                    options.Remove(type);
                }
            }
        }
        private bool Explore(IEventHandler handler, ExploreType type, Ship ship)
        {
            switch (type)
            {
            case ExploreType.LostColony:
                return LostColony(handler, ship);
            case ExploreType.GlobalEvent:
                return GlobalEvent(handler, ship);
            case ExploreType.Death:
                return Death(handler, ship);
            case ExploreType.Production:
                return Production(handler, ship);
            case ExploreType.Wormhole:
                return Wormhole(handler, ship);
            case ExploreType.Experience:
                return Experience(handler, ship);
            case ExploreType.SalvageShip:
                return SalvageShip(handler, ship);
            case ExploreType.Population:
                return Population(handler, ship);
            case ExploreType.Valuables:
                return Valuables(handler, ship);
            default:
                throw new Exception();
            }
        }

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

            double value = 1 + ( armada / 26.0 + Consts.Income * ( 5 * pop + 2 * quality ) / 7.0 ) / (double)Tile.Game.GetPlayers().Count;
            return Game.Random.GaussianOE(value, .3, .3, GenerateConsolationValue(value));
        }
        private static double GenerateConsolationValue(double value)
        {
            double avg = Math.Pow(value, .65);
            return Game.Random.GaussianCapped(avg, .21, Math.Max(1, 2 * avg - value));
        }

        private double ConsolationValue()
        {
            return GenerateConsolationValue(this.value);
        }

        private int GetDesignResearch(Player player)
        {
            double avg = ( 1 * ( ( 1 * player.ResearchDisplay + 2 * player.Research ) / 3.0 )
                    + 2 * ( player.LastResearched + this.value ) + 4 * ( Tile.Game.AvgResearch ) ) / 7.0;
            return Game.Random.GaussianOEInt(avg, .13, .013);
        }

        private Tile GetRandomTile()
        {
            Tile tile = Tile.Game.GetRandomTile();
            while (!( tile.SpaceObject is Anomaly ) && Game.Random.Bool(1 - 1 / ( 13.0 + Math.Sqrt(Tile.Game.MapSize) / 16.9 )))
            {
                foreach (Tile neighbor in Tile.GetNeighbors(tile))
                    if (neighbor.SpaceObject is Anomaly)
                        return tile;
                tile = Tile.Game.GetRandomTile();
            }
            return tile;
        }

        private static Dictionary<Player, int> GetPlayerProximity(Tile tile)
        {
            Dictionary<Player, int> retVal = new Dictionary<Player, int>();
            foreach (Player player in tile.Game.GetPlayers())
            {
                double distance = 0, tot = 0;
                foreach (Colony colony in player.GetColonies())
                {
                    double weight = colony.Planet.Quality + colony.Population + 1;
                    distance += weight * Tile.GetDistance(tile, colony.Tile);
                    tot += weight;
                }
                distance /= tot;
                retVal.Add(player, Game.Random.Round(tile.Game.Diameter / distance));
            }
            return retVal;
        }

        private Dictionary<Colony, int> GetPlayerColonyWeights(Player player, out double total)
        {
            total = 0;
            var colonies = new Dictionary<Colony, int>();
            foreach (Colony colony in player.GetColonies())
            {
                int weight = Game.Random.Round(( colony.Planet.Quality + colony.Population + 1 )
                        * Tile.Game.Diameter / (double)Tile.GetDistance(this.Tile, colony.Tile));
                colonies.Add(colony, weight);
                total += weight;
            }
            return colonies;
        }

        internal static HashSet<ISpaceObject> GetAttInv(Tile target, bool inv)
        {
            return GetAttInv(target, null, inv);
        }

        private static void GetAttInvPlayers(Tile target, Ship anomShip, out Player oneInv, out bool twoInv, out Player oneAtt, out bool twoAtt)
        {
            GetAttInvPlayers(GetAttInv(target, anomShip, true), out oneInv, out twoInv);
            GetAttInvPlayers(GetAttInv(target, anomShip, false), out oneAtt, out twoAtt);
        }
        private static void GetAttInvPlayers(HashSet<ISpaceObject> spaceObjs, out Player one, out bool two)
        {
            one = null;
            two = false;
            foreach (ISpaceObject spaceObj in spaceObjs)
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

        private static HashSet<ISpaceObject> GetAttInv(Tile target, Ship anomShip, bool inv)
        {
            HashSet<ISpaceObject> retVal = new HashSet<ISpaceObject>();

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
                    if (attShip.Tile != target && CanAttack(attShip, attShip.Tile, target, anomShip, inv))
                        retVal.Add(attShip);

            return retVal;
        }
        private static bool CanAttack(Ship attShip, Tile attShipTile, Tile target, Ship anomShip, bool inv)
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

        private enum ExploreType
        {
            Death,
            Experience,
            GlobalEvent,
            LostColony,
            Population,
            Production,
            SalvageShip,
            Valuables,
            Wormhole,
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
                Dictionary<Player, int> playerProximity = GetPlayerProximity(this.Tile);

                if (Game.Random.Bool() && Colony(handler, anomShip, anomShip.Player, playerProximity))
                    return true;

                while (playerProximity.Count > 0)
                    if (Colony(handler, anomShip, Game.Random.SelectValue(playerProximity), playerProximity))
                        return true;

                return false;
            }
            else
            {
                return Colony(handler, anomShip, oneInv);
            }
        }
        private bool Colony(IEventHandler handler, Ship anomShip, Player player, Dictionary<Player, int> playerProximity)
        {
            if (Colony(handler, anomShip, player))
                return true;
            playerProximity.Remove(player);
            return false;
        }
        private bool Colony(IEventHandler handler, Ship anomShip, Player player)
        {
            int distance = int.MaxValue;
            foreach (Colony colony in player.GetColonies())
                distance = Math.Min(distance, Tile.GetDistance(this.Tile, colony.Tile));
            double cost = int.MaxValue;
            foreach (ShipDesign design in player.GetShipDesigns())
                if (design.Colony)
                    cost = Math.Min(cost, design.Upkeep * distance / (double)design.Speed
                            + design.Cost - design.GetColonizationValue(Tile.Game.MapSize, player.LastResearched));

            double amount = this.value - cost;
            double mult = Consts.GetColonizationMult() * 1.3;
            if (amount < Consts.GetColonizationCost(Consts.PlanetConstValue, mult))
                return false;

            handler.Explore(AnomalyType.LostColony, player);

            if (player != anomShip.Player)
                anomShip.Player.GoldIncome(ConsolationValue());

            Planet planet = Tile.Game.CreatePlanet(this.Tile);
            planet.ReduceQuality(planet.Quality - MattUtil.TBSUtil.FindValue(delegate(int value)
            {
                return ( amount > Consts.GetColonizationCost(Consts.PlanetConstValue + value, mult) );
            }, 0, Consts.NewPlanetQuality(), false));
            int production = Game.Random.Round(amount - Consts.GetColonizationCost(Consts.PlanetConstValue + planet.Quality, mult));
            Colony newColony = player.NewColony(handler, planet, 0, 0, production);

            return true;
        }

        #endregion //LostColony

        #region GlobalEvent

        private bool GlobalEvent(IEventHandler handler, Ship anomShip)
        {
            double quality = 0, pop = 0, colonies = 0;
            foreach (Planet planet in Tile.Game.GetPlanets())
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
                double terraformAmt = Consts.AverageQuality + Consts.PlanetConstValue;
                double apocalypseAmt = quality / 2.0;

                if (Game.Random.Bool(terraformAmt / ( terraformAmt + apocalypseAmt )))
                    return DamageVictory(handler, anomShip);
                else
                    return Terraform(handler, anomShip);
            }
            else
            {
                foreach (Player player in Tile.Game.GetPlayers())
                    foreach (Ship ship in player.GetShips())
                        pop += ship.Population;
                double forExplorer = this.value * Consts.PopulationForGoldHigh;
                double diePct = Game.Random.GaussianCapped(.3, 0.169, .13);

                double addAmt = forExplorer + ( forExplorer * ( 2 *
                        ( ( 2 * quality + 1 * pop ) / 3.0 / Consts.AverageQuality )
                        + 4 * colonies + 1 * Tile.Game.GetPlanets().Count ) / 7.0 );
                double killAmt = diePct * pop;

                if (Game.Random.Bool(addAmt / ( addAmt + killAmt )))
                    return KillPop(handler, anomShip, diePct);
                else
                    return AddPop(handler, anomShip, addAmt, forExplorer);
            }
        }

        private bool DamageVictory(IEventHandler handler, Ship anomShip)
        {
            handler.Explore(AnomalyType.Apocalypse);

            Dictionary<Player, double> addGold = new Dictionary<Player, double>();
            addGold[anomShip.Player] = ConsolationValue();
            foreach (Planet planet in Tile.Game.GetPlanets())
            {
                double gold = 0;
                if (planet.Colony != null)
                    gold = Math.Sqrt(( planet.Colony.Population + 1.0 ) / ( planet.Quality + 1.0 )) * .65;

                double before = Consts.GetColonizationCost(planet.Quality, Consts.GetColonizationMult());
                planet.DamageVictory();
                gold *= before - Consts.GetColonizationCost(planet.Quality, Consts.GetColonizationMult());

                if (planet.Colony != null)
                {
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
            Dictionary<Colony, int> colonies = new Dictionary<Colony, int>();
            double[] colonyChances = new double[anomShip.Player.GetColonies().Count];
            int idx = -1;
            foreach (Colony colony in anomShip.Player.GetColonies())
            {
                colonyChances[++idx] = .26 * Tile.Game.Diameter / (double)Tile.GetDistance(colony.Tile, this.Tile);
                int amt = GetTerraformAmt(colonyChances[idx]);
                if (amt > 0)
                    colonies.Add(colony, amt);
            }
            if (colonies.Count == 0)
                return false;

            while (colonies.Count > 0)
            {
                Colony trgColony = Game.Random.SelectValue(colonies);

                const double costMult = 1.69;
                int addQuality = Consts.NewPlanetQuality() + Game.Random.GaussianOEInt(Consts.PlanetConstValue, .65, .39, 1);
                double before = Consts.GetColonizationCost(trgColony.Planet.Quality, costMult);
                double after = Consts.GetColonizationCost(trgColony.Planet.Quality + addQuality, costMult);
                double expectCost = before - after;
                double actualCost = Player.RoundGold(this.value + Consts.GetColonizationMult() * before - Consts.GetColonizationMult() * after, true);

                if (handler.Explore(AnomalyType.AskTerraform, trgColony, addQuality, actualCost, expectCost, colonyChances))
                {
                    trgColony.Planet.ReduceQuality(-addQuality);
                    trgColony.Player.AddGold(actualCost);
                    return true;
                }
                else
                {
                    colonies.Remove(trgColony);
                    foreach (Colony choice in colonies.Keys.ToArray())
                    {
                        int amt = GetTerraformAmt(.52 * colonies[choice]);
                        if (amt > 0)
                            colonies[choice] = amt;
                        else
                            colonies.Remove(choice);
                    }
                }
            }

            return true;
        }
        private static int GetTerraformAmt(double amt)
        {
            return Game.Random.GaussianOEInt(amt, .26, .13);
        }

        private bool KillPop(IEventHandler handler, Ship anomShip, double diePct)
        {
            handler.Explore(AnomalyType.Apocalypse);

            anomShip.Player.AddGold(ConsolationValue(), true);

            foreach (Player player in Tile.Game.GetPlayers())
            {
                foreach (Colony colony in player.GetColonies())
                    colony.LosePopulation(GetPopChange(colony.Population * diePct, colony.Population * .52), true);
                foreach (Ship ship in player.GetShips())
                    ship.LosePopulation(GetPopChange(ship.Population * diePct, ship.Population * .52), true);
            }

            return true;
        }

        private bool AddPop(IEventHandler handler, Ship anomShip, double addAmt, double forExplorer)
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
                    pair.Key.LosePopulation(-GetPopChange(amt * pair.Value,
                            Game.Random.Bool() ? .91 * pair.Key.Population : 1.3 * amt * pair.Value));
            }

            return true;
        }

        private static int GetPopChange(double amt, double upperCap)
        {
            int cap = Math.Max(1, Game.Random.Round(2 * amt - upperCap));
            if (cap > amt)
                cap = (int)amt;
            return Game.Random.GaussianCappedInt(amt, .091, cap);
        }

        #endregion //GlobalEvent

        #region Death

        private bool Death(IEventHandler handler, Ship ship)
        {
            int damage = ( ( ship.MaxHP < 2 || Game.Random.Bool() ) ? ( ship.MaxHP ) : ( 1 + Game.Random.WeightedInt(ship.MaxHP - 2, .26) ) );

            handler.Explore(AnomalyType.Death, damage);

            if (damage < ship.HP)
            {
                ship.Player.GoldIncome(ConsolationValue() + ship.GetDisbandValue(damage));

                int pop = ship.Population;
                ship.AddExperience(ship.Damage(damage));
                if (pop > 0)
                    ship.AddCostExperience(( pop - ship.Population ) * Consts.TroopExperienceMult);
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
            double production = value / 1.3;

            bool notify = true;
            AnomalyType type;
            switch (Game.Random.Next(13))
            {
            case 0:
            case 1:
                type = AnomalyType.Soldiers;
                break;
            case 2:
            case 3:
                type = AnomalyType.PlanetDefenses;
                break;
            case 4:
            case 5:
                type = AnomalyType.Production;
                break;
            case 6:
            case 7:
            case 8:
                type = AnomalyType.Production;
                if (single != null)
                {
                    type = handler.Explore(AnomalyType.AskProductionOrDefense, single, production) ? AnomalyType.Production : AnomalyType.SoldiersAndDefense;
                    notify = false;
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
                value = production;

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
                any |= PushShip(handler, !any, anomShip);
            if (Game.Random.Bool())
                any |= PullIn(handler, !any, anomShip);
            if (Game.Random.Bool())
                any |= CreateAnomalies(handler, !any, anomShip);

            if (any)
                anomShip.LoseMove();
            else
                any = CreateTeleporter(handler);

            if (any)
                anomShip.Player.GoldIncome(ConsolationValue());

            return any;
        }

        private bool PushShip(IEventHandler handler, bool notify, Ship ship)
        {
            Tile tile = GetRandomTile();
            while (tile.SpaceObject is Anomaly)
                tile = MoveTile(tile);
            if (tile.SpaceObject == null)
            {
                Player oneInv, oneAtt;
                bool twoInv, twoAtt;
                GetAttInvPlayers(tile, ship, out oneInv, out twoInv, out oneAtt, out twoAtt);
                if (!twoAtt && ( oneAtt == null || oneAtt == ship.Player ))
                {
                    if (notify)
                        handler.Explore(Anomaly.AnomalyType.Wormhole);

                    ship.Teleport(tile);
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

            Dictionary<ISpaceObject, int> objects = new Dictionary<ISpaceObject, int>();
            if (Tile.Game.CheckPlanetDistance(this.Tile))
            {
                double mult = ( twoAtt ? 1.69 : ( ( twoInv || ( oneInv != null ) || ( oneAtt != null ) ) ? 1.3 : 1.0 ) );
                foreach (Planet planet in Tile.Game.GetPlanets())
                {
                    bool colony = ( planet.Colony != null );
                    if (!twoInv || !colony)
                        AddPullChance(objects, oneInv, planet, ( colony ? 1.3 : 1 ) * mult, anomShip);
                }
            }

            foreach (Player player in Game.Random.Iterate(Tile.Game.GetPlayers()))
                if (!twoAtt || player == anomShip.Player)
                    foreach (Ship ship in Game.Random.Iterate(player.GetShips()))
                        AddPullChance(objects, player == anomShip.Player ? null : oneAtt, ship, ( objects.Count > 0 ) ? .21 : .52, anomShip);

            if (objects.Count > 0)
            {
                foreach (Tile tile in Tile.Game.GetMap())
                    if (tile.SpaceObject is Anomaly)
                        AddPullChance(objects, null, tile.SpaceObject, .91, null);

                if (notify)
                    handler.Explore(Anomaly.AnomalyType.Wormhole);

                ISpaceObject teleport = Game.Random.SelectValue(objects);
                Ship ship = teleport as Ship;
                if (ship != null)
                {
                    ship.Teleport(this.Tile);
                }
                else if (teleport is Anomaly)
                {
                    teleport.Tile.SpaceObject = null;
                    new Anomaly(this.Tile);
                }
                else
                {
                    Planet planet = (Planet)teleport;
                    planet.Teleport(this.Tile);
                }
                return true;
            }

            return false;
        }
        private void AddPullChance(Dictionary<ISpaceObject, int> objects, Player can, ISpaceObject spaceObj, double div, Ship anomShip)
        {
            if (spaceObj != anomShip && ( can == null || spaceObj.Player == null || spaceObj.Player == can ))
            {
                double avg = Tile.GetDistance(this.Tile, spaceObj.Tile) * div;
                avg = ( Tile.Game.Diameter + 6.5 ) / ( avg * avg + 13 );
                if (avg > 1)
                    avg = Math.Sqrt(avg);
                else
                    avg *= avg;
                int amt = Game.Random.OEInt(avg);
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
                        if (CanAttack(attShip, attShipTile, colony.Tile, anomShip, true))
                            return true;
                    foreach (Ship trgShip in player.GetShips())
                        if (CanAttack(attShip, attShipTile, trgShip.Tile, anomShip, false))
                            return true;
                }
            return false;
        }

        private bool CreateAnomalies(IEventHandler handler, bool notify, Ship anomShip)
        {
            bool retVal = false;
            int create = Game.Random.OEInt(2.1);
            for (int a = 0 ; a < create ; ++a)
            {
                Tile tile = this.Tile;
                int move = Game.Random.OEInt(( anomShip.CurSpeed + anomShip.MaxSpeed ) / 5.2 + .91 + Tile.Game.Diameter / 21.0);
                for (int b = 0 ; b < move ; ++b)
                    tile = MoveTile(tile);
                if (tile.SpaceObject == null)
                {
                    if (notify)
                        handler.Explore(Anomaly.AnomalyType.Wormhole);
                    notify = false;

                    new Anomaly(tile);
                    retVal = true;
                }
            }
            return retVal;
        }
        private static Tile MoveTile(Tile tile)
        {
            HashSet<Tile> neighbors = Tile.GetNeighbors(tile);
            neighbors.Add(tile);
            foreach (Tile neighbor in Game.Random.Iterate(neighbors))
                if (neighbor == tile ? Game.Random.Bool() : neighbor.SpaceObject == null)
                    return neighbor;
            return Tile.GetNeighbors(tile).ElementAt(Game.Random.Next(neighbors.Count));
        }

        private bool CreateTeleporter(IEventHandler handler)
        {
            return Tile.Game.CreateTeleporter(handler, this.Tile, GetRandomTile());
        }

        #endregion //Wormhole

        #region Experience

        private bool Experience(IEventHandler handler, Ship ship)
        {
            handler.Explore(AnomalyType.Experience);

            ship.AddAnomalyExperience(handler, this.value, Game.Random.Bool(), Game.Random.Bool());

            return true;
        }

        #endregion //Experience

        #region SalvageShip

        private bool SalvageShip(IEventHandler handler, Ship anomShip)
        {
            Player player = anomShip.Player;
            if (Game.Random.Bool())
                player = Game.Random.SelectValue(GetPlayerProximity(this.Tile));

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
                Ship s1 = anomShip, s2 = newShip;
                if (Game.Random.Bool())
                {
                    s1 = newShip;
                    s2 = anomShip;
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

        #region Population

        private bool Population(IEventHandler handler, Ship ship)
        {
            int pop = Game.Random.Round(this.value * Consts.PopulationForGoldHigh);
            double soldiers = this.value / Consts.ExpForSoldiers;

            bool canPop = ( pop <= ship.FreeSpace );
            if (ship.Population > 0)
            {
                double soldierChance = ship.GetSoldierPct() / 1.69;
                soldierChance = 2.1 / ( 2.1 + soldiers / (double)ship.Population + soldierChance * soldierChance );
                if (canPop)
                    soldierChance /= 2.6;

                if (Game.Random.Bool(soldierChance))
                {
                    handler.Explore(AnomalyType.PickupSoldiers, soldiers);

                    ship.AddSoldiers(soldiers);
                    return true;
                }
            }
            if (canPop)
            {
                handler.Explore(AnomalyType.PickupPopulation, pop);

                ship.AddPopulation(pop);
                return true;
            }

            return false;
        }

        #endregion //Population

        #region Valuables

        private bool Valuables(IEventHandler handler, Ship anomShip)
        {
            int amt = Game.Random.Round(this.value);

            bool research;
            switch (Game.Random.Next(13))
            {
            case 1:
            case 2:
                research = handler.Explore(AnomalyType.AskResearchOrGold, amt);
                break;
            case 3:
            case 4:
            case 5:
                research = true;
                break;
            default:
                research = false;
                amt = -1;
                break;
            }

            if (research)
            {
                anomShip.Player.FreeResearch(handler, amt, GetDesignResearch(anomShip.Player));
            }
            else
            {
                double addGold = amt;
                if (amt == -1)
                    addGold = Player.RoundGold(this.value, true);

                handler.Explore(AnomalyType.Gold, addGold);

                anomShip.Player.AddGold(addGold);
            }

            return true;
        }

        #endregion //Valuables
    }
}
