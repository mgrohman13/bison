using ClassLibrary1.Pieces.Terrain;
using System;
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
            SetBehavior(new Killable(this, values.Killable));

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
            GetBehavior<IKillable>().Upgrade(values.Killable);
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
            private double costMult, vision, valueMult, sustainMult;
            private IKillable.Values killable;
            public Values()
            {
                this.killable = new(-1, -1);
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeExtractorValue(1);
            }

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
                //researchMult = Math.Pow(researchMult, .5);
                int defense = Game.Rand.Round(15 * Math.Pow(researchMult, .7));
                this.vision = 5 * Math.Pow(researchMult, .5);
                this.killable = new(defense, killable.Resilience);
            }
            private void UpgradeExtractorValue(double researchMult)
            {
                double resilience = Consts.GetPct(.3, Math.Pow(researchMult, .4));
                this.valueMult = Math.Pow(researchMult, .5);
                this.sustainMult = Math.Pow(researchMult, .3);
                this.killable = new(killable.Defense, resilience);
            }
        }
    }
}
