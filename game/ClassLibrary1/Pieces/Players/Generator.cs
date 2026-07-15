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

        private double _rounding;

        private Generator(Tile tile)
            : base(tile, Values.Vision)
        {
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, new IKillable.Values(), Values.Resilience));
            Upgrade(Research.Type.Mech);
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => true;
        public bool CanRepair() => Consts.CanRepair(this);

        internal static Generator NewGenerator(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Generator obj = new(tile);
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }
        internal override void Cost(out int energy, out int mass) =>
            Cost(Game, out energy, out mass);

        internal override void OnResearch(Research.Type type)
        {
            Upgrade(type);
        }
        private void Upgrade(Research.Type type)
        {
            if (Values.AffectedBy(type))
                this._rounding = Game.Rand.NextDouble();

            Values values = GetValues(Game);
            GetBehavior<IKillable>().Upgrade([values.GetKillable(HitsMult(), _rounding)], Values.Resilience);
            this.Vision = Values.Vision;
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

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

            private int energy, mass;
            private double hits, inc;

            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingDefense(1);
                UpgradeAmbientGenerator(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public static double Vision => Attack.MELEE_RANGE;
            public double EnergyInc => inc;

            public IKillable.Values GetKillable(double hitsMult, double rounding)
            {
                return new(DefenseType.Hits, MTRandom.Round(hits * hitsMult, rounding));
            }

            public static bool AffectedBy(Research.Type type) => type switch
            {
                Research.Type.BuildingCost => false,
                Research.Type.BuildingDefense => true,
                Research.Type.AmbientGenerator => false,
                _ => false
            };
            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingDefense(researchMult);
                else if (type == Research.Type.AmbientGenerator)
                    UpgradeAmbientGenerator(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                double costMult = ResearchUpgValues.Calc(UpgType.TurretCost, researchMult);
                this.energy = Game.Rand.Round(Consts.GeneratorEnergyCost * costMult);
                this.mass = Game.Rand.Round(Consts.GeneratorMassCost * costMult);
            }
            private void UpgradeBuildingDefense(double researchMult)
            {
                this.hits = ResearchUpgValues.Calc(UpgType.ExtractorDefense, researchMult);
            }
            private void UpgradeAmbientGenerator(double researchMult)
            {
                this.inc = ResearchUpgValues.Calc(UpgType.AmbientGenerator, researchMult);
            }
        }
    }
}
