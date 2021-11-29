using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces.Players
{
    public class Extractor : PlayerPiece, IKillable
    {
        private readonly IKillable killable;

        public readonly Resource Resource;
        public Piece Piece => this;

        private Extractor(Resource Resource, IKillable.Values killable)
            : base(Resource.Tile, 5)
        {
            this.killable = new Killable(this, killable);
            SetBehavior(this.killable);

            this.Resource = Resource;
        }
        internal static Extractor NewExtractor(Resource resource, double researchMult)
        {
            resource.Game.RemovePiece(resource);

            researchMult = Math.Pow(researchMult, .6);
            double hits = 50 * researchMult;

            Extractor obj = new(resource, new(hits));
            resource.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(out double energy, out double mass, Resource resource, double researchMult)
        {
            resource.GetCost(out energy, out mass);
            researchMult = Math.Pow(researchMult, .5);
            energy /= researchMult;
            mass /= researchMult;
        }

        internal override void Die()
        {
            base.Die();
            Game.AddPiece(Resource);
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            Resource.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
        }
        internal override void EndTurn()
        {
            Resource.Extract();
        }

        public override string ToString()
        {
            return Resource.GetResourceName() + " Extractor";
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldMax => killable.ShieldMax;
        public double ShieldLimit => killable.ShieldLimit;

        void IKillable.Damage(ref double damage, ref double shieldDmg)
        {
            killable.Damage(ref damage, ref shieldDmg);
        }

        #endregion IKillable
    }
}
