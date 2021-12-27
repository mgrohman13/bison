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
            this._rangeMult = Game.Rand.GaussianOE(values.BuilderRange, .169, .13, 1) / values.BuilderRange;

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
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock(Game.Player.Research);
            Values values = GetValues(Game);

            this._vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.Killable);
            if (HasBehavior<IRepair>(out IRepair repair))
                repair.Upgrade(values.GetRepair(_rangeMult));
            Builder.UpgradeAll(this, values.GetRepair(_rangeMult).Builder);
        }
        private void Unlock(Research research)
        {
            Values values = GetValues(Game);
            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, values.GetRepair(_rangeMult).Builder));
            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.FactoryRepair))
                SetBehavior(new Repair(this, values.GetRepair(_rangeMult)));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.FactoryConstructor))
                SetBehavior(new Builder.BuildConstructor(this, values.GetRepair(_rangeMult).Builder));
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
                Cost(Game, out int energy, out int mass);
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
            private int energy, mass;
            private double vision;
            private IKillable.Values killable;
            private IRepair.Values repair;
            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeFactoryRepair(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public IKillable.Values Killable => killable;
            public double BuilderRange => repair.Builder.Range;
            public IRepair.Values GetRepair(double rangeMult)
            {
                IRepair.Values repair = this.repair;
                double range = repair.Builder.Range * rangeMult;
                double rate = Consts.GetPct(repair.Rate, 1 / Math.Pow(rangeMult, 1.17));
                return new(new(range), rate);
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
                this.energy = Game.Rand.Round(650 / researchMult);
                this.mass = Game.Rand.Round(250 / researchMult);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                int hits = Game.Rand.Round(50 * researchMult);
                this.vision = 6 * researchMult;
                this.killable = new(hits, .5);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                double repairRange = 6.5 * researchMult;
                double repairRate = Consts.GetPct(.065, researchMult);
                this.repair = new(new(repairRange), repairRate);
            }
        }
    }
}
