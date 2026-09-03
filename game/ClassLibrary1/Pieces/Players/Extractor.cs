using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Extractor : PlayerPiece, IKillable.IRepairable
    {
        //public static double Resilience => Values.Resilience;

        public readonly Resource Resource;

        public double Sustain => Resource.Sustain * GetValues(Game).SustainMult;
        private double _rounding;

        private Extractor(Tile tile, Resource Resource)
            : base(tile, 0)
        {
            this.Resource = Resource;
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, new IKillable.Values(), Resource.Resilience));
            Upgrade(Research.Type.Mech);
        }

        internal static Extractor NewExtractor(Resource resource)
        {
            Tile tile = resource.Tile;
            resource.Die();

            Extractor obj = new(tile, resource);
            resource.Game.AddPiece(obj);
            return obj;
        }
        internal override void Cost(out int energy, out int mass) =>
            Cost(Resource, out energy, out mass);
        private void BaseCost(out int energy, out int mass) =>
            Cost(Resource, 1, out energy, out mass);
        public static void Cost(Resource resource, out int energy, out int mass) =>
            Cost(resource, GetValues(resource.Game).CostMult, out energy, out mass);
        private static void Cost(Resource resource, double costMult, out int energy, out int mass) =>
            resource.GetCost(costMult, out energy, out mass);

        internal override void OnResearch(Research.Type type)
        {
            Upgrade(type);
        }
        private void Upgrade(Research.Type type)
        {
            if (Values.AffectedBy(type))
                this._rounding = Game.Rand.NextDouble();

            Values values = GetValues(Game);
            GetBehavior<IKillable>().Upgrade([values.GetKillable(HitsMult(), _rounding)], Resource.Resilience);
            this.Vision = values.Vision;
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.BuildingAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        internal override void Die(out Tile tile, out double treasure)
        {
            Die(true, out tile, out treasure);
        }
        private void Die(bool resource, out Tile tile, out double treasure)
        {
            base.Die(out tile, out treasure);
            //TODO: Consts
            if (resource && VanishStr() > Game.Rand.GaussianOE(13, .26, .13))
            {
                Resource.SetTile(tile);
            }
            else
            {
                BaseCost(out int energy, out int mass);
                treasure += energy + mass * Game.Consts.EnergyMassRatio;
            }
        }

        internal override void StartTurn()
        {
            base.StartTurn();

            if (VanishStr() <= Math.Min(Game.Rand.OEInt(), Game.Rand.OE()))
            {
                Die(false, out _, out _);
            }
            else
            {
                IKillable killable = GetBehavior<IKillable>();
                Defense hits = killable.Hits;
                int max = GetValues(Game).GetHits(HitsMult(), _rounding);
                int cur = MTRandom.Round(max * hits.DefenseCur / (double)hits.DefenseMax, _rounding);
                if (cur < 1)
                    cur = 1;
                killable.SetHits(cur, max);
            }
        }
        private double VanishStr()
        {
            Resource.GetIncome(out double energyInc, out double massInc, out double researchInc);
            return Math.Abs(energyInc) + Math.Abs(massInc) + Math.Abs(researchInc);
        }

        private double HitsMult()
        {
            BaseCost(out int energy, out int mass);
            return HitsMult(Game.Consts, energy + mass * Game.Consts.EnergyMassRatio);
        }
        internal static double HitsMult(Consts consts, double cost)
        {
            double AvgCost = (consts.BiomassExtractorEnergyCost
                + consts.MetalExtractorEnergyCost + consts.ArtifactExtractorEnergyCost
                + (consts.BiomassExtractorMassCost + consts.MetalExtractorMassCost
                + consts.ArtifactExtractorMassCost) * consts.EnergyMassRatio) / 3.0;
            return Math.Pow(cost / AvgCost, consts.ExtractorHitsPow);
        }

        internal override void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
            Resource.GenerateResources(this, GetValues(Game).ValueMult, out double e, out double m, out double r);
            if (ShutOff())
                e = m = r = 0;
            energyInc += e;
            massInc += m;
            researchInc += r;

            base.GenerateResources(ref energyInc, ref massInc, ref researchInc);
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            if (!ShutOff())
            {
                Values values = GetValues(Game);
                Resource.Extract(this, values.ValueMult, values.SustainMult);
            }
            else
                ;

            //will end up being slightly cheaper to repair than in GetUpkeep because of extracted resource value
            base.EndTurn(ref energyUpk, ref massUpk);
        }
        private bool ShutOff()
        {
            Resource.GetIncome(out double energyInc, out _, out _);
            return energyInc < 0 && Side.Energy < 0;
        }

        public override string ToString()
        {
            return Resource.GetResourceName() + " Extractor " + PieceNum;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            //public const double Resilience = .3;

            private double costMult, vision, hits, valueMult, sustainMult;

            public double CostMult => costMult;
            public double Vision => vision;
            public double ValueMult => valueMult;
            public double SustainMult => sustainMult;
            public double Hits => hits;

            public IKillable.Values GetKillable(double hitsMult, double rounding)
            {
                return new(DefenseType.Hits, GetHits(hitsMult, rounding));
            }
            public int GetHits(double hitsMult, double rounding)
            {
                return MTRandom.Round(hits * hitsMult, rounding);
            }

            public static bool AffectedBy(Research.Type type) => type switch
            {
                Research.Type.BuildingCost => true,
                Research.Type.BuildingDefense => true,
                Research.Type.ExtractorValue => true,
                _ => false
            };
            public void Init(Game game)
            {
                UpgradeBuildingCost(game, 1);
                UpgradeBuildingDefense(game, 1);
                UpgradeExtractorValue(game, 1);
                this.valueMult = 1;
                this.sustainMult = 1;
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(game, researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingDefense(game, researchMult);
                else if (type == Research.Type.ExtractorValue)
                    UpgradeExtractorValue(game, researchMult);
            }
            private void UpgradeBuildingCost(Game game, double researchMult)
            {
                this.costMult = game.ResearchUpgValues.Calc(UpgType.ExtractorCost, researchMult);
            }
            private void UpgradeBuildingDefense(Game game, double researchMult)
            {
                this.vision = game.ResearchUpgValues.Calc(UpgType.ExtractorVision, researchMult);
                this.hits = game.ResearchUpgValues.Calc(UpgType.ExtractorDefense, researchMult);
            }
            private void UpgradeExtractorValue(Game game, double researchMult)
            {
                this.valueMult = game.ResearchUpgValues.Calc(UpgType.ExtractorValue, researchMult);
                this.sustainMult = game.ResearchUpgValues.Calc(UpgType.ExtractorSustain, researchMult);
            }
        }
    }
}
