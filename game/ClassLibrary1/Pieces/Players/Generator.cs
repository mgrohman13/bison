using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Linq;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Generator : FoundationPiece, IKillable.IRepairable
    {
        public static double Resilience => Values.Resilience;

        private readonly double _rounding;//_mult

        private Generator(Tile tile, Values values)
            : base(tile, Values.Vision)
        {
            //this._mult = Game.Rand.GaussianCapped(1, .13);
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, values.GetKillable(HitsMult(), _rounding), Values.Resilience));
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

            this.Vision = Values.Vision;
            GetBehavior<IKillable>().Upgrade([values.GetKillable(HitsMult(), _rounding)], Values.Resilience);
        }

        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }
        internal static double GetRounding(Game game)
        {
            return GetValues(game).CostRounding;
        }

        internal override void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
            energyInc += Consts.GetDamagedValue(this, GetGenValue(), 0);
            researchInc -= Consts.GeneratorResearchUpk;

            base.GenerateResources(ref energyInc, ref massInc, ref researchInc);
        }
        private double GetGenValue(Tile testNew = null) => GetGenValue(Tile, this, testNew);
        private static double GetGenValue(Tile tile, Generator generator = null, Tile testNew = null)
        {
            Game game = tile.Map.Game;
            static double Logistic(double dist) =>
                (1 - 1 / (1.0 + Math.Pow(Math.E, -9.1 * (dist / (Consts.ResourceAvgDist * 1.13) - 1))));
            double div = 1 + game.Player.PiecesOfType<Generator>().Where(g => g != generator)
                .Select(g => g.Tile).Append(testNew).Where(t => t != null)
                .Select(t => t.GetDistance(tile)).Sum(Logistic);
            return GetValues(game).EnergyInc / div + Consts.GeneratorConstValue;
        }
        public static void PlacementEfficiency(Tile testNew, out double energy, out double pct)
        {
            Game game = testNew.Map.Game;
            double Sum(Func<Generator, double> GetGenValue) => game.Player.PiecesOfType<Generator>().Sum(GetGenValue);
            energy = GetGenValue(testNew) + Sum(g => g.GetGenValue(testNew)) - Sum(g => g.GetGenValue());
            pct = energy / GetValues(game).EnergyInc;
        }

        private static double HitsMult() =>
            Extractor.HitsMult(Consts.GeneratorEnergyCost + Consts.GeneratorMassCost * Consts.EnergyMassRatio);

        public override string ToString()
        {
            return "Ambient Generator " + PieceNum;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            public const double Resilience = .2;
            private double _costMult, _hits, _inc, _rounding;

            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeAmbientGenerator(1);
            }

            public static double Vision => Attack.MELEE_RANGE;
            public double CostRounding => _rounding;
            public double EnergyInc => _inc;

            internal void GetCost(out int energy, out int mass)
            {
                energy = MTRandom.Round(_costMult * Consts.GeneratorEnergyCost, 1 - CostRounding);
                mass = MTRandom.Round(_costMult * Consts.GeneratorMassCost, CostRounding);
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
