using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Extractor : PlayerPiece, IKillable
    {
        private readonly IKillable killable;

        public readonly Resource Resource;
        public Piece Piece => this;

        private Extractor(Map.Tile tile, Resource Resource, double vision, IKillable.Values killable)
            : base(tile, vision)
        {
            this.killable = new Killable(this, killable);
            SetBehavior(this.killable);

            this.Resource = Resource;
        }
        internal static Extractor NewExtractor(Resource resource)
        {
            Map.Tile tile = resource.Tile;
            resource.Game.RemovePiece(resource);

            double researchMult = Math.Pow(resource.Game.Player.GetResearchMult(), .6);
            double hits = 75 * researchMult;
            double vision = 5 * researchMult;

            Extractor obj = new(tile, resource, vision, new(hits, .78));
            resource.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(out double energy, out double mass, Resource resource)
        {
            resource.GetCost(out energy, out mass);
            double researchMult = Math.Pow(resource.Game.Player.GetResearchMult(), .5);
            energy /= researchMult;
            mass /= researchMult;
        }

        double IKillable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            Cost(out double energy, out double mass, Resource);
            return Consts.GetRepairCost(energy, mass);
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Resource.SetTile(tile);
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            Resource.GenerateResources(Piece, ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
        }
        internal override void EndTurn()
        {
            Resource.Extract();
            base.EndTurn();
        }

        public override string ToString()
        {
            return Resource.GetResourceName() + " Extractor " + PieceNum;
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Resilience => killable.Resilience;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldMax => killable.ShieldMax;
        public double ShieldLimit => killable.ShieldLimit;
        public bool Dead => killable.Dead;

        double IKillable.GetInc()
        {
            return killable.GetInc();
        }

        void IKillable.Repair(double hits)
        {
            killable.Repair(hits);
        }
        void IKillable.Damage(double damage, double shieldDmg)
        {
            killable.Damage(damage, shieldDmg);
        }

        #endregion IKillable
    }
}
