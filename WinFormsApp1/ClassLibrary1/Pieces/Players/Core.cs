using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IKillable, IRepair, IBuilder.IBuildConstructor, IBuilder.IBuildMech
    {
        private readonly IKillable killable;
        private readonly IRepair repair;
        private readonly IBuilder.IBuildConstructor buildConstructor;
        private readonly IBuilder.IBuildMech buildMech;

        public Piece Piece => this;

        private Core(Game game, IKillable.Values killable, IRepair.Values repair)
            : base(game.Map.GetTile(0, 0), 4)
        {
            this.killable = new Killable(this, killable);
            this.repair = new Repair(this, repair);
            buildConstructor = new Builder.BuildConstructor(this);
            buildMech = new Builder.BuildMech(this);
            SetBehavior(this.killable, this.repair, this.buildConstructor, this.buildMech);
        }
        internal static Core NewCore(Game game)
        {
            Core obj = new(game, new(100, .91, .25, 1, 100, 300), new(Consts.MinMapCoord - 1.5, .1));
            game.AddPiece(obj);
            return obj;
        }

        internal override void Die()
        {
            Game.End();
        }

        double IKillable.RepairCost => GetRepairCost();
        public static double GetRepairCost()
        {
            return Consts.GetRepairCost(0, 5000);
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            energyInc += 300;
            massInc += 100;
            researchInc += 15;
        }

        public override string ToString()
        {
            return "Core";
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
