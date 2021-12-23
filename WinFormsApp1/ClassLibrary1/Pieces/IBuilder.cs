using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces
{
    public interface IBuilder : IBehavior
    {
        public double Range { get; }
        public double RangeBase { get; }

        internal void Upgrade(Values values);

        public interface IBuildConstructor : IBuilder
        {
            public Constructor Build(Map.Tile tile);
        }
        public interface IBuildExtractor : IBuilder
        {
            public Extractor Build(Resource resource);
        }
        public interface IBuildMech : IBuilder
        {
            public Mech Build(Map.Tile tile, MechBlueprint blueprint);
        }
        public interface IBuildFactory : IBuilder
        {
            public Factory Build(Foundation foundation);
        }
        public interface IBuildTurret : IBuilder
        {
            public Turret Build(Foundation foundation);
        }

        [Serializable]
        public struct Values
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
