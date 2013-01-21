using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class Anomaly : ISpaceObject
    {
        private readonly Tile _tile;

        [NonSerialized]
        private double _value;

        internal Anomaly(Tile tile)
        {
            this._tile = tile;
            tile.SpaceObject = this;
        }

        public Tile Tile
        {
            get
            {
                return _tile;
            }
        }

        public Player Player
        {
            get
            {
                return null;
            }
        }

        private double Value
        {
            get
            {
                if (double.IsNaN(this._value))
                    this._value = GenerateValue();
                return this._value;
            }
        }

        private double GenerateValue()
        {
            double quality = 0, pop = 0, armada = 0;
            foreach (Planet planet in Tile.Game.GetPlanets())
            {
                if (planet.Colony == null)
                {
                    quality += planet.Quality / 2.1;
                }
                else
                {
                    quality += planet.Quality;
                    pop += planet.Colony.Population;
                    armada += planet.Colony.ArmadaCost / 1.69 + planet.Colony.production / 2.6;
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

            double income = 2.1 * Consts.Income * ( 5 * pop + 2 * quality ) / 7.0;
            double assets = .13 * armada;
            return Game.Random.GaussianOE(( income + assets ) / Tile.Game.GetPlayers().Length, .39, .26, 1);
        }

        internal void Explore(IEventHandler handler, Ship ship)
        {
            this._value = double.NaN;
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
                ExploreType type = Game.Random.SelectValue(options);
                if (Explore(handler, type, ship))
                    return;
                else
                    options.Remove(type);
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

            double amount = this.Value - cost;
            double mult = Consts.GetColonizationMult();
            if (amount < Consts.GetColonizationCost(Planet.ConstValue, mult))
                return false;

            handler.Explore(AnomalyType.LostColony, player);

            if (player != anomShip.Player)
                anomShip.Player.GoldIncome(ConsolationValue());

            Planet planet = Tile.Game.CreatePlanet(this.Tile);
            planet.ReduceQuality(planet.Quality - MattUtil.TBSUtil.FindValue(delegate(int value)
            {
                return ( amount > Consts.GetColonizationCost(Planet.ConstValue + value, mult) );
            }, 0, Consts.NewPlanetQuality(), false));
            int production = Game.Random.Round(amount - Consts.GetColonizationCost(Planet.ConstValue + planet.Quality, mult));
            Colony newColony = player.NewColony(handler, planet, 0, 0, production);

            return true;
        }

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
                double terraformAmt = Consts.AverageQuality + Planet.ConstValue;
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
                double forExplorer = this.Value * Consts.PopulationForGoldHigh;
                double diePct = Game.Random.GaussianCapped(.3, 0.091, .13);

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
                    gold = Math.Sqrt(( planet.Colony.Population + 1.0 ) / ( planet.Quality + 1.0 )) * .52;

                gold = Consts.GetColonizationCost(planet.DamageVictory(), gold);

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
                colonyChances[++idx] = .26 * Tile.Game.Diameter / Tile.GetDistance(colony.Tile, this.Tile);
                int amt = GetTerraformAmt(colonyChances[idx]);
                if (amt > 0)
                    colonies.Add(colony, amt);
            }
            if (colonies.Count == 0)
                return false;

            while (colonies.Count > 0)
            {
                int addQuality = Consts.NewPlanetQuality() + Game.Random.GaussianOEInt(Planet.ConstValue, .65, .39, 1);
                double expectCost = Consts.GetColonizationCost(addQuality, 1.69);
                double bonus = ConsolationValue();
                double actualCost = Game.Random.GaussianOE((float)( expectCost - bonus ), Consts.ColonizationCostRndm, Consts.ColonizationCostRndm, (float)-bonus);

                Colony trgColony = Game.Random.SelectValue(colonies);
                if (handler.Explore(AnomalyType.AskTerraform, trgColony, addQuality, actualCost, expectCost, colonyChances))
                {
                    trgColony.Planet.ReduceQuality(-addQuality);
                    trgColony.Player.AddGold(-actualCost);
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

            anomShip.Player.GoldIncome(ConsolationValue());

            foreach (Player player in Tile.Game.GetPlayers())
            {
                foreach (Colony colony in player.GetColonies())
                    colony.LosePopulation(Game.Random.Round(colony.Population * diePct));
                foreach (Ship ship in player.GetShips())
                    ship.LosePopulation(Game.Random.Round(ship.Population * diePct));
            }

            return true;
        }
        private bool AddPop(IEventHandler handler, Ship anomShip, double addAmt, double forExplorer)
        {
            handler.Explore(AnomalyType.PopulationGrowth);

            foreach (Player player in Tile.Game.GetPlayers())
            {
                double amt = ( addAmt - forExplorer ) / Tile.Game.GetPlayers().Length;
                if (anomShip.Player == player)
                    amt += forExplorer;

                double total;
                Dictionary<Colony, int> colonies = GetPlayerColonyWeights(player, out total);
                amt /= -total;

                foreach (var pair in colonies)
                    pair.Key.LosePopulation(Game.Random.Round(amt * pair.Value));
            }

            return true;
        }

        private double ConsolationValue()
        {
            return Math.Pow(this.Value, .78);
        }

        private bool Death(IEventHandler handler, Ship ship)
        {
            int damage = ( ( ship.MaxHP < 2 || Game.Random.Bool() ) ? ( ship.MaxHP ) : ( 1 + Game.Random.WeightedInt(ship.MaxHP - 2, .39) ) );

            handler.Explore(AnomalyType.Death, damage);

            if (damage < ship.HP)
            {
                if (damage > ship.HP)
                    damage = ship.HP;

                ship.Player.GoldIncome(ConsolationValue() + ship.GetDisbandValue(damage));

                int pop = ship.Population;
                ship.AddExperience(ship.Damage(damage));
                if (pop > 0)
                    ship.AddCostExperience(( pop - ship.Population ) * Consts.TroopExperienceMult);
            }
            else
            {
                ship.Player.AddGold(Value + ship.DisbandValue, true);

                ship.Destroy(true, true);
            }

            return true;
        }

        private bool Production(IEventHandler handler, Ship anomShip)
        {
            double total;
            Dictionary<Colony, int> colonies = GetPlayerColonyWeights(anomShip.Player, out total);

            Colony single = null;
            if (colonies.Count == 1 || Game.Random.Bool())
                single = Game.Random.SelectValue(colonies);

            double value = this.Value;
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
                foreach (var pair in colonies)
                    PlanetDefenseRemoteSoldiersProduction(type, pair.Key, value * pair.Value / total);
            }
            else
            {
                if (notify)
                    handler.Explore(type, single, value);
                PlanetDefenseRemoteSoldiersProduction(type, single, value);
            }

            return true;
        }

        private Dictionary<Colony, int> GetPlayerColonyWeights(Player player, out double total)
        {
            total = 0;
            var colonies = new Dictionary<Colony, int>();
            foreach (Colony colony in player.GetColonies())
            {
                int weight = Game.Random.Round(( colony.Planet.Quality + colony.Population + 1 )
                        * Tile.Game.Diameter / Tile.GetDistance(this.Tile, colony.Tile));
                colonies.Add(colony, weight);
                total += weight;
            }
            return colonies;
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

            return any;
        }
        private bool PushShip(IEventHandler handler, bool notify, Ship ship)
        {
            Tile tile = Tile.Game.GetRandomTile();
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
            double mult = ( twoAtt ? 1.69 : ( ( twoInv || ( oneInv != null ) || ( oneAtt != null ) ) ? 1.3 : 1.0 ) );
            foreach (Planet planet in Tile.Game.GetPlanets())
            {
                bool colony = ( planet.Colony != null );
                if (!twoInv || !colony)
                    AddPullChance(objects, oneInv, planet, ( colony ? 16.9 : 13.0 ) * mult, anomShip);
            }

            foreach (Player player in Game.Random.Iterate(Tile.Game.GetPlayers()))
                if (!twoAtt || player == anomShip.Player)
                    foreach (Ship ship in Game.Random.Iterate(player.GetShips()))
                        AddPullChance(objects, player == anomShip.Player ? null : oneAtt, ship, ( objects.Count > 0 ) ? 7.8 : 11.7, anomShip);

            if (objects.Count > 0)
            {
                if (notify)
                    handler.Explore(Anomaly.AnomalyType.Wormhole);

                ISpaceObject teleport = Game.Random.SelectValue(objects);
                Ship ship = teleport as Ship;
                if (ship != null)
                    ship.Teleport(this.Tile);
                else
                    ( (Planet)teleport ).Teleport(this.Tile);
                return true;
            }

            return false;
        }
        private void AddPullChance(Dictionary<ISpaceObject, int> objects, Player can, ISpaceObject spaceObj, double div, Ship anomShip)
        {
            if (spaceObj != anomShip && ( can == null || spaceObj.Player == null || spaceObj.Player == can ))
            {
                double avg = Tile.Game.Diameter / div / ( Tile.GetDistance(this.Tile, spaceObj.Tile) + 3.9 ) * 2.6;
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
            return Tile.Game.CreateTeleporter(handler, this.Tile);
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
        internal static HashSet<ISpaceObject> GetAttInv(Tile target, Ship anomShip, bool inv)
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

        private bool Experience(IEventHandler handler, Ship ship)
        {
            handler.Explore(AnomalyType.Experience);

            ship.AddAnomalyExperience(handler, this.Value, Game.Random.Bool(), Game.Random.Bool());

            return true;
        }

        private bool SalvageShip(IEventHandler handler, Ship anomShip)
        {
            Player player = anomShip.Player;
            if (Game.Random.Bool())
                player = Game.Random.SelectValue(GetPlayerProximity(this.Tile));

            handler.Explore(AnomalyType.SalvageShip, player);

            double min = this.Value, max = GenerateValue();
            if (min > max)
            {
                double temp = min;
                min = max;
                max = temp;
            }

            ShipDesign design = new ShipDesign(player, GetDesignResearch(player), Tile.Game.MapSize, min, max);
            Ship newShip = player.NewShip(handler, this.Tile, design);
            player.GoldIncome(this.Value - design.Cost);
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
                double gold = ConsolationValue() + ( this.Value - ConsolationValue() ) * pct / 4.0;

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

        private bool Population(IEventHandler handler, Ship ship)
        {
            int pop = Game.Random.Round(this.Value * Consts.PopulationForGoldHigh);
            double soldiers = this.Value / Consts.ExpForSoldiers;

            bool canPop = ( pop <= ship.FreeSpace );
            if (ship.Population > 0)
            {
                double soldierChance = ship.GetSoldierPct() / 1.69;
                soldierChance = 2.1 / ( 2.1 + soldiers / ship.Population + soldierChance * soldierChance );
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

        private bool Valuables(IEventHandler handler, Ship anomShip)
        {
            bool research;
            switch (Game.Random.Next(13))
            {
            case 1:
            case 2:
                research = handler.Explore(AnomalyType.AskResearchOrGold, this.Value);
                break;
            case 3:
            case 4:
            case 5:
                research = true;
                break;
            default:
                research = false;
                handler.Explore(AnomalyType.Gold, this.Value);
                break;
            }

            if (research)
                anomShip.Player.FreeResearch(handler, Game.Random.Round(this.Value), GetDesignResearch(anomShip.Player));
            else
                anomShip.Player.AddGold(this.Value, true);

            return true;
        }
        private int GetDesignResearch(Player player)
        {
            double avg = ( 1 * ( ( 1 * player.ResearchDisplay + 2 * player.Research ) / 3.0 )
                    + 2 * ( player.LastResearched + this.Value ) + 4 * ( Tile.Game.AvgResearch ) ) / 7.0;
            return Game.Random.GaussianOEInt(avg, .13, .013);
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
            Soldiers,
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
            SoldiersAndDefense,
            Wormhole,
        }
    }
}
