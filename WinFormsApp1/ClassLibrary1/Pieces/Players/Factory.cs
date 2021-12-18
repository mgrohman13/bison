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
        public Piece Piece => this;

        private Factory(Map.Tile tile, double vision)
            : base(tile, vision)
        {
            SetBehavior(new Killable(this, GetValues(tile.Map.Game).killable));
            Unlock(tile.Map.Game.Player.Research);
        }

        internal static Factory NewFactory(Foundation foundation)
        {
            Map.Tile tile = foundation.Tile;
            foundation.Game.RemovePiece(foundation);

            Factory obj = new(tile, GetValues(tile.Map.Game).vision);
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            FactoryValues values = GetValues(game);
            energy = values.energy;
            mass = values.mass;
        }

        internal override void OnResearch(Research.Type type, double researchMult)
        {
            GetValues(Game).Upgrade(type, researchMult);
            Unlock(Game.Player.Research);
        }
        private void Unlock(Research research)
        {
            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.FactoryRepair))
                SetBehavior(new Repair(this, GetValues(Game).repair));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.FactoryConstructor))
                SetBehavior(new Builder.BuildConstructor(this));
            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this));
        }

        private static FactoryValues GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<FactoryValues>(() => new());
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Foundation.NewFoundation(tile);
        }

        double IKillable.IRepairable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            Cost(Game, out double energy, out double mass);
            return Consts.GetRepairCost(energy, mass);
        }

        public override string ToString()
        {
            return "Factory " + PieceNum;
        }

        private class FactoryValues : IUpgradeValues
        {
            public double energy, mass, vision;
            public IKillable.Values killable;
            public IRepair.Values repair;
            public FactoryValues()
            {
                UpgradeFactoryCost(1);
                UpgradeBuildingHits(1);
            }

            internal void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.FactoryCost)
                    researchMult = UpgradeFactoryCost(researchMult);
                else if (type == Research.Type.FactoryRepair)
                    researchMult = UpgradeFactoryRepair(researchMult);
                else if (type == Research.Type.BuildingHits)
                    researchMult = UpgradeBuildingHits(researchMult);
            }
            private double UpgradeFactoryCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .2);
                this.energy = this.mass = 300 / researchMult;
                return researchMult;
            }
            private double UpgradeFactoryRepair(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                double repairRange = 6.5 * researchMult;
                double repairRate = .078 * researchMult;
                this.repair = new(repairRange, repairRate);
                return researchMult;
            }
            private double UpgradeBuildingHits(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                this.vision = 6 * researchMult;
                double hits = 25 * researchMult;
                this.killable = new(hits, .65);
                return researchMult;
            }
        }
    }
}
