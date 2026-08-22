using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Builder(Piece piece, IBuilder.Values values) : IBuilder
    {
        private bool _built = true;

        bool IBehavior.AllowMultiple => true;

        private readonly Piece _piece = piece;
        public Piece Piece => _piece;

        private IBuilder.Values _values = values;

        public bool Built => _built;
        public double Range => Consts.GetDamagedValue(Piece, RangeBase, Attack.MELEE_RANGE);
        public double RangeBase => _values.Range;

        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        internal static void UpgradeAll(Piece piece, IBuilder.Values values)
        {
            foreach (IBuilder builder in Game.Rand.Iterate(piece.GetBehaviors<IBuilder>()))
                builder.Upgrade(values);
        }
        void IBuilder.Upgrade(IBuilder.Values values)
        {
            _values = values;
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
        }
        void IBehavior.StartTurn()
        {
            StartTurn();
        }
        protected void StartTurn()
        {
            //base.StartTurn();
            _built = false;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
        }
        double IBehavior.Die()
        {
            return 0;
        }

        private bool Validate(Tile tile, bool empty)
        {
            //check blocks
            return tile != null && tile.Visible && tile.GetDistance(Piece.Tile) <= Range
                && (empty ? tile.Piece == null : !tile.Piece.HasBehavior(out IMissileSilo silo) || silo.NumMissiles == 0);
        } 

        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildConstructor(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildConstructor
        {
            public Constructor Build(Tile tile)
            {
                if (Validate(tile, true))
                {
                    Constructor.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Constructor.NewConstructor(tile, false);
                    }
                }
                return null;
            }
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildExtractor(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildExtractor
        {
            public Extractor Build(Resource resource)
            {
                if (resource != null && Validate(resource.Tile, false))
                {
                    Extractor.Cost(resource, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Extractor.NewExtractor(resource);
                    }
                }
                return null;
            }
            //public Extractor Replace(bool doReplace, Extractor extractor, out int energy, out int mass, out bool couldReplace, out bool canReplace)
            //{
            //    return Replace(doReplace, extractor,
            //        (out e, out m) => Extractor.Cost(out e, out m, extractor.Resource),
            //        () => extractor.Resource.Rounding,
            //        () => Extractor.NewExtractor(extractor.Resource),
            //        false, out energy, out mass, out couldReplace, out canReplace); //true
            //}
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildMech(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildMech
        {
            public Mech Build(Tile tile, MechBlueprint blueprint)
            {
                if (Validate(tile, true))
                {
                    if (Piece.Game.Player.Spend(blueprint.Energy, blueprint.Mass))
                    {
                        _built = true;
                        return Mech.NewMech(tile, blueprint);
                    }
                }
                return null;
            }
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildOutpost(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildOutpost
        {
            public Outpost Build(Foundation foundation)
            {
                if (foundation != null && Validate(foundation.Tile, false))
                {
                    Outpost.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Outpost.NewOutpost(foundation);
                    }
                }
                return null;
            }
            //public FoundationPiece Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass, out bool couldReplace, out bool canReplace)
            //{
            //    Tile tile = foundationPiece?.Tile;
            //    return Replace(doReplace, foundationPiece,
            //        (out e, out m) => Outpost.Cost(Piece.Game, out e, out m),
            //        () => Outpost.GetRounding(Piece.Game),
            //        () => Outpost.NewOutpost((Foundation)tile.Piece),
            //        false, out energy, out mass, out couldReplace, out canReplace);
            //}
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildFactory(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildFactory
        {
            public Factory Build(Foundation foundation)
            {
                if (foundation != null && Validate(foundation.Tile, false))
                {
                    Factory.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Factory.NewFactory(foundation);
                    }
                }
                return null;
            }
            //public FoundationPiece Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass, out bool couldReplace, out bool canReplace)
            //{
            //    Tile tile = foundationPiece?.Tile;
            //    return Replace(doReplace, foundationPiece,
            //        (out e, out m) => Factory.Cost(Piece.Game, out e, out m),
            //        () => Factory.GetRounding(Piece.Game),
            //        () => Factory.NewFactory((Foundation)tile.Piece),
            //        false, out energy, out mass, out couldReplace, out canReplace);
            //}
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildTurret(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildTurret
        {
            public Turret Build(Foundation foundation)
            {
                if (foundation != null && Validate(foundation.Tile, false))
                {
                    Turret.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Turret.NewTurret(foundation);
                    }
                }
                return null;
            }
            //public FoundationPiece Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass, out bool couldReplace, out bool canReplace)
            //{
            //    Tile tile = foundationPiece?.Tile;
            //    return Replace(doReplace, foundationPiece,
            //        (out e, out m) => Turret.Cost(Piece.Game, out e, out m),
            //        () => Turret.GetRounding(Piece.Game),
            //        () => Turret.NewTurret((Foundation)tile.Piece),
            //        false, out energy, out mass, out couldReplace, out canReplace);
            //}
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildGenerator(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildGenerator
        {
            public Generator Build(Foundation foundation)
            {
                if (foundation != null && Validate(foundation.Tile, false))
                {
                    Generator.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Generator.NewGenerator(foundation);
                    }
                }
                return null;
            }
            //public FoundationPiece Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass, out bool couldReplace, out bool canReplace)
            //{
            //    Tile tile = foundationPiece?.Tile;
            //    return Replace(doReplace, foundationPiece,
            //        (out e, out m) => Generator.Cost(Piece.Game, out e, out m),
            //        () => Generator.GetRounding(Piece.Game),
            //        () => Generator.NewGenerator((Foundation)tile.Piece),
            //        false, out energy, out mass, out couldReplace, out canReplace);
            //}
        }
        [Serializable]
        [DataContract(IsReference = true)]
        public class BuildDrone(Piece piece, IBuilder.Values values) : Builder(piece, values), IBuilder.IBuildDrone
        {
            public Drone Build(Tile tile)
            {
                if (Validate(tile, true))
                {
                    Drone.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        _built = true;
                        return Drone.NewDrone(tile);
                    }
                }
                return null;
            }
        }
        //public class BuildMissile : Builder, IBuilder.IBuildMissile
        //{
        //    public BuildMissile(IMissileSilo silo, IBuilder.Values values)
        //        : base(silo.Piece, values)
        //    {
        //    }
        //    public void Build()
        //    {
        //        //if (Validate(tile, true))
        //        //{
        //        //    Drone.Cost(Piece.Game, out int energy, out int mass);
        //        //    if (Piece.Game.Player.Spend(energy, mass))
        //        //        return Drone.NewDrone(tile);
        //        //}
        //        //return null;
        //    }
        //}
    }
}
