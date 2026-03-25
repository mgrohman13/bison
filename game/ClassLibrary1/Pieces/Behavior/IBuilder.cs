using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior
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
        public interface IBuildExtractor : IReplacer<Extractor>
        {
            public Extractor Build(Resource resource);
        }
        public interface IBuildMech : IBuilder
        {
            public Mech Build(Tile tile, MechBlueprint blueprint);
        }
        public interface IBuildOutpost : IReplacer<FoundationPiece>
        {
            public Outpost Build(Foundation foundation);
        }
        public interface IBuildFactory : IReplacer<FoundationPiece>
        {
            public Factory Build(Foundation foundation);
        }
        public interface IBuildTurret : IReplacer<FoundationPiece>
        {
            public Turret Build(Foundation foundation);
        }
        public interface IBuildGenerator : IReplacer<FoundationPiece>
        {
            public Generator Build(Foundation foundation);
        }
        public interface IReplacer<T> : IBuilder
        {
            public T Replace(bool doReplace, T old, out int energy, out int mass, out bool couldReplace, out bool canReplace);
        }
        public interface IBuildDrone : IBuilder
        {
            public Drone Build(Tile tile);
        }
        //public interface IBuildMissile : IBuilder
        //{
        //    public void Build();
        //}

        [Serializable]
        [DataContract(IsReference = true)]
        public readonly struct Values(double range)
        {
            private readonly double _range = range;

            public Values(IBuilder builder) : this(builder.RangeBase) { }
            public double Range => _range;
        }
    }
}
