using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using Tile = ClassLibrary1.Map.Map.Tile;

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
            foreach (IBuilder builder in Game.Rand.Iterate(piece.GetBehaviors<IBuilder>()))
                builder.Upgrade(values);
        }
        void IBuilder.Upgrade(IBuilder.Values values)
        {
            this._values = values;
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
        }
        void IBehavior.StartTurn()
        {
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
        }

        private bool Validate(Tile tile, bool empty)
        {
            //check blocks
            return (tile != null && (!empty || tile.Piece == null) && tile.Visible && tile.GetDistance(this.Piece.Tile) <= Range);
        }
        private bool Replace(bool doReplace, PlayerPiece piece, CostFunc GetNewCost, Func<double> GetRounding, Action NewPiece, bool validateHits, out int energy, out int mass)
        {
            if (piece != null && Validate(piece.Tile, false) && piece.HasBehavior(out IKillable killable)
                && (!validateHits || killable.Hits.DefenseCur < killable.Hits.DefenseMax))
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

                double totCur = 0, totStart = 0, addEnergy = 0;
                void ApplyValue(double cur, double start, double energyMult)
                {
                    cur = Consts.StatValue(cur);
                    start = Consts.StatValue(start);
                    if (cur > start)
                    {
                        addEnergy += (cur - start) * energyMult;
                        cur = start;
                    }
                    totCur += cur;
                    totStart += start;
                }
                foreach (var d in killable.Protection)
                    ApplyValue(d.DefenseCur, CombatTypes.GetStartCur(d.Type, d.DefenseMax), Consts.EnergyPerShield); //assuming EnergyPerShield...
                if (piece.HasBehavior(out IAttacker attacker))
                    foreach (var a in attacker.Attacks)
                        ApplyValue(a.AttackCur, CombatTypes.GetStartCur(a.Type, a.AttackMax), Consts.EnergyPerAttack);
                if (totCur > totStart) throw new Exception();
                double mult = totStart == 0 ? 1 : ((totCur / totStart) + 1.0) / 2.0;

                mult *= Consts.ReplaceRefundPct * Consts.StatValue(killable.Hits.DefenseCur) / Consts.StatValue(killable.Hits.DefenseMax);
                double rounding = 1 - GetRounding();
                energy = MTRandom.Round(newEnergy - energy * mult - addEnergy, 1 - rounding);
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
            public Constructor Build(Tile tile)
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
                    false, out energy, out mass); //true
            }
        }
        [Serializable]
        public class BuildMech : Builder, IBuilder.IBuildMech
        {
            public BuildMech(Piece piece, IBuilder.Values values)
                : base(piece, values)
            {
            }
            public Mech Build(Tile tile, MechBlueprint blueprint)
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
                Tile tile = foundationPiece?.Tile;
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
                Tile tile = foundationPiece?.Tile;
                return Replace(doReplace, foundationPiece,
                    (out int e, out int m) => Turret.Cost(Piece.Game, out e, out m),
                    () => Turret.GetRounding(Piece.Game),
                    () => Turret.NewTurret((Foundation)tile.Piece),
                    false, out energy, out mass);
            }
        }
    }
}
