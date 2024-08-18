using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Generator : FoundationPiece, IKillable.IRepairable
    {
        private readonly double _rounding;//_mult

        private Generator(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            //this._mult = Game.Rand.GaussianCapped(1, .13);
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, values.GetKillable(HitsMult(), _rounding), values.Resilience));
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out int energy, out int mass);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => true;
        public bool CanRepair() => Consts.CanRepair(this);

        internal static Generator NewGenerator(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Generator obj = new(tile, GetValues(tile.Map.Game));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            GetValues(game).GetCost(out energy, out mass);
        }

        internal override void OnResearch(Research.Type type)
        {
            Values values = GetValues(Game);

            this.Vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(new[] { values.GetKillable(HitsMult(), _rounding) }, values.Resilience);
        }

        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }
        internal static double GetRounding(Game game)
        {
            return GetValues(game).Rounding;
        }

        internal override void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
            energyInc += Consts.GetDamagedValue(this, GetValues(Game).EnergyInc, 0);
            researchInc -= Consts.GeneratorResearchUpk;
        }

        private static double HitsMult() =>
            Extractor.HitsMult(Consts.GeneratorEnergyCost + Consts.GeneratorMassCost * Consts.EnergyMassRatio);

        public override string ToString() => "Ambient Generator";

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .2;
            private double _costMult, _hits, _inc, _rounding;

            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeAmbientGenerator(1);
            }

            public double Vision => Attack.MELEE_RANGE;
            public double Resilience => resilience;
            public double Rounding => _rounding;
            public double EnergyInc => _inc;

            internal void GetCost(out int energy, out int mass)
            {
                energy = MTRandom.Round(_costMult * Consts.GeneratorEnergyCost, 1 - Rounding);
                mass = MTRandom.Round(_costMult * Consts.GeneratorMassCost, Rounding);
            }

            public IKillable.Values GetKillable(double hitsMult, double rounding)
                => new(DefenseType.Hits, MTRandom.Round(_hits * hitsMult, rounding));

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.AmbientGenerator)
                    UpgradeAmbientGenerator(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                this._costMult = ResearchUpgValues.Calc(UpgType.AmbientGeneratorCost, researchMult);
                this._rounding = Game.Rand.NextDouble();
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                double defAvg = ResearchUpgValues.Calc(UpgType.ExtractorDefense, researchMult);
                this._hits = defAvg;
            }
            private void UpgradeAmbientGenerator(double researchMult)
            {
                this._inc = ResearchUpgValues.Calc(UpgType.AmbientGenerator, researchMult);
            }
        }
    }
}
