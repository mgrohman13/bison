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
        public interface IBuildExtractor : IBuilder, IReplaceable<Extractor>
        {
            public Extractor Build(Resource resource);
        }
        public interface IBuildMech : IBuilder
        {
            public Mech Build(Tile tile, MechBlueprint blueprint);
        }
        public interface IBuildFactory : IBuilder, IReplaceable<FoundationPiece>
        {
            public Factory Build(Foundation foundation);
        }
        public interface IBuildTurret : IBuilder, IReplaceable<FoundationPiece>
        {
            public Turret Build(Foundation foundation);
        }
        public interface IBuildGenerator : IBuilder, IReplaceable<FoundationPiece>
        {
            public Generator Build(Foundation foundation);
        }
        public interface IReplaceable<T>
        {
            public bool Replace(bool doReplace, T old, out int energy, out int mass, out bool couldReplace);
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
            public Values(IBuilder builder) : this(builder.RangeBase) { }
            public double Range => _range;
        }
    }
}
