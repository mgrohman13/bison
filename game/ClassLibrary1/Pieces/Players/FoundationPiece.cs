using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class FoundationPiece : PlayerPiece
    {
        internal FoundationPiece(Tile tile, double vision)
            : base(tile, vision)
        {
        }

        internal override void Die(out Tile tile, out double treasure)
        {
            base.Die(out tile, out treasure);
            Foundation.NewFoundation(tile);
        }

        protected abstract bool CanReplace<T>(out Tuple<double, double> rounding);
        public Outpost ReplaceOutpost(bool doReplace, out int energy, out int mass, out bool replaceable)
        {
            replaceable = HasBehavior<IBuilder.IBuildOutpost>();
            Outpost.Cost(Game, out int energyCost, out int massCost);
            return Replace(doReplace, ref replaceable, out energy, out mass, energyCost, massCost, f => Outpost.NewOutpost(f));
        }
        public Factory ReplaceFactory(bool doReplace, out int energy, out int mass, out bool replaceable)
        {
            replaceable = HasBehavior<IBuilder.IBuildFactory>();
            Factory.Cost(Game, out int energyCost, out int massCost);
            return Replace(doReplace, ref replaceable, out energy, out mass, energyCost, massCost, f => Factory.NewFactory(f));
        }
        public Turret ReplaceTurret(bool doReplace, out int energy, out int mass, out bool replaceable)
        {
            replaceable = HasBehavior<IBuilder.IBuildTurret>();
            Turret.Cost(Game, out int energyCost, out int massCost);
            return Replace(doReplace, ref replaceable, out energy, out mass, energyCost, massCost, f => Turret.NewTurret(f));
        }
        public Generator ReplaceGenerator(bool doReplace, out int energy, out int mass, out bool replaceable)
        {
            replaceable = HasBehavior<IBuilder.IBuildGenerator>();
            Generator.Cost(Game, out int energyCost, out int massCost);
            return Replace(doReplace, ref replaceable, out energy, out mass, energyCost, massCost, f => Generator.NewGenerator(f));
        }
        private T Replace<T>(bool doReplace, ref bool replaceable, out int energy, out int mass,
            double energyCost, double massCost, Func<Foundation, T> NewPiece) where T : FoundationPiece
        {
            T newPiece = null;

            replaceable &= CanReplace<T>(out var tuple) && typeof(T) != this.GetType();
            if (replaceable)
            {

                double mult = Game.Consts.UpgRefundValue / Game.Consts.DisbandValue;
                DisbandValue(out double e, out double m);
                e *= mult;
                m *= mult;

                double rounding = (tuple.Item1 + tuple.Item2) % 1;
                energy = MTRandom.Round(energyCost - e, Consts.MAX_ROUND - rounding);
                mass = MTRandom.Round(massCost - m, rounding);

                if (doReplace && Game.Player.Spend(energy, mass))
                {
                    Die(out Tile tile, out double treasure);
                    Game.Enemy.AddResources(-treasure);
                    newPiece = NewPiece(tile.Piece as Foundation);
                }
            }
            else
            {
                energy = mass = 0;
            }

            return newPiece;
        }
    }
}
