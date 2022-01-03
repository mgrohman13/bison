using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IKillable.IRepairable
    {
        public Piece Piece => this;

        private Core(Game game, Values values)
            : base(game.Map.GetTile(0, 0), values.Vision)
        {
            SetBehavior(new Killable(this, values.Killable), new Repair(this, values.Repair));
            Unlock(game.Player.Research);
        }
        internal static Core NewCore(Game game)
        {
            Core obj = new(game, GetValues(game));
            game.AddPiece(obj);
            return obj;
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock(Game.Player.Research);
            Values values = GetValues(Game);

            this._vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.Killable);
            GetBehavior<IRepair>().Upgrade(values.Repair);
            Builder.UpgradeAll(this, values.Repair.Builder);
        }
        private void Unlock(Research research)
        {
            Values values = GetValues(Game);
            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, values.Repair.Builder));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.Constructor))
                SetBehavior(new Builder.BuildConstructor(this, values.Repair.Builder));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        internal override void Die()
        {
            Game.End();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                return Consts.GetRepairCost(GetValues(Game).Energy, GetValues(Game).Mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc);
            energyInc += 350;
            massInc += 100;
            researchInc += 15;
        }

        public override string ToString()
        {
            return "Core";
        }


        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .7, armor = .25;
            private readonly double energy, mass;
            private readonly IRepair.Values repair;

            private double vision;
            private IKillable.Values killable;

            public Values()
            {
                this.energy = 2100;
                this.mass = 2100;
                this.repair = new(new(7.5), .1);

                this.killable = new(-1, -1);
                UpgradeBuildingHits(1);
            }

            public double Energy => energy;
            public double Mass => mass;
            public double Vision => vision;
            public IKillable.Values Killable => killable;
            public IRepair.Values Repair => repair;

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingHits)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.CoreShields)
                    UpgradeCoreShields(researchMult);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .4);
                int hits = Game.Rand.Round(100 * researchMult);
                this.vision = 3 * researchMult;
                this.killable = new(hits, resilience, armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
            }
            private void UpgradeCoreShields(double researchMult)
            {
                double max = 78 * Math.Pow(researchMult, .7);
                double limit = 91 * Math.Pow(researchMult, .8);
                int shieldMax = Game.Rand.Round(max);
                int shieldLimit = Game.Rand.Round(limit);
                double mult = Math.Pow((max * 2 + limit) / (shieldMax * 2 + shieldLimit), 1 / 6.5);
                double shieldInc = 1.3 * mult * Math.Pow(researchMult, .9);
                this.killable = new(killable.HitsMax, resilience, armor, shieldInc, shieldMax, shieldLimit);
            }
        }
    }
}
