using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using static ClassLibrary1.Map.Map;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Treasure : Piece
    {
        internal static readonly double ConvertResearch = Consts.ResearchMassConversion * Consts.MechMassDiv;

        private double? _value;

        internal Treasure(Tile tile, double? value) : base(null, tile)
        {
            this._value = value;
        }

        internal static Treasure NewTreasure(Tile tile, double? value = null)
        {
            Treasure obj = new(tile, value);
            tile.Map.Game.AddPiece(obj);
            if (tile.GetAdjacentTiles().Any(t => t.Piece is PlayerPiece))
            {
                obj.Collect();
                obj = null;
            }
            return obj;
        }

        internal static void Collect(Tile tile)
        {
            foreach (var n in Game.Rand.Iterate(tile.GetAdjacentTiles()))
                if (n.Piece is Treasure t)
                    t.Collect();
        }
        private void Collect()
        {
            Tile tile = Tile;
            this.Die();

            Dictionary<Func<Tile, double, double>, int> choices = new() {
                { Research, 5 },
                { NewResource, 4 },
                { Alien, 3 },
                { Mech, 2 },
            };
            choices.Add(CollectResources, choices.Values.Sum() * 2 + 3);
            var Func = Game.Rand.SelectValue(choices);
            if (_value.HasValue)
                Func = CollectResources;

            double value = _value ?? (210 + Game.Turn) * 9.1;
            double min = _value.HasValue ? 13 : 650;
            if (value < min)
                min = value * .21;
            value = Func(tile, Game.Rand.GaussianOE(value, .26, .13, min));
            Game.Enemy.AddEnergy(Game.Rand.Round(value * .52));
        }

        private double CollectResources(Tile tile, double value)
        {
            Game.CollectResources(value, out int energy, out int mass);
            //RaiseCollectEvent($"Energy: {energy}  Mass: {mass}");
            return energy + mass * Consts.MechMassDiv;
        }
        private double Research(Tile tile, double value)
        {
            //int research = Game.Rand.Round(value / ConvertResearch);
            //Game.Player.Research.FreeTech(research);
            //return research * ConvertResearch;

            var type = Game.Player.Research.AddResearch(value / ConvertResearch, out int add);
            RaiseCollectEvent($"Research: {add}", type.HasValue);
            return add * ConvertResearch;
        }
        private double NewResource(Tile tile, double value)
        {
            Game.Map.GenResources(_ => tile, Game.Rand.DoubleHalf());
            //RaiseCollectEvent($"");
            return 0;
        }
        private double Alien(Tile tile, double value)
        {
            value = Game.Enemy.SpawnAlien(() => tile, value);
            //RaiseCollectEvent($"");
            return Game.Rand.Bool() ? CollectResources(tile, value) : 0;
        }
        private double Mech(Tile tile, double value)
        {
            double targetMult = Math.Sqrt(1.3);
            double rangeMult = Game.Rand.GaussianCapped(targetMult, .0169, Math.Sqrt(targetMult));
            int min = Game.Rand.Round(value / rangeMult);
            int max = Game.Rand.Round(value * rangeMult);

            double convert = Math.Sqrt(ConvertResearch);
            value /= convert;

            Research research = Game.Player.Research;
            double totalLevel = research.GetTotalLevel();
            totalLevel += ClassLibrary1.Research.GetNext(totalLevel) / 2.0 + value;
            int researchLevel = Game.Rand.RangeInt(Game.Rand.Round(research.GetBlueprintLevel() + value),
                Game.Rand.Round(totalLevel + value));
            double researchCost = (researchLevel - totalLevel) * Math.Sqrt(convert);

            MechBlueprint blueprint;
            do
                blueprint = MechBlueprint.MechOneOff(new ResearchMinMaxCost(research, min, max), researchLevel);
            while (Game.Map.PathFindCore(tile, GetMove(blueprint.Movable), _ => false) == null);
            static double GetMove(IMovable.Values movable) => (movable.MoveMax + movable.MoveLimit) / 2.0;
            Players.Mech.NewMech(tile, blueprint);

            RaiseCollectEvent($"Research Level: {researchLevel}");
            return blueprint.Energy + blueprint.Mass * Consts.MechMassDiv + researchCost;
        }

        public override string ToString() => _value.HasValue ? "Resources ~ " + _value.Value.ToString("0") : "Unknown Object";

        internal static void RaiseCollectEvent(int energy, int mass) =>
            RaiseCollectEvent($"Energy: {energy}  Mass: {mass}");
        private static void RaiseCollectEvent(string info, bool research = false) =>
            CollectEvent?.Invoke(null, new CollectEventArgs(info, research));

        public delegate void CollectEventHandler(object sender, CollectEventArgs e);
        public static event CollectEventHandler CollectEvent;
        public class CollectEventArgs
        {
            public readonly string info;
            public readonly bool research;
            public CollectEventArgs(string info, bool research)
            {
                this.info = info;
                this.research = research;
            }
        }
    }
}
