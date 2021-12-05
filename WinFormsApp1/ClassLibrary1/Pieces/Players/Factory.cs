using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Factory : PlayerPiece, IKillable, IRepair, IBuilder.IBuildConstructor, IBuilder.IBuildMech
    {
        private readonly IKillable killable;
        private readonly IRepair repair;
        private readonly IBuilder.IBuildConstructor buildConstructor;
        private readonly IBuilder.IBuildMech buildMech;

        public Piece Piece => this;

        private Factory(Map.Tile tile, double vision, IKillable.Values killable, IRepair.Values repair)
            : base(tile, vision)
        {
            this.killable = new Killable(this, killable);
            this.repair = new Repair(this, repair);
            this.buildConstructor = new Builder.BuildConstructor(this);
            this.buildMech = new Builder.BuildMech(this);
            SetBehavior(this.killable, this.repair, this.buildConstructor, this.buildMech);
        }
        internal static Factory NewFactory(Foundation foundation)
        {
            Map.Tile tile = foundation.Tile;
            foundation.Game.RemovePiece(foundation);

            double researchMult = Math.Pow(foundation.Game.Player.GetResearchMult(), .6);
            double hits = 25 * researchMult;
            double repairRange = 6.5 * researchMult;
            double repairRate = .078 * researchMult;
            double vision = 6 * researchMult;

            Factory obj = new(tile, vision, new(hits, .65), new(repairRange, repairRate));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            double researchMult = Math.Pow(game.Player.GetResearchMult(), .2);
            energy = 150 / researchMult;
            mass = 150 / researchMult;
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Foundation.NewFoundation(tile);
        }

        double IKillable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            Cost(Game, out double energy, out double mass);
            return Consts.GetRepairCost(energy, mass);
        }

        public override string ToString()
        {
            return "Factory " + PieceNum;
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Resilience => killable.Resilience;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldIncBase => killable.ShieldIncBase;
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

        #region IRepair 

        public double Range => repair.Range;
        public double RangeBase => repair.RangeBase;
        public double Rate => repair.Rate;
        public double RateBase => repair.RateBase;

        double IRepair.GetRepairInc(IKillable killable)
        {
            return repair.GetRepairInc(killable);
        }

        #endregion IRepair 

        #region IBuilding 

        public Constructor Build(Map.Tile tile)
        {
            return buildConstructor.Build(tile);
        }
        public Mech Build(Map.Tile tile, MechBlueprint blueprint)
        {
            return buildMech.Build(tile, blueprint);
        }

        #endregion IBuilding

    }
}
