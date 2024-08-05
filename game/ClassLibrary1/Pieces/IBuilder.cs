using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces
{
    public interface IBuilder : IBehavior
    {
        public double Range { get; }
        public double RangeBase { get; }

        internal void Upgrade(Values values);

        public interface IBuildConstructor : IBuilder
        {
            public Constructor Build(Tile tile);
        }
        public interface IBuildExtractor : IBuilder
        {
            public Extractor Build(Resource resource);
            public bool Replace(bool doReplace, Extractor extractor, out int energy, out int mass);
        }
        public interface IBuildMech : IBuilder
        {
            public Mech Build(Tile tile, MechBlueprint blueprint);
        }
        public interface IBuildFactory : IBuilder
        {
            public Factory Build(Foundation foundation);
            public bool Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass);
        }
        public interface IBuildTurret : IBuilder
        {
            public Turret Build(Foundation foundation);
            public bool Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass);
        }
        public interface IBuildDrone : IBuilder
        {
            public Drone Build(Tile tile);
        }

        [Serializable]
        public readonly struct Values
        {
            private readonly double _range;
            public Values(double range)
            {
                this._range = range;
            }
            public double Range => _range;
        }
    }
}
