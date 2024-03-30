using ClassLibrary1.Pieces.Terrain;
using System;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Extractor : PlayerPiece, IKillable.IRepairable
    {
        public readonly Resource Resource;
        public Piece Piece => this;

        public double Sustain => Resource.Sustain * GetValues(Game).SustainMult;

        private Extractor(Tile tile, Resource Resource, Values values)
            : base(tile, values.Vision)
        {
            SetBehavior(new Killable(this, values.Killable, values.Resilience));

            this.Resource = Resource;
        }
        internal static Extractor NewExtractor(Resource resource)
        {
            Tile tile = resource.Tile;
            resource.Die();

            Extractor obj = new(tile, resource, GetValues(resource.Game));
            resource.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(out int energy, out int mass, Resource resource)
        {
            resource.GetCost(GetValues(resource.Game).CostMult, out energy, out mass);
        }

        internal override void OnResearch(Research.Type type)
        {
            Values values = GetValues(Game);

            this._vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(new[] { values.Killable }, values.Resilience);
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass, Resource);
                return Consts.GetRepairCost(energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.ExtractorAutoRepair);

        internal override void Die()
        {
            Tile tile = this.Tile;
            base.Die();
            Resource.SetTile(tile);
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc);
            Resource.GenerateResources(Piece, GetValues(Game).ValueMult, ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc);
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            double energyInc = 0, massInc = 0, researchInc = 0;
            GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc);
            base.EndTurn(ref energyUpk, ref massUpk);
            Resource.Extract(Piece, GetValues(Game).SustainMult);
        }

        public override string ToString()
        {
            return Resource.GetResourceName() + " Extractor " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilienceBase = .3;

            private double costMult, vision, valueMult, sustainMult, resilience;
            private IKillable.Values killable;
            public Values()
            {
                this.killable = new(DefenseType.Hits, -1);
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeExtractorValue(1);
            }

            public double Resilience => resilience;
            public double CostMult => costMult;
            public double Vision => vision;
            public double ValueMult => valueMult;
            public double SustainMult => sustainMult;
            public IKillable.Values Killable => killable;

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.ExtractorValue)
                    UpgradeExtractorValue(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                this.costMult = 1 / Math.Pow(researchMult, .6);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                double defAvg = 15 * Math.Pow(researchMult, .4);
                const double lowPenalty = 3;
                if (researchMult < lowPenalty)
                    defAvg *= researchMult / lowPenalty;
                int defense = Game.Rand.Round(defAvg);
                this.vision = 5 * Math.Pow(researchMult, .7);
                this.killable = new(DefenseType.Hits, defense);
            }
            private void UpgradeExtractorValue(double researchMult)
            {
                this.resilience = Consts.GetPct(resilienceBase, Math.Pow(researchMult, .4));

                this.valueMult = Math.Pow(researchMult, .5);
                this.sustainMult = Math.Pow(researchMult, .3);
                this.killable = new(DefenseType.Hits, killable.Defense);
            }
        }
    }
}
