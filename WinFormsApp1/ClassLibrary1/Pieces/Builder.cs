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
    [Serializable]
    public class Builder : IBuilder
    {
        bool IBehavior.AllowMultiple => true;

        private readonly Piece _piece;
        public Piece Piece => _piece;

        private IBuilder.Values _values;

        public double Range => Consts.GetDamagedValue(Piece, RangeBase, 1);
        public double RangeBase => _values.Range;

        public Builder(Piece piece, IBuilder.Values values)
        {
            this._piece = piece;
            this._values = values;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        internal static void UpgradeAll(Piece piece, IBuilder.Values values)
        {
            foreach (IBuilder builder in piece.GetBehaviors<IBuilder>())
                builder.Upgrade(values);
        }
        void IBuilder.Upgrade(IBuilder.Values values)
        {
            this._values = values;
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
        }

        private bool Validate(Map.Tile tile, bool empty)
        {
            return (tile != null && (!empty || tile.Piece == null) && tile.Visible && tile.GetDistance(this.Piece.Tile) <= Range);
        }
        private bool Replace(bool doReplace, PlayerPiece piece, CostFunc GetNewCost, Func<double> GetRounding, Action NewPiece, bool validateHits, out int energy, out int mass)
        {
            if (piece != null && Validate(piece.Tile, false) && piece.HasBehavior(out IKillable killable) && (!validateHits || killable.HitsCur < killable.HitsMax))
            {
                GetNewCost(out int newEnergy, out int newMass);
                if (piece is Extractor)
                {
                    energy = newEnergy;
                    mass = newMass;
                }
                else if (piece is Factory)
                    Factory.Cost(piece.Game, out energy, out mass);
                else if (piece is Turret)
                    Turret.Cost(piece.Game, out energy, out mass);
                else throw new Exception();

                double mult = killable.HitsCur / (double)killable.HitsMax * Consts.ReplaceRefundPct;
                double rounding = GetRounding();
                energy = MTRandom.Round(newEnergy - energy * mult, rounding);
                mass = MTRandom.Round(newMass - mass * mult, rounding);

                if (Piece.Game.Player.Has(energy, mass))
                {
                    if (doReplace && Piece.Game.Player.Spend(energy, mass))
                    {
                        piece.Die();
                        NewPiece();
                    }
                    return true;
                }
            }
            energy = mass = 0;
            return false;
        }
        private delegate void CostFunc(out int energy, out int mass);

        [Serializable]
        public class BuildConstructor : Builder, IBuilder.IBuildConstructor
        {
            public BuildConstructor(Piece piece, IBuilder.Values values)
                : base(piece, values)
            {
            }
            public Constructor Build(Map.Tile tile)
            {
                if (Validate(tile, true))
                {
                    Constructor.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                        return Constructor.NewConstructor(tile, false);
                }
                return null;
            }
        }
        [Serializable]
        public class BuildExtractor : Builder, IBuilder.IBuildExtractor
        {
            public BuildExtractor(Piece piece, IBuilder.Values values)
                : base(piece, values)
            {
            }
            public Extractor Build(Resource resource)
            {
                if (resource != null && Validate(resource.Tile, false))
                {
                    Extractor.Cost(out int energy, out int mass, resource);
                    if (Piece.Game.Player.Spend(energy, mass))
                        return Extractor.NewExtractor(resource);
                }
                return null;
            }
            public bool Replace(bool doReplace, Extractor extractor, out int energy, out int mass)
            {
                return Replace(doReplace, extractor,
                    (out int e, out int m) => Extractor.Cost(out e, out m, extractor.Resource),
                    () => extractor.Resource.Rounding,
                    () => Extractor.NewExtractor(extractor.Resource),
                    true, out energy, out mass);
            }
        }
        [Serializable]
        public class BuildMech : Builder, IBuilder.IBuildMech
        {
            public BuildMech(Piece piece, IBuilder.Values values)
                : base(piece, values)
            {
            }
            public Mech Build(Map.Tile tile, MechBlueprint blueprint)
            {
                if (Validate(tile, true))
                {
                    if (Piece.Game.Player.Spend(blueprint.Energy, blueprint.Mass))
                        return Mech.NewMech(tile, blueprint);
                }
                return null;
            }
        }
        [Serializable]
        public class BuildFactory : Builder, IBuilder.IBuildFactory
        {
            public BuildFactory(Piece piece, IBuilder.Values values)
                : base(piece, values)
            {
            }
            public Factory Build(Foundation foundation)
            {
                if (foundation != null && Validate(foundation.Tile, false))
                {
                    Factory.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                        return Factory.NewFactory(foundation);
                }
                return null;
            }
            public bool Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass)
            {
                Map.Tile tile = foundationPiece?.Tile;
                return Replace(doReplace, foundationPiece,
                    (out int e, out int m) => Factory.Cost(Piece.Game, out e, out m),
                    () => Factory.GetRounding(Piece.Game),
                    () => Factory.NewFactory((Foundation)tile.Piece),
                    false, out energy, out mass);
            }
        }
        [Serializable]
        public class BuildTurret : Builder, IBuilder.IBuildTurret
        {
            public BuildTurret(Piece piece, IBuilder.Values values)
               : base(piece, values)
            {
            }
            public Turret Build(Foundation foundation)
            {
                if (foundation != null && Validate(foundation.Tile, false))
                {
                    Turret.Cost(Piece.Game, out int energy, out int mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                        return Turret.NewTurret(foundation);
                }
                return null;
            }
            public bool Replace(bool doReplace, FoundationPiece foundationPiece, out int energy, out int mass)
            {
                Map.Tile tile = foundationPiece?.Tile;
                return Replace(doReplace, foundationPiece,
                    (out int e, out int m) => Turret.Cost(Piece.Game, out e, out m),
                    () => Turret.GetRounding(Piece.Game),
                    () => Turret.NewTurret((Foundation)tile.Piece),
                    false, out energy, out mass);
            }
        }
    }
}
