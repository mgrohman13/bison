using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class Anomaly : ISpaceObject
    {
        private readonly Tile tile;

        [NonSerialized]
        private double _value;

        internal Anomaly(Tile tile)
        {
            this.tile = tile;
            tile.SpaceObject = this;
        }

        public Tile Tile
        {
            get
            {
                return tile;
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
            foreach (Planet p in Tile.Game.GetPlanets())
            {
                if (p.Colony == null)
                {
                    quality += p.Quality / 2.1;
                }
                else
                {
                    quality += p.Quality;
                    pop += p.Colony.Population;
                    armada += p.Colony.ArmadaCost / 1.69 + p.Colony.production / 2.6;
                }
            }
            foreach (Player p in Tile.Game.GetPlayers())
            {
                foreach (Ship ship in p.GetShips())
                {
                    pop += ship.Population / 1.3;
                    armada += ship.GetCostAvgResearch();
                }
                armada += p.TotalGold / 3.0;
            }

            double income = 1.69 * Consts.Income * ( 5 * pop + 2 * quality ) / 7.0;
            double assets = .13 * armada;
            return Game.Random.GaussianOE(( income + assets ) / Tile.Game.GetPlayers().Length, .39, .26, 1);
        }

        internal void Explore(IEventHandler handler, Ship ship)
        {
            this._value = double.NaN;
            Tile.SpaceObject = null;

            Planet planet = Tile.Game.CreateAnomalyPlanet(this.Tile);
            if (planet != null)
            {
                ship.Player.GoldIncome(ConsolationValue());
                handler.Explore(AnomalyType.NewPlanet, planet);
                return;
            }

            Dictionary<AnomalyType, int> options = new Dictionary<AnomalyType, int>();
            //options.Add(AnomalyType.Death, 4);
            options.Add(AnomalyType.Apocalypse, 5);
            options.Add(AnomalyType.Colony, 6);
            //options.Add(AnomalyType.PlanetDefense, 10);
            options.Add(AnomalyType.Ship, 35);
            options.Add(AnomalyType.Experience, 40);
            options.Add(AnomalyType.Wormhole, 45);
            options.Add(AnomalyType.Population, 50);
            options.Add(AnomalyType.Gold, 55);

            while (true)
            {
                AnomalyType type = Game.Random.SelectValue(options);
                if (Explore(handler, type, ship))
                    return;
                else
                    options.Remove(type);
            }
        }

        private bool Explore(IEventHandler handler, AnomalyType type, Ship ship)
        {
            switch (type)
            {
            case AnomalyType.Colony:
                return Colony(handler, ship);
            case AnomalyType.Apocalypse:
                return ApocalypseTerraform(handler, ship);
            case AnomalyType.Death:
                return Death(handler, ship);
            case AnomalyType.PlanetDefense:
                return PlanetDefenseRemoteSoldiersProduction(handler, ship);
            case AnomalyType.Wormhole:
                return Wormhole(handler, ship);
            case AnomalyType.Experience:
                return Experience(handler, ship);
            case AnomalyType.Ship:
                return Ship(handler, ship);
            case AnomalyType.Population:
                return PopulationThisSoldiers(handler, ship);
            case AnomalyType.Gold:
                return GoldResearch(handler, ship);
            default:
                throw new Exception();
            }
        }

        private bool Colony(IEventHandler handler, Ship ship)
        {
            if (!Tile.Game.CheckPlanetDistance(this.Tile))
                return false;

            Player player = ship.Player;

            int distance = int.MaxValue;
            foreach (Colony colony in player.GetColonies())
                distance = Math.Min(distance, Tile.GetDistance(this.Tile, colony.Tile));
            double cost = int.MaxValue;
            foreach (ShipDesign design in player.GetShipDesigns())
                if (design.Colony)
                    cost = Math.Min(cost, design.Upkeep * distance / (double)design.Speed
                            + design.Cost - design.GetColonizationValue(Tile.Game.MapSize, player.LastResearched));

            double amount = Value - cost;
            double mult = Consts.GetColonizationMult();
            if (amount < Consts.GetColonizationCost(Planet.ConstValue, mult))
                return false;

            if (player != ship.Player)
                ship.Player.GoldIncome(ConsolationValue());

            Planet planet = Tile.Game.CreatePlanet(this.Tile);
            planet.ReduceQuality(planet.Quality - MattUtil.TBSUtil.FindValue(delegate(int value)
            {
                return ( amount > Consts.GetColonizationCost(Planet.ConstValue + value, mult) );
            }, 0, Consts.NewPlanetQuality(), false));
            int production = Game.Random.Round(amount - Consts.GetColonizationCost(Planet.ConstValue + planet.Quality, mult));
            Colony newColony = player.NewColony(handler, planet, 0, 0, production);

            handler.Explore(AnomalyType.Colony, newColony);

            return true;
        }

        private bool ApocalypseTerraform(IEventHandler handler, Ship ship)
        {
            //other chance to damage/destroy/improve planets?
            //chance to do something to pop instead of quality?

            double terraform = Consts.AverageQuality + Planet.ConstValue;
            double apocalypse = 0;
            foreach (Planet p in Tile.Game.GetPlanets())
                apocalypse += p.Quality / 2.0;

            if (Game.Random.Bool(terraform / ( terraform + apocalypse )))
                return Apocalypse(handler, ship);
            else
                return Terraform(handler, ship);
        }
        private bool Apocalypse(IEventHandler handler, Ship ship)
        {
            handler.Explore(AnomalyType.Apocalypse);

            Dictionary<Player, double> addGold = new Dictionary<Player, double>();
            addGold[ship.Player] = ConsolationValue();
            foreach (Planet p in Tile.Game.GetPlanets())
            {
                double gold = 0;
                if (p.Colony != null)
                    gold = Math.Sqrt(( p.Colony.Population + 1.0 ) / ( p.Quality + 1.0 )) * .52;

                gold = Consts.GetColonizationCost(p.DamageVictory(), gold);

                if (p.Colony != null)
                {
                    double amt;
                    addGold.TryGetValue(p.Colony.Player, out amt);
                    addGold[p.Colony.Player] = amt + gold;
                }
            }

            double min = double.MaxValue;
            foreach (double gold in addGold.Values)
                min = Math.Min(min, gold);
            foreach (var pair in addGold)
                pair.Key.AddGold(pair.Value - min, true);

            return true;
        }
        private bool Terraform(IEventHandler handler, Ship ship)
        {
            //adds module to ship that can be attached to a planet?

            Dictionary<Colony, int> colonies = new Dictionary<Colony, int>();
            double[] start = new double[ship.Player.GetColonies().Count];
            int a = -1;
            foreach (Colony c in ship.Player.GetColonies())
            {
                start[++a] = .26 * Tile.Game.Diameter / Tile.GetDistance(c.Tile, Tile);
                int amt = GetTerraformAmt(start[a]);
                if (amt > 0)
                    colonies.Add(c, amt);
            }
            if (colonies.Count == 0)
                return false;

            while (colonies.Count > 0)
            {
                int quality = Consts.NewPlanetQuality() + Game.Random.GaussianOEInt(Planet.ConstValue, .65, .39, 1);
                double avg = Consts.GetColonizationCost(quality, 1.69);
                double bonus = ConsolationValue();
                double cost = Game.Random.GaussianOE((float)( avg - bonus ), Consts.ColonizationCostRndm, Consts.ColonizationCostRndm, (float)-bonus);

                Colony colony = Game.Random.SelectValue(colonies);
                if (handler.Explore(AnomalyType.Terraform, colony, "quality:", quality, "cost:", cost, "avg:", avg, "start:", start))
                {
                    colony.Planet.ReduceQuality(-quality);
                    colony.Player.AddGold(-cost);
                    return true;
                }
                else
                {
                    colonies.Remove(colony);
                    foreach (Colony c in colonies.Keys.ToArray())
                    {
                        int amt = GetTerraformAmt(.52 * colonies[c]);
                        if (amt > 0)
                            colonies[c] = amt;
                        else
                            colonies.Remove(c);
                    }
                }
            }

            return true;
        }
        private static int GetTerraformAmt(double amt)
        {
            return Game.Random.GaussianOEInt(amt, .26, .13);
        }

        private double ConsolationValue()
        {
            return Math.Pow(Value, .78);
        }

        private bool Death(IEventHandler handler, Ship ship)
        {
            //destroy/damage ship?
            //troop battle?
            //bombard a planet?

            handler.Explore(AnomalyType.Death);

            throw new NotImplementedException();
        }

        private bool PlanetDefenseRemoteSoldiersProduction(IEventHandler handler, Ship ship)
        {
            //ship design upgrade/switch?

            AnomalyType type;
            switch (Game.Random.Next(7))
            {
            case 0:
            case 1:
                type = AnomalyType.PlanetDefense;
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                type = AnomalyType.RemoteSoldiers;
                //rate?
                //value /= Consts.ProductionForSoldiers;
                break;
            case 6:
                type = AnomalyType.Production;
                break;
            default:
                throw new Exception();
            }

            handler.Explore(type);

            //added randomly to some/all friendly planets

            throw new NotImplementedException();
        }

        private bool Wormhole(IEventHandler handler, Ship ship)
        {
            handler.Explore(AnomalyType.Wormhole);

            bool any = false;

            if (Game.Random.Bool())
                any |= PushShip(ship);
            if (Game.Random.Bool())
                any |= PullIn(ship);
            if (Game.Random.Bool())
                any |= CreateAnomalies(ship);

            if (any)
                ship.LoseMove();
            else
                any = CreateTeleporter();

            return any;
        }
        private bool PushShip(Ship ship)
        {
            Tile tile = Tile.Game.GetRandomTile();
            if (tile.SpaceObject == null)
            {
                Player oneInv, oneAtt;
                bool twoInv, twoAtt;
                GetAttInvPlayers(tile, ship, out oneInv, out twoInv, out oneAtt, out twoAtt);
                if (!twoAtt && ( oneAtt == null || oneAtt == ship.Player ))
                {
                    ship.Teleport(tile);
                    return true;
                }
            }
            return false;
        }
        private bool PullIn(Ship ship)
        {
            Player oneInv, oneAtt;
            bool twoInv, twoAtt;
            GetAttInvPlayers(Tile, ship, out oneInv, out twoInv, out oneAtt, out twoAtt);

            Dictionary<ISpaceObject, int> objects = new Dictionary<ISpaceObject, int>();
            double mult = ( twoAtt ? 1.69 : ( ( twoInv || ( oneInv != null ) || ( oneAtt != null ) ) ? 1.3 : 1.0 ) );
            foreach (Planet p in Tile.Game.GetPlanets())
            {
                bool colony = ( p.Colony != null );
                if (!twoInv || !colony)
                    AddPullChance(objects, oneInv, p, ( colony ? 16.9 : 13.0 ) * mult);
            }

            foreach (Player p in Game.Random.Iterate(Tile.Game.GetPlayers()))
                if (!twoAtt || p == ship.Player)
                    foreach (Ship s in Game.Random.Iterate(p.GetShips()))
                        if (!CanAttack(s, Tile, ship))
                            AddPullChance(objects, p == ship.Player ? null : oneAtt, s, ( objects.Count > 0 ) ? 7.8 : 11.7);

            if (objects.Count > 0)
            {
                ISpaceObject teleport = Game.Random.SelectValue(objects);
                Ship s = teleport as Ship;
                if (s != null)
                    s.Teleport(Tile);
                else
                    ( (Planet)teleport ).Teleport(Tile);
                return true;
            }

            return false;
        }
        private void AddPullChance(Dictionary<ISpaceObject, int> objects, Player can, ISpaceObject o, double div)
        {
            if (can == null || o.Player == null || o.Player == can)
            {
                double avg = Tile.Game.Diameter / div / ( Tile.GetDistance(Tile, o.Tile) + 3.9 ) * 2.6;
                if (avg > 1)
                    avg = Math.Sqrt(avg);
                else
                    avg *= avg;
                int amt = Game.Random.OEInt(avg);
                if (amt > 0)
                    objects.Add(o, amt);
            }
        }
        private bool CanAttack(Ship s, Tile tile, Ship anomShip)
        {
            foreach (Player p in Tile.Game.GetPlayers())
                if (p != s.Player && p != anomShip.Player)
                {
                    foreach (Colony c in p.GetColonies())
                        if (CanAttack(s, tile, c.Tile, anomShip, true))
                            return true;
                    foreach (Ship s2 in p.GetShips())
                        if (CanAttack(s, tile, s2.Tile, anomShip, false))
                            return true;
                }
            return false;
        }
        private bool CreateAnomalies(Ship ship)
        {
            bool retVal = false;
            int create = Game.Random.OEInt(2.1);
            for (int a = 0 ; a < create ; ++a)
            {
                Tile tile = this.Tile;
                int move = Game.Random.OEInt(( ship.CurSpeed + ship.MaxSpeed ) / 5.2 + .91 + Tile.Game.Diameter / 21.0);
                for (int b = 0 ; b < move ; ++b)
                    tile = DoMove(tile);
                if (tile.SpaceObject == null)
                {
                    new Anomaly(tile);
                    retVal = true;
                }
            }
            return retVal;
        }
        private Tile DoMove(Tile tile)
        {
            HashSet<Tile> neighbors = Tile.GetNeighbors(tile);
            neighbors.Add(tile);
            foreach (Tile t in Game.Random.Iterate(neighbors))
                if (t == tile ? Game.Random.Bool() : t.SpaceObject == null)
                    return t;
            return Tile.GetNeighbors(tile).ElementAt(Game.Random.Next(neighbors.Count));
        }
        private bool CreateTeleporter()
        {
            return Tile.Game.CreateTeleporter(Tile);
        }

        private static void GetAttInvPlayers(Tile tile, Ship ship, out Player oneInv, out bool twoInv, out Player oneAtt, out bool twoAtt)
        {
            GetAttInvPlayers(GetAttInv(tile, ship, true), out oneInv, out twoInv);
            GetAttInvPlayers(GetAttInv(tile, ship, false), out oneAtt, out twoAtt);
        }
        private static void GetAttInvPlayers(HashSet<ISpaceObject> objs, out Player one, out bool two)
        {
            one = null;
            two = false;
            foreach (ISpaceObject o in objs)
                if (one == null)
                {
                    one = o.Player;
                }
                else if (one != o.Player)
                {
                    one = null;
                    two = true;
                    return;
                }
        }
        internal static HashSet<ISpaceObject> GetAttInv(Tile tile, Ship ship, bool inv)
        {
            HashSet<ISpaceObject> retVal = new HashSet<ISpaceObject>();

            //check for attacks from Planet Defenses
            if (!inv)
                foreach (Tile t in Tile.GetNeighbors(tile))
                {
                    Planet p = t.SpaceObject as Planet;
                    if (p != null)
                    {
                        if (p.Colony != null)
                            retVal.Add(p);
                        break;
                    }
                }

            foreach (Player p in tile.Game.GetPlayers())
                foreach (Ship s in p.GetShips())
                    if (s.Tile != tile && CanAttack(s, s.Tile, tile, ship, inv))
                        retVal.Add(s);

            return retVal;
        }

        private static bool CanAttack(Ship s, Tile shipTile, Tile target, Ship anomShip, bool inv)
        {
            int diff = GetSpeed(anomShip, s) - Tile.GetDistance(target, shipTile);
            if (inv)
            {
                if (diff > -2)
                    if (s.Population > 0 || s.DeathStar)
                    {
                        return true;
                    }
                    else if (s.FreeSpace > 0)
                    {
                        //check if the ship could conceivably pick up some population
                        foreach (Colony c in s.Player.GetColonies())
                            if (c.AvailablePop > 0 && GetSpeed(anomShip, s) > Tile.GetDistance(shipTile, c.Tile) - 2)
                                return true;
                        foreach (Ship s2 in s.Player.GetShips())
                            if (s2.AvailablePop > 0 && GetSpeed(anomShip, s) + GetSpeed(anomShip, s2) > Tile.GetDistance(shipTile, s2.Tile) - 2)
                                return true;
                    }
            }
            else if (diff > -1)
            {
                return true;
            }
            return false;
        }
        private static int GetSpeed(Ship anomShip, Ship s)
        {
            return ( anomShip == s ? 0 : s.CurSpeed );
        }

        private bool Experience(IEventHandler handler, Ship ship)
        {
            //other ships?
            //may change name?
            //chance to add cur speed?
            //chance to reduce cost/upk?
            //repair?

            handler.Explore(AnomalyType.Experience);

            ship.AddAnomalyExperience(handler, Value, Game.Random.Bool(), Game.Random.Bool());

            return true;
        }

        private bool Ship(IEventHandler handler, Ship ship)
        {
            //neutral ships?

            Player player = ship.Player;
            if (Game.Random.Bool())
                player = Game.Random.SelectValue(GetPlayerProximity(this.Tile));

            double min = Value, max = GenerateValue();
            if (min > max)
            {
                double temp = min;
                min = max;
                max = temp;
            }

            ShipDesign design = new ShipDesign(player, GetDesignResearch(player), Tile.Game.MapSize, min, max);
            Ship newShip = player.NewShip(handler, tile, design);
            player.GoldIncome(Value - design.Cost);
            if (newShip.Player == ship.Player)
                newShip.LoseMove();

            handler.Explore(AnomalyType.Ship, newShip);

            if (ship.Player != player)
            {
                Ship s1 = ship, s2 = newShip;
                if (Game.Random.Bool())
                {
                    s1 = newShip;
                    s2 = ship;
                }

                double pct = AttackShip(handler, s1, s2);
                pct += AttackShip(handler, s2, s1);

                pct = 2 - pct;
                if (ship.Dead)
                    ++pct;
                const double free = .52;
                pct += free;
                double gold = Value * pct / ( 3 + free );

                if (ship.Dead)
                    ship.Player.AddGold(gold, true);
                else
                    ship.Player.GoldIncome(gold);
            }

            return true;
        }
        private double AttackShip(IEventHandler handler, Ship att, Ship def)
        {
            if (!att.Dead && !def.Dead && Game.Random.Bool() && ( Game.Random.Bool() || handler.ConfirmCombat(att, def) ))
                return att.AttackAnomalyShip(handler, def);
            return 1;
        }

        private static Dictionary<Player, int> GetPlayerProximity(Tile tile)
        {
            Dictionary<Player, int> retVal = new Dictionary<Player, int>();
            foreach (Player p in tile.Game.GetPlayers())
            {
                double distance = 0, tot = 0;
                foreach (Colony c in p.GetColonies())
                {
                    double weight = c.Planet.Quality + c.Population;
                    distance += weight * Tile.GetDistance(tile, c.Tile);
                    tot += weight;
                }
                distance /= tot;
                retVal.Add(p, Game.Random.Round(tile.Game.Diameter / distance));
            }
            return retVal;
        }

        private bool PopulationThisSoldiers(IEventHandler handler, Ship ship)
        {
            int pop = Game.Random.Round(Value * Consts.PopulationForGoldHigh);
            double soldiers = Value / Consts.ExpForSoldiers;

            bool canPop = ( pop <= ship.FreeSpace );
            if (ship.Population > 0)
            {
                double soldierChance = ship.GetSoldierPct() / 1.69;
                soldierChance = 2.1 / ( 2.1 + soldiers / ship.Population + soldierChance * soldierChance );
                if (canPop)
                    soldierChance /= 2.6;

                if (Game.Random.Bool(soldierChance))
                {
                    handler.Explore(AnomalyType.ThisSoldiers, soldiers);
                    ship.AddSoldiers(soldiers);
                    return true;
                }
            }
            if (canPop)
            {
                handler.Explore(AnomalyType.Population, pop);
                ship.AddPopulation(pop);
                return true;
            }

            return false;
        }

        private bool GoldResearch(IEventHandler handler, Ship ship)
        {
            bool research;
            switch (Game.Random.Next(13))
            {
            case 1:
            case 2:
                research = handler.Explore(AnomalyType.GoldResearch, Value);
                break;
            case 3:
            case 4:
            case 5:
                research = true;
                break;
            default:
                research = false;
                handler.Explore(AnomalyType.Gold, Value);
                break;
            }

            if (research)
                ship.Player.FreeResearch(handler, Game.Random.Round(Value), GetDesignResearch(ship.Player));
            else
                ship.Player.AddGold(Value, true);

            return true;
        }
        private int GetDesignResearch(Player player)
        {
            double avg = ( 1 * ( ( 1 * player.ResearchDisplay + 2 * player.Research ) / 3.0 )
                    + 2 * ( player.LastResearched + Value ) + 4 * ( Tile.Game.AvgResearch ) ) / 7.0;
            return Game.Random.GaussianOEInt(avg, .13, .013);
        }

        public enum AnomalyType
        {
            NewPlanet,
            Gold,
            Research,
            GoldResearch,
            Production,
            Ship,
            Experience,
            Death,
            Wormhole,
            Terraform,
            Apocalypse,
            Colony,
            Population,
            ThisSoldiers,
            RemoteSoldiers,
            PlanetDefense,
        }
    }
}
