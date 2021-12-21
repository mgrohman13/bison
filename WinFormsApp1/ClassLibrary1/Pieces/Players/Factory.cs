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
    public class Factory : PlayerPiece, IKillable.IRepairable
    {
        private readonly double _rangeMult;
        public Piece Piece => this;

        private Factory(Map.Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._rangeMult = Game.Rand.GaussianOE(values.RepairRange, .169, .13, 1) / values.RepairRange;

            SetBehavior(new Killable(this, values.Killable));
            Unlock(tile.Map.Game.Player.Research);
        }

        internal static Factory NewFactory(Foundation foundation)
        {
            Map.Tile tile = foundation.Tile;
            foundation.Game.RemovePiece(foundation);

            Factory obj = new(tile, GetValues(tile.Map.Game));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock(Game.Player.Research);
            Values values = GetValues(Game);
            GetBehavior<IKillable>().Upgrade(values.Killable);
            if (HasBehavior<IRepair>(out IRepair repair))
                repair.Upgrade(values.GetRepair(_rangeMult));
        }
        private void Unlock(Research research)
        {
            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this));
            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.FactoryRepair))
                SetBehavior(new Repair(this, GetValues(Game).GetRepair(_rangeMult)));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.FactoryConstructor))
                SetBehavior(new Builder.BuildConstructor(this));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Foundation.NewFoundation(tile);
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out double energy, out double mass);
                return Consts.GetRepairCost(energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.FactoryAutoRepair);

        public override string ToString()
        {
            return "Factory " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private double energy, mass, vision;
            private IKillable.Values killable;
            private IRepair.Values repair;
            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeFactoryRepair(1);
            }

            public double Energy => energy;
            public double Mass => mass;
            public double Vision => vision;
            public IKillable.Values Killable => killable;
            public double RepairRange => repair.Range;
            public IRepair.Values GetRepair(double rangeMult)
            {
                IRepair.Values repair = this.repair;
                double range = repair.Range * rangeMult;
                double rate = repair.Rate / Math.Pow(rangeMult, .78);
                return new(range, rate);
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingHits)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.FactoryRepair)
                    UpgradeFactoryRepair(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .3);
                this.energy = 650 / researchMult;
                this.mass = 250 / researchMult;
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                double hits = 50 * researchMult;
                this.vision = 6 * researchMult;
                this.killable = new(hits, .5);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .7);
                double repairRange = 6.5 * researchMult;
                double repairRate = .078 * researchMult;
                this.repair = new(repairRange, repairRate);
            }
        }
    }
}
