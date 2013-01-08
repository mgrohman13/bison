using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    public class Anomaly : ISpaceObject
    {
        private readonly Tile tile;

        internal Anomaly(Tile tile)
        {
            this.tile = tile;
        }

        public Tile Tile
        {
            get
            {
                return tile;
            }
        }

        private double GetValue()
        {
            //value based on income
            //    incorporate armada, quality?
            throw new NotImplementedException();
        }

        internal void Explore(IEventHandler handler, Ship ship)
        {
            Tile.SpaceObject = null;

            Planet planet = Tile.Game.CreatePlanet();
            if (planet != null)
            {
                handler.Explore(AnomalyType.NewPlanet, planet);
                return;
            }

            Dictionary<AnomalyType, int> options = new Dictionary<AnomalyType, int>();
            options.Add(AnomalyType.Colony, 2);
            options.Add(AnomalyType.Apocalypse, 3);
            options.Add(AnomalyType.Death, 5);
            options.Add(AnomalyType.PlanetDefense, 20);
            options.Add(AnomalyType.Wormhole, 39);
            options.Add(AnomalyType.Experience, 40);
            options.Add(AnomalyType.Ship, 44);
            options.Add(AnomalyType.Population, 52);
            options.Add(AnomalyType.Gold, 78);

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
            //if not too close
            //2/3 friendly, 1/3 hostile
            //0 pop
            //defense? prod? soldiers?
            //quality based on gold/prod cost of colonizing from nearest planet as compared to default anomaly value

            Colony colony = null;
            handler.Explore(AnomalyType.Colony, colony);

            throw new NotImplementedException();
        }

        private bool ApocalypseTerraform(IEventHandler handler, Ship ship)
        {
            //other chance to damage/destroy/improve planets?

            double terraform = Consts.AverageQuality + Planet.ConstValue;
            double apocalypse = 0;
            foreach (Planet p in Tile.Game.GetPlanets())
                apocalypse += p.Quality / 2.0;

            if (Game.Random.Bool(terraform / ( terraform + apocalypse )))
                return Apocalypse(handler);
            else
                return Terraform(handler, ship);
        }
        private bool Apocalypse(IEventHandler handler)
        {
            handler.Explore(AnomalyType.Apocalypse);

            Dictionary<Player, double> addGold = new Dictionary<Player, double>();
            foreach (Planet p in Tile.Game.GetPlanets())
            {
                double gold = 0;
                if (p.Colony != null)
                    gold = Math.Sqrt(( p.Colony.Population + 1.0 ) / ( p.Quality + 1.0 )) * .26;

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
            Dictionary<Colony, int> colonies = new Dictionary<Colony, int>();
            foreach (Colony c in ship.Player.GetColonies())
            {
                int amt = GetTerraformAmt(.26 * Tile.Game.Diameter / Tile.GetDistance(c.Tile, Tile));
                if (amt > 0)
                    colonies.Add(c, amt);
            }
            if (colonies.Count == 0)
                return false;

            //adds module to ship that can be attached to a planet?

            while (colonies.Count > 0)
            {
                //dont rand each time?
                int quality = Consts.NewPlanetQuality() + Game.Random.GaussianCappedInt(Planet.ConstValue, 1, 1);
                double cost = Game.Random.GaussianOE(Consts.GetColonizationCost(quality, .39), Consts.ColonizationCostRndm, Consts.ColonizationCostRndm);

                Colony colony = Game.Random.SelectValue(colonies);
                if (handler.Explore(AnomalyType.Terraform, colony, quality, cost))
                {
                    colony.Planet.ReduceQuality(-quality);
                    //GoldIncome?
                    colony.Player.AddGold(-cost);
                    break;
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

            double value = GetValue();

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
                value /= Consts.ProductionForSoldiers;
                break;
            case 6:
                type = AnomalyType.Production;
                break;
            default:
                throw new Exception();
            }

            handler.Explore(type, value);

            //added randomly to some/all friendly planets

            throw new NotImplementedException();
        }

        private bool Wormhole(IEventHandler handler, Ship ship)
        {
            //move anomaly? create more?

            handler.Explore(AnomalyType.Wormhole);

            throw new NotImplementedException();
        }

        private bool Experience(IEventHandler handler, Ship ship)
        {
            //other ships?
            //may change name?
            //chance to add cur speed?
            //chance to reduce cost/upk?
            //repair?

            handler.Explore(AnomalyType.Experience, this);

            ship.AddAnomalyExperience(handler, GetValue(), Game.Random.Bool(), Game.Random.Bool());

            return true;
        }

        private bool Ship(IEventHandler handler, Ship ship)
        {
            //neutral ships?

            Player player = ship.Player;
            if (Game.Random.Next(3) == 0)
                do
                {
                    player = Tile.Game.GetPlayers()[Game.Random.Next(Tile.Game.GetPlayers().Length)];
                } while (player == ship.Player);

            double min = GetShipCost(), max = GetShipCost();
            if (min > max)
            {
                double temp = min;
                min = max;
                max = temp;
            }

            //random 1-time design, research based on LastResearched
            //    special name?

            //Ship newShip = ShipDesign.GetAnomalyShip(this.Tile, player, min, max);

            //handler.Explore(AnomalyType.Ship, newShip);

            //randomly chance of auto attack/defend hostile, sometimes option to confirm
            //    chance to add cur speed?

            throw new NotImplementedException();
        }
        private double GetShipCost()
        {
            return GetValue() * 1.69;
        }

        private bool PopulationThisSoldiers(IEventHandler handler, Ship ship)
        {
            //chance to add cur speed?

            double value = GetValue();

            //rate?
            int pop = Game.Random.Round(value * Consts.PopulationForGoldHigh);
            //rate?
            double soldiers = value / Consts.ProductionForSoldiers;

            bool canPop = ( pop <= ship.FreeSpace );
            if (ship.Population > 0)
            {
                double soldierChance = ship.GetTotalSoldierPct() / 1.69;
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
            double value = GetValue();

            if (Game.Random.Bool(.39))
            {
                handler.Explore(AnomalyType.Research, value);
                ship.Player.FreeResearch(value);
            }
            else
            {
                handler.Explore(AnomalyType.Gold, value);
                ship.Player.AddGold(value, true);
            }

            return true;
        }

        public enum AnomalyType
        {
            NewPlanet,
            Gold,
            Research,
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
